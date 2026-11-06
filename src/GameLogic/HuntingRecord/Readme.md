# Hunting record and ranking

The hunting record of Season 21: per character, map and day the time spent, the monsters killed, the experience gained by
kills (normal and master) and the damage dealt to monsters, on the hunting maps of S21Emu `HuntingRecord.txt` (59 maps;
the kills of its 64 event and seasonal monsters don't count). The in-game ranking orders the characters by total level
(level + master level) and by the kills and the hunting experience of the week.

- `HuntingRecordPlugIn` (off by default, the world's switch) listens to kills, experience, hits and map changes; the time
  of a visit is added when the character leaves the map, today's row in a request includes the running visit.
- The records are runtime state, not configuration: `CharacterStateStore` keeps them in `StateFile`
  (`state/hunting_records.json`, written at most every `SaveInterval`), so they survive a restart; losing the file only
  resets the records.
- Design (no source): the records kept (`DaysKept`, 7: the window shows a week), the start of the week (`WeekStartsOn`,
  Tuesday), the page size (10), the ranks listed (100) and how long a computed ranking is shown (`RankingRefreshInterval`,
  1 minute). A ranking only lists characters which hunted since the feature is on.
- The Babylon client asks with `C1 EE D3` (ranking page) and `C1 EE D4` (hunting record of a map) and gets `C2 EE D0` and
  `C2 EE D1` (`docs/babylon.md`). A Season 6 client has no such windows.
- The data comes with the update "Add the hunting record of Season 21" (`Persistence/Initialization/VersionSeasonSix/Season21`).
