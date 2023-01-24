using System;
using System.Collections.Generic;
using System.Diagnostics;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class HL2Ep2 : GameSupport
    {
        // how to match this timing with demos:
        // start: 
        // ending: the tick where velocity changes from 600.X to 0.0 AFTER the camera effects (cl_showpos 1)

        private int _basePlayerLaggedMovementOffset = -1;
        private float _prevLaggedMovementValue;

        public HL2Ep2()
        {
            this.AddFirstMap("ep2_outland_01");
            this.AddLastMap("ep2_outland_12a");
            AdditionalGameSupport = new List<GameSupport>()
            {
                new HL2Mods.DarkIntervention(),
                new HL2Mods.HellsMines(),
                new HL2Mods.UpmineStruggle(),
                new HL2Mods.City17IsFarAway(),
                new HL2Mods.Uzvara(),
                new HL2Mods.A2BTrajectory(),
                new HL2Mods.MountainCaves()
            };
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            GameMemory.GetBaseEntityMemberOffset("m_flLaggedMovementValue", state, state.GameEngine.ServerModule, out _basePlayerLaggedMovementOffset);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (state.PlayerEntInfo.EntityPtr != IntPtr.Zero && _basePlayerLaggedMovementOffset != -1)
                state.GameProcess.ReadValue(state.PlayerEntInfo.EntityPtr + _basePlayerLaggedMovementOffset, out _prevLaggedMovementValue);
        }

        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsFirstMap && state.PlayerEntInfo.EntityPtr != IntPtr.Zero && _basePlayerLaggedMovementOffset != -1)
            {
                // "OnMapSpawn" "startcar_speedmod,ModifySpeed,0,0,-1"
                // "OnMapSpawn" "startcar_speedmod,ModifySpeed,1,12.5,-1"

                float laggedMovementValue;
                state.GameProcess.ReadValue(state.PlayerEntInfo.EntityPtr + _basePlayerLaggedMovementOffset, out laggedMovementValue);

                if (laggedMovementValue.BitEquals(1.0f) && !_prevLaggedMovementValue.BitEquals(1.0f))
                {
                    Debug.WriteLine("ep2 start");
                    OnceFlag = true;
                    actions.Start(StartOffsetMilliseconds); return;
                }

                _prevLaggedMovementValue = laggedMovementValue;
            }
            else if (this.IsLastMap)
            {
                // "OnTrigger4" "cvehicle.hangar,EnterVehicle,,0,1"

                if (state.PlayerParentEntityHandle.Current != -1
                    && state.PlayerParentEntityHandle.Old == -1)
                {
                    Debug.WriteLine("ep2 end");
                    OnceFlag = true;
                    actions.End(); 
                }
            }

            return;
        }
    }
}
