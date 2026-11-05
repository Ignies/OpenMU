// <copyright file="ConfigurationApplies.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Web.AdminPanel.Operations.Site;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;

/// <summary>
/// When a saved value reaches the running game (Readme.md, "Applies"): <see cref="Live"/> at once (the game reads it
/// when it is used, or re-reads it on the change notification), <see cref="Next"/> for what is made or loaded after
/// the save (a character's next login, the next event, items made from then on), <see cref="Restart"/> only when the
/// server starts again. Read from the game's own change handlers; a type not named here is <see cref="Next"/>.
/// </summary>
internal static class ConfigurationApplies
{
    /// <summary>Applies at once.</summary>
    public const string Live = "live";

    /// <summary>Applies to what is loaded or made after the save.</summary>
    public const string Next = "next";

    /// <summary>Applies after a restart of the server.</summary>
    public const string Restart = "restart";

    private static readonly Dictionary<Type, string> ByType = new()
    {
        // GameContext re-reads the rates and rebuilds the experience tables on its change notification
        [typeof(GameConfiguration)] = Live,

        // spawned monsters reload their attributes (MapInitializer), spawn areas re-spawn, stores reload
        [typeof(MonsterDefinition)] = Live,
        [typeof(MonsterAttribute)] = Live,
        [typeof(MonsterSpawnArea)] = Live,
        [typeof(ItemStorage)] = Live,

        // read when they are used: a drop, a warp, a gate, a mix
        [typeof(DropItemGroup)] = Live,
        [typeof(ItemDropItemGroup)] = Live,
        [typeof(WarpInfo)] = Live,
        [typeof(ExitGate)] = Live,
        [typeof(EnterGate)] = Live,
        [typeof(GameMapDefinition)] = Live,
        [typeof(JewelMix)] = Live,
        [typeof(ItemCrafting)] = Live,

        // the connect servers restart themselves on a change (ConfigurationChangeHandler); the IP resolver is set again
        [typeof(ConnectServerDefinition)] = Live,
        [typeof(SystemConfiguration)] = Live,

        // read once when the server starts
        [typeof(GameServerDefinition)] = Restart,
        [typeof(GameServerEndpoint)] = Restart,
        [typeof(GameServerConfiguration)] = Restart,
        [typeof(ChatServerDefinition)] = Restart,
        [typeof(ChatServerEndpoint)] = Restart,
    };

    private static readonly Dictionary<(Type Type, string Field), string> ByField = new()
    {
        // the terrain is built with the map instance; a new instance (another server start) reads it
        [(typeof(GameMapDefinition), nameof(GameMapDefinition.TerrainData))] = Restart,

        // item definitions have no change handler: what is read on use applies at once, the drop generator keeps its
        // lists from the start (the game bridge's item probe, openmu/readme.md "Item definitions")
        [(typeof(ItemDefinition), nameof(ItemDefinition.Value))] = Live,
        [(typeof(ItemDefinition), nameof(ItemDefinition.QualifiedCharacters))] = Live,
        [(typeof(ItemDefinition), nameof(ItemDefinition.DropsFromMonsters))] = Restart,
        [(typeof(ItemDefinition), nameof(ItemDefinition.DropLevel))] = Restart,
    };

    /// <summary>When a type's (or one of its fields') saved values apply.</summary>
    /// <param name="type">The data model type.</param>
    /// <param name="field">The field, or null for the type.</param>
    /// <returns><see cref="Live"/>, <see cref="Next"/> or <see cref="Restart"/>.</returns>
    public static string Of(Type type, string? field = null)
        => field is not null && ByField.TryGetValue((type, field), out var byField) ? byField : ByType.GetValueOrDefault(type, Next);

    /// <summary>The latest of two (restart after next after live).</summary>
    /// <param name="a">One.</param>
    /// <param name="b">The other.</param>
    /// <returns>The later one.</returns>
    public static string Latest(string a, string b) => Rank(a) >= Rank(b) ? a : b;

    private static int Rank(string applies) => applies switch { Live => 0, Next => 1, _ => 2 };
}
