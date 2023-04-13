using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class TheLostCity : GameSupport
    {
        // start: on first map
        // ending: when the gunship dies and queues final output

        public TheLostCity()
        {
            this.AddFirstMap("lostcity01");
            this.AddLastMap("lostcity02");
            this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);

            WhenOutputIsQueued(ActionType.AutoEnd, "fade1", "fade");
        }
    }
}
