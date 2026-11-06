// <copyright file="EnhanceTreeBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Character;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.EnhanceTree;
using MUnique.OpenMU.GameLogic.Views.Character;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Shows the skill enhancement tree to the Babylon client (<see cref="EnhanceTreeState"/>, <see cref="EnhanceTreeAddPointResult"/>).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.EnhanceTreeBabylonPlugIn_Name), Description = nameof(PlugInResources.EnhanceTreeBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("8D4E2A71-B6C3-4F19-9E05-3A7C1D8F6B24")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class EnhanceTreeBabylonPlugIn : IEnhanceTreeViewPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnhanceTreeBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public EnhanceTreeBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask ShowEnhanceTreeAsync()
    {
        if (this._player.Connection is not { } connection
            || EnhanceTreeFeaturePlugIn.GetConfiguration(this._player.GameContext) is not { } configuration
            || this._player.SelectedCharacter is not { CharacterClass: { } characterClass } character
            || ClassCodes.GetBaseClass(characterClass.Number) is not { } baseClass
            || characterClass.GetEvolution() is not (ClassEvolution.Fourth or ClassEvolution.Fifth))
        {
            return;
        }

        var treeSkills = configuration.GetTreeSkills(baseClass);
        var learned = character.LearnedSkills.Where(s => s.Skill is { } skill && treeSkills.Contains(skill.Number)).ToList();
        var free = EnhanceTreePoints.GetFree(this._player, configuration);

        int Write()
        {
            var size = EnhanceTreeStateRef.GetRequiredSize(learned.Count);
            var span = connection.Output.GetSpan(size)[..size];
            var packet = new EnhanceTreeStateRef(span)
            {
                FreePoints = (ushort)free,
                SpentPoints = (ushort)learned.Sum(s => s.Level),
                SkillCount = (ushort)learned.Count,
            };

            for (var i = 0; i < learned.Count; i++)
            {
                var entry = packet[i];
                entry.SkillNumber = (ushort)learned[i].Skill!.Number;
                entry.Level = (byte)learned[i].Level;
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask ShowEnhanceTreePointResultAsync(EnhanceTreePointResult result, short skillNumber, int level)
    {
        var free = EnhanceTreeFeaturePlugIn.GetConfiguration(this._player.GameContext) is { } configuration
            ? EnhanceTreePoints.GetFree(this._player, configuration)
            : 0;
        await this._player.Connection.SendEnhanceTreeAddPointResultAsync((byte)result, (ushort)skillNumber, (byte)level, (ushort)free).ConfigureAwait(false);
    }
}
