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
    /// <summary>The most guidance recipes one EE 9E holds.</summary>
    private const int MaximumGuidance = 60;

    /// <summary>A dominant element or fate that is not there.</summary>
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
    public async ValueTask SendForgeSkyAsync(StarSmithForgeSkyMessage sky)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var count = Math.Min(sky.Entries.Count, StarSmithForgeSkyMessage.MaximumEntries);
        int Write()
        {
            var size = StarSmithForgeSkyRef.GetRequiredSize(count);
            var packet = new StarSmithForgeSkyRef(connection.Output.GetSpan(size)[..size])
            {
                Result = (byte)sky.Result,
                TargetSlot = sky.TargetSlot,
                Strikes = (byte)Math.Clamp(sky.Strikes, 0, byte.MaxValue),
                Risk = (byte)sky.Risk,
                StarCount = (byte)Math.Clamp(sky.StarCount, 0, byte.MaxValue),
                BeatIntervalMs = (ushort)Math.Clamp(sky.BeatIntervalMs, 0, ushort.MaxValue),
                FirstBeatMs = (ushort)Math.Clamp(sky.FirstBeatMs, 0, ushort.MaxValue),
                EntryCount = (byte)count,
            };
            for (var i = 0; i < count; i++)
            {
                var source = sky.Entries[i];
                var entry = packet[i];
                entry.Kind = source.Kind;
                entry.A = source.A;
                entry.B = source.B;
                entry.C = source.C;
                entry.D = source.D;
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask SendForgeStrikeAsync(int star, StarSmithStrike strike, int strikesLeft, IReadOnlyList<short> guidance)
    {
        if (this._player.Connection is not { } connection)
        {
            return;
        }

        var struck = strike.Result is StarSmithStrikeResult.Linked or StarSmithStrikeResult.Cracked;
        var count = struck ? Math.Min(guidance.Count, MaximumGuidance) : 0;
        int Write()
        {
            var size = StarSmithForgeStrikeResultRef.GetRequiredSize(count);
            var packet = new StarSmithForgeStrikeResultRef(connection.Output.GetSpan(size)[..size])
            {
                Answer = (byte)strike.Result,
                Star = (byte)Math.Clamp(star, 0, byte.MaxValue),
                Element = struck ? (byte)strike.Star.Element : (byte)0,
                Value = struck ? (byte)Math.Clamp(strike.Star.Value, 0, byte.MaxValue) : (byte)0,
                Flags = struck && strike.Star.IsVoid ? (byte)1 : (byte)0,
                Timing = (byte)strike.Timing,
                StrikesLeft = (byte)Math.Clamp(strikesLeft, 0, byte.MaxValue),
                GuidanceCount = (byte)count,
            };
            for (var i = 0; i < count; i++)
            {
                var entry = packet[i];
                entry.Recipe = (ushort)guidance[i];
            }

            return size;
        }

        await connection.SendAsync(Write).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask SendForgeResultAsync(StarSmithForgeOutcome outcome, ForgeResult? result)
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
                Dominant = result?.Dominant is { } dominant ? (byte)dominant : NoValue,
                Fate = result?.Fail is { } fail ? (byte)fail : NoValue,
            };
            if (result is not null)
            {
                packet.Flags = (byte)((result.Recipe is null && result.State == StarSmithForgeState.Sealed ? 1 : 0) | (result.FirstCompletion ? 2 : 0));
                packet.Recipe = (ushort)(result.Recipe?.Number ?? 0);
                packet.Rarity = (byte)result.Rarity;
                packet.Extension = (byte)Math.Clamp(result.Extension, 0, byte.MaxValue);
                foreach (var element in Enum.GetValues<StarSmithElement>())
                {
                    packet.ElementMix[(int)element] = (byte)Math.Clamp(result.ElementMix.GetValueOrDefault(element), 0, byte.MaxValue);
                }

                packet.CleanStrikes = (byte)Math.Clamp(result.CleanStrikes, 0, byte.MaxValue);
                packet.StrikeCount = (byte)Math.Clamp(result.Path.Count, 0, byte.MaxValue);
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
            ? ((row.Ingredients.Count + IngredientsPerLine - 1) / IngredientsPerLine) + ((row.BoostsSeen.Count + BoostsPerLine - 1) / BoostsPerLine)
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
}
