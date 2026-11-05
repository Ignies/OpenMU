# Site bridge

The website's way into the admin operations, for a website that runs its own admin pages over the game: it signs its
admins in, checks their flags, and calls `SiteBridge.InvokeAsync(operation, argumentsJson, actor, role)` through its
game bridge plugin. The signature uses framework types only, so the plugin binds it by reflection and still builds
against an OpenMU without this folder (there the call answers "not here" and the site keeps its own pages).
Startup attaches it to the host when the admin panel runs (`SiteBridge.Attach`).

## Operations
`ops` lists them: every `[AdminOperation]` method of the configuration, plugin, server, account, guild, mini game and
audit operations, named `area.method` (`config.search`, `plugins.list`, ...), with the role each needs. The site names
the role of its admin; a call below the operation's role is refused. The single-object writes (`config_update`, ...)
stay out: every site write is a changeset.

`SiteConfigOperations`:
- `config.schema`: every type and field: kind (`scalar`, `enum`, `reference`, `references`, `children`, `child`),
  writable, nullable, bounds (declared, of the number type, or the model's own checks), enum values, captions, and
  when it applies.
- `config.list`: a page of a type, with a text search, conditions `[{field, op, value}]` (eq, ne, gt, ge, lt, le,
  contains, in; a reference by its identifier) and the nearest owner `{type, ids, collection}`.
- `config.read`: one object, its version, and the owners above it.
- `config.change`: a changeset, rows `{op: update|create|delete, type, id, version, expect, fields, parentType,
  parentId, collection}`; `config.bulk`: set, multiply or add to a number field of what a filter takes (150 at a time);
  `config.export` / `config.import`: an object and everything it owns; `config.history` / `config.revert`.

## Changesets
Every write is planned against the current state first: a row with a `version` (or `expect`ed values) that no longer
matches is refused as stale with what is there now. The preview returns the change (each row before and after) and a
token bound to it; applying plans it again, so anything that moved in between refuses the token. One audit row keeps
the whole change (at most 15000 characters; the site splits larger edits), and its revert is the inverse changeset,
refused when anything it changed was changed again.

After the save the game is told about each object the save did not announce (the in-memory demo announces nothing,
see `SiteBridgeOptions`) and about every object that owns a touched one: the game registers monsters, not their
attributes, so a monster's HP reaches the spawned monsters through its owner.

## Applies
`ConfigurationApplies` says when a saved value reaches the game, from the game's own change handlers: `live` (read
when used, or re-read on the notification: rates, monsters, spawns, drops, gates, warps, mixes), `next` (what is
loaded or made after the save: classes at the next login, skills, items, events), `restart` (server definitions,
endpoints, the terrain).

## Demo
`-demo` has no database: `TransientAdminAuditRepository` keeps the audit for the process, so history and revert work.

## Tests
`tests/MUnique.OpenMU.Web.Tests/Site/SiteBridgeTests.cs`.
