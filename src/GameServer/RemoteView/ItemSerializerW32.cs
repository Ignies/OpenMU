// <copyright file="ItemSerializerW32.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameServer.RemoteView;

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.ItemExpiry;
using MUnique.OpenMU.Network.PlugIns;
using MUnique.OpenMU.Persistence;
using MUnique.OpenMU.PlugIns;

using static ItemSerializerHelper;

/// <summary>
/// The item serializer of the Babylon client in fork mode: a fixed 32 byte layout (W32) with room for the item
/// groups, numbers, options and slots of later seasons. The byte layout is documented in docs/babylon.md, "W32".
/// </summary>
/// <remarks>
/// Every field which Season 6 knows keeps its Season 6 meaning, so the client reads both layouts into the same item.
/// Of the fields of later seasons, the mastery options, the mastery bonus, the sphere levels up to 10 and the grade
/// options of the fourth and fifth wings (kind block) are filled; the elements and the other kind blocks
/// stay zero until the systems which fill them exist.
/// </remarks>
[Guid("5C1B8E2A-7F4D-4A93-B6E0-3D9A2C8F1E57")]
[PlugIn]
[Display(Name = nameof(PlugInResources.ItemSerializerW32_Name), Description = nameof(PlugInResources.ItemSerializerW32_Description), ResourceType = typeof(PlugInResources))]
[MinimumClient(BabylonClient.MinimumSeason, BabylonClient.MinimumEpisode, ClientLanguage.Invariant)]
public class ItemSerializerW32 : IItemSerializer
{
    /// <summary>
    /// The length of an item.
    /// </summary>
    public const int Length = 32;

    /// <summary>
    /// The feature id (<see cref="ForkFeatures"/>) of this format.
    /// </summary>
    public const string FeatureId = "wire_w32";

    private const int GroupIndex = 0;
    private const int NumberIndex = 1;
    private const int LevelIndex = 3;
    private const int DurabilityIndex = 4;
    private const int FlagsIndex = 5;
    private const int OptionIndex = 6;
    private const int ExcellentIndex = 7;
    private const int MasteryBonusIndex = 9;
    private const int AncientIndex = 10;
    private const int HarmonyIndex = 11;
    private const int SocketBonusIndex = 12;
    private const int SlotsIndex = 13;
    private const int SlotLevelsIndex = 18;
    private const int KindBlockIndex = 22;
    private const int WingGradeEntries = 3;
    private const byte NoWingGrade = 0xFF;
    private const int ExpiryIndex = 28;

    private const byte SkillFlag = 0x01;
    private const byte LuckFlag = 0x02;
    private const byte ExcellentFlag = 0x04;
    private const byte AncientFlag = 0x08;
    private const byte HarmonyFlag = 0x10;
    private const byte GuardianFlag = 0x20;
    private const byte SocketsFlag = 0x40;
    private const byte TimedFlag = 0x80;

    private const byte NibbleMask = 0x0F;
    private const byte AncientBonusLevelMask = 0b1100;
    private const byte AncientDiscriminatorMask = 0b0011;
    private const int Season6ExcellentMask = 0x3F;
    private const int MasteryMask = 0x1FC0;
    private const int WingOptionKindShift = 4;
    private const int WingOptionKindMask = 0b11;
    private const int WingOptionMask = 0x0F;

    /// <inheritdoc/>
    public bool CarriesWideItems => true;

    /// <inheritdoc/>
    public int NeededSpace => Length;

    /// <inheritdoc/>
    public int SerializeItem(Span<byte> target, Item item)
    {
        item.ThrowNotInitializedProperty(item.Definition is null, nameof(item.Definition));
        target = target[..Length];
        target.Clear();

        target[GroupIndex] = item.Definition.Group;
        BinaryPrimitives.WriteUInt16LittleEndian(target[NumberIndex..], (ushort)item.Definition.Number);
        target[LevelIndex] = item.IsTrainablePet() ? (byte)0 : item.Level;
        target[DurabilityIndex] = item.Durability();

        var itemOption = item.ItemOptions.FirstOrDefault(o => o.ItemOption?.OptionType == ItemOptionTypes.Option && !o.ItemOption.IsFixedOption());
        var optionLevel = itemOption?.Level ?? item.GetFixedOptionBits();
        var optionKind = itemOption?.ItemOption?.Number ?? 0;
        target[OptionIndex] = (byte)((optionLevel & NibbleMask) | ((optionKind & NibbleMask) << 4));

        // Bits 0-5 are the low six bits of the Season 6 byte 3, so the wing option kind rides there as well.
        var excellent = GetExcellentByte(item) | GetFenrirByte(item) | ((optionKind & WingOptionKindMask) << WingOptionKindShift);
        BinaryPrimitives.WriteUInt16LittleEndian(target[ExcellentIndex..], (ushort)((excellent & Season6ExcellentMask) | (GetMasteryBits(item) & MasteryMask)));
        target[MasteryBonusIndex] = GetMasteryBonusByte(item);
        SetWingGrades(target[KindBlockIndex..], item);

        if (item.ItemSetGroups.FirstOrDefault(set => set.AncientSetDiscriminator != 0) is { } ancientSet)
        {
            var bonusLevel = item.ItemOptions.FirstOrDefault(o => o.ItemOption?.OptionType == ItemOptionTypes.AncientBonus)?.Level ?? 0;
            target[AncientIndex] = (byte)((ancientSet.AncientSetDiscriminator & AncientDiscriminatorMask) | ((bonusLevel << 2) & AncientBonusLevelMask));
        }

        target[HarmonyIndex] = GetHarmonyByte(item);
        target[SocketBonusIndex] = GetSocketBonusByte(item);
        SetSlots(target, item);
        BinaryPrimitives.WriteUInt32LittleEndian(target[ExpiryIndex..], item.GetExpiryUnixSeconds());
        target[FlagsIndex] = GetFlags(target, item);
        return Length;
    }

