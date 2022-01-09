SourceSplit
===========

SourceSplit is a [LiveSplit] component for Source engine games. It aims to support every game and engine version, but some features are only available on games where support has been added.

This particular flavor of SourceSplit is 2838's fork of the project, featuring many sweeping changes, major bugfixes and enormous additional game/mod support.

Features
--------
  * Keeps track of Game Time through memory injection, accounting for every available tick with options to remove Pause Time and accomodate game speed modifications.
  * Auto-Splits on map changes, configurable through a map blacklist or a whitelist.
  * Auto-Splits on pre-determined events for some games with such needs, such as The Stanley Parable.
  * Auto-Start, Stop and Resetting on supported games.
  * Displaying the timer in higher precision or as a tick count.

Installation
-------
There are two ways to install SourceSplit:
  * Through the Splits Editor, which requires entering the name of the game you wish to use SourceSplit with. If a game isn't fully supported, enter Source Engine instead.
  * Through the Layout Editor, under the Control option in the Add drop down.

Configure
---------
Clicking "Settings" in the Splits Editor, or double clicking SourceSplit's entry in the Layouts Editor will open the Setting menu, which contains:

#### Auto Split
This controls how SourceSplit should Auto-Split. Here you can
  * Enable / Disable this feature
  * Control how often Auto-Splits should happen.
  * Configure and choose between a map whitelist or blacklist.

#### Auto Start/End/Reset
This controls how SourceSplit should Auto-Start or Stop. Here you can
  * Enable / Disable this feature
  * Configure a custom Auto-Start which will start the timer when the given map is newly loaded (for example: through the `map` command)

#### Game Processes
This controls what Process Names SourcSplit should be looking for to hook into. This must be populated with your game's Process Name in order for SourceSplit to work. Default values have already been filled in and can be retrieved using the "Defaults" button.

#### Game Time
This determines what Game Timing Method SourceSplit should be using. This includes:
  * Automatic, which lets SourceSplit decide which Timing Method is preferable for each game / mod.
  * Engine Ticks, which tells SourceSplit to time ticks when the game is active (for example: a map is loaded) and is not paused.
  * Engine Ticks with Pauses, which tells SourceSplit to time ticks like above but with pause time.
  * All Engine Ticks, which tells SourceSplit to count all possible game ticks, even when the game isn't active (for example: no map is loaded).

#### Miscellaneous
This tab contains miscellanea such as
##### Additional TImers
These timers are drawn in the UI like a text component. SourceSplit must be loaded in the Layout Editor for these to work. Currently there are options for
  * A precision-configurable Timer that can go up to 7 decimal numbers, using either Game Time or Real Time.
  * A tick Counter which converts the current Game Time to ticks.
##### Map Times
This button opens the list of previous map auto-splits. This can also be opened through the LiveSplit right click drop down, under Control.
##### Timer Behavior
This controls certain details with timing behaviors. Currently there are options for:
  * Resuming SourceSplit's IGT upon the next time the game is unpaused, if the timer was started when the game was paused.

