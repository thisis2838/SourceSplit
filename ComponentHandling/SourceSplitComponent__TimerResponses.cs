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

namespace LiveSplit.SourceSplit.ComponentHandling
{
    partial class SourceSplitComponent : IComponent
    {
        private void RegisterTimerResponses(LiveSplitState state)
        {
            _timer = new TimerModel { CurrentState = state };

            state.OnUndoSplit += state_OnUndoSplit;
            state.OnSplit += state_OnSplit;
            state.OnReset += state_OnReset;
            state.OnStart += state_OnStart;
        }


        void state_OnStart(object sender, EventArgs e)
        {
            _miscTimeOffset = _timer.CurrentState.TimePausedAt;

            _sessions.Clear();
            _sessions.Add(new Session(_currentMap, _intervalPerTick));
            _sessions.Current.OffsetTicks = (int)Settings.SLPenalty.Value;

            // assume we're holding pause
            _holdingFirstPause = Settings.HoldUntilPause.Value;
            _cumulativeTime = 0;
            _inactiveTime = TimeSpan.Zero;
            _disconnectTime = TimeSpan.Zero;
            _timer.InitializeGameTime();
            _splitOperations.Clear();
            _splitCount = 0;

            TimerOnReset?.Invoke(sender, new TimerResetEventArgs(true));
        }

        void state_OnReset(object sender, TimerPhase t)
        {
            // some game has unspecific starts like if the player's position isn't something which
            // can be repeated easily by accident, so this is a _onceflag but reset on timer reset.
            TimerOnReset?.Invoke(sender, new TimerResetEventArgs(false));
        }

        void state_OnUndoSplit(object sender, EventArgs e)
        {
            _splitOperations.UndoLast();
        }

        void state_OnSplit(object sender, EventArgs e)
        {
            Debug.WriteLine("split at time " + this.GameTime);

            if (_splitOperations.HasJustAutoSplit)
                _splitOperations.HasJustAutoSplit = false;
            else
                _splitOperations.AddSplit(SplitType.User);
        }
    }
}
