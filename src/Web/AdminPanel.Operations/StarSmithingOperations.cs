// <copyright file="StarSmithingOperations.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.AdminPanel.Operations;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.StarSmithing;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.Persistence.AdminAuth;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The website's dye editor (Admin > Dyes, Hidden-Babylon docs/star_smithing.md section 9): reads and replaces the dyes of
/// the Star Smithing plugin's configuration, every other setting kept.
/// </summary>
public sealed class StarSmithingOperations(IPersistenceContextProvider persistence, IDataSource<GameConfiguration> configurations, AdminOperationExecutor executor, AdminChangeConfirmation confirmations, AdminWriteSettings settings)
{
    /// <summary>Reads the dyes: group, number, the item's name, colours as <c>#rrggbb</c>, strength cap and flags.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The dyes.</returns>
    [AdminOperation(AdminRole.Viewer)]
    [NotExposedToAgents("The dyes are edited on the website (Admin > Star Smithing > Dyes).")]
    public async ValueTask<JsonObject> DyesAsync(CancellationToken cancellationToken)
    {
        var config = await configurations.GetOwnerAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        var plugin = config.PlugInConfigurations.SingleOrDefault(p => p.TypeId == typeof(StarSmithingFeaturePlugIn).GUID);
        var dyes = (plugin is null ? null : Read(plugin))?.Dyes ?? StarSmithingConfiguration.DefaultDyes();
        return new JsonObject
        {
            ["dyes"] = new JsonArray(dyes.Select(d => (JsonNode)new JsonObject
            {
                ["group"] = (int)d.Group,
                ["number"] = (int)d.Number,
                ["name"] = config.Items.FirstOrDefault(i => i.Group == d.Group && i.Number == d.Number)?.Name.ValueInNeutralLanguage ?? string.Empty,
                ["swatches"] = new JsonArray(d.Swatches.Select(c => (JsonNode)$"#{c:x6}").ToArray()),
                ["maximumStrength"] = (int)d.MaximumStrength,
                ["hue"] = d.Hue,
                ["keep"] = d.Keep,
                ["radiant"] = d.Radiant,
            }).ToArray()),
        };
    }