Fully Supported Games and Mods
---------------------
  * Half-Life 2
  * Half-Life 2: Episode One
  * Half-Life 2: Episode Two
  * Half-Life 2: Lost Coast
  * Half-Life 2: Survivor
  * Half-Life Source
  * Portal
  * Portal 2
  * Infra
  * Hunt Down The Freeman
  * Black Mesa
  * Estranged: Act I
  * Transmissions: Element 120
  * The Beginner's Guide
  * The Stanley Parable
  * Prospekt
  * Synergy
  * SiN Episodes
  * The following Portal related mod(s):  [Elevators](https://www.moddb.com/mods/portal-elevators) • [The Flash Version Mappack](https://portalmaps.wecreatestuff.com/) • [Portal Stories: Mel](https://store.steampowered.com/app/317400/Portal_Stories_Mel/) • [Aperture Tag](https://store.steampowered.com/app/280740/Aperture_Tag_The_Paint_Gun_Testing_Initiative/) • [Reversed Map Order](https://www.speedrun.com/portal_reversed_map_order/resources) • [Still Alive](https://www.moddb.com/mods/portal-still-alive-pc) • [Portal Pro](https://www.moddb.com/mods/portal-pro) • [Rexaura](https://store.steampowered.com/app/317790/Rexaura/) • [Canonical Base of ℝxℝxℝ](https://www.moddb.com/mods/portal-canonical-base-of-xx) • [Portal Prelude](http://www.portalprelude.com/) • [ERROR](https://www.moddb.com/mods/error2) • [Epic Edition](https://www.moddb.com/mods/portal-epic-edition)
  * The following Half-Life 2 related mod(s): [1187: Episode One](https://www.moddb.com/mods/1187) • [Too Many Crates!](https://www.moddb.com/mods/too-many-crates) • [Da Baby](https://drive.google.com/file/d/1AEB1oOUM_vgkyjuzXgp3rlG2_YHhjV1_/view?usp=sharing) • [Dank Memes](https://www.moddb.com/mods/dank-memes) • [DayHard](https://www.moddb.com/mods/dayhard) • [Dear Esther](https://www.moddb.com/mods/dear-esther) • [Deep Down](https://www.moddb.com/mods/half-life-2-deep-down) • [Deeper Down](https://www.moddb.com/mods/half-life-2-deeper-down) • [Downfall](https://store.steampowered.com/app/587650/HalfLife_2_DownFall/) • [Entropy Zero](https://store.steampowered.com/app/714070/Entropy__Zero/) • [Exit 2](https://www.moddb.com/mods/exit-2) • [Gordon Freakman 1](https://www.moddb.com/mods/gordon-freakman) • [Gordon Freakman 2: Kleiner-Life](https://www.moddb.com/mods/gordon-freakman-2-kleiner-life) • [Get A Life](https://www.moddb.com/mods/get-a-life) • [Grey](https://www.moddb.com/mods/grey) • [ICE](https://www.moddb.com/mods/ice-a-half-life-2-expansion-pack) • [Kill The Monk](https://www.moddb.com/mods/kill-the-monk) • [Logistique: Act 1](https://store.steampowered.com/app/1154130/Logistique_Act_1/) • [Mission Improbable](https://www.runthinkshootlive.com/posts/mission-improbable/) • [The PTSD Mod](https://www.moddb.com/mods/the-ptsd-mod) • [Sniper's Episode](https://www.speedrun.com/patches/Snipers_Episode_ptqds.zip) • [Terminal 7](https://www.moddb.com/mods/terminal-7) • [The Citizen](https://www.moddb.com/mods/the-citizen) • [The Citizen 2](https://www.moddb.com/mods/the-citizen-part-ii) • [The Citizen Returns](https://www.moddb.com/mods/the-citizen-returns) • [The Lost City](https://www.moddb.com/mods/the-lost-city) • [Uncertainty Principle](https://www.moddb.com/mods/uncertainty-principle) • [Watching Paint Dry: The Game](https://www.moddb.com/mods/watching-paint-dry-the-game) • [Year Long Alarm](https://store.steampowered.com/app/747250/HalfLife_2_Year_Long_Alarm/) • [Think Tank](https://www.runthinkshootlive.com/posts/think-tank/) • [Gnome](https://www.moddb.com/mods/map-labs/downloads/atom-3-gnome) • [Backwards Mod](https://drive.google.com/file/d/1Eb2irBuVacM-jLbBKPDi-vZSUUxCvvQt/view) • [Reject](https://www.runthinkshootlive.com/posts/reject/) • [TrapVille](https://www.runthinkshootlive.com/posts/trapville/) • [RTSLVille](https://www.runthinkshootlive.com/posts/runthinkshootliveville/) • [Half-Life: Abridged](https://www.runthinkshootlive.com/posts/half-life-abridged/) • [Episode One (RTSL)](https://www.runthinkshootlive.com/posts/episode-one-map-labs-2/) • [CombinationVille](https://www.runthinkshootlive.com/posts/combinationville/) • [PhaseVille](https://www.runthinkshootlive.com/posts/phaseville/) • [Companion Piece](https://www.runthinkshootlive.com/posts/companion-piece/) • [School Adventures](https://www.moddb.com/mods/school-adventures) • [Cutsceneless Mod](https://mega.nz/#F!yjgQiYKL!CeObY9822otooK31Y6A2FQ) • [Experimental Fuel](https://www.runthinkshootlive.com/posts/experimental-fuel/) • [Tinje](https://www.runthinkshootlive.com/posts/tinje/) • [Dark Intervention](https://www.runthinkshootlive.com/posts/dark-intervention/) • [Hells Mines](https://www.runthinkshootlive.com/posts/hells-mines/) • [Upmine Struggle](https://www.runthinkshootlive.com/posts/upmine-struggle/) • [The Stanley Parable (mod)](https://www.moddb.com/mods/the-stanley-parable) • [Black Mesa (mod)](https://www.moddb.com/mods/black-mesa) • [Precursor](https://www.moddb.com/mods/precursor) • [Genry's Great Escape From City 13](https://store.steampowered.com/app/1341060/HalfLife_2_Genrys_Great_Escape_From_City_13/) • [Offshore](https://www.moddb.com/mods/offshore) • [Hangover](https://www.moddb.com/mods/hangover) • [Dangerous World](https://www.moddb.com/mods/dangerous-world) • [Very Hard Mod](https://www.moddb.com/addons/very-hard-mod-steampipe-fixed)
  * The following Black Mesa related mod(s): [Hazard Course](https://www.moddb.com/mods/black-mesa-hazard-course) • [Further Data](https://steamcommunity.com/sharedfiles/filedetails/?id=2316239201)
  * (more soon)

Technical Information
---------------------
#### How It Works
SourceSplit relies entirely on external memory reading to monitor in-game variables such as player or entities' properties like position, health, flags, etc; and events such as map changes, entity inputs and outputs, screen fades, etc.. To achieve a high level of compatibility even with untested games, SourceSplit makes heavy use of [Signature Scanning](https://wiki.alliedmods.net/Signature_scanning) to find pointers and useful memory offsets. Many hours were put into extensive testing and decompiling to reverse engineer these pointers with tools such as IDA Pro, OllyDbg, Ghidra, and the Source SDK.

#### Timing Accuracy
SourceSplit injects and memory detour into the engine host's [update loop](https://github.com/VSES/SourceEngine2007/blob/43a5c90a5ada1e69ca044595383be67f40b33c61/src_main/engine/host.cpp#L2632), which is ran every single tick to update various aspects of the game, to increment a custom tick counter independent of the game's own tick counter. This allows SourceSplit to very accurately measure game time and avoid issues with the game's tick counter such as [Pause Abuse](https://www.youtube.com/watch?v=NJCWzwX_2rg). Ticks may only be lost due to slow polling rates or other low level problems.

tl;dr The timing is very accurate and should never be more than half of a second off.

Credits
---------------------
  * Fatalis for the original version.
  * 2838 for current maintenece and development.
