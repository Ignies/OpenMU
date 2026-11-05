// <copyright file="Season21ItemBans.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;

/// <summary>
/// What an item of Season 21 may not do.
/// </summary>
[Flags]
internal enum Season21ItemBans
{
    /// <summary>
    /// No restriction.
    /// </summary>
    None = 0,

    /// <summary>
    /// It can't be traded.
    /// </summary>
    Trade = 1,

    /// <summary>
    /// It can't be dropped.
    /// </summary>
    Drop = 2,

    /// <summary>
    /// It can't be stored in the vault.
    /// </summary>
    Store = 4,

    /// <summary>
    /// It can't be sold in a personal store.
    /// </summary>
    Shop = 8,

    /// <summary>
    /// It can't be sold to a merchant.
    /// </summary>
    Sell = 16,

    /// <summary>
    /// It can't be repaired.
    /// </summary>
    Repair = 32,
}
