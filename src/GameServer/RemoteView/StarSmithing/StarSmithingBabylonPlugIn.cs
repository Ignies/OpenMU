// <copyright file="StarSmithingBabylonPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView.StarSmithing;

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.StarSmithing;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.StarSmithing;
using MUnique.OpenMU.Network;
using MUnique.OpenMU.Network.Packets.Babylon;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.PlugIns;
using ForgeResult = MUnique.OpenMU.GameLogic.StarSmithing.StarSmithForgeResult;
using GuidebookEntry = MUnique.OpenMU.GameLogic.StarSmithing.StarSmithGuidebookEntry;
using SmithState = MUnique.OpenMU.GameLogic.StarSmithing.StarSmithState;

/// <summary>
/// Sends Star Smithing to the Babylon client (EE 91, EE 92, EE 97, EE 98, EE 9D-9F).
/// </summary>
[PlugIn]
[Display(Name = nameof(PlugInResources.StarSmithingBabylonPlugIn_Name), Description = nameof(PlugInResources.StarSmithingBabylonPlugIn_Description), ResourceType = typeof(PlugInResources))]
[Guid("782B871C-4E9B-4BAF-AC71-6413DBDA80AE")]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class StarSmithingBabylonPlugIn : IStarSmithingViewPlugIn
{
    /// <summary>The placements one EE 9E holds.</summary>
    private const int MaximumPlacements = 12;

    /// <summary>The kind of an EE 90 shape line (PR 7a).</summary>
    private const byte ShapeLine = 4;

    /// <summary>A timing, tier or fate that is not there.</summary>
    private const byte NoValue = 0xFF;

    /// <summary>Where the entries of EE 90 start.</summary>
    private const int GuidebookEntriesAt = 8;

    /// <summary>The kind of an EE 90 ingredient line.</summary>
    private const byte IngredientLine = 2;

    /// <summary>The kind of an EE 90 boost line.</summary>
    private const byte BoostLine = 3;

    /// <summary>The ingredients an EE 90 line holds.</summary>
    private const int IngredientsPerLine = 12;

    /// <summary>The boosts an EE 90 line holds.</summary>
    private const int BoostsPerLine = 8;

    private readonly RemotePlayer _player;

    /// <summary>
    /// Initializes a new instance of the <see cref="StarSmithingBabylonPlugIn"/> class.
    /// </summary>
    /// <param name="player">The player.</param>
    public StarSmithingBabylonPlugIn(RemotePlayer player) => this._player = player;

    /// <inheritdoc />
    public async ValueTask SendStateAsync(SmithState state)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        int Write()
        {
            var size = StarSmithStateRef.Length;
            _ = new StarSmithStateRef(connection.Output.GetSpan(size)[..size])
            {
                Flags = (byte)((state.HammerHeld ? 1 : 0) | (state.HammerBroken ? 2 : 0)),
                Level = (ushort)Math.Clamp(state.Level, 0, ushort.MaxValue),
                Experience = (uint)Math.Clamp(state.Experience, 0, uint.MaxValue),
                NextLevelExperience = (uint)Math.Clamp(state.NextLevelExperience, 0, uint.MaxValue),
                HammerDurability = (byte)Math.Clamp(state.HammerDurability, 0, byte.MaxValue),
            };
            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask SendAnimationAsync(Player smith, StarSmithAction action, byte strikes)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var id = smith.GetId(this._player);
        int Write()
        {
            var size = StarSmithAnimationRef.Length;
            _ = new StarSmithAnimationRef(connection.Output.GetSpan(size)[..size]) { ObjectId = id, Action = (byte)action, Strikes = strikes };
            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask SendResultAsync(StarSmithAction action, StarSmithResultCode result, byte refusal, uint zenCost)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        int Write()
        {
            var size = StarSmithResultRef.Length;
            var span = connection.Output.GetSpan(size)[..size];
            span.Clear();
            _ = new StarSmithResultRef(span) { Action = (byte)action, Result = (byte)result, Refusal = refusal, ZenCost = zenCost };
            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask SendDyesAsync(IReadOnlyList<StarSmithDye> dyes)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var count = Math.Min(dyes.Count, byte.MaxValue);
        int Write()
        {
            var size = StarSmithDyesRef.GetRequiredSize(count);
            var packet = new StarSmithDyesRef(connection.Output.GetSpan(size)[..size]) { Count = (byte)count };
            for (var i = 0; i < count; i++)
            {
                var dye = dyes[i];
                var entry = packet[i];
                entry.Group = dye.Group;
                entry.Number = (ushort)dye.Number;
                entry.Flags = (byte)((dye.Hue ? 1 : 0) | (dye.Keep ? 2 : 0) | (dye.Radiant ? 4 : 0));
                entry.MaximumStrength = dye.MaximumStrength;
                var swatches = Math.Min(dye.Swatches.Count, StarSmithDye.MaximumSwatches);
                entry.SwatchCount = (byte)swatches;
                for (var s = 0; s < swatches; s++)
                {
                    entry.Swatches[s * 3] = (byte)(dye.Swatches[s] >> 16);
                    entry.Swatches[(s * 3) + 1] = (byte)(dye.Swatches[s] >> 8);
                    entry.Swatches[(s * 3) + 2] = (byte)dye.Swatches[s];
                }
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask SendForgeAttemptAsync(StarSmithForgeAttemptMessage message)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        int Write()
        {
            var size = StarSmithForgeAttemptRef.Length;
            var span = connection.Output.GetSpan(size)[..size];
            span.Clear();
            var packet = new StarSmithForgeAttemptRef(span)
            {
                Result = (byte)message.Result,
                TargetSlot = message.TargetSlot,
            };
            if (message.Result != StarSmithResultCode.Done)
            {
                return size;
            }

            packet.Strikes = (byte)Math.Clamp(message.Strikes, 0, byte.MaxValue);
            packet.Risk = (byte)message.Risk;
            packet.FreeSlots = (byte)Math.Clamp(message.FreeSlots, 0, byte.MaxValue);
            packet.Slots = (byte)Math.Clamp(message.Slots, 0, byte.MaxValue);
            packet.BeatIntervalMs = (ushort)Math.Clamp(message.BeatIntervalMs, 0, ushort.MaxValue);
            packet.FirstBeatMs = (ushort)Math.Clamp(message.FirstBeatMs, 0, ushort.MaxValue);
            if (message.RevealedFault is { } fault)
            {
                packet.Flags = 1;
                StarSmithWire.WritePoint(packet.RevealedFault, fault.A);
                StarSmithWire.WritePoint(packet.RevealedFault[4..], fault.B);
            }

            packet.FaultRadius = message.FaultRadius;
            packet.CloseRadius = message.CloseRadius;
            packet.NodeGap = message.NodeGap;
            packet.FaultCount = (byte)Math.Clamp(message.FaultCount, 0, byte.MaxValue);
            packet.LayerCount = (byte)StarSmithWire.WriteLayers(packet.Layers, message.Layers);
            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask SendForgeNodeAsync(StarSmithFigureAnswer answer)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        int Write()
        {
            var size = StarSmithForgeNodeResultRef.Length;
            var span = connection.Output.GetSpan(size)[..size];
            span.Clear();
            var packet = new StarSmithForgeNodeResultRef(span)
            {
                Kind = (byte)answer.Kind,
                Answer = (byte)answer.Result,
                Timing = answer.Timing is { } timing ? (byte)timing : NoValue,
                StrikesLeft = (byte)Math.Clamp(answer.StrikesLeft, 0, byte.MaxValue),
                Flags = (byte)answer.Flags,
                FigurePlacements = (byte)Math.Clamp(answer.FigurePlacements, 0, byte.MaxValue),
                Banked = (byte)Math.Clamp(answer.Banked, 0, byte.MaxValue),
                Tier = answer.Measure is { } measure && answer.Match is not null ? (byte)measure.Tier : NoValue,
            };
            if (answer.Result is not (StarSmithFigureResult.Done or StarSmithFigureResult.Cracked))
            {
                return size;
            }

            if (answer.Match is { } match)
            {
                packet.Match = (ushort)match.Number;
                packet.Look = (byte)match.Look;
                packet.Score = (ushort)Math.Clamp(answer.Measure?.Score ?? 0, 0, 1000);
            }

            packet.Turn = (ushort)(short)Math.Clamp(Math.Round(answer.Turn * 10, MidpointRounding.AwayFromZero), short.MinValue, short.MaxValue);
            packet.Ratio = (ushort)Math.Clamp(Math.Round(answer.Ratio * 1000, MidpointRounding.AwayFromZero), 0, ushort.MaxValue);
            var guide = answer.Guide.Take(StarSmithForge.MaximumGuide).ToList();
            packet.GuideCount = (byte)guide.Count;
            for (var i = 0; i < guide.Count; i++)
            {
                BinaryPrimitives.WriteUInt16LittleEndian(packet.Guide[(i * 2)..], (ushort)guide[i]);
            }

            var placements = answer.Placements.Take(MaximumPlacements).ToList();
            for (var i = 0; i < placements.Count; i++)
            {
                var at = packet.Placements[(i * 3)..];
                at[0] = (byte)placements[i].Timing;
                BinaryPrimitives.WriteUInt16LittleEndian(at[1..], placements[i].Accuracy is { } accuracy ? (ushort)Math.Clamp(accuracy, 0, 1000) : ushort.MaxValue);
            }

            packet.HullCount = (byte)StarSmithWire.WritePoints(packet.Hull, answer.Hull);
            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask SendForgeResultAsync(StarSmithForgeOutcome outcome, ForgeResult? result, int experience, bool firstSeal)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        int Write()
        {
            var size = StarSmithForgeResultRef.Length;
            var span = connection.Output.GetSpan(size)[..size];
            span.Clear();
            var packet = new StarSmithForgeResultRef(span)
            {
                Outcome = (byte)outcome,
                Fate = result?.Fail is { } fail && result.State != StarSmithForgeState.Sealed ? (byte)fail : NoValue,
            };
            if (result is null)
            {
                return size;
            }

            packet.Flags = (byte)((result.Attunement ? 1 : 0) | (firstSeal ? 2 : 0));
            packet.Rarity = (byte)result.Rarity;
            packet.CleanPlacements = (byte)Math.Clamp(result.CleanPlacements, 0, byte.MaxValue);
            packet.Placements = (byte)Math.Clamp(result.Placements, 0, byte.MaxValue);
            packet.Experience = (uint)Math.Max(0, experience);
            packet.FateConstellation = (ushort)(result.FateRecipe?.Number ?? 0);
            if (result.State == StarSmithForgeState.Sealed)
            {
                var figures = result.Figures.Take(StarSmithSlots.Maximum).ToList();
                packet.FigureCount = (byte)figures.Count;
                for (var i = 0; i < figures.Count; i++)
                {
                    var at = packet.Figures[(i * 6)..];
                    BinaryPrimitives.WriteUInt16LittleEndian(at, (ushort)figures[i].Recipe.Number);
                    at[2] = (byte)figures[i].Tier;
                    at[3] = (byte)figures[i].Recipe.Look;
                    at[4] = (byte)figures[i].Change;
                    at[5] = (byte)Math.Clamp(figures[i].LayerIndex, 0, byte.MaxValue);
                }
            }
            else
            {
                packet.FigureCount = (byte)Math.Clamp(result.Lost, 0, byte.MaxValue);
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask SendPingAsync(uint token)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        int Write()
        {
            var size = ForkPingRef.Length;
            var span = connection.Output.GetSpan(size)[..size];
            span.Clear();
            _ = new ForkPingRef(span) { Token = token };
            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask SendGuidebookAsync(int total, IReadOnlyList<GuidebookEntry> found)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        // A found row is followed by its ingredient lines (12 a line) and boost lines (8 a line); the whole stays a C2.
        static int LinesOf(GuidebookEntry row) => row.Kind == StarSmithGuidebookEntryKind.Found
            ? (row.Geometry is null ? 0 : 1) + ((row.Ingredients.Count + IngredientsPerLine - 1) / IngredientsPerLine) + ((row.BoostsSeen.Count + BoostsPerLine - 1) / BoostsPerLine)
            : 0;
        var rows = new List<GuidebookEntry>();
        var count = 0;
        foreach (var row in found)
        {
            var needed = 1 + LinesOf(row);
            if (StarSmithGuidebookRef.GetRequiredSize(count + needed) > ushort.MaxValue)
            {
                break;
            }

            rows.Add(row);
            count += needed;
        }

        int Write()
        {
            var size = StarSmithGuidebookRef.GetRequiredSize(count);
            var span = connection.Output.GetSpan(size)[..size];
            span.Clear();
            var packet = new StarSmithGuidebookRef(span)
            {
                Total = (byte)Math.Clamp(total, 0, byte.MaxValue),
                EntryCount = (ushort)count,
            };
            var index = 0;
            foreach (var row in rows)
            {
                var entry = packet[index++];
                entry.Kind = (byte)row.Kind;
                entry.Text = row.Text;
                if (row.Kind != StarSmithGuidebookEntryKind.Found)
                {
                    entry.HintKind = (byte)row.HintKind;
                    entry.HintGroup = row.HintGroup;
                    entry.HintNumber = (ushort)row.HintNumber;
                    continue;
                }

                entry.Recipe = (ushort)row.Recipe;
                entry.Made = (ushort)Math.Clamp(row.Made, 0, ushort.MaxValue);
                entry.ShapeLength = (byte)Math.Clamp(row.ShapeLength, 0, byte.MaxValue);
                entry.Lines = (byte)LinesOf(row);
                if (row.Geometry is { } geometry)
                {
                    var line = span.Slice(GuidebookEntriesAt + (index++ * StarSmithGuidebookEntryRef.Length), StarSmithGuidebookEntryRef.Length);
                    line[0] = ShapeLine;
                    line[1] = (byte)Math.Clamp(geometry.Nodes, 0, byte.MaxValue);
                    line[2] = (byte)((geometry.Closed ? 1 : 0) | (geometry.Extension is not null ? 2 : 0));
                    line[3] = (byte)row.Look;
                    for (var i = 0; i < Math.Min(geometry.Turns.Count, 11); i++)
                    {
                        BinaryPrimitives.WriteInt16LittleEndian(line[(6 + (i * 2))..], Tenths(geometry.Turns[i]));
                    }

                    for (var i = 0; i < Math.Min(geometry.Lengths.Count, 12); i++)
                    {
                        BinaryPrimitives.WriteUInt16LittleEndian(line[(28 + (i * 2))..], Permille(geometry.Lengths[i]));
                    }

                    if (geometry.Extension is { } extension)
                    {
                        BinaryPrimitives.WriteInt16LittleEndian(line[52..], Tenths(extension.Turn));
                        BinaryPrimitives.WriteUInt16LittleEndian(line[54..], Permille(extension.Length));
                    }
                }

                foreach (var chunk in row.Ingredients.Chunk(IngredientsPerLine))
                {
                    var line = span.Slice(GuidebookEntriesAt + (index++ * StarSmithGuidebookEntryRef.Length), StarSmithGuidebookEntryRef.Length);
                    line[0] = IngredientLine;
                    line[1] = (byte)chunk.Length;
                    for (var i = 0; i < chunk.Length; i++)
                    {
                        var at = line[(2 + (i * 5))..];
                        at[0] = chunk[i].Group;
                        BinaryPrimitives.WriteUInt16LittleEndian(at[1..], (ushort)chunk[i].Number);
                        at[3] = chunk[i].Minimum;
                        at[4] = chunk[i].Maximum;
                    }
                }

                foreach (var chunk in row.BoostsSeen.Chunk(BoostsPerLine))
                {
                    var line = span.Slice(GuidebookEntriesAt + (index++ * StarSmithGuidebookEntryRef.Length), StarSmithGuidebookEntryRef.Length);
                    line[0] = BoostLine;
                    line[1] = (byte)chunk.Length;
                    for (var i = 0; i < chunk.Length; i++)
                    {
                        var at = line[(2 + (i * 9))..];
                        at[0] = (byte)chunk[i].Stat;
                        BinaryPrimitives.WriteInt32LittleEndian(at[1..], chunk[i].Minimum);
                        BinaryPrimitives.WriteInt32LittleEndian(at[5..], chunk[i].Maximum);
                    }
                }
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    private static short Tenths(double degrees) => (short)Math.Clamp(Math.Round(degrees * 10, MidpointRounding.AwayFromZero), short.MinValue, short.MaxValue);

    private static ushort Permille(double ratio) => (ushort)Math.Clamp(Math.Round(ratio * 1000, MidpointRounding.AwayFromZero), 0, ushort.MaxValue);
}
