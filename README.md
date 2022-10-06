SourceSplit
===========

SourceSplit is a [LiveSplit](livesplit.org/) component which adds automated Start, Stop, and Splitting functionality for Source Engine games, along with other features geared towards speedrunning.  

This is 2838's fork of the project, featuring many improvements, fixes, and support for many more games and mods.

Features
--------  
* Main Features 
    * Keeps track of Game Time, with options to choose which ticks to account for.
    * Auto-Splits on map changes, configurable with a map transition blacklist or whitelist
    * For Games that support them:
    	* Auto-Splits on special pre-determined game events. 
    	* Auto-Start, Stop and Resetting on supported games.  
    	* Interoperation with Demo Recording, to use both in-game clocks for timing what Demos do not, and Demo time for more accurate time keeping.
* Additional Features  
    * Auto-Starting the timer upon doing a `map` command to a specific map.
    * Displaying the timer in higher precision or as a tick count.
    * Displaying game time for each map.  
    * Printing information of Demos which have just finished recording to the Console.

Installation
-------
There are two ways to install SourceSplit:  

Through the Splits Editor:
* Open the Splits Editor
* Enter the name of your game/mod into the *Game Name* field. An option to activate the splitter will now be available. 
* If the splitter is not accrediting both fatalis and 2838, then this fork of SourceSplit does not fully that game/mod. In this case, enter `Source Engine` insead.

Or through the Layout Editor:  
* Download a build from the *Releases* section of this page, then move `LiveSplit.SourceSplit.dll` to `<LiveSplit's root folder>/Components`  
* Open the Layout Editor, then click on *+*, hover over *Control* and click on *SourceSplit*. A warning will pop up if the splitter is already loaded in the Splits Editor.

