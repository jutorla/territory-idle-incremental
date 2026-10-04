# Territory Idle – QoL mod changelog

## 1.3.1 – 2026-10-04

- **Spoils of War** now gives **Fame instead of gold**: 0.2 Fame per level (0.2 / 0.4 / 0.6 / 0.8 / 1.0) for every tile you conquer in battle, which is also that many perk points. The gold payout was too strong.

## 1.3.0 – 2026-10-04

- **Fame Shop rebuilt: 32 perks in 4 branches** (Industry, Conquest, Devotion, Legacy). Each branch is a "diamond": a root, two paths and a **keystone** that needs both paths. Most perks are now mechanics, and several grow with your progress (tiles, Fame, continents, rituals, ships, workers).
- **Four keystones:**
  - **Foremen:** 1% per level of your wheat, wood, stone and faith income is added to your gold.
  - **Cleave:** each monster kill counts as up to 3 kills toward capturing the tile.
  - **Timeless Rites:** rituals stop getting more expensive with every cast.
  - **Legend of Fame:** each Fame point gives more gold when you abdicate.
- **Other new perks:** hire several workers at once, slower worker-cost growth, cheaper tile-price growth, tile refunds and conquest payouts, plunder from kills, restore your buildings after abdication, keep part of your stock when abdicating, fewer ships needed to sail, more Fame when sailing, cheaper religion perks.
- No automation perks, and nothing that duplicates an Amber Shop item. The Amber Shop itself is untouched.
- Perks bought in 1.2 are refunded because the tree changed. Your Fame is untouched.

## 1.2.0 – 2026-10-04

- **Bigger Fame Shop:** the skill tree now has **28 perks in 4 branches** (new branch: **Devotion**), up from 12 in 3. Each branch is a 7-perk tree with two forks and a capstone that needs both sides.
- **Much stronger perks** (about 2–3x per level), with new kinds of upgrades:
  - **Prosperity:** separate wheat, wood and stone boosts, gold income, and the *Golden Age* capstone (+12% to all production per level).
  - **Industry:** hire several workers at once, slower worker-cost growth (*Economies of Scale*), +1000 starting resources or faith per level.
  - **Valor:** hero damage, damage reduction, attack speed, dodge, and the *Warlord's Banner* capstone.
  - **Devotion:** holiness, ritual power and automation, and *Apotheosis* (+20% production per level while a ritual is running).
- Perks bought in 1.1.0 are refunded because the tree changed. Your Fame is untouched.

## 1.1.0 – 2026-10-04

- **Fame Shop:** a new button next to the Amber Shop. Every Fame point you earn (by sailing away to a new continent) is also a perk point, to spend on a skill tree of 12 permanent upgrades, 5 levels each, that make the game less grindy:
  - **Prosperity:** more wheat/wood/stone, more faith, more gold when abdicating, cheaper tiles.
  - **Industry:** faster and cheaper worker hiring, bigger starting resources and faith in every new game.
  - **Valor:** fewer monsters per tile battle, more hero experience, longer and cheaper rituals.
  Perks are kept through abdications and new continents, and can be reset for free. A badge on the button shows your unspent points.
- The QoL Features page has a new **Fame Shop** switch (OFF hides the button and disables every perk; your purchases are kept).

## 1.0.0 – 2026-10-03

The game is now more **incremental than idle**: you decide the pace instead of waiting for timers.

- **Instant ritual finish:** click the Ritual button while a ritual is running to finish it right away and get everything it would have produced.
- **Game speed button:** a new button in the left column cycles the game speed 1x → 2x → 5x → 10x → 20x (right click goes back). The real speed depends on your PC.
- **Fast fights:** when the hero's hit kills the next monster, the fight continues immediately, up to 100 kills at once.
- **QoL Features options page:** every change above can be switched on/off in *Options (gear icon) → QoL Features...*. Settings are remembered.

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
