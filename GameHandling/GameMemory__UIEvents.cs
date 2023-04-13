using System;
using System.Threading;
using LiveSplit.SourceSplit.Utilities;

namespace LiveSplit.SourceSplit.GameHandling
{
    public class SourceSplitSynchronizationContext
    {
        private SynchronizationContext _sync = null;
        public SourceSplitSynchronizationContext(SynchronizationContext sync)
        {
            _sync = sync;
        }

        public bool Stop = false;
        public void Post(SendOrPostCallback callback)
        {
            if (Stop)
            {
                Logging.WriteLine("Tried to invoke UI thread when it has been closed. Callback: " + callback.Method.Name.ToString());
                return;
            }
            _sync.Post(callback, null);
        }
    }

    partial class GameMemory
    {

        // these functions are ugly but it means we don't have to worry about implicitly captured closures

        public event EventHandler<MapChangedEventArgs> OnMapChanged;
        public class MapChangedEventArgs : EventArgs
        {
            public string Map { get; internal set; }
            public string PrevMap { get; internal set; }
            public bool IsGeneric { get; internal set; }
        }
        public void SendMapChangedEvent(string mapName, string prevMapName, bool isGeneric = false)
        {
            _uiThread.Post(d => 
            {
                this.OnMapChanged?.Invoke(this, new MapChangedEventArgs() 
                { 
                    Map = mapName, 
                    PrevMap = prevMapName, 
                    IsGeneric = isGeneric
                });
            });
        }

        public event EventHandler<SessionTicksUpdateEventArgs> OnSessionTimeUpdate;
        public class SessionTicksUpdateEventArgs : EventArgs
        {
            public long TickDifference { get; internal set; }
        }
        public void SendSessionTimeUpdateEvent(long tickDifference)
        {
            // note: sometimes this takes a few ms
            _uiThread.Post(d => 
            {
                this.OnSessionTimeUpdate?.Invoke(this, new SessionTicksUpdateEventArgs() { TickDifference = tickDifference });
            });
        }

        public event EventHandler<SessionStartedEventArgs> OnSessionStarted;
        public class SessionStartedEventArgs : EventArgs
        {
            public string Map{ get; set; }
        }
        public void SendSessionStartedEvent(string map)
        {
            _uiThread.Post(d => 
            {
                this.OnSessionStarted?.Invoke(this, new SessionStartedEventArgs() { Map = map} );
            });
        }

        public event EventHandler OnSessionEnded;
        public void SendSessionEndedEvent()
        {
            _uiThread.Post(d => 
            {
                this.OnSessionEnded?.Invoke(this, EventArgs.Empty);
            });
        }

        public event EventHandler OnNewGameStarted;
        public void SendNewGameStartedEvent(string map)
        {
            _uiThread.Post(d => 
            {
                this.OnNewGameStarted?.Invoke(this, EventArgs.Empty);
            });
        }

        public event EventHandler<MiscTimeEventArgs> OnMiscTime;
        public class MiscTimeEventArgs : EventArgs
        {
            public long TickDifference { get; internal set; }
            public MiscTimeType Type { get; internal set; }
        }
        public enum MiscTimeType
        {
            StartPause,
            EndPause,
            PauseTime,
            ClientDisconnectTime
        }
        public void SendMiscTimeEvent(long tickDiff, MiscTimeType type)
        {
            _uiThread.Post(d => 
            {
                this.OnMiscTime?.Invoke(this, new MiscTimeEventArgs() { TickDifference = tickDiff, Type = type });
            });
        }

        public event EventHandler<SetTickRateEventArgs> OnSetTickRate;
        public class SetTickRateEventArgs : EventArgs
        {
            public float IntervalPerTick { get; internal set; }
        }
        public void SendSetTickRateEvent(float intervalPerTick)
        {
            _uiThread.Post(d => 
            {
                this.OnSetTickRate?.Invoke(this, new SetTickRateEventArgs() { IntervalPerTick = intervalPerTick });
            });
        }

        public event EventHandler<SetTimingSpecificsEventArgs> OnSetTimingSpecifics;
        public class SetTimingSpecificsEventArgs : EventArgs
        {
            public TimingSpecifics Specifics { get; internal set; }
        }
        public void SendSetTimingSpecificsEvent(TimingSpecifics specifics)
        {
            _uiThread.Post(d => 
            {
                this.OnSetTimingSpecifics?.Invoke(this, new SetTimingSpecificsEventArgs() { Specifics = specifics });
            });
        }

        public event EventHandler<OnGameAttachedEventArgs> OnGameAttached;
        public class OnGameAttachedEventArgs : EventArgs
        {
            public Type GameSupportType { get; internal set; }
        }
        public void SendGameAttachedEvent(Type gameSupportType)
        {
            _uiThread.Post(d => 
            {
                this.OnGameAttached?.Invoke(this, new OnGameAttachedEventArgs() { GameSupportType = gameSupportType });
            });
        }


        public event EventHandler<GameStatusEventArgs> OnGameStatusChanged;
        public class GameStatusEventArgs : EventArgs
        {
            public bool IsActive { get; internal set; }
        }
        public void SendGameStatusEvent(bool isActive)
        {
            _uiThread.Post(d => 
            {
                this.OnGameStatusChanged?.Invoke(this, new GameStatusEventArgs() { IsActive = isActive });
            });
        }

        public event EventHandler<CurrentDemoInfoEvent> OnUpdateCurrentDemoInfoEvent;
        public class CurrentDemoInfoEvent : EventArgs
        {
            public long TickCount { get; internal set; }
            public string Name { get; internal set; }
            public bool IsRecording { get; internal set; }
        }
        public void SendCurrentDemoInfoEvent(long tick, string name, bool rec)
        {
            _uiThread.Post(d => 
            {
                this.OnUpdateCurrentDemoInfoEvent?.Invoke(this, new CurrentDemoInfoEvent() 
                { 
                    TickCount = tick, 
                    Name = name, 
                    IsRecording = rec 
                });
            });
        }
    }
}
