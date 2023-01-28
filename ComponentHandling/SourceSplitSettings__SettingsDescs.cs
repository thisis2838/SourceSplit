using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using LiveSplit.SourceSplit.Utilities;
using System.Xml;
using System.Drawing;
using static LiveSplit.SourceSplit.ComponentHandling.SourceSplitSettingsHelp;
using LiveSplit.SourceSplit.ComponentHandling;
using System.Security;

namespace LiveSplit.SourceSplit.ComponentHandling
{
    public partial class SourceSplitSettings : UserControl
    {
        private void SetSettingDescriptions()
        {
            const string EXPLAIN_GENERIC_LOAD = "New Game map loads are map changes which are not triggered by a changelevel trigger, or through using the changelevel command. Instead, it is typically triggered using the 'map' command. If triggered through the console, a dedicated loading screen with a progress bar will apprear. If triggered by the map using entities, a black screen is shown.";

            const string EXPLAIN_CHANGELEVEL = "Changelevels are map changes which are triggered by a changelevel trigger, or through using the changelevel command. When a changelevel is triggered, the game will freeze the screen, and display a box with 'LOADING' written on it.";

            const string EXPLAIN_SESSION = "A session is the time between 2 loads.";

            Instance.SetName(labVersion, "Version text");
            Instance.SetDescription(labVersion,
                @"This is the version of this build of SourceSplit. The build date is parenthesized and appended to it.");

            Instance.SetName(tlpAutoStartEndReset, "Main Functions");

            Instance.SetDescription(gAutoSplit, 
                "Settings related to Automatic Splitting.");
            
            Instance.SetDescription(chkAutoSplitEnabled,
                "This option enables or disables Automatic Splitting.");

            Instance.SetName(gMapTransitions, "Split on Map Transitions");
            Instance.SetDescription(gMapTransitions,
                @"This option includes options for configuring how SourceSplit should treat types of map transitions, and whether to split upon detecting one.");

            Instance.SetName(panSplitInterval, "Every ? transition");
            Instance.SetDescription(panSplitInterval,
                @"This option decides whether SourceSplit should skip a number of considered map transitions before Auto-Splitting on one.

If set to 1, SourceSplit will not skip any transitions; if set to 2, SourceSplit will skip 1 transition, before splitting on the next one; and etc....

This mechanism resets when the timer is reset.");

            Instance.SetDescription(chkSplitGenericMap,
                @"This option decides whether SourceSplit should consider New Game map loads as map transitions, or ignore them.

" + EXPLAIN_GENERIC_LOAD);

            Instance.SetName(dgvMapTransitions, "Map Transition List");
            Instance.SetDescription(dgvMapTransitions,
                @"This option is the list of map transitions which is to be compared to one for filtering to decide whether SourceSplit should consider it.

The left-hand side of each line is the source map, and the corresponding right-hand side of each line is the destination map in a map transition.

Select and type in the bottom-most row to add a new entry. Select an entire line and hit Delete to remove that line. 
Double click on a cell to begin editing it. Select one or more cells and hit backspace to clear them.

Enter '*' to signify any map.");

            Instance.SetName(cmbMTLMode, "Map Transition Filtering Type");
            Instance.SetDescription(cmbMTLMode,
                @"This option decides what SourceSplit should do if a map transition is included in the list.

If set to 'Allow', SourceSplit will consider this map transition. If set to 'Disallow', SourceSplit will ignore this map transition.");

            Instance.SetName(chkUseMTL, "Only ? Map Transitions");
            Instance.SetDescription(chkUseMTL,
                @"This option decides whether to enable or disable filtering map transitions to decide which ones SourceSplit should consider.");

            Instance.SetName(gMTL, "Only ? Map Transitions");
            Instance.SetDescription(gMTL,
                @"This option contains settings of the filtering applied onto detected map transitions to decide whether SourceSplit should consider them");

            Instance.SetDescription(chkSplitSpecial,
                @"This option decides whether SourceSplit should Auto-Split upon detecting a game/mod-specific pre-defined event such as completing objectives, reaching specific destinations, etc..");

            Instance.SetName(gAdditionalAutoStart, "Also Auto-Start when");
            Instance.SetDescription(gbAutoStartEndReset,
                @"This option contains settings for Automatic Starting, Stopping and Resetting");

            Instance.SetDescription(chkAutoStart,
                @"This option enables or disables Automatic Starting");

            Instance.SetDescription(chkAutoStop,
                @"This option enables or disables Automatic Stop");

            Instance.SetDescription(chkAutoReset,
                @"This option enables or disables Automatic Resetting");

            Instance.SetDescription(chkAllowAddAutoStart,
                @"This option enables or disables an additional Auto-Start condition.");

            Instance.SetName(gAdditionalAutoStart, "Also Auto-Start when...");
            Instance.SetDescription(gAdditionalAutoStart,
                @"This option contains settings of the Additional Auto-Start, which will be triggered along with the normal Auto-Start.");

            Instance.SetName(cmbAddAutoStartMode, "Additional Auto-Start condition");
            Instance.SetDescription(cmbAddAutoStartMode,
                $@"This option decides the condition of the additional Auto-Start

If the condition is 'Starting a New Game on this map', when a New Game map load is detected, the game will compare the destination map to the input string. 
If the condition is 'Transitioning to this map', when a changelevel map transition is detected, the game will compare the destination map to the input string.
If the condition is 'Loading a save with this name', when a save is loaded, the game will compare the name of the save to the input string.

The input string does not need to include the file extension, if there is one.

{EXPLAIN_CHANGELEVEL}

{EXPLAIN_GENERIC_LOAD}
");

            Instance.SetName(boxAddAutoStartName, "Additional Auto-Start input string");
            Instance.SetDescription(boxAddAutoStartName,
                @"This option defines the input string that the specified additional Auto-Start condition should use");

            Instance.SetDescription(gbTiming, 
                @"This option contain settings for configuring how SourceSplit should count Game Time");

            Instance.SetDescription(gTimingMethods,
                @"This option contains settings for choosing what types of time SourceSplit should consider.");

            Instance.SetDescription(chkCountEngineTicks,
                @"This setting decides whether SourceSplit should count Engine Ticks into Game Time. 

Engine Ticks are ticks when the game is active (i.e. a map is loaded), and when physics is being simulated (i.e. when the game isn't paused)");

            Instance.SetDescription(chkCountPauses,
                @"This setting decides whether SourceSplit should count Pauses into Game Time. 

These moments are when the game is displaying its load screen; or displaying the 'LOADING' plaque, if the pause was triggered using the 'pause' command.");

            Instance.SetDescription(chkCountDisconnect,
                @"This setting decides whether SourceSplit should count Disconnects into Game Time.

These moments are when the game is not active (i.e. when no map is loaded, and the game is sitting idle in the menus, with a picture for its background).");

            Instance.SetDescription(chkNoGamePresent,
                @"This setting decides whether SourceSplit should count No Game time into Game Time.

These moments are when the game is not open. This does seem weird, yes, but it's there, because...");

            Instance.SetDescription(chkDemoInterop,
                @"This setting decides whether SourceSplit should perform interoperation with the game/mod's Demo Recording.

If enabled, SourceSplit will incorporate Game Time measured in demos which are being recorded, or have just finished recording while the game/mod is running.

In some games/mods, this setting is forced on.");

            Instance.SetDescription(chkAutomatic,
                @"This setting decides whether SourceSplit should automatically decide what the best Timing Method for the game/mod.

Enabling this will lock the controls in Timing Method.");

            Instance.SetDescription(gbAdditionalTimer,
                @"This setting contains options for enabling or disabling Additional Timers.

These timers are displayed on LiveSplit like a Text Component. Their position is dependent on SourceSplit's position in the list in LiveSplit's Layout Editor.

These timers will only work if SourceSplit is loaded in the Layout through the Layout Editor.");

            Instance.SetName(gHigherPrecision, "Show Higher Precision Time");
            Instance.SetDescription(gHigherPrecision,
                @"This option contain settings for enabling, disabling, and configuring the Higher Precision Timer.

This timer's precision can be configured, and can be instructed to show the alternate timing method compared to the current one. (If comparing to Game Time, then it will read Real Time, and vice versa).");

            Instance.SetDescription(chkShowGameTime,
                @"This option enables or disables the Higher Precision Timer.");

            string decimalPlacesHelp = @"This option decides the precision for the Higher Precision Timer.

The maximum value is 7.";
            Instance.SetName(nudDecimalPlaces, "Decimal Places");
            Instance.SetDescription(nudDecimalPlaces, decimalPlacesHelp);
            Instance.SetDescription(label3, decimalPlacesHelp);

            Instance.SetDescription(chkShowAlt,
                @"This option decides whether the Higher Precision Timer will show the alternate timing method compared to the current one. 

If comparing to Game Time, then it will read Real Time, and vice versa.");

            Instance.SetDescription(chkShowTickCount,
                $@"This option enables or disables the Game Time Tick Count.

This is an Additional Timer which will show both the current Game Time as ticks, and the current session's tick count.

{EXPLAIN_SESSION}");

            Instance.SetDescription(chkShowCurDemo,
                @"This option decide whether SourceSplit should show the information of the currently-recorded demo.

This Additional Timer will show the name and the current recording time of the demo which is being recorded. If no demo is being recorded, the information of the previously recorded demo will be shown.");

            Instance.SetDescription(gbMapTimes,
                $@"This option shows the Session and Map times window. 

This window shows the time of the recorded sessions, and tallies the time and displays them by map.

{EXPLAIN_SESSION}
");

            Instance.SetName(gPrintDemoInfo, "Print info of Demos after recording");
            Instance.SetDescription(gPrintDemoInfo,
                @"This option contain settings for printing information of demos which have just finished recording into the game/mod's console.

A custom external Demo Parser can be specified to be used instead of SourceSplit's internal one.");

            Instance.SetName(boxDemoParserPath, "Path of external Demo Parser");
            Instance.SetDescription(boxDemoParserPath,
                @"This option decides the path of the external Demo Parser, for use to print the information of the last recorded demo.

This Demo Parser must automatically print the demo's information when given the double-quoted path to it as a single standalone parameter.");

            Instance.SetDescription(chkPrintDemoInfo,
                @"This option enables or disables printing the information of demos which have just finished recording into the game/mod's console.

Note: this may not work for all games/mods.");

            Instance.SetDescription(gTimerBehavior,
                @"This option contains settings for changing timer's behavior upon reacting to various events.");

            Instance.SetDescription(chkHoldUntilPause,
                @"This option decides whether the timer should pause when it is started while the game is paused, and then immediately resume upon the very next moment the game is not paused.

If 'Pauses' is not enabled in 'Timing Options', and this is enabled, the timer will still behave as described.");

            Instance.SetDescription(chkRTAStartOffset,
                @"This option decides if the built-in Game Time offset for Auto-Start should be applied onto Real Time.

Some games may have hardcoded Auto-Start time offsets, such as Portal when loading vault save. When an Auto-Start is triggered, that time offset is then applied onto Game Time, but not Real Time. This setting decides whether that offset should also be applied onto Real Time.");

            Instance.SetDescription(groupBox5,
                @"This option contain settings for handling Saving and Loading.");

            Instance.SetDescription(chkServerInitialTicks,
                @"This option enables or disables including server initializtion ticks when counting Game Time.

These ticks happen before the game is fully loaded, and before demo recording can usually begin.");

            string slPenaltyHelp = @"This option decides the number of ticks which should be added to Game Time when the game finishes loading something.

Because this is only added when the game finishes loading, rapidly Save/Load buffering will not rapidly add this number of ticks to the timer.

In some games/mods, this setting is forced to 1.";
            Instance.SetName(nudSLPenalty, "Ticks to add to IGT per finished load");
            Instance.SetDescription(nudSLPenalty, slPenaltyHelp);
            Instance.SetDescription(label4, slPenaltyHelp);

            Instance.SetDescription(boxSplitInstead,
                @"This option decides whether an Auto-Split should be triggered when an Auto-Reset does.

If Auto-Resetting is disabled, an Auto-Split will not be triggered.");

            Instance.SetDescription(chkResetMapTransitions,
                @"This option enables or disables optimizations for multi-run speedruns.

Multi-run speedruns are runs which repeat a category more than once, resulting in Auto-Reset and Auto-Start being triggered more than once. This setting changes some internal logic to accommodate such runs.");

            Instance.SetDescription(chkFirstMapReset,
                $@"This option decides whether an Auto-Reset should be triggered when a New Game map load to the pre-defined first map of the game/mod is triggered.

{EXPLAIN_GENERIC_LOAD}
");

            Instance.SetName(labCurrentGame, "Detected game/mod");
            Instance.SetDescription(labCurrentGame,
                @"The game/mod that is identified by SourceSplit");

            Instance.SetName(labRunningFor, "Running for");
            Instance.SetDescription(labRunningFor,
                @"The amount of time that this instance of SourceSplit has been running for.");

            Instance.SetDescription(butGRepo,
                @"This button opens the Github repository of this SourceSplit fork.");

            Instance.SetDescription(butReport,
                @"This button opens the Github Issues page of this SourceSplit fork, where you can report an issue that you've encountered while using the tool.");

            Instance.SetDescription(butReleases,
                @"This button opens the Github Releases page of this SourceSplit fork, where you can find the latest and previous releases of this SourceSplit fork.");

            Instance.SetDescription(butSetup,
                @"This button opens the Configuring and Setup guide on this SourceSplit fork's Github page, where you can find detailed guides on installing and using this tool.");

            Instance.SetDescription(groupBox6,
                @"The main people behind SourceSplit.

Fatalis is the creator of the original versions of SourceSplit.
2838 is the creator of this fork of SourceSplit, and is also currently its maintainer.");

            Instance.SetDescription(groupBox7,
                @"The people who tested v3.3.0.

Version 3.3.0 represented a massive change of this fork of SourceSplit, introducing many new features and changes to code. This required extensive testing, to ensure timing accuracy, and tool stability.");
        }
    }
}
