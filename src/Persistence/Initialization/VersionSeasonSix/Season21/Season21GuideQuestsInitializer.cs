// <copyright file="Season21GuideQuestsInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

using System.Text.Json;
using MUnique.OpenMU.GameLogic.GuideQuests;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Sets the steps of the Season 21 quest guide (<c>Resources/Season21/GuideQuests.json</c>, written by
/// <c>Generator/season21-guide-quests.mjs</c> from S21Emu <c>QuestGuide/QuestGuide.txt</c>) and turns the guide quests on.
/// </summary>
internal class Season21GuideQuestsInitializer
{
    private const string ResourceName = "MUnique.OpenMU.Persistence.Initialization.Resources.Season21.GuideQuests.json";

    private readonly IContext _context;

    private readonly GameConfiguration _gameConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="Season21GuideQuestsInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Season21GuideQuestsInitializer(IContext context, GameConfiguration gameConfiguration)
    {
        this._context = context;
        this._gameConfiguration = gameConfiguration;
    }

    /// <summary>
    /// Sets the steps, unless the configuration has some already.
    /// </summary>
    public void Initialize()
    {
        var configuration = Season21PlugInConfigurations.GetOrCreate<GuideQuestConfiguration>(this._context, this._gameConfiguration, typeof(GuideQuestPlugIn).GUID, out var plugIn);
        if (configuration is null || configuration.Steps.Count > 0)
        {
            // A configuration which can't be read, or one which already has its steps, is the operator's.
            return;
        }

        var steps = Load();
        foreach (var step in steps)
        {
            // A reward the item catalog doesn't know would only be refused at the delivery.
            step.Items.RemoveAll(i => !this._gameConfiguration.Items.Any(d => d.Group == i.Group && d.Number == i.Number));
        }

        configuration.Steps = steps;

        // The first application turns the feature on; later ones leave the operator's switch alone.
        plugIn.IsActive = true;
        plugIn.SetConfiguration(configuration, null);
    }

    private static List<GuideQuestStep> Load()
    {
        using var stream = typeof(Season21GuideQuestsInitializer).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"The resource {ResourceName} is missing.");
        return JsonSerializer.Deserialize<List<GuideQuestStep>>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException($"The resource {ResourceName} is empty.");
    }
}
