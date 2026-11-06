// <copyright file="SkillListViewBabylonPlugIn.cs" company="MUnique">
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
/// The skill list of the Babylon client: the whole list as <see cref="SkillListWide"/>, which isn't limited to the 62
/// skills of the Season 6 packet, followed by the skill enhancement tree. Single skills are added and removed as in
/// Season 6.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.SkillListViewBabylonPlugIn_Name), Description = nameof(PlugInResources.SkillListViewBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("5A9F1C38-7D24-4E6B-8B13-C0E4F2A97D51")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class SkillListViewBabylonPlugIn : SkillListViewPlugIn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SkillListViewBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public SkillListViewBabylonPlugIn(RemotePlayer player)
        : base(player)
    {
    }

    /// <inheritdoc/>
    public override async ValueTask UpdateSkillListAsync()
    {
        if (this.Player.Connection is not { } connection)
        {
            return;
        }

        this.BuildSkillList();

        int Write()
        {
            var size = SkillListWideRef.GetRequiredSize(this.SkillList.Count);
            var span = connection.Output.GetSpan(size)[..size];
            var packet = new SkillListWideRef(span)
            {
                SkillCount = (ushort)this.SkillList.Count,
            };

            for (var i = 0; i < this.SkillList.Count; i++)
            {
                var entry = packet[i];
                entry.SkillIndex = (ushort)i;
                if (this.SkillList[i] is { } skill)
                {
                    entry.SkillNumber = (ushort)skill.Number;
                    entry.Level = skill.MasterDefinition is null ? (byte)0 : (byte)(this.Player.SkillList!.GetSkill((ushort)skill.Number)?.Level ?? 0);
                }
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
        await this.Player.InvokeViewPlugInAsync<IEnhanceTreeViewPlugIn>(p => p.ShowEnhanceTreeAsync()).ConfigureAwait(false);
    }
}
