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
