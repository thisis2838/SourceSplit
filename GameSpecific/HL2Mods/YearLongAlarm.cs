using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class YearLongAlarm : GameSupport
    {
        // start: on first map
        // ending: when one of the 2 gunships' HP drops from 0 to lower

        private int _baseEntityHealthOffset = -1;

        // there are 2 gunships, each of them killed will count as a timer end. one of them spawns at level start and is later deleted

        // gunships' old hp
        private int[] _gunshipOldHP = new int[] { -1, 100 };
        // gunships' current hp
        private int[] _gunshipHP = new int[] { -1, 100 };
        // gunships' index, used for searching their pointers
        private int[] _gunshipIndex = new int[] { -1, -1 };
        // gunships' names, used for searching for their indices
        private string[] _gunshipName = new string[] { "gunship", "gunship_intro" };

        public YearLongAlarm()
        {
            this.AddFirstMap("yla_mine");
            this.AddLastMap("yla_bridge");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);
        }
        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            GameMemory.GetBaseEntityMemberOffset("m_iHealth", state, state.GameEngine.ServerModule, out _baseEntityHealthOffset);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            if (IsLastMap && _baseEntityHealthOffset != -1)
            {
                for (int i = 0; i <= 1; i++)
                {
                    // get the gunships' indicies
                    _gunshipIndex[i] = state.GameEngine.GetEntIndexByName(_gunshipName[i]);

                    // and decide their hp
                    if (_gunshipIndex[i] != -1)
                        _gunshipHP[i] = state.GameProcess.ReadValue<int>(state.GameEngine.GetEntityByIndex(_gunshipIndex[i]) + _baseEntityHealthOffset);
                    else 
                        _gunshipHP[i] = -1;

                    Debug.WriteLine(_gunshipName[i] + " index is " + _gunshipIndex[i]);
                }

            }
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (OnceFlag)
                return;

            if (this.IsLastMap)
            {
                // check if the trigger that spawns the 2nd gunship has been triggered, if so, check for its pointer
                if (_gunshipIndex[0] == -1 && state.GameEngine.GetEntIndexByPos(-13.39f, 227.25f, 393.67f) == -1)
                    _gunshipIndex[0] = state.GameEngine.GetEntIndexByName(_gunshipName[0]);

                for (int i = 0; i <= 1; i++)
                {
                    // store the old hp
                    _gunshipOldHP[i] = _gunshipHP[i];
                    // get the gunship's pointer
                    IntPtr ptr = state.GameEngine.GetEntityByIndex(_gunshipIndex[i]);
                    // if the gunship hasn't spawned in yet or they're deleted, exit early and reset its old index
                    if (_gunshipIndex[i] == -1 || ptr == IntPtr.Zero)
                    {
                        _gunshipIndex[i] = -1;
                        continue;
                    }
                    else
                    {
                        // get the new hp
                        _gunshipHP[i] = state.GameProcess.ReadValue<int>(ptr + _baseEntityHealthOffset);
                        // now compare
                        if (_gunshipOldHP[i] > 0 && _gunshipHP[i] <= 0)
                        {
                            Debug.WriteLine("year long alarm end");
                            Debug.WriteLine(_gunshipName[i] + " died at hp " + _gunshipHP[i] + " and old hp " + _gunshipOldHP[i]);
                            OnceFlag = true;
                            actions.End(EndOffsetMilliseconds); return;
                        }
                    }
                }
            }

            return;
        }
    }
}
