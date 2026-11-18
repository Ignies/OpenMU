// <copyright file="ItemModerationOperations.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.AdminPanel.Operations;

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic.ItemCustomization;
using MUnique.OpenMU.GameLogic.TextModeration;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.Persistence.AdminAuth;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Moderation of the names and lore of customised items (Star Smithing): the word list of <see cref="TextModerationPlugIn"/>,
/// the same check the game runs, and an admin's reset of a name or lore with a reason. Writes are previewed and audited.
/// </summary>
/// <param name="persistence">The persistence provider.</param>
/// <param name="configurations">The active configuration source.</param>
/// <param name="executor">The audit executor.</param>
/// <param name="confirmations">The confirmation coordinator.</param>
/// <param name="settings">The write policy.</param>
public sealed class ItemModerationOperations(IPersistenceContextProvider persistence, IDataSource<GameConfiguration> configurations, AdminOperationExecutor executor, AdminChangeConfirmation confirmations, AdminWriteSettings settings)
{
    /// <summary>The most entries of the word list.</summary>
    public const int MaximumWords = 2000;

    /// <summary>The longest word list entry.</summary>
    public const int MaximumWordLength = 40;

    /// <summary>The longest reset reason.</summary>
    public const int MaximumReasonLength = 200;

    /// <summary>Checks a name or lore as the game would store it.</summary>
    /// <param name="kind"><c>name</c> or <c>lore</c>.</param>
    /// <param name="text">The text.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>{ok, text, refusal, word}</c>.</returns>
    [AdminOperation(AdminRole.Viewer)]
    [NotExposedToAgents("Moderating player text on items is a website task (Admin > Star Smithing > Moderation).")]
    public async ValueTask<JsonObject> CheckTextAsync(string kind, string text, CancellationToken cancellationToken)
    {
        var verdict = TextModeration.Check(KindOf(kind), text, await this.WordListAsync(cancellationToken).ConfigureAwait(false));
        return new JsonObject
        {
            ["ok"] = verdict.Ok,
            ["text"] = verdict.Text,
            ["refusal"] = verdict.Ok ? null : JsonNamingPolicy.CamelCase.ConvertName(verdict.Refusal.ToString()),
            ["word"] = verdict.Word,
        };
    }

    /// <summary>The word list.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>{words: [{word, substring, language}]}</c>.</returns>
    [AdminOperation(AdminRole.Viewer)]
    [NotExposedToAgents("Moderating player text on items is a website task (Admin > Star Smithing > Moderation).")]
    public async ValueTask<JsonObject> WordsAsync(CancellationToken cancellationToken)
    {
        var words = (await this.WordListAsync(cancellationToken).ConfigureAwait(false))?.Words ?? [];
        return new JsonObject { ["words"] = new JsonArray(words.Select(w => (JsonNode)new JsonObject { ["word"] = w.Word, ["substring"] = w.Substring, ["language"] = w.Language }).ToArray()) };
    }