    /// <inheritdoc/>
    public Item DeserializeItem(Span<byte> source, GameConfiguration gameConfiguration, IContext persistenceContext)
    {
        var group = source[GroupIndex];
        var number = BinaryPrimitives.ReadUInt16LittleEndian(source[NumberIndex..]);
        var definition = gameConfiguration.Items.FirstOrDefault(def => def.Number == number && def.Group == group)
                         ?? throw new ArgumentException($"Couldn't find the item definition for the given byte array. Extracted item number and group: {number}, {group}");

        var item = persistenceContext.CreateNew<Item>();
        item.Definition = definition;
        item.Level = source[LevelIndex];
        item.Durability = source[DurabilityIndex];

        var flags = source[FlagsIndex];
        if ((flags & SkillFlag) != 0)
        {
            if (definition.Skill is null)
            {
                throw new ArgumentException($"The skill flag was set, but a skill is not defined for the specified item ({number}, {group})");
            }

            item.HasSkill = true;
        }

        if ((flags & LuckFlag) != 0)
        {
            AddLuckOption(persistenceContext, item);
        }

        var excellent = BinaryPrimitives.ReadUInt16LittleEndian(source[ExcellentIndex..]) & Season6ExcellentMask;
        var isWing = HasPossibleOption(definition, ItemOptionTypes.Wing);
        if (HasPossibleOption(definition, ItemOptionTypes.Excellent))
        {
            ReadExcellentOptionBits(excellent, persistenceContext, item);
        }
        else if (isWing)
        {
            ReadWingOptionBits(excellent & WingOptionMask, persistenceContext, item);
        }
        else
        {
            // no excellent options
        }

        var optionLevel = source[OptionIndex] & NibbleMask;
        AddNormalOption(isWing ? source[OptionIndex] >> 4 : 0, optionLevel, persistenceContext, item);

        var ancient = source[AncientIndex];
        if ((ancient & (AncientBonusLevelMask | AncientDiscriminatorMask)) != 0)
        {
            AddAncientOption(ancient & AncientDiscriminatorMask, (ancient & AncientBonusLevelMask) >> 2, persistenceContext, item);
        }

        if ((flags & GuardianFlag) != 0)
        {
            AddLevel380Option(persistenceContext, item);
        }

        if (HasPossibleOption(definition, ItemOptionTypes.BlackFenrir))
        {
            ReadFenrirOptions((byte)excellent, persistenceContext, item);
        }

        ReadMasteryOptions(BinaryPrimitives.ReadUInt16LittleEndian(source[ExcellentIndex..]), source[MasteryBonusIndex], persistenceContext, item);
        ReadWingGrades(source[KindBlockIndex..], persistenceContext, item);

        if (definition.MaximumSockets == 0)
        {
            AddHarmonyOption(source[HarmonyIndex], persistenceContext, item);
        }
        else
        {
            ReadSocketBonus(source[SocketBonusIndex], persistenceContext, item);
            ReadSlots(source, persistenceContext, item);
        }

        return item;
    }

    private static int GetMasteryBits(Item item)
    {
        var bits = 0;
        foreach (var link in item.ItemOptions)
        {
            if (link.ItemOption is { } option && option.OptionType == Season21ItemOptionTypes.Mastery)
            {
                bits |= 1 << option.Number;
            }
        }

        return bits;
    }

    private static byte GetMasteryBonusByte(Item item)
        => item.ItemOptions.FirstOrDefault(o => o.ItemOption?.OptionType == Season21ItemOptionTypes.MasteryBonus) is { ItemOption: { } bonus } link
            ? (byte)((link.Level & NibbleMask) | ((bonus.Number & NibbleMask) << 4))
            : (byte)0;

