using LiveSplit.SourceSplit.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LiveSplit.SourceSplit.GameSpecific;
using LiveSplit.SourceSplit.GameSpecific.BMSMods;
using LiveSplit.SourceSplit.GameSpecific.HL2Mods;
using LiveSplit.SourceSplit.GameSpecific.PortalMods;
using LiveSplit.SourceSplit.ComponentHandling;
using System.Runtime.InteropServices.WindowsRuntime;

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
                    return new TheFlashVersion();
                case "bms":
                    return new BMSRetail();
                case "lostcoast":
                    return new LostCoast();
                case "estrangedact1":
                    return new EstrangedAct1();
                case "ptsd":
                    return new Ptsd1();
                case "missionimprobable":
                    return new MImp();
                case "downfall":
                    return new Downfall();
                case "uncertaintyprinciple":
                    return new UncertaintyPrinciple();
                case "watchingpaintdry":
                case "watchingpaintdry2":
                    return new WatchingPaintDry();
                case "mod_episodic":
                    return new SnipersEp();
                case "deepdown":
                    return new DeepDown();
                case "dank_memes":
                    return new DankMemes();
                case "freakman":
                    return new Freakman1();
                case "freakman-kleinerlife":
                    return new Freakman2();
                case "crates":
                    return new TooManyCrates();
                case "te120":
                    return new TE120();
                case "dear esther":
                    return new DearEsther();
                case "exit 2":
                    return new Exit2();
                case "dayhard":
                    return new DayHard();
                case "thestanleyparable":
                case "thestanleyparabledemo":
                    return new TheStanleyParable();
                case "hdtf":
                    return new HDTF();
                case "beginnersguide":
                    return new TheBeginnersGuide();
                case "icemod":
                    return new ICE();
                case "dababy":
                    return new DaBaby();
                case "infra":
                    return new Infra();
                case "yearlongalarm":
                    return new YearLongAlarm();
                case "killthemonk":
                    return new KillTheMonk();
                case "logistique":
                    return new Logistique();
                case "hl1":
                    return new HLS();
                case "backwardsmod":
                    return new BackwardsMod();
                case "school_adventures":
                case "school_adventures_oe":
                case "school_adventures oe":
                    return new SchoolAdventures();
                case "the lost city":
                case "thelostcity":
                    return new TheLostCity();
                case "entropyzero":
                    return new EntropyZero();
                case "entropyzero2":
                    return new EntropyZero2();
                case "deeperdown":
                    return new DeeperDown();
                case "thinktank":
                    return new ThinkTank();
                case "gnome":
                    return new Gnome();
                case "hl2-sp-reject":
                    return new Reject();
                case "thc16-trapville":
                    return new TrapVille();
                case "runthinkshootliveville":
                    return new RTSLVille();
                case "abridged":
                    return new Abridged();
                case "episodeone":
                    return new EpisodeOne();
                case "combinationville":
                    return new CombinationVille();
                case "sdk-2013-sp-tlc18-c4-phaseville":
                    return new PhaseVille();
                case "companionpiece":
                    return new CompanionPiece();
                case "the citizen":
                case "thecitizen":
                    return new TheCitizen();
                case "the citizen 2":
                case "thecitizen2":
                case "the citizen returns":
                case "thecitizenreturns":
                    return new TheCitizen2AndReturns();
                case "1187":
                    return new ElevenEightySevenEp1();
                case "prospekt":
                    return new Prospekt();
                case "t7":
                    return new Terminal7();
                case "get_a_life":
                    return new GetALife();
                case "grey":
                    return new Grey();
                case "precursor":
                    return new Precursor();
                case "portalreverse":
                    return new PRMO();
                case "stillalive":
                case "portal-stillalive":
                    return new StillAlive();
                case "ggefc13":
                    return new GGEFC13();
                case "rexaura":
                    return new Rexaura();
                case "pcborrr":
                    return new PCBORRR();
                case "portal pro":
                case "portalpro":
                    return new PortalPro();
                case "portal prelude":
                case "portalprelude":
                    return new PortalPrelude();
                case "ptsd_2":
                    return new Ptsd2();
                case "survivor":
                    return new HL2Survivor();
                case "offshore":
                    return new Offshore();
                case "hangover":
                    return new Hangover();
                case "synergy":
                    return new Synergy();
                case "portal epic edition":
                    return new EpicEdition();
                case "error":
                    return new ERROR();
                case "dangerousworld":
                    return new DangerousWorld();
                case "se1":
                    return new SiNEpisodes();
                case "southernmost":
                case "southernmostcombine":
                    return new SouthernmostCombine();
                case "veryhardmod":
                    return new VeryHardMod();
                case "localmotive":
                    return new Localmotive();
                case "crumbsoftruth":
                    return new CrumbsOfTruth();
                case "darkevening": 
                    return new DarkEvening();
                case "halflife2-episode3":
                    return new EpisodeThree();
                case "foresttrain":
                    return new ForestTrain();
                case "metastasis":
                    return new Minerva();
                case "evacuation":
                    return new Evacuation();
            }

            return new DefaultGame();
        }
    }

    class DefaultGame : GameSupport
    {

    }
}
