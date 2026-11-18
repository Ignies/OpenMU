// <copyright file="ItemLookBytes.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.ItemCustomization;

using System.Buffers.Binary;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

/// <summary>
/// The stored recolour look (the site's item look JSON, its <c>item_tint_look</c>) as the 31 wire bytes of an item
/// customisation: byte for byte the body of the Hidden plugin's <c>E7 09</c> tint entry (Readme.md).
/// </summary>
public static class ItemLookBytes
{
    /// <summary>
    /// The length of the look on the wire.
    /// </summary>
    public const int Length = 31;

    private const int DefaultStrength = 85;
    private const int DefaultKeepRange = 30;
    private const byte CuratedTune = 100;

    private static readonly string[] Modes = ["none", "tone", "hue"];
    private static readonly string[] Effects = ["none", "poison", "fire", "water", "wind", "earth", "blood", "lightning"];
    private static readonly string[] TuneKeys = ["size", "amount", "glow", "speed", "reach", "surface"];

    /// <summary>
    /// Writes a stored look.
    /// </summary>
    /// <param name="json">The stored look.</param>
    /// <param name="target">The 31 bytes, left zero when there's no look.</param>
    /// <returns>True, when there's a look.</returns>
    public static bool TryWrite(string? json, Span<byte> target)
    {
        target[..Length].Clear();
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return Write(document.RootElement, target);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// Reads the 31 look bytes back into the look JSON (the reverse of <see cref="TryWrite"/>): a smith's recolour
    /// arrives as bytes (EE 93) and is stored as JSON.
    /// </summary>
    /// <param name="look">The look bytes.</param>
    /// <returns>The look JSON, or null for no look (all zero, or a mode or effect past the known ones).</returns>
    public static string? TryRead(ReadOnlySpan<byte> look)
    {
        if (look.Length < Length || look[0] >= Modes.Length || look[14] >= Effects.Length)
        {
            return null;
        }

        var mode = look[0];
        var fx = look[14];
        var hasGlow = (look[1] & 2) != 0;
        if ((mode == 0 || look[5] == 0) && !hasGlow && fx == 0)
        {
            return null;
        }

        var json = new JsonObject { ["mode"] = Modes[mode] };
        if (mode != 0)
        {
            json["color"] = Hex(look[2..]);
        }

        json["strength"] = look[5];
        json["hue"] = BinaryPrimitives.ReadUInt16BigEndian(look[6..]);
        if ((look[1] & 1) != 0)
        {
            json["keep_from"] = BinaryPrimitives.ReadUInt16BigEndian(look[8..]);
            json["keep_range"] = look[10];
        }

        if (hasGlow)
        {
            json["glow"] = Hex(look[11..]);
        }

        if (fx > 0)
        {
            json["fx"] = Effects[fx];
            var colors = new JsonArray();
            for (var i = 0; i < Math.Min((int)look[15], 3); i++)
            {
                colors.Add(Hex(look[(16 + (i * 3))..]));
            }

            json["fx_colors"] = colors;
            var tune = new JsonObject();
            for (var i = 0; i < TuneKeys.Length; i++)
            {
                tune[TuneKeys[i]] = look[25 + i];
            }

            json["fx_tune"] = tune;
        }

        return json.ToJsonString();
    }

    private static string Hex(ReadOnlySpan<byte> rgb) => $"#{rgb[0]:x2}{rgb[1]:x2}{rgb[2]:x2}";

    private static bool Write(JsonElement look, Span<byte> target)
    {
        if (look.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var color = Color(look, "color");
        var mode = Math.Max(0, Array.IndexOf(Modes, Text(look, "mode") ?? "tone"));
        if (mode == 1 && color < 0)
        {
            mode = 0;
        }

        var strength = Math.Clamp(Number(look, "strength") ?? DefaultStrength, 0, 100);
        var glow = Color(look, "glow");
        var fx = Math.Max(0, Array.IndexOf(Effects, Text(look, "fx") ?? "none"));
        if ((mode == 0 || strength == 0) && glow < 0 && fx == 0)
        {
            return false;
        }

        var keepFrom = Number(look, "keep_from");
        target[0] = (byte)mode;
        target[1] = (byte)((keepFrom is null ? 0 : 1) | (glow < 0 ? 0 : 2));
        WriteColor(target[2..], color);
        target[5] = (byte)strength;
        BinaryPrimitives.WriteUInt16BigEndian(target[6..], (ushort)((((Number(look, "hue") ?? 0) % 360) + 360) % 360));
        if (keepFrom is { } from)
        {
            BinaryPrimitives.WriteUInt16BigEndian(target[8..], (ushort)(((from % 360) + 360) % 360));
            target[10] = (byte)Math.Clamp(Number(look, "keep_range") ?? DefaultKeepRange, 5, 75);
        }

        WriteColor(target[11..], glow);
        target[14] = (byte)fx;
        if (fx > 0 && look.TryGetProperty("fx_colors", out var colors) && colors.ValueKind == JsonValueKind.Array)
        {
            var count = 0;
            foreach (var entry in colors.EnumerateArray())
            {
                if (count < 3 && ParseColor(entry.ValueKind == JsonValueKind.String ? entry.GetString() : null) is var rgb and >= 0)
                {
                    WriteColor(target[(16 + (count * 3))..], rgb);
                    count++;
                }
            }

            target[15] = (byte)count;
        }

        var hasTune = fx > 0 && look.TryGetProperty("fx_tune", out var tune) && tune.ValueKind == JsonValueKind.Object;
        for (var i = 0; i < TuneKeys.Length; i++)
        {
            target[25 + i] = hasTune ? (byte)Math.Clamp(Number(look.GetProperty("fx_tune"), TuneKeys[i]) ?? CuratedTune, 0, 250) : CuratedTune;
        }

        return true;
    }

    private static string? Text(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static int? Number(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) switch
        {
            true when value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var n) => n,
            true when value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) => n,
            _ => null,
        };

    private static int Color(JsonElement element, string name) => ParseColor(Text(element, name));

    private static int ParseColor(string? hex) =>
        hex is { Length: 7 } && hex[0] == '#' && int.TryParse(hex.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb) ? rgb : -1;

    private static void WriteColor(Span<byte> target, int rgb)
    {
        if (rgb >= 0)
        {
            target[0] = (byte)(rgb >> 16);
            target[1] = (byte)(rgb >> 8);
            target[2] = (byte)rgb;
        }
    }
}
