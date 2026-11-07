// <copyright file="UpdateMasterSkillsBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Character;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Views.Character;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The master skill list of the Babylon client (<see cref="MasterSkillListWide"/>): each skill by its number. The Season 6
/// list names a slot of the Season 6 client tree, which the classes after Season 6 do not have.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.UpdateMasterSkillsBabylonPlugIn_Name), Description = nameof(PlugInResources.UpdateMasterSkillsBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("E2C47B19-3F8A-4D60-B5A2-9C1D6E3F7A08")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class UpdateMasterSkillsBabylonPlugIn : IUpdateMasterSkillsPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateMasterSkillsBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public UpdateMasterSkillsBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc/>
    public async ValueTask UpdateMasterSkillsAsync()
    {
        if (this._player.Connection is not { } connection
            || this._player.SelectedCharacter is not { } character
            || this._player.SkillList is not { } skillList)
        {
            return;
        }

        var masterSkills = skillList.Skills.Where(s => s.Skill?.MasterDefinition?.Root is not null).ToList();

        int Write()
        {
            var size = MasterSkillListWideRef.GetRequiredSize(masterSkills.Count);
            var span = connection.Output.GetSpan(size)[..size];
            var packet = new MasterSkillListWideRef(span)
            {
                MasterLevelUpPoints = (ushort)character.MasterLevelUpPoints,
                SkillCount = (ushort)masterSkills.Count,
            };

            for (var i = 0; i < masterSkills.Count; i++)
            {
                var entry = packet[i];
                entry.SkillNumber = (ushort)masterSkills[i].Skill!.Number;
                entry.Level = (byte)masterSkills[i].Level;
                entry.DisplayValue = masterSkills[i].CalculateDisplayValue();
                entry.DisplayValueOfNextLevel = masterSkills[i].CalculateNextDisplayValue();
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }
}
