using System.Diagnostics;
using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.TimeFormatters;
using LiveSplit.UI.Components;
using LiveSplit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml;
using System.Windows.Forms;
using LiveSplit.SourceSplit.GameSpecific;
using static LiveSplit.SourceSplit.GameHandling.GameMemory;
using LiveSplit.SourceSplit.GameHandling;
using System.Reflection;
using LiveSplit.SourceSplit.Utilities;
using LiveSplit.SourceSplit.ComponentHandling;
using System.IO;

namespace LiveSplit.SourceSplit.ComponentHandling
{
    partial class SourceSplitComponent : IComponent
    {
        private void RegisterGameMemoryResponses()
        {
            _gameMemory.OnGameAttached += gameMemory_OnGameAttached;
            _gameMemory.OnSetTickRate += gameMemory_OnSetTickRate;
            _gameMemory.OnSetTimingSpecifics += gameMemory_OnSetTimingSpecifics;
            _gameMemory.OnGameStatusChanged += _gameMemory_OnGameStatusChanged;

            _gameMemory.OnSessionStarted += gameMemory_OnSessionStarted;
            _gameMemory.OnSessionEnded += gameMemory_OnSessionEnded;

            _gameMemory.OnSessionTimeUpdate += gameMemory_OnSessionTimeUpdate;
            _gameMemory.OnMiscTime += gameMemory_OnMiscTime;
            _gameMemory.OnUpdateCurrentDemoInfoEvent += _gameMemory_OnUpdateCurrentDemoInfoEvent;

            _gameMemory.OnMapChanged += gameMemory_OnMapChanged;
            _gameMemory.OnNewGameStarted += gameMemory_OnNewGameStarted;

            _gameMemory.TimerActions.OnPlayerGainedControl += gameMemory_OnPlayerGainedControl;
            _gameMemory.TimerActions.OnPlayerLostControl += gameMemory_OnPlayerLostControl;
            _gameMemory.TimerActions.ManualSplit += gameMemory_ManualSplit;
        }

        private void gameMemory_OnGameAttached(object sender, OnGameAttachedEventArgs e)
        {
            SettingControl.SetCurrentGame(e.GameSupportType);
        }
        void gameMemory_OnSetTickRate(object sender, SetTickRateEventArgs e)
        {
            // if we keep the ticks from the previous session, a tick rate change may happen due to 
            // different game versions, causing the existing ticks to produce the wrong time
            // so store this time and reset the total tick count
            Debug.WriteLine("tickrate " + e.IntervalPerTick);
            Debug.WriteLine($"cumulative time = {TimeSpan.FromTicks(_cumulativeTime)} + {TimeSpan.FromTicks(GameTime.Ticks - _cumulativeTime)}");
            _cumulativeTime += GameTime.Ticks - _cumulativeTime;

            _inactiveTime = TimeSpan.Zero;
            _disconnectTime = TimeSpan.Zero;
            _miscTimeOffset = TimeSpan.Zero;
            _sessions.Clear();

            _intervalPerTick = e.IntervalPerTick;
        }
        void gameMemory_OnSetTimingSpecifics(object sender, SetTimingSpecificsEventArgs e)
        {
            _timingSpecifics = e.Specifics;
        }
        private void _gameMemory_OnGameStatusChanged(object sender, GameStatusEventArgs e)
        {
            _addInactiveTime = !e.IsActive;
        }


        // called when player is fully in game
        void gameMemory_OnSessionTimeUpdate(object sender, SessionTicksUpdateEventArgs e)
        {
            if (e.TickDifference < 0)
                Debug.WriteLine($"session update tick difference under 0!! ({e.TickDifference})");
            else if (e.TickDifference == 0)
                return;

            if (!_countEngineTicks)
                return;

            if (_sessions.Current != null)
            {
                _sessions.Current.ActiveTicks += e.TickDifference;
                if (_sessions.Current.Ended)
                    Debug.WriteLine($"adding {e.TickDifference} active ticks to session that's ended!");
            }

            _holdingFirstPause = false;
        }
        void gameMemory_OnMiscTime(object sender, MiscTimeEventArgs e)
        {
            if (e.TickDifference < 0)
                Debug.WriteLine($"misc time update tick difference under 0! ({e.TickDifference})");
            else if (e.TickDifference == 0)
                return;

            switch (e.Type)
            {
                case MiscTimeType.PauseTime:
                    {
                        if (!_countPauses || _holdingFirstPause)
                            return;

                        if (_sessions.Current != null)
                        {
                            _sessions.Current.PausedTicks += e.TickDifference;
                            if (_sessions.Current.Ended)
                                Debug.WriteLine($"adding {e.TickDifference} paused ticks to session that's ended!");
                        }
                        break;
                    }
                case MiscTimeType.ClientDisconnectTime:
                    {
                        if (!_countDisconnects)
                            return;

                        _disconnectTime += TicksToTime(e.TickDifference);
                        break;
                    }
                case MiscTimeType.EndPause:
                    _holdingFirstPause = false;
                    break;
            }
        }
        private void _gameMemory_OnUpdateCurrentDemoInfoEvent(object sender, CurrentDemoInfoEvent e)
        {
            if (!Settings.ShowCurDemo.Value)
                return;

            // pad at most 4 spaces in front of tick count to align tick counts from 0 to 99999
            // doesn't always line up due to font...
            // todo: check if figure space breaks any font
            _curDemoComponent.SetText($"{Path.GetFileNameWithoutExtension(e.Name)} | {StringUtils.NumberAlign(e.TickCount, 5)}");
            _curDemoComponent.SetName(e.IsRecording ? "Cur. Demo" : "Prev. Demo");
        }

