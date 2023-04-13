using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific.HL2Mods
{
    class PersonOfInterest : GameSupport
    {
        // start:   on first map
        // end:     when the output to begin the outro credits is fired

        public PersonOfInterest()
        {
            AddFirstMap("poi_map1");
            StartOnFirstLoadMaps.AddRange(FirstMaps);
            AddLastMap("hl2poi_map4");

            WhenOutputIsFired(ActionType.AutoEnd, "endgame_credits", "rolloutrocredits");
        }
    }
}