Configuring
---------------------
Information on configuring SourceSplit can be found [here](CONFIGURING.md).


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
* The following Portal mod(s):  [Elevators](https://www.moddb.com/mods/portal-elevators) • [The Flash Version Mappack](https://portalmaps.wecreatestuff.com/) • [Portal Stories: Mel](https://store.steampowered.com/app/317400/Portal_Stories_Mel/) • [Aperture Tag](https://store.steampowered.com/app/280740/Aperture_Tag_The_Paint_Gun_Testing_Initiative/) • [Reversed Map Order](https://www.speedrun.com/portal_reversed_map_order/resources) • [Still Alive](https://www.moddb.com/mods/portal-still-alive-pc) • [Portal Pro](https://www.moddb.com/mods/portal-pro) • [Rexaura](https://store.steampowered.com/app/317790/Rexaura/) • [Canonical Base of ℝxℝxℝ](https://www.moddb.com/mods/portal-canonical-base-of-xx) • [Portal Prelude](http://www.portalprelude.com/) • [ERROR](https://www.moddb.com/mods/error2) • [Epic Edition](https://www.moddb.com/mods/portal-epic-edition) • [Portal: Crumbs of Truth](https://www.moddb.com/mods/portal-crumbs-of-truth)
* The following Half-Life 2 related mod(s): [1187: Episode One](https://www.moddb.com/mods/1187) • [Too Many Crates!](https://www.moddb.com/mods/too-many-crates) • [Da Baby](https://drive.google.com/file/d/1AEB1oOUM_vgkyjuzXgp3rlG2_YHhjV1_/view?usp=sharing) • [Dank Memes](https://www.moddb.com/mods/dank-memes) • [DayHard](https://www.moddb.com/mods/dayhard) • [Dear Esther](https://www.moddb.com/mods/dear-esther) • [Deep Down](https://www.moddb.com/mods/half-life-2-deep-down) • [Deeper Down](https://www.moddb.com/mods/half-life-2-deeper-down) • [Downfall](https://store.steampowered.com/app/587650/HalfLife_2_DownFall/) • [Entropy Zero](https://store.steampowered.com/app/714070/Entropy__Zero/) • [Exit 2](https://www.moddb.com/mods/exit-2) • [Gordon Freakman 1](https://www.moddb.com/mods/gordon-freakman) • [Gordon Freakman 2: Kleiner-Life](https://www.moddb.com/mods/gordon-freakman-2-kleiner-life) • [Get A Life](https://www.moddb.com/mods/get-a-life) • [Grey](https://www.moddb.com/mods/grey) • [ICE](https://www.moddb.com/mods/ice-a-half-life-2-expansion-pack) • [Kill The Monk](https://www.moddb.com/mods/kill-the-monk) • [Logistique: Act 1](https://store.steampowered.com/app/1154130/Logistique_Act_1/) • [Mission Improbable](https://www.runthinkshootlive.com/posts/mission-improbable/) • [The PTSD Mod](https://www.moddb.com/mods/the-ptsd-mod) • [Sniper's Episode](https://www.speedrun.com/patches/Snipers_Episode_ptqds.zip) • [Terminal 7](https://www.moddb.com/mods/terminal-7) • [The Citizen](https://www.moddb.com/mods/the-citizen) • [The Citizen 2](https://www.moddb.com/mods/the-citizen-part-ii) • [The Citizen Returns](https://www.moddb.com/mods/the-citizen-returns) • [The Lost City](https://www.moddb.com/mods/the-lost-city) • [Uncertainty Principle](https://www.moddb.com/mods/uncertainty-principle) • [Watching Paint Dry: The Game](https://www.moddb.com/mods/watching-paint-dry-the-game) • [Year Long Alarm](https://store.steampowered.com/app/747250/HalfLife_2_Year_Long_Alarm/) • [Think Tank](https://www.runthinkshootlive.com/posts/think-tank/) • [Gnome](https://www.moddb.com/mods/map-labs/downloads/atom-3-gnome) • [Backwards Mod](https://drive.google.com/file/d/1Eb2irBuVacM-jLbBKPDi-vZSUUxCvvQt/view) • [Reject](https://www.runthinkshootlive.com/posts/reject/) • [TrapVille](https://www.runthinkshootlive.com/posts/trapville/) • [RTSLVille](https://www.runthinkshootlive.com/posts/runthinkshootliveville/) • [Half-Life: Abridged](https://www.runthinkshootlive.com/posts/half-life-abridged/) • [Episode One (RTSL)](https://www.runthinkshootlive.com/posts/episode-one-map-labs-2/) • [CombinationVille](https://www.runthinkshootlive.com/posts/combinationville/) • [PhaseVille](https://www.runthinkshootlive.com/posts/phaseville/) • [Companion Piece](https://www.runthinkshootlive.com/posts/companion-piece/) • [School Adventures](https://www.moddb.com/mods/school-adventures) • [Cutsceneless Mod](https://mega.nz/#F!yjgQiYKL!CeObY9822otooK31Y6A2FQ) • [Experimental Fuel](https://www.runthinkshootlive.com/posts/experimental-fuel/) • [Tinje](https://www.runthinkshootlive.com/posts/tinje/) • [Dark Intervention](https://www.runthinkshootlive.com/posts/dark-intervention/) • [Hells Mines](https://www.runthinkshootlive.com/posts/hells-mines/) • [Upmine Struggle](https://www.runthinkshootlive.com/posts/upmine-struggle/) • [The Stanley Parable (mod)](https://www.moddb.com/mods/the-stanley-parable) • [Black Mesa (mod)](https://www.moddb.com/mods/black-mesa) • [Precursor](https://www.moddb.com/mods/precursor) • [Genry's Great Escape From City 13](https://store.steampowered.com/app/1341060/HalfLife_2_Genrys_Great_Escape_From_City_13/) • [Offshore](https://www.moddb.com/mods/offshore) • [Hangover](https://www.moddb.com/mods/hangover) • [Dangerous World](https://www.moddb.com/mods/dangerous-world) • [Very Hard Mod](https://www.moddb.com/addons/very-hard-mod-steampipe-fixed)
* The following Black Mesa related mod(s): [Hazard Course](https://www.moddb.com/mods/black-mesa-hazard-course) • [Further Data](https://steamcommunity.com/sharedfiles/filedetails/?id=2316239201)
* (more soon)

Technical Information
---------------------
#### How It Works  
SourceSplit heavily relies on external memory reading using Window's ReadProcessMemory function to poll data. Pointers for values necessary for timing are found through [Signature Scanning](https://wiki.alliedmods.net/Signature_scanning). This allows the component to achieve a high compatibility rate.

#### Timing Accuracy
SourceSplit attempts to find the [`host_tickcount` variable](https://github.com/VSES/SourceEngine2007/blob/43a5c90a5ada1e69ca044595383be67f40b33c61/src_main/engine/host.cpp#L2646) that the game uses to track how many updates the main engine update loop has done. This allows SourceSplit to very accurately measure game time and avoid issues with the game’s tick counter such as Pause Abuse. Ticks may only be lost due to slow polling rates or other low level problems.  

SourceSplit can also look current information such as recording tick of the Demo Recorder to use the currently recorded Demo's time. When a demo finishes recording, the component will parse the demo and add any time that was missed due to polling. This allows SourceSplit to match Demo time near-perfectly, even under extreme conditions such as save/load abuse or [long sessions with many loads](https://www.youtube.com/watch?v=_jXgA-HVME8).

tl;dr The timing is very accurate and should never be more than a few ticks off.

Credits
---------------------
* Fatalis for the original version.
* 2838 for current maintenece and development.
