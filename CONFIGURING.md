
SourceSplit
---------

## Introduction
SourceSplit is a LiveSplit component, primarily designed for timing Source Engine speedruns, and comes with many features geared toward such purpose.

## Installation
SourceSplit is a LiveSplit Auto-Splitter, so it can be installed through either the Splits Editor or the Layout Editor. Installing it in the latter is recommended as it allows for more features.

### Through Splits Editor
If your game already supports new SourceSplit,
* Open the Splits Editor in LiveSplit
* Enter the name of your Game at the top
* The option for activating SourceSplit should now appear. If so, click it.  

### Through Layout Editor
* Close LiveSplit (if it is open).
* Grab the latest version from the Github's Releases [page](https://github.com/thisis2838/SourceSplit/releases).
* Move the newly-downloaded `LiveSplit.SourceSplit.dll` to `<your LiveSplit's root folder> / Components`. If prompted to replace the existing .dll file, agree to do so.
* Launch LiveSplit, then open the Layout Editor
* Click on the + icon, then hover over *Control*, then *SourceSplit*
* SourceSplit should now appear in the list in the Editor.

## Configuring
To configure SourceSplit, right click LiveSplit, hover over *Control* and click on *SourceSplit: Settings*. 

In the Settings window, there are many sections for configuring various aspects of the tool. You can hover over an option to see a description of it.

#### Before we move on
Before we move onto explaining each option, we will need to distinguish between a *map transition* and a *New Game map load*
* A *map transition* is a transition between 2 maps which have a direct connection. Entering the trigger to do such will display the *LOADING* label on screen. The only way to trigger these transitions through the console is with the `changelevel` commands.
* A *New Game map load*, or *Starting a New Game on a map*, or *triggering a New Game map load to a map*, is a transition between 2 maps which can be disparate. An example of such a transition is by using the `map` command to go to another map. In games/mods that use this transition, entering the trigger to do such will show a black screen, before the next map appears.

#### Auto Split
This controls how SourceSplit should Auto-Split. Here you can
* Enable / Disable this feature
* Enable / Disable splitting on Map Transitions. This also toggles its children controls, which include:
    * Enabling / Disabling, and setting the split interval. The timer will only split every `N`th transition if enabled.
    * Enabling / Disabling counting New Game map loads
    * Enabling / Disabling, and configuring map transition filtering, which when enabled, allows or disallows certain map transitions. To edit the list, type on the bottom-most line; to delete a row, highlight that row and hit Delete.
* Enable / Disable splitting on Special Pre-defined Events, which will split when SourceSplit detects a game event defined in SourceSplit's code. This is separate from map transitions, and standard mechanisms for preventing repeats or splitting by interval do not apply here. 

#### Auto Start / Stop / Reset
This controls how SourceSplit should Auto-Start, Stop and Reset. Here you can
* Enable / Disable Auto-Start, Stop and Reset individually.
* Enable / Disable an additional Auto-Start condition, which allows you to Auto-Start the timer when one of the following happens (note that the input needn't include file extensions)
    * Starting a New Game on a specific map
    * Transitioning to a specific map
    * Loading a save file with a specific file   

#### Timing Options
This controls how SourceSplit should be counting Game Time.  
Timing Methods include:
* *Engine Ticks*, which tells the tool to count ticks when physics is simluated (i.e. when a map is loaded and the game isn't loading or paused)
* *Pauses*, which tells the tool to count ticks when the game is paused.
* *Disconnects*, which tells the tool to count ticks when no map is loaded. (e.g. using the `disconnect` command).
* *No Game*, which tells the tool to count time when no game is running (please excuse the obvious counter-intuive point of this option, blame *really creative* Portal runners...)  

Other options include:
* *Interoperation with Demo Recording*, which tells SourceSplit to monitor the Demo Recorder and created Demos. This allows SourceSplit to more accurately match up to time indicated by Demos.
* *Let SourceSplit Decide*, which lets SourceSplit choose what timing method works best for the current game/mod.

#### Miscellaneous
This tab contains miscellanea such as

##### Additional TImers
These timers are drawn in the UI like a text component. SourceSplit must be loaded in the Layout Editor for these to work. Currently there are options for
* Showing *Higher Precision Time*: A precision-configurable Timer that can go up to 7 decimal numbers, measuring the current or alternate timing method.
* Showing *Game Time Tick Count*: A tick Counter which converts the current Game Time to ticks.  It will also append the tick count of the current Session (which is the time since the last game load of any kind).
* Showing *Currently-recorded Demo*: Displaying the name and current time of the currently-recorded Demo.

##### Session and Map Times
This button opens the *Session and Map Times* window, which shows Game Time for each Session and Map in the run.

##### Print info of Demos after recording
This option toggles the printing of info of a Demo after its' recording is finished.  
*Path of Demo Parser* is the Demo Parser program whose output, when given the demo, should be printed in the Console. If none is specified, SourceSplit will use its own parser.

##### Timer Behavior
This controls certain details with timing behaviors. Currently there are options for:
* *Resuming timer after initial unpause*. When starting the timer manually, if the game is paused, and this option is enabled, will pause the timer until the game unpauses.
* *Auto-Start RTA and IGT with the same time offset*, which when enabled, applies the timing offset that is automatically applied onto Game Time on an Auto-Start to Real Time.  
* *Save/Load Handling*, which tells the tool how to react to save/loads. It includes:
    * *Count server initialization ticks*, which when enabled, tells the timer to also count ticks before a map is fully loaded.
    * *Ticks to add per Load*, which is the number of ticks the timer should add to Game Time the first moment when a save can be made after a load.  
* *Auto-Split when an Auto-Reset would occur*, which when enabled, turns all Auto-Resets into Auto-Splits.
* *Optimize for multi-run speedruns*. which when enabled, internally changes some settings to better support multi-run speedruns (e.g. any% 25 times, etc..)  
* *Auto-Reset when starting a New Game on the first map*, which resets the timer when you trigger a New Game map load to the first map (e.g. `map` comamnd).

