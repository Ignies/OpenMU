# Text moderation

Player text on items (Star Smithing, Hidden-Babylon `docs/star_smithing.md` "Player text"): names and lore show at once,
filtered here, and are moderated after the fact (reports, an admin's reset with a reason).

- `TextModeration.Check(kind, text, configuration)`: the text as stored (NFC, trimmed, lines joined with a line feed)
  or why it is refused. A name is 3 to 24 text elements of letters (any script), digits, spaces, `'` and `-`, without
  two spaces in a row. A lore is at most 140 characters on at most 3 lines, without control or invisible characters,
  links (a scheme, `www.`, a name with a common top-level domain) or colour codes (`#rrggbb`, `^1`, `&a`, `{color`).
- The word list: an entry is a whole word, or `Substring` anywhere. Text and entries are folded first
  (`TextModeration.Fold`): lower case, accents dropped, Cyrillic and Greek look-alike letters and the digits and
  symbols written for letters (`0`, `1`, `3`, `4`, `5`, `7`, `8`, `@`, `$`) read as latin letters. `Language` only
  files an entry for the editor; every entry applies to every text.
- `TextModerationPlugIn` keeps the list (its custom configuration, empty by default) and is active by default. The
  website reads and replaces it through the site bridge (`items.words`, `items.setWords`, previewed and audited) and
  checks its own texts with `items.checkText` (`Web/AdminPanel.Operations/ItemModerationOperations.cs`).
- A reset (`items.resetText`, Operator): the name or the lore goes, the item keeps everything else, its record's
  `Version` goes up (clients fetch it again) and `ItemCustomizationCache.Forget` drops it at once. The audit row keeps
  the old text and the reason.

Tests: `tests/MUnique.OpenMU.Tests/Babylon/TextModerationTest.cs`,
`tests/MUnique.OpenMU.Web.Tests/Site/ItemModerationOperationsTests.cs`.
