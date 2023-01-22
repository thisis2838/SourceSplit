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
using System.IO;

namespace LiveSplit.SourceSplit.GameHandling
{
    abstract partial class GameSupport
    {
        // TODO: redesign this to utilize more info like gameinfo.txt and existing files
        /// <summary>
        /// Get the game-specific code from specified game directory
        /// </summary>
        public static GameSupport Get(GameState state)
        {
            switch (state.GameDir.ToLower().Trim())
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
                case "hl2-sp-reject":
                    return new Reject();
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
                case "amalgam":
                    return new Amalgam();
                case "avenueodessa":
                    return new AvenueOdessa();
                case "clonemachine":
                    return new CloneMachine();
                case "dark17":
                    return new Dark17();                
                case "jollyhardcore":
                    return new JollysHardcoreMod();
                case "sebastian":
                    return new Sebastian();
                case "nh":
                case "nh2":
                    return new NightmareHouse2();
                case "aberration":
                    return new Aberration();
                case "awakening":
                    return new Awakening();
                case "call-in":
                    return new CallIn();
                case "combinedestiny":
                    return new CombineDestiny();
                case "daylight":
                    return new Daylight();
                case "expectation":
                    return new Expectation();
                case "depot":
                    return new Depot();
                case "half-life 2 riot act":
                    return new RiotAct();
            }

            var rtsl = IsRTSLMapPack(state);
            if (rtsl != null) return rtsl;

            return new DefaultGame();
        }

        // mapping challenges as hosted and/or published by RunThinkShootLive
        // each entry is given a chapter slot and is required to end with a game disconnect or main menu load (not always followed)
        // they all auto-start on first map and split and auto-end with disconnect
        private static GameSupport IsRTSLMapPack(GameState state)
        {
            switch (state.GameDir.ToLower())
            {
                // rtsl hosted and exclusive competitions
                case "thc16-chasmville":
                case "hl2-ep2-sp-the-72-second-emc":
                case "thc16-trapville":
                case "runthinkshootliveville":
                case "combinationville":
                case "sdk-2013-sp-tlc18-c4-phaseville":

                // map lab competitions
                case "abridged":
                case "episodeone":
                case "companionpiece":
                case "thinktank":
                case "gnome":
                case "escapeville":
                    return new RTSLPack();
            }

            if (File.Exists(Path.Combine(state.AbsoluteGameDir, "maplab.fgd")))
            {
                Debug.WriteLine("maplab fgd file found");
                return new RTSLPack();
            }

            var gameInfoPath = Path.Combine(state.AbsoluteGameDir, "gameinfo.txt");
            if (File.Exists(gameInfoPath))
            {
                var gameinfo = File.ReadAllText(gameInfoPath);
                List<string> targets = new()
                {
                    "mapping challenge", "hammer cup"
                };
                if (targets.Any(x => gameinfo.ToLower().Contains(x)))
                {
                    Debug.WriteLine("gameinfo.txt contains strings related to RTSL map challeneges");
                    return new RTSLPack();
                }
            }

            return null;
        }
    }

    class DefaultGame : GameSupport
    {

    }
}
