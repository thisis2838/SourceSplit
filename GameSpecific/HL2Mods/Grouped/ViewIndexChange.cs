/*
 *  this file contains games / mods which follow similar patterns for auto start and stopping:
 *  start and/or end on view entity switching from and/or to a camera entity with a specific name
 */

using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class ViewIndexChange : GameSupport
    {
        private int _startViewIndex = -1;
        private int _endViewIndex = -1;

        protected string StartCameraName = null;
        protected string EndCameraName = null;

        public ViewIndexChange(string firstMap, string lastMap)
        {
            if (firstMap != null) AddFirstMap(firstMap);
            if (lastMap != null) AddLastMap(lastMap);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap)
            {
                _startViewIndex = state.GameEngine.GetEntIndexByName(StartCameraName);
            }

            if (IsLastMap)
            {
                _endViewIndex = state.GameEngine.GetEntIndexByName(EndCameraName);
            }
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (IsFirstMap && _startViewIndex != -1)
            {
                if (state.PlayerViewEntityIndex.ChangedFromTo(_startViewIndex, 1))
                {
                    actions.Start(StartOffsetMilliseconds);
                    Debug.WriteLine($"{this.GetType().Name} start");
                }
            }
            if (IsLastMap && _endViewIndex != -1)
            { 
                if (state.PlayerViewEntityIndex.ChangedFromTo(1, _endViewIndex))
                {
                    actions.End(EndOffsetMilliseconds);
                    Debug.WriteLine($"{this.GetType().Name} end");
                }
            }
        }
    }

    class City17IsFarAway : ViewIndexChange
    {
        public City17IsFarAway() : base("station", "finale")
        {
            StartCameraName = "start_camera";
            EndCameraName = "game_end_point_view";
        }
    }

    class DaBaby : ViewIndexChange
    {
        public DaBaby() : base("dababy_hallway_ai", "dababy_hallway_ai")
        {
            StartCameraName = "viewcontrol";
            EndCameraName = "final_viewcontrol";
        }
    }

    class EpisodeThree : ViewIndexChange
    {
        public EpisodeThree() : base("01_spymap_ep3", "35_spymap_ep3")
        {
            StartCameraName = "camera10";
            EndCameraName = "camera1a";
        }
    }
    
    class Exit2 : ViewIndexChange
    {
        public Exit2() : base("e2_01", "e2_07")
        {
            StartCameraName = "view";
            EndCameraName = "view";
        }
    }

    class GetALife : ViewIndexChange
    {
        public GetALife() : base("boulevard", "labo2")
        {
            StartCameraName = "point_viewcontrolintro";
            EndCameraName = "point_viewcontrol_finboss1";
        }
    }

    class Grey : ViewIndexChange
    {
        public Grey() : base("map0", "map11")
        {
            StartCameraName = "asd2";
            EndCameraName = "camz1";
        }
    }

    class Precursor : ViewIndexChange
    {
        public Precursor() : base("r_map1", "r_map7")
        {
            StartCameraName = "camera2_camera";
            EndCameraName = "end_lockplayer";
        }
    }

    class SchoolAdventures : ViewIndexChange
    {
        public SchoolAdventures() : base("sa_01", "sa_04")
        {
            StartOnFirstLoadMaps.Add("sa_01");
            EndCameraName = "viewcontrol_credits";
        }
    }

    class Sebastian : ViewIndexChange
    {
        public Sebastian() : base("sebastian_1_1", "sebastian_2_1")
        {
            StartCameraName = "viewcon";
            EndCameraName = "viewcon";
        }
    }

    class SouthernmostCombine : ViewIndexChange
    {
        public SouthernmostCombine() : base("smc_town01", "smc_powerplant03")
        {
            StartCameraName = "cam";
            EndCameraName = "view_gman";
        }
    }
}