    /// <summary>Replaces the dyes, checked by <see cref="StarSmithDyes.Validate"/>; the other settings stay.</summary>
    /// <param name="dyes">A JSON array of <c>{group, number, swatches: ["#rrggbb"], maximumStrength, hue, keep, radiant}</c>.</param>
    /// <param name="apply">Whether to apply the confirmed preview.</param>
    /// <param name="confirmationToken">The preview token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The preview or applied result.</returns>
    [AdminOperation(AdminRole.Operator, ReadOnly = false)]
    [NotExposedToAgents("The dyes are edited on the website (Admin > Star Smithing > Dyes).")]
    public async ValueTask<JsonObject> SetDyesAsync(string dyes, bool apply, string? confirmationToken, CancellationToken cancellationToken)
    {
        var actor = await AdminChangeExecution.RequireAsync(executor, settings, AdminRoles.Operator, cancellationToken).ConfigureAwait(false);
        var list = Parse(dyes);
        using var gate = apply ? await confirmations.Gate.LockAsync(cancellationToken).ConfigureAwait(false) : null;
        var configId = (await configurations.GetOwnerAsync(cancellationToken: cancellationToken).ConfigureAwait(false)).GetId();
        using var context = persistence.CreateNewContext();
        if (apply && context is IConfigurationEditContext editContext)
        {
            await editContext.BeginConfigurationEditAsync(cancellationToken).ConfigureAwait(false);
        }

        var config = await context.GetByIdAsync<GameConfiguration>(configId, cancellationToken).ConfigureAwait(false)
            ?? throw new AdminOperationException("The active configuration could not be loaded.");
        if (StarSmithDyes.Validate(list, (group, number) => config.Items.Any(i => i.Group == group && i.Number == number)) is { } why)
        {
            throw new AdminOperationException(why, AdminOperationErrorKind.InvalidInput);
        }

        var plugin = config.PlugInConfigurations.SingleOrDefault(p => p.TypeId == typeof(StarSmithingFeaturePlugIn).GUID)
            ?? throw new AdminOperationException("The Star Smithing plugin is not installed yet. Start the game server once.");
        var settingsNow = Read(plugin) ?? new StarSmithingConfiguration();
        var before = settingsNow.Dyes.Count;
        settingsNow.Dyes = list;
        var json = JsonSerializer.Serialize(settingsNow);
        var change = new JsonObject { ["kind"] = "dyes", ["before"] = before, ["after"] = list.Count };
        return await AdminChangeExecution.RunAsync(
            executor,
            confirmations,
            actor,
            AdminRoles.Operator,
            "smithing_set_dyes",
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

    /// <summary>
    /// Reads the recipes and the balance in the bridge's camelCase shape (StarSmithing Readme.md, "PR 6 contract"), with
    /// the code ceilings, the stats and the names of the items the recipes use.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>{config: {recipes, balance}, ceilings, stats, items}</c>.</returns>
    [AdminOperation(AdminRole.Viewer)]
    [NotExposedToAgents("The recipes are edited on the website (Admin > Star Smithing > Recipes and Balance).")]
    public async ValueTask<JsonObject> ConfigAsync(CancellationToken cancellationToken)
    {
        var config = await configurations.GetOwnerAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        var plugin = config.PlugInConfigurations.SingleOrDefault(p => p.TypeId == typeof(StarSmithingFeaturePlugIn).GUID);
        var smithing = (plugin is null ? null : Read(plugin)) ?? new StarSmithingConfiguration();
        var used = smithing.Recipes
            .SelectMany(r => r.Ingredients.Select(i => (i.Group, i.Number)).Concat(r.Catalysts.Select(c => (c.Group, c.Number))).Append((r.HintGroup, r.HintNumber)))
            .ToHashSet();
        var ceilings = new JsonObject();
        foreach (var stat in Enum.GetValues<StarSmithBoostStat>())
        {
            ceilings[StarSmithConfigJson.Name(stat)] = StarSmithBalance.Ceiling(stat);
        }

        return new JsonObject
        {
            ["config"] = StarSmithConfigJson.Write(smithing),
            ["ceilings"] = ceilings,
            ["stats"] = new JsonArray(Enum.GetValues<StarSmithBoostStat>().Select(s => (JsonNode)StarSmithConfigJson.Name(s)).ToArray()),
            ["items"] = new JsonArray(config.Items
                .Where(i => used.Contains(((byte)i.Group, i.Number)))
                .OrderBy(i => i.Group).ThenBy(i => i.Number)
                .Select(i => (JsonNode)new JsonObject { ["group"] = (int)i.Group, ["number"] = (int)i.Number, ["name"] = i.Name.ValueInNeutralLanguage })
                .ToArray()),
        };
    }

    /// <summary>
    /// Replaces the recipes and/or the balance, checked by <see cref="StarSmithConfigValidator.Validate"/>; a part left
    /// out and every other setting stay. A refusal names each field at fault (<c>fields</c>).
    /// </summary>
    /// <param name="config">The JSON <c>{recipes?, balance?}</c> in the shape <see cref="ConfigAsync"/> reads.</param>
    /// <param name="apply">Whether to apply the confirmed preview.</param>
    /// <param name="confirmationToken">The preview token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The preview or applied result.</returns>
    [AdminOperation(AdminRole.Operator, ReadOnly = false)]
    [NotExposedToAgents("The recipes are edited on the website (Admin > Star Smithing > Recipes and Balance).")]
    public async ValueTask<JsonObject> SetConfigAsync(string config, bool apply, string? confirmationToken, CancellationToken cancellationToken)
    {
        var actor = await AdminChangeExecution.RequireAsync(executor, settings, AdminRoles.Operator, cancellationToken).ConfigureAwait(false);
        var (recipes, balance, readErrors) = StarSmithConfigJson.Read(config);
        if (readErrors.Count > 0 && recipes is null && balance is null)
        {
            throw Refusal(readErrors);
        }

        using var gate = apply ? await confirmations.Gate.LockAsync(cancellationToken).ConfigureAwait(false) : null;
        var configId = (await configurations.GetOwnerAsync(cancellationToken: cancellationToken).ConfigureAwait(false)).GetId();
        using var context = persistence.CreateNewContext();
        if (apply && context is IConfigurationEditContext editContext)
        {
            await editContext.BeginConfigurationEditAsync(cancellationToken).ConfigureAwait(false);
        }

        var gameConfig = await context.GetByIdAsync<GameConfiguration>(configId, cancellationToken).ConfigureAwait(false)
            ?? throw new AdminOperationException("The active configuration could not be loaded.");
        var plugin = gameConfig.PlugInConfigurations.SingleOrDefault(p => p.TypeId == typeof(StarSmithingFeaturePlugIn).GUID)
            ?? throw new AdminOperationException("The Star Smithing plugin is not installed yet. Start the game server once.");
        var settingsNow = Read(plugin) ?? new StarSmithingConfiguration();
        var recipesBefore = settingsNow.Recipes.Count;
        var balanceBefore = JsonSerializer.Serialize(settingsNow.Balance);
        settingsNow.Recipes = recipes ?? settingsNow.Recipes;
        settingsNow.Balance = balance ?? settingsNow.Balance;
        var errors = readErrors
            .Concat(StarSmithConfigValidator.Validate(settingsNow, (group, number) => gameConfig.Items.Any(i => i.Group == group && i.Number == number)))
            .GroupBy(e => e.Field)
            .Select(g => g.First())
            .ToList();
        if (errors.Count > 0)
        {
            throw Refusal(errors);
        }

        var json = JsonSerializer.Serialize(settingsNow);
        var change = new JsonObject
        {
            ["kind"] = "config",
            ["recipesBefore"] = recipesBefore,
            ["recipesAfter"] = settingsNow.Recipes.Count,
            ["balanceChanged"] = balanceBefore != JsonSerializer.Serialize(settingsNow.Balance),
        };
        return await AdminChangeExecution.RunAsync(
            executor,
            confirmations,
            actor,
            AdminRoles.Operator,
            "smithing_set_config",
            new { recipes = settingsNow.Recipes.Count, fingerprint = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(json))) },
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

    private static AdminOperationException Refusal(IReadOnlyCollection<StarSmithConfigError> errors)
        => new(
            errors.Count == 1 ? "1 field is wrong." : $"{errors.Count} fields are wrong.",
            AdminOperationErrorKind.InvalidInput,
            fields: new JsonArray(errors.Select(e => (JsonNode)new JsonObject { ["field"] = e.Field, ["message"] = e.Message }).ToArray()));

    private static StarSmithingConfiguration? Read(PlugInConfiguration plugin)
    {
        try
        {
            return plugin.CustomConfiguration is { Length: > 0 } json ? JsonSerializer.Deserialize<StarSmithingConfiguration>(json) : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static List<StarSmithDye> Parse(string dyes)
    {
        JsonArray? array;
        try
        {
            array = JsonNode.Parse(dyes) as JsonArray;
        }
        catch (JsonException)
        {
            array = null;
        }

        if (array is null)
        {
            throw new AdminOperationException("The dyes must be a JSON array.", AdminOperationErrorKind.InvalidInput);
        }

        var list = new List<StarSmithDye>();
        foreach (var node in array.OfType<JsonObject>())
        {
            var swatches = new List<int>();
            foreach (var text in node["swatches"]?.AsArray().Select(s => s?.GetValue<string>() ?? string.Empty) ?? [])
            {
                var hex = text.Trim().TrimStart('#');
                if (hex.Length != 6 || !int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var colour))
                {
                    throw new AdminOperationException($"'{text}' is not a colour as #rrggbb.", AdminOperationErrorKind.InvalidInput);
                }

                swatches.Add(colour);
            }

            list.Add(new StarSmithDye
            {
                Group = (byte)Math.Clamp(node["group"]?.GetValue<int>() ?? 14, 0, byte.MaxValue),
                Number = (short)Math.Clamp(node["number"]?.GetValue<int>() ?? 0, 0, short.MaxValue),
                Swatches = swatches,
                MaximumStrength = (byte)Math.Clamp(node["maximumStrength"]?.GetValue<int>() ?? 85, 0, 255),
                Hue = node["hue"]?.GetValue<bool>() ?? false,
                Keep = node["keep"]?.GetValue<bool>() ?? false,
                Radiant = node["radiant"]?.GetValue<bool>() ?? false,
            });
        }

        return list;
    }
}
