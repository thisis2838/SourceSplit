using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class SimpleEtape : GameSupport
    {
        // start: on map load
        // end: when final fade is triggered

        public SimpleEtape()
        {
            AddFirstMap("Simple_etape");
            AddLastMap("Simple_etape");

            StartOnFirstLoadMaps.AddRange(FirstMaps);

            WhenOutputIsQueued(ActionType.AutoEnd, "game_text03");
        }
    }
}
