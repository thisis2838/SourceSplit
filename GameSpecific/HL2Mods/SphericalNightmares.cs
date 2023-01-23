using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameHandling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class SphericalNightmares : GameSupport
    {
        // start:   when the output to force enter the starting pod is fired
        // end:     when a new game load is made to sn_outro from sn_level04a

        public SphericalNightmares()
        {
            AddFirstMap("sn_level01a");

            WhenEntityIsKilled(ActionType.AutoStart, "anim_gordon_standup");
        }

        protected override bool OnNewGameInternal(GameState state, TimerActions actions, string newMapName)
        {
            if (state.Map.Current == "sn_level04a" && newMapName == "sn_outro")
            {
                actions.End();
                return false;
            }

            return true;
        }
    }
}
