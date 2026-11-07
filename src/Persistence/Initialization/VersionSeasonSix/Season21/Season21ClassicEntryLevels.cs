// <copyright file="Season21ClassicEntryLevels.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// The Season 21 entry levels of the warp list entries and gates of the classic (Season 6) maps, from the server data of
/// record (S21Emu <c>Move/Move.txt</c> MinLevel and <c>Move/Gate.xml</c> MinLevel of the enter gates).
/// </summary>
/// <remarks>
/// Only the warps and gates which the Season 6 data has; warp 48 is La Cleon here but the Event Square in the data, so it
/// is left out. The progression update writes them into <c>ProgressionConfiguration.Season21ClassicWarpLevels</c> and
/// <c>Season21ClassicGateLevels</c>.
/// </remarks>
internal static class Season21ClassicEntryLevels
{
    /// <summary>
    /// Gets the levels of the warp list entries by warp index.
    /// </summary>
    public static IReadOnlyList<(int Index, int Level)> Warps { get; } =
    [
        (1, 50), (2, 1), (3, 1), (4, 10), (5, 10), (6, 10), (7, 10), (8, 20),
        (9, 25), (10, 30), (11, 50), (12, 80), (13, 90), (14, 40), (15, 40), (16, 40),
        (17, 50), (18, 50), (19, 70), (20, 70), (21, 130), (22, 160), (23, 160), (25, 150),
        (26, 170), (27, 380), (28, 160), (29, 160), (30, 300), (31, 1), (32, 10), (33, 380),
        (34, 380), (42, 300), (43, 10), (44, 1), (45, 380), (46, 280), (47, 380),
    ];

    /// <summary>
    /// Gets the levels of the enter gates by gate number.
    /// </summary>
    public static IReadOnlyList<(int Number, int Level)> Gates { get; } =
    [
        (1, 20), (3, 0), (5, 25), (7, 20), (9, 30), (11, 25), (13, 30), (15, 25),
        (18, 10), (20, 0), (23, 10), (25, 10), (28, 40), (30, 40), (32, 40), (34, 50),
        (36, 50), (38, 70), (40, 70), (43, 10), (45, 50), (47, 50), (53, 130), (55, 130),
        (62, 160), (64, 70), (96, 0), (98, 0), (102, 10), (107, 0), (109, 0), (117, 10),
        (120, 150), (121, 10), (123, 10), (125, 160), (127, 130), (129, 300), (131, 160), (259, 10),
        (261, 10), (263, 10), (265, 50), (274, 380), (276, 380), (277, 380), (279, 380), (280, 380),
        (282, 380), (283, 380), (285, 380), (286, 380), (288, 380), (290, 380), (292, 380), (308, 0),
        (310, 0), (313, 0), (315, 0), (318, 0), (320, 0), (323, 0), (325, 0), (327, 0),
        (336, 280), (338, 150), (340, 380), (342, 280),
    ];
}
