# Territory Idle – QoL mod

An unofficial mod for **Territory Idle** (game version 167) that makes the game more *incremental* than idle and adds a lot of content: buildings, heroes, gods, relics, empires and a perk tree. Everything can be switched on or off in *Options (gear icon) → QoL Features...*, and your settings are remembered.

## Quick summary

### Pace and quality of life
- **Instant ritual finish:** click the Ritual button while a ritual is running to finish it right away and get everything it would have produced.
- **Game speed button:** cycles the speed 1x → 2x → 5x → 10x → 20x (right click goes back).
- **Fast fights:** when the hero's hit kills the next monster, the fight continues at once, up to 100 kills at a time.
- **5-tile Empire points** (OFF by default): Empire points for every 5 tiles instead of 15, recounted at once. Switch it OFF before you uninstall the mod.
- **Amber upgrades, one by one:** a switch for each lasting Amber Shop item (+25% wheat, wood, stone and faith, starting resources and faith, 99 workers on construction, 1.5x battle speed, double hero stats) and an *All upgrades* button. Your Amber and purchases are never changed.

### Fame Shop (switch *Fame Shop*)
- A new button next to the Amber Shop. Every Fame point you earn by sailing away is also a perk point.
- **32 perks in 4 branches** (Industry, Conquest, Devotion, Legacy). Each branch has a root, two paths and a **keystone** that needs both: *Foremen*, *Cleave*, *Timeless Rites* and *Legend of Fame*.
- Perks are kept through abdications and new continents and can be reset for free. *Spoils of War* gives Fame for every tile you conquer.

### Extra buildings (switch *Extra buildings*)
- **16 new buildings.** On an empty tile, press **More >** in the Build, Bld. Stone and Bld. Heroic menus, or open **Bld. Marine** on a coastal tile. Bonuses count over all your tiles of that type and have **no limit**; reductions never reach zero (every 100 workers halve the amount).

  | Menu | Building | Effect |
  |---|---|---|
  | Build | Windmill | +2% wheat on all wheat fields per miller |
  | Build | Market | +1 gold per second per 10 merchants (also raises the abdication payout) |
  | Build | Sawmill | +2% wood on all forest camps per sawyer |
  | Build | Tavern | +2% hero experience per barkeeper |
  | Stone | Monolith | +0.5% to all production per keeper |
  | Stone | Library | +2% hiring speed in every building per scribe |
  | Stone | Observatory | +1 second ritual duration per astronomer |
  | Stone | Bank | +1% gold for abdication per banker |
  | Heroic | Arena | the academy hero gains experience every second, per gladiator |
  | Heroic | Watchtower | every 100 watchmen halve the monsters needed per tile |
  | Heroic | Barracks | +0.5% hero HP per recruit |
  | Marine | Harbor | +1% to everything your ships produce per docker |
  | Marine | Fishery | +1% wheat production per fisherman |
  | Marine | Lighthouse | +2% fame per keeper when you sail away |
  | Marine (page 2) | Sail Loft | every 100 sailmakers halve the ship price |
  | Marine (page 2) | Naval Academy | +0.5% hero HP and damage per marine |

- **4 special buildings** (**Bld. Special**, the fifth button of an empty tile). Each has **requirements** to build (shown in the tooltip with [OK] / [X]), you can have **only one**, and the bonus works only while the requirements hold. They give a flat **x2** and more for every worker:

  | Building | Requirements | Bonus |
  |---|---|---|
  | Hermitage | no Temples and no Cathedrals | x2 wheat, wood and stone production |
  | Imperial Mint | no Academies, at least 20 tiles | x2 gold per second and gold for abdication |
  | Citadel | an Academy, a Training hall and a Forge | x2 hero HP and damage |
  | Grand Sanctum | a Temple, a Cathedral and 30 monks | x2 faith production, rituals last 60 s longer |

- The new buildings have the **Spd**, **Clear** and **stop autohire** controls like the game's own, and they get no free starting workers from the Empire perk or the mutation.
- Switching the option OFF hides the buttons and pauses the bonuses; buildings you already built stay. **Before you uninstall the mod, clear these buildings** (or abdicate): the unmodified game cannot load a save that contains them.

### Hero extras (switch *Hero extras*)
- **7 new classes:** Knight, Ranger, Cleric, Duelist, Warlock, Martyr and Scholar. Each unlocks by reaching a level with another class, and unlocked classes stay unlocked when you sail away.
- **Class tooltip:** hover the hero to read its class and what it does.
- **20 new weapons, 5 new shields (two of them off-hand daggers) and 8 new helmets**, which unlock with the hero's level and show their effect in the tooltip. Weapons can be upgraded in the Forge. The game had no icons left for some of them, so they come with new pixel-art icons.

### Divine extras (switch *Divine extras*)
- **5 new religion gods** (Merchant, Sun, Storm, Night goddess, World turtle), each with its own art, 3 perks, a ritual that multiplies its holiness and a personal relic.
- **3 new servants** (Oracle, Treasurer, Architect) with 2 perks each. The servant screen has two rows now.
- **12 new pantheon gods**, **3 new relics** (Abundance, Fortune, Valor), **3 new mutations** (Giant heart, Gilded blood, Chimera) and **3 new ritual types** (gold, faith, wisdom).
- **6 new Empire rows** in a second tab (*Empires II*): Mongolian, Persian, Babylonian, Mayan, Viking and Indian. They are open from the start.
- Switching the option OFF hides all of this and pauses its effects; what you chose stays chosen.

## Install

1. **Export your save in the game first** (*Options → Export Save*, or *Save/Load to PC*) and keep it somewhere safe.
2. Download this repository (*Code → Download ZIP*) and unzip it.
3. Close the game and double-click **`Install.bat`**. It finds your game, keeps a backup of the original as `data.win.bak` (don't delete it), downloads [UndertaleModTool](https://github.com/UnderminersTeam/UndertaleModTool) once (checked by SHA-256) and patches your own copy of the game.
4. Start the game and open *Options → QoL Features...*.

To remove the mod, close the game and double-click **`Uninstall.bat`**. A Steam update replaces the patched file, so run `Install.bat` again after an update (if the game version changed, the mod may need an update first; the installer will tell you).

## Disclaimer

- **Unofficial fan mod.** The author of this mod does not own Territory Idle and is not its developer or publisher. This project is not affiliated with, endorsed by or supported by them. Territory Idle, its name, code, art and all other assets belong to their respective owners (developer: Aleksandr Golovkin). No ownership of any of it is claimed.
- **No game files included.** This repository contains only patch scripts and an installer. It modifies your own copy of the game on your own computer. A legitimate copy of Territory Idle is required; this mod is not a way to obtain the game.
- **No warranty, use at your own risk.** The mod is provided "as is". It may crash the game, corrupt or change your save, affect your progress or stop working after a game update. The author is not responsible for any loss or damage resulting from its use. Always export your save first.
- **Do not contact the game's developer or Steam support about problems with the modded game.**
- **Takedown.** If the rights holder asks for this project to be changed or removed, it will be.
