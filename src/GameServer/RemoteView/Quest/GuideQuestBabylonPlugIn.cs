// <copyright file="GuideQuestBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.Quest;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.GuideQuests;
using MUnique.OpenMU.GameLogic.Views.Quest;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// The Babylon implementation of the <see cref="IGuideQuestViewPlugIn"/>: C1 EE C0.
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.GuideQuestBabylonPlugIn_Name), Description = nameof(PlugInResources.GuideQuestBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("B4E19A63-7C25-4D8E-9F01-3A6B8D2C5E74")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class GuideQuestBabylonPlugIn : IGuideQuestViewPlugIn
{
    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="GuideQuestBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public GuideQuestBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc />
    public async ValueTask ShowGuideQuestAsync(GuideQuestView view, GuideQuestResult result)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var step = view.Step;
        var items = step?.Items.Take(byte.MaxValue).ToList() ?? [];
        int Write()
        {
            var size = GuideQuestStateRef.GetRequiredSize(items.Count);
            var span = connection.Output.GetSpan(size)[..size];

            // The fields of a missing step stay zero.
            span.Clear();
            var packet = new GuideQuestStateRef(span)
            {
                Status = (byte)view.Status,
                Result = (byte)result,
                StepIndex = (ushort)Math.Clamp(view.Index, 0, ushort.MaxValue),
                StepCount = (ushort)Math.Clamp(view.Count, 0, ushort.MaxValue),
                Progress = (ushort)Math.Clamp(view.Progress, 0, ushort.MaxValue),
                RewardItemCount = (byte)items.Count,
            };
            if (step is not null)
            {
                packet.StepNumber = (ushort)step.Number;
                packet.Chapter = step.Chapter;
                packet.Objective = (byte)step.Objective;
                packet.MinimumLevel = (ushort)Math.Clamp(step.MinimumLevel, 0, ushort.MaxValue);
                packet.Target = (uint)Math.Max(0, step.Target);
                packet.TargetNumber = (ushort)step.TargetNumber;
                packet.RequiredCount = (ushort)Math.Clamp(step.GetRequiredProgress(), 0, ushort.MaxValue);
                packet.MapNumber = (ushort)step.Map;
                packet.X = step.X;
                packet.Y = step.Y;
                packet.Gate = (ushort)Math.Max((short)0, step.Gate);
                packet.Experience = (uint)Math.Max(0, step.Experience);
                packet.Money = (uint)Math.Max(0, step.Money);
                packet.Name = step.Name;
            }

            for (var i = 0; i < items.Count; i++)
            {
                var row = packet[i];
                row.Group = items[i].Group;
                row.Number = (ushort)items[i].Number;
                row.Count = items[i].Count;
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }
}
