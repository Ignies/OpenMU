// <copyright file="AddProgressionUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Progression;
using MUnique.OpenMU.GameLogic.Resets;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Turns on the progression of the Season 21 maps, in the stars mode by default, and the reset feature with the star
/// costs which the stars need.
/// </summary>
/// <remarks>
/// The rows of the Season 21 maps stay as they are: the progression computes the effective numbers at runtime. An
/// existing progression configuration is kept; of the reset configuration, only the values which are still at the
/// reset feature's defaults get the star costs.
/// </remarks>
[PlugIn]
[Display(Name = nameof(Properties.PlugInResources.AddProgressionUpdatePlugIn_Name), Description = nameof(Properties.PlugInResources.AddProgressionUpdatePlugIn_Description), ResourceType = typeof(Properties.PlugInResources))]
[Guid("51343E19-54FB-4A24-A531-16FFAFBDC7C5")]
public class AddProgressionUpdatePlugIn : UpdatePlugInBase
{
    /// <summary>
    /// The group of the Jewel of Life, the item of the later stars.
    /// </summary>
    private const byte JewelOfLifeGroup = 14;

    /// <summary>
    /// The number of the Jewel of Life.
    /// </summary>
    private const short JewelOfLifeNumber = 16;

    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => Properties.PlugInResources.AddProgressionUpdatePlugIn_Name;

    /// <inheritdoc />
    public override string Description => Properties.PlugInResources.AddProgressionUpdatePlugIn_Description;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override bool RunsOnNewDatabase => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 11, 05, 18, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    public override IEnumerable<UpdateDependency> DependsOn => [typeof(AddSeason21MapsUpdatePlugIn).GUID];

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        var referenceHandler = new ItemReferenceHandler(gameConfiguration);
        var progression = GetOrCreate(context, gameConfiguration, typeof(ProgressionFeaturePlugIn).GUID);
        progression.IsActive = true;
        if (progression.GetConfiguration<ProgressionConfiguration>(null) is not { } configuration)
        {
            configuration = new ProgressionConfiguration();
            progression.SetConfiguration(configuration, null);
        }

        if (configuration.Mode != ProgressionMode.Stars)
        {
            return ValueTask.CompletedTask;
        }

        var reset = GetOrCreate(context, gameConfiguration, typeof(ResetFeaturePlugIn).GUID);
        ResetConfiguration? resetConfiguration;
        try
        {
            resetConfiguration = reset.GetConfiguration<ResetConfiguration>(referenceHandler) ?? new ResetConfiguration();
        }
        catch (JsonException)
        {
            // A configuration which can't be read is the operator's; it stays.
            return ValueTask.CompletedTask;
        }

        var jewelOfLife = gameConfiguration.Items.FirstOrDefault(i => i.Group == JewelOfLifeGroup && i.Number == JewelOfLifeNumber);
        StarResetDefaults.ApplyTo(resetConfiguration, jewelOfLife);
        reset.SetConfiguration(resetConfiguration, referenceHandler);
        reset.IsActive = true;
        return ValueTask.CompletedTask;
    }

    private static PlugInConfiguration GetOrCreate(IContext context, GameConfiguration gameConfiguration, Guid typeId)
    {
        if (gameConfiguration.PlugInConfigurations.FirstOrDefault(c => c.TypeId == typeId) is { } existing)
        {
            return existing;
        }

        var created = context.CreateNew<PlugInConfiguration>();
        created.SetGuid(typeId);
        created.TypeId = typeId;
        gameConfiguration.PlugInConfigurations.Add(created);
        return created;
    }

    /// <summary>
    /// A reference handler which writes items by their id and reads them back from the game configuration.
    /// </summary>
    private sealed class ItemReferenceHandler(GameConfiguration gameConfiguration) : ReferenceHandler
    {
        /// <inheritdoc />
        public override ReferenceResolver CreateResolver() => new ItemReferenceResolver(gameConfiguration);
    }

    /// <summary>
    /// The resolver of <see cref="ItemReferenceHandler"/>.
    /// </summary>
    private sealed class ItemReferenceResolver(GameConfiguration gameConfiguration) : ReferenceResolver
    {
        /// <inheritdoc />
        public override void AddReference(string referenceId, object value)
        {
            // The items come from the game configuration.
        }

        /// <inheritdoc />
        public override string GetReference(object value, out bool alreadyExists)
        {
            if (value is IIdentifiable identifiable)
            {
                alreadyExists = true;
                return identifiable.Id.ToString();
            }

            alreadyExists = false;
            return string.Empty;
        }

        /// <inheritdoc />
        public override object ResolveReference(string referenceId)
        {
            return gameConfiguration.Items.FirstOrDefault(i => i is IIdentifiable identifiable && identifiable.Id.ToString() == referenceId)
                   ?? throw new JsonException($"The item '{referenceId}' is not in the configuration.");
        }
    }
}