    private static void SetWingGrades(Span<byte> block, Item item)
    {
        var grades = item.ItemOptions
            .Where(o => o.ItemOption?.OptionType == Season21ItemOptionTypes.WingGrade)
            .OrderBy(o => o.Index)
            .ThenBy(o => o.ItemOption!.Number)
            .Take(WingGradeEntries)
            .ToList();
        if (grades.Count == 0)
        {
            return;
        }

        for (var entry = 0; entry < WingGradeEntries; entry++)
        {
            block[entry * 2] = entry < grades.Count ? (byte)grades[entry].ItemOption!.Number : NoWingGrade;
            block[(entry * 2) + 1] = entry < grades.Count ? (byte)Math.Max(grades[entry].Level - 1, 0) : (byte)0;
        }
    }

    private static void ReadMasteryOptions(ushort excellentWord, byte bonusByte, IContext persistenceContext, Item item)
    {
        var options = item.Definition!.PossibleItemOptions.SelectMany(o => o.PossibleOptions).ToList();
        foreach (var option in options.Where(o => o.OptionType == Season21ItemOptionTypes.Mastery && o.Number < 16 && (excellentWord & MasteryMask & (1 << o.Number)) != 0))
        {
            AddLink(option, 0, 0, persistenceContext, item);
        }

        if (bonusByte != 0 && options.FirstOrDefault(o => o.OptionType == Season21ItemOptionTypes.MasteryBonus && o.Number == bonusByte >> 4) is { } bonus)
        {
            AddLink(bonus, bonusByte & NibbleMask, 0, persistenceContext, item);
        }
    }

    private static void ReadWingGrades(ReadOnlySpan<byte> block, IContext persistenceContext, Item item)
    {
        var grades = item.Definition!.PossibleItemOptions.SelectMany(o => o.PossibleOptions).Where(o => o.OptionType == Season21ItemOptionTypes.WingGrade).ToList();
        if (grades.Count == 0)
        {
            return;
        }

        for (var entry = 0; entry < WingGradeEntries; entry++)
        {
            var number = block[entry * 2];
            if (number != NoWingGrade && grades.FirstOrDefault(o => o.Number == number) is { } option)
            {
                AddLink(option, block[(entry * 2) + 1] + 1, entry, persistenceContext, item);
            }
        }
    }

    private static void AddLink(IncreasableItemOption option, int level, int index, IContext persistenceContext, Item item)
    {
        var link = persistenceContext.CreateNew<ItemOptionLink>();
        link.ItemOption = option;
        link.Level = level;
        link.Index = index;
        item.ItemOptions.Add(link);
    }

    private static bool HasPossibleOption(ItemDefinition definition, ItemOptionType optionType)
        => definition.PossibleItemOptions.Any(o => o.PossibleOptions.Any(p => p.OptionType == optionType));

    private static byte GetFlags(ReadOnlySpan<byte> target, Item item)
    {
        byte flags = 0;
        flags |= item.HasSkill ? SkillFlag : (byte)0;
        flags |= item.ItemOptions.Any(o => o.ItemOption?.OptionType == ItemOptionTypes.Luck) ? LuckFlag : (byte)0;
        flags |= target[ExcellentIndex] != 0 || target[ExcellentIndex + 1] != 0 ? ExcellentFlag : (byte)0;
        flags |= target[AncientIndex] != 0 ? AncientFlag : (byte)0;
        flags |= target[HarmonyIndex] != 0 ? HarmonyFlag : (byte)0;
        flags |= item.ItemOptions.Any(o => o.ItemOption?.OptionType == ItemOptionTypes.GuardianOption) ? GuardianFlag : (byte)0;
        flags |= item.SocketCount > 0 ? SocketsFlag : (byte)0;
        flags |= item.ExpiresAt is not null ? TimedFlag : (byte)0;
        return flags;
    }

    private static void ReadSlots(ReadOnlySpan<byte> source, IContext persistenceContext, Item item)
    {
        var socketCount = 0;
        for (var slot = 0; slot < MaximumSockets; slot++)
        {
            var option = source[SlotsIndex + slot];
            if (option == NoSocket)
            {
                continue;
            }

            socketCount++;
            if (option != EmptySocket)
            {
                AddSocketOption(option, GetSlotLevel(source, slot), slot, persistenceContext, item);
            }
        }

        item.SocketCount = socketCount;
    }

    private static int GetSlotLevel(ReadOnlySpan<byte> source, int slot)
    {
        var levels = source[SlotLevelsIndex + (slot / 2)];
        return slot % 2 == 0 ? levels & NibbleMask : levels >> 4;
    }

    private static void SetSlots(Span<byte> target, Item item)
    {
        for (var slot = 0; slot < MaximumSockets; slot++)
        {
            if (slot >= item.SocketCount)
            {
                target[SlotsIndex + slot] = NoSocket;
                continue;
            }

            if (GetSocketOption(item, slot) is not (var optionIndex, var level))
            {
                target[SlotsIndex + slot] = EmptySocket;
                continue;
            }

            target[SlotsIndex + slot] = optionIndex;
            var levelNibble = (byte)Math.Clamp(level, 0, NibbleMask);
            target[SlotLevelsIndex + (slot / 2)] |= slot % 2 == 0 ? levelNibble : (byte)(levelNibble << 4);
        }
    }
}
