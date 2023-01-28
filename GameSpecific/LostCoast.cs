using System;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Resources;
using System.Windows.Forms.VisualStyles;
using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameHandling;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameSpecific
{
    class LostCoast : GameSupport
    {
        // start: when the input to kill the blackout entity's parent is fired
        // ending: when the final output is fired by the trigger_once

        private ValueWatcher<float> _splitTime = new ValueWatcher<float>();
        private ValueWatcher<float> _splitTime2 = new ValueWatcher<float>();

        private CustomCommand _extraSplits = new CustomCommand
        (
            "extra_splits_any", "0",
            "Enables extra split points for the current Any%% route:\n\t" +
                "- Split when firing the RPG\n\t" +
                "- Split when getting enough vertical speed to reach to the same height as the trigger (not a guarantee of reaching the trigger, as you can lose speed along the way or fly in the wrong direction)"
        );

        private bool _onceRpgFire = false;
        private int _activeWeaponOffset = -1;
        private int _weaponAmmoTypeOffset = -1;
        private int _weaponPrimaryAttacksOffset = 0x558;
        private const int _rpgAmmoType = 8;
        private MemoryWatcher<int> _activeWeaponIndex;
        private ValueWatcher<int> _rpgFireCount = new ValueWatcher<int>(0);
        private IntPtr _rpgPtr = IntPtr.Zero;

        private bool _onceVelocity = false;
        private int _entityVelOffset = -1;
        private MemoryWatcher<Vector3f> _playerVelocity;
        private const float _triggerUndersideHeight = 2521.26f - 10f - 72f; // should be -20f but we'll assume you'd wanna be at least 10 units into the trigger for a good chance...

        public LostCoast()
        {
            this.AddFirstMap("hdrtest"); //beta%
            this.AddLastMap("d2_lostcoast");

            CommandHandler.Commands.Add(_extraSplits);
        }

        protected override void OnGameAttachedInternal(GameState state, TimerActions actions)
        {
            var server = state.GetModule("server.dll");
            var scanner = new SignatureScanner(state.GameProcess, server.BaseAddress, server.ModuleMemorySize);
            GameMemory.GetBaseEntityMemberOffset("m_hActiveWeapon", state.GameProcess, scanner, out _activeWeaponOffset);
            //GameMemory.GetBaseEntityMemberOffset("m_iPrimaryAttacks", state.GameProcess, scanner, out _weaponPrimaryAttacksOffset);
            GameMemory.GetBaseEntityMemberOffset("m_iPrimaryAmmoType", state.GameProcess, scanner, out _weaponAmmoTypeOffset);
            GameMemory.GetBaseEntityMemberOffset("m_vecVelocity", state.GameProcess, scanner, out _entityVelOffset);
        }

        protected override void OnSessionStartInternal(GameState state, TimerActions actions)
        {
            _rpgPtr = IntPtr.Zero;
            _rpgFireCount.Current = 0;

            if (state.PlayerEntInfo.EntityPtr != IntPtr.Zero)
            {
                _splitTime.Current = state.GameEngine.GetOutputFireTime("blackout", "Kill", "");
                _splitTime2.Current = state.GameEngine.GetOutputFireTime("csystem_sound_start", "PlaySound", "");

                if (_activeWeaponOffset != -1)
                {
                    _activeWeaponIndex = new MemoryWatcher<int>(state.PlayerEntInfo.EntityPtr + _activeWeaponOffset);
                    _activeWeaponIndex.OnChanged += (s, e) =>
                    {
                        var ind = _activeWeaponIndex.Current & 0xfff;
                        var ptr = state.GameEngine.GetEntityByIndex(ind);
                        var type = state.GameProcess.ReadValue<int>(ptr + _weaponAmmoTypeOffset);
                        Debug.WriteLine
                        (
                            $"Index is {ind}, " +
                            $"ptr is {ptr.ToInt32():X}, ",
                            $"ammo type is {type}, "
                        );

                        if (type == _rpgAmmoType)
                            _rpgPtr = ptr;
                    };
                }

                if (_entityVelOffset != -1)
                    _playerVelocity = new MemoryWatcher<Vector3f>(state.PlayerEntInfo.EntityPtr + _entityVelOffset);
            }
        }

        protected override void OnTimerResetInternal(bool resetFlagTo)
        {
            _onceVelocity = _onceRpgFire = false;
        }


        protected override void OnUpdateInternal(GameState state, TimerActions actions)
        {
            if (_activeWeaponOffset != -1 && _weaponAmmoTypeOffset != -1)
            {
                _activeWeaponIndex?.Update(state.GameProcess);
                if (_rpgPtr != IntPtr.Zero) _rpgFireCount.Current = state.GameProcess.ReadValue<int>(_rpgPtr + _weaponPrimaryAttacksOffset);
                else _rpgFireCount.Current = 0;
                if (_extraSplits.Boolean && !_onceRpgFire)
                {
                    if (_rpgFireCount.Current - _rpgFireCount.Old == 1)
                    {
                        _onceRpgFire = true;
                        Debug.WriteLine($"lostcoast extra - rpg firing");
                        actions.Split();
                    }
                }
            }

            if (_entityVelOffset != -1)
            {
                _playerVelocity?.Update(state.GameProcess);
                var vel = _playerVelocity.Current.Z;
                var height = state.PlayerPosition.Current.Z;
                var hDelta = _triggerUndersideHeight - height;
                var projectedHeight = vel * vel / (2 * 600f);
                bool reachable = (projectedHeight >= hDelta) && (hDelta <= 0 || vel >= 0);

                if (reachable && _extraSplits.Boolean && !_onceVelocity && _onceRpgFire)
                {
                    _onceVelocity = true;
                    Debug.WriteLine($"lostcoast extra - up to speed");
                    actions.Split();
                }
            }

            if (OnceFlag)
                return;

            _splitTime.Current = state.GameEngine.GetOutputFireTime("blackout", "Kill", "");
            if (_splitTime.ChangedFrom(0))
            {
                Debug.WriteLine("lostcoast start");
                // no once flag because the end wont trigger otherwise
                actions.Start(StartOffsetMilliseconds); 
            }

            _splitTime2.Current = state.GameEngine.GetOutputFireTime("csystem_sound_start", "PlaySound", "");
            if (_splitTime2.ChangedFrom(0))
            {
                Debug.WriteLine("lostcoast end");
                OnceFlag = true;
                actions.End(EndOffsetMilliseconds);
            }

            return;
        }
    }
}
