using LiveSplit.SourceSplit.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LiveSplit.SourceSplit.GameSpecific;
using LiveSplit.SourceSplit.ComponentHandling;

namespace LiveSplit.SourceSplit.GameHandling
{
    abstract partial class GameSupport
    {
        // TODO: redesign this to utilize more info like gameinfo.txt and existing files
        /// <summary>
        /// Get the game-specific code from specified game directory
        /// </summary>
        public static GameSupport FromGameDir(string gameDir)
        {
            switch (gameDir.ToLower().Trim())
            {
                case "hl2oe":
                case "hl2":
                case "ghosting":
                case "ghostingmod":
                case "ghostingmod2": //hl2 category extensions, NOTE: these are only guesses for the folder name
                case "ghostingmod3":
                case "ghostingmod4":
                case "cutsceneless":
                    return new HL2();
                case "episodic":
                    return new HL2Ep1();
                case "ep2":
                    return new HL2Ep2();
                case "portal":
                case "portal_rtx":
                case "portalelevators":
                case "cssmovement":
                    return new Portal();
                case "portal_tfv":
                    return new PortalMods_TheFlashVersion();
                case "portal2":
                    return new Portal2();
                case "aperturetag":
                    return new ApertureTag();
                case "portal_stories":
                    return new PortalStoriesMel();
                case "bms":
                    return new BMSRetail();
                case "lostcoast":
                    return new LostCoast();
                case "estrangedact1":
                    return new EstrangedAct1();
                case "ptsd":
                    return new HL2Mods_Ptsd1();
                case "missionimprobable":
                    return new HL2Mods_MImp();
                case "downfall":
                    return new HL2Mods_Downfall();
                case "uncertaintyprinciple":
                    return new HL2Mods_UncertaintyPrinciple();
                case "watchingpaintdry":
                case "watchingpaintdry2":
                    return new HL2Mods_WatchingPaintDry();
                case "mod_episodic":
                    return new HL2Mods_SnipersEp();
                case "deepdown":
                    return new HL2Mods_DeepDown();
                case "dank_memes":
                    return new HL2Mods_DankMemes();
                case "freakman":
                    return new HL2Mods_Freakman1();
                case "freakman-kleinerlife":
                    return new HL2Mods_Freakman2();
                case "crates":
                    return new HL2Mods_TooManyCrates();
                case "te120":
                    return new TE120();
                case "dear esther":
                    return new HL2Mods_DearEsther();
                case "exit 2":
                    return new HL2Mods_Exit2();
                case "dayhard":
                    return new HL2Mods_DayHard();
                case "thestanleyparable":
                case "thestanleyparabledemo":
                    return new TheStanleyParable();
                case "hdtf":
                    return new HDTF();
                case "beginnersguide":
                    return new TheBeginnersGuide();
                case "icemod":
                    return new HL2Mods_ICE();
                case "dababy":
                    return new HL2Mods_DaBaby();
                case "infra":
                    return new Infra();
                case "yearlongalarm":
                    return new HL2Mods_YearLongAlarm();
                case "killthemonk":
                    return new HL2Mods_KillTheMonk();
                case "logistique":
                    return new HL2Mods_Logistique();
                case "hl1":
                    return new HLS();
                case "backwardsmod":
                    return new HL2Mods_BackwardsMod();
                case "school_adventures":
                case "school_adventures_oe":
                case "school_adventures oe":
                    return new HL2Mods_SchoolAdventures();
                case "the lost city":
                case "thelostcity":
                    return new HL2Mods_TheLostCity();
                case "entropyzero":
                    return new HL2Mods_EntropyZero();
                case "deeperdown":
                    return new HL2Mods_DeeperDown();
                case "thinktank":
                    return new HL2Mods_ThinkTank();
                case "gnome":
                    return new HL2Mods_Gnome();
                case "hl2-sp-reject":
                    return new HL2Mods_Reject();
                case "thc16-trapville":
                    return new HL2Mods_TrapVille();
                case "runthinkshootliveville":
                    return new HL2Mods_RTSLVille();
                case "abridged":
                    return new HL2Mods_Abridged();
                case "episodeone":
                    return new HL2Mods_EpisodeOne();
                case "combinationville":
                    return new HL2Mods_CombinationVille();
                case "sdk-2013-sp-tlc18-c4-phaseville":
                    return new HL2Mods_PhaseVille();
                case "companionpiece":
                    return new HL2Mods_CompanionPiece();
                case "the citizen":
                case "thecitizen":
                    return new HL2Mods_TheCitizen();
                case "the citizen 2":
                case "thecitizen2":
                case "the citizen returns":
                case "thecitizenreturns":
                    return new HL2Mods_TheCitizen2AndReturns();
                case "1187":
                    return new HL2Mods_1187Ep1();
                case "prospekt":
                    return new Prospekt();
                case "t7":
                    return new HL2Mods_Terminal7();
                case "get_a_life":
                    return new HL2Mods_GetALife();
                case "grey":
                    return new HL2Mods_Grey();
                case "precursor":
                    return new HL2Mods_Precursor();
                case "portalreverse":
                    return new PortalMods_PRMO();
                case "stillalive":
                case "portal-stillalive":
                    return new PortalMods_StillAlive();
                case "ggefc13":
                    return new HL2Mods_GGEFC13();
                case "rexaura":
                    return new PortalMods_Rexaura();
                case "pcborrr":
                    return new PortalMods_PCBORRR();
                case "portal pro":
                case "portalpro":
                    return new PortalMods_PortalPro();
                case "portal prelude":
                case "portalprelude":
                    return new PortalMods_PortalPrelude();
                case "ptsd_2":
                    return new HL2Mods_Ptsd2();
                case "survivor":
                    return new HL2Survivor();
                case "offshore":
                    return new HL2Mods_Offshore();
                case "hangover":
                    return new HL2Mods_Hangover();
                case "synergy":
                    return new Synergy();
                case "portal epic edition":
                    return new PortalMods_EpicEdition();
                case "error":
                    return new PortalMods_ERROR();
                case "dangerousworld":
                    return new HL2Mods_DangerousWorld();
                case "se1":
                    return new SiNEpisodes();
                case "southernmost":
                case "southernmostcombine":
                    return new HL2Mods_SouthernmostCombine();
                case "veryhardmod":
                    return new HL2Mods_VeryHardMod();
                case "localmotive":
                    return new HL2Mods_Localmotive();
                case "crumbsoftruth":
                    return new PortalMods_CrumbsOfTruth();
            }

            return new DefaultGame();
        }
    }

    class DefaultGame : GameSupport
    {

    }
}