        // called immediately after OnSessionEnded if it was a changelevel
        void gameMemory_OnMapChanged(object sender, MapChangedEventArgs e)
        {
            if (e.Map == e.PrevMap) return;
            if (e.Map == "" || e.PrevMap == "") return;

            Debug.WriteLine("gameMemory_OnMapChanged " + e.Map + " " + e.PrevMap);

            if (!(Settings.AutoSplitOnLevelTrans.Value && !e.IsGeneric))
                if (!(Settings.AutoSplitOnGenericMap.Value && e.IsGeneric))
                    return;

            if (_timerIsRunning)
            {
                if (Settings.UseMTL.Value)
                {
                    bool included = Settings.MapTransitionList.Value.Count() == 0 ||
                        Settings.MapTransitionList.Value.ToList().Any(x =>
                        {
                            return (x[0] == "*" || x[0] == e.PrevMap) && (x[1] == "*" || x[1] == e.Map);
                        });

                    if ((Settings.MTLModeSetting.Value == MTLMode.Allow && !included)
                        || (Settings.MTLModeSetting.Value == MTLMode.Disallow && included))
                        goto skipadd;
                }

                if (!_splitOperations.ExistsTransition(e.PrevMap, e.Map, SplitType.AutoSplitter))
                    AutoSplit(e.PrevMap, e.Map);
            }

            skipadd:;
            // prevent adding map time twice
        }
        void gameMemory_OnNewGameStarted(object sender, EventArgs e)
        {
            if (!Settings.AutoStartEnabled.Value ||
                !Settings.FirstMapAutoReset.Value)
                return;

            if (_timer.CurrentState.CurrentPhase == TimerPhase.Running &&
                !Settings.AutoResetEnabled.Value)
                return;

            _timer.Reset();
        }

        // first tick when player is fully in game
        void gameMemory_OnSessionStarted(object sender, SessionStartedEventArgs e)
        {
            _currentMap = e.Map;
            _sessions.Add(new Session(_currentMap, _intervalPerTick));
            _sessions.Current.OffsetTicks = (int)Settings.SLPenalty.Value;
        }


        // player is no longer fully in the game
        void gameMemory_OnSessionEnded(object sender, EventArgs e)
        {
            if (_sessions.Current == null)
                return;

            _sessions.Current.Ended = true;

            Debug.WriteLine($"session ended, total time: {_sessions.Current.TotalTicks} ticks = " +
                $"{TicksToTime(_sessions.Current.TotalTicks)}");
            Debug.WriteLine($"current time: {_sessions.Count} ses. : {TicksToTime(_sessions.TotalTicks)} | {GameTime}");
        }

        void gameMemory_OnPlayerGainedControl(object sender, TimerActionArgs e)
        {
            if (Settings.ResetMapTransitions.Value)
            {
                _splitOperations.Clear();
                _splitCount = 0;
            }

            if (!Settings.AutoStartEnabled.Value)
                return;

            if (_timer.CurrentState.CurrentPhase == TimerPhase.Running)
            {
                if (Settings.SplitInstead.Value)
                {
                    gameMemory_ManualSplit(sender, e);
                    return;
                }
                else if (!Settings.AutoResetEnabled.Value)
                    return;
            }

            _timer.Reset(); // make sure to reset for games that start from a quicksave (Aperture Tag)
            _timer.Start();

            if (Settings.RTAStartOffset.Value)
            {
                var time = TimeSpan.FromMilliseconds(e.MillisecondOffset);
                // can't add for some reason...
                _timer.CurrentState.AdjustedStartTime -= -time;
            }

            if (_sessions.Current != null)
                _sessions.Current.StartTick = _sessions.Current.TotalTicks + MillisecondsToTicks(e.MillisecondOffset);
        }
        void gameMemory_OnPlayerLostControl(object sender, TimerActionArgs e)
        {
            if (!Settings.AutoStopEnabled.Value)
                return;

            if (_sessions.Current != null)
                _sessions.Current.OffsetTicks -= MillisecondsToTicks(e.MillisecondOffset);

            this.DoSplit(MillisecondsToTicks(e.MillisecondOffset), false);
        }
        void gameMemory_ManualSplit(object sender, TimerActionArgs e)
        {
            if (!Settings.AutoSplitEnabled.Value || 
                !Settings.AutoSplitOnSpecial.Value)
                return;

            if (_sessions.Current != null)
                _sessions.Current.OffsetTicks -= MillisecondsToTicks(e.MillisecondOffset);

            this.DoSplit(MillisecondsToTicks(e.MillisecondOffset), true);
        }




    }
}