    /// <summary>Replaces the word list (entries trimmed; empty ones and repeats dropped).</summary>
    /// <param name="words">A JSON array of <c>{word, substring, language}</c>.</param>
    /// <param name="apply">Whether to apply the confirmed preview.</param>
    /// <param name="confirmationToken">The preview token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The preview or applied result.</returns>
    [AdminOperation(AdminRole.Operator, ReadOnly = false)]
    [NotExposedToAgents("Moderating player text on items is a website task (Admin > Star Smithing > Moderation).")]
    public async ValueTask<JsonObject> SetWordsAsync(string words, bool apply, string? confirmationToken, CancellationToken cancellationToken)
    {
        var actor = await AdminChangeExecution.RequireAsync(executor, settings, AdminRoles.Operator, cancellationToken).ConfigureAwait(false);
        var list = ParseWords(words);
        using var gate = apply ? await confirmations.Gate.LockAsync(cancellationToken).ConfigureAwait(false) : null;
        var configId = (await configurations.GetOwnerAsync(cancellationToken: cancellationToken).ConfigureAwait(false)).GetId();
        using var context = persistence.CreateNewContext();
        if (apply && context is IConfigurationEditContext editContext)
        {
            await editContext.BeginConfigurationEditAsync(cancellationToken).ConfigureAwait(false);
        }

        var config = await context.GetByIdAsync<GameConfiguration>(configId, cancellationToken).ConfigureAwait(false)
            ?? throw new AdminOperationException("The active configuration could not be loaded.");
        var plugin = config.PlugInConfigurations.SingleOrDefault(p => p.TypeId == typeof(TextModerationPlugIn).GUID)
            ?? throw new AdminOperationException("The text moderation plugin is not installed yet. Start the game server once.");
        var before = Read(plugin)?.Words ?? [];
        var json = JsonSerializer.Serialize(new TextModerationConfiguration { Words = list });
        var change = new JsonObject
        {
            ["kind"] = "wordList",
            ["before"] = before.Count,
            ["after"] = list.Count,
            ["added"] = new JsonArray(list.Where(w => !before.Any(b => Same(b, w))).Select(w => (JsonNode)w.Word).ToArray()),
            ["removed"] = new JsonArray(before.Where(b => !list.Any(w => Same(b, w))).Select(b => (JsonNode)b.Word).ToArray()),
        };
        return await AdminChangeExecution.RunAsync(
            executor,
            confirmations,
            actor,
            AdminRoles.Operator,
            "items_set_words",
            new { count = list.Count, fingerprint = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(json))) },
            change,
            apply,
            confirmationToken,
            async () =>
            {
                plugin.CustomConfiguration = json;
                await AdminChangeExecution.SaveAsync(context, cancellationToken).ConfigureAwait(false);
                await configurations.InvalidateAsync().ConfigureAwait(false);
            },
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Resets the name or the lore of a customised item, with a reason; the item keeps everything else.</summary>
    /// <param name="customizationId">The record's id.</param>
    /// <param name="field"><c>name</c> or <c>lore</c>.</param>
    /// <param name="reason">Why, for the audit.</param>
    /// <param name="apply">Whether to apply the confirmed preview.</param>
    /// <param name="confirmationToken">The preview token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The preview or applied result.</returns>
    [AdminOperation(AdminRole.Operator, ReadOnly = false)]
    [NotExposedToAgents("Moderating player text on items is a website task (Admin > Star Smithing > Moderation).")]
    public async ValueTask<JsonObject> ResetTextAsync(Guid customizationId, string field, string reason, bool apply, string? confirmationToken, CancellationToken cancellationToken)
    {
        var actor = await AdminChangeExecution.RequireAsync(executor, settings, AdminRoles.Operator, cancellationToken).ConfigureAwait(false);
        var name = KindOf(field) == TextKind.Name;
        reason = (reason ?? string.Empty).Trim();
        if (reason.Length == 0 || reason.Length > MaximumReasonLength)
        {
            throw new AdminOperationException($"A reset needs a reason of 1 to {MaximumReasonLength} characters.", AdminOperationErrorKind.InvalidInput);
        }

        using var gate = apply ? await confirmations.Gate.LockAsync(cancellationToken).ConfigureAwait(false) : null;
        using var context = persistence.CreateNewContext();
        var record = await context.GetByIdAsync<ItemCustomization>(customizationId, cancellationToken).ConfigureAwait(false)
            ?? throw new AdminOperationException("No such customised item.", AdminOperationErrorKind.InvalidInput);
        var previous = name ? record.Name : record.Lore;
        var change = new JsonObject
        {
            ["kind"] = "itemText",
            ["itemId"] = record.ItemId.ToString(),
            ["field"] = name ? "name" : "lore",
            ["before"] = previous,
            ["reason"] = reason,
            ["version"] = record.Version,
        };
        return await AdminChangeExecution.RunAsync(
            executor,
            confirmations,
            actor,
            AdminRoles.Operator,
            "items_reset_text",
            new { customizationId, field = name ? "name" : "lore", reason, record.Version },
            change,
            apply,
            confirmationToken,
            async () =>
            {
                if (name)
                {
                    record.Name = null;
                }
                else
                {
                    record.Lore = null;
                }

                record.Version++;
                record.UpdatedAt = DateTime.UtcNow;
                await AdminChangeExecution.SaveAsync(context, cancellationToken).ConfigureAwait(false);
                ItemCustomizationCache.Forget(record.ItemId);
            },
            cancellationToken).ConfigureAwait(false);
    }

    private static TextKind KindOf(string kind) => kind?.Trim().ToLowerInvariant() switch
    {
        "name" => TextKind.Name,
        "lore" => TextKind.Lore,
        _ => throw new AdminOperationException("The field is name or lore.", AdminOperationErrorKind.InvalidInput),
    };

    private static bool Same(ModeratedWord a, ModeratedWord b) =>
        string.Equals(a.Word, b.Word, StringComparison.OrdinalIgnoreCase) && a.Substring == b.Substring;

    private static TextModerationConfiguration? Read(PlugInConfiguration plugin)
    {
        try
        {
            return plugin.CustomConfiguration is { Length: > 0 } json ? JsonSerializer.Deserialize<TextModerationConfiguration>(json) : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static List<ModeratedWord> ParseWords(string words)
    {
        JsonArray entries;
        try
        {
            entries = JsonNode.Parse(words) as JsonArray ?? throw new JsonException();
        }
        catch (JsonException)
        {
            throw new AdminOperationException("The word list is a JSON array of {word, substring, language}.", AdminOperationErrorKind.InvalidInput);
        }

        var list = new List<ModeratedWord>();
        foreach (var entry in entries.OfType<JsonObject>())
        {
            var word = entry["word"]?.GetValue<string>()?.Trim() ?? string.Empty;
            if (word.Length == 0)
            {
                continue;
            }

            if (word.Length > MaximumWordLength)
            {
                throw new AdminOperationException($"A word list entry is at most {MaximumWordLength} characters: {word[..MaximumWordLength]}...", AdminOperationErrorKind.InvalidInput);
            }

            var moderated = new ModeratedWord { Word = word, Substring = entry["substring"]?.GetValue<bool>() ?? false, Language = entry["language"]?.GetValue<string>()?.Trim() ?? string.Empty };
            if (!list.Any(w => Same(w, moderated)))
            {
                list.Add(moderated);
            }
        }

        return list.Count <= MaximumWords ? list
            : throw new AdminOperationException($"The word list holds at most {MaximumWords} entries.", AdminOperationErrorKind.InvalidInput);
    }

    private async ValueTask<TextModerationConfiguration?> WordListAsync(CancellationToken cancellationToken)
    {
        var config = await configurations.GetOwnerAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return config.PlugInConfigurations.FirstOrDefault(p => p.TypeId == typeof(TextModerationPlugIn).GUID) is { } plugin ? Read(plugin) : null;
    }
}
