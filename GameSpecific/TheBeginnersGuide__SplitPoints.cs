using LiveSplit.ComponentUtil;
using LiveSplit.SourceSplit.GameHandling;

namespace LiveSplit.SourceSplit.GameSpecific
{
    partial class TheBeginnersGuide : GameSupport
    {

        //WHISPER
        private TBGFireTimeWatcher _w_getGunFireTime = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "alarm")
        {
            Map = "whisper",
            Description = "When getting the gun",
        };
        private TBGFireTimeWatcher _w_paRoomFireTime = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "pa2")
        {
            Map = "whisper",
            Description = "When first entering the room with the line 'Security hull breached...'",
        };
        private TBGFireTimeWatcher _w_paRoomDoors = new TBGFireTimeWatcher(OutputFireType.FinishedDelay, "doors_4")
        {
            Map = "whisper",
            Description = "When the doors for the room with the line 'Security hull breached...' opens"
        };
        private TBGFireTimeWatcher _w_mazeRoomExit = new TBGFireTimeWatcher(OutputFireType.FinishedDelay, "doors_7")
        {
            Map = "whisper",
            Description = "When reaching the end of the maze' opens",
        };
        private TBGFireTimeWatcher _w_death = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_9")
        {
            Map = "whisper",
            Description = "When jumping into the beam"
        };

        //BACKWARDS
        private TBGFireTimeWatcher _b_farWallFireTime = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_4")
        {
            Map = "backwards",
            Description = "When toucnhing the wall with 'But the future could not be seen'"
        };
        private TBGEntityDeletion _b_bottomOfStaircase = new TBGEntityDeletion(new Vector3f(1232f, -744f, -20f))
        {
            Map = "backwards",
            Description = "When reaching the bottom of the staircase"
        };
        private TBGFireTimeWatcher _b_topOfStaircase = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_5")
        {
            Map = "backwards",
            Description = "When reaching to the top of the staircase"
        };
        private TBGPlayerViewEntity _b_blackout = new TBGPlayerViewEntity(PlayerViewEntityChangeType.ChangedTo, "cam_black")
        {
            Map = "backwards",
            Description = "When the screen blacks out"
        };

        //ENTERING
        private TBGFireTimeWatcher _en_blackout = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_1")
        {
            Map = "entering",
            Description = "When the screen blacks out"
        };

        //STAIRS
        private TBGFireTimeWatcher _st_finalSlowTrig = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_3")
        {
            Map = "stairs",
            Description = "When the doors open"
        };
        private TBGFireTimeWatcher _st_closeDoor = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_4")
        {
            Map = "stairs",
            Description = "When the door to the room is closed"
        };

        //PUZZLE
        private TBGFireTimeWatcher _pz_reachedDoor = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "puzzlehint")
        {
            Map = "puzzle",
            Description = "When reaching near the first door",
        };
        private TBGFireTimeWatcher _pz_doorClose = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_4")
        {
            Map = "puzzle",
            Description = "When the final door closes"
        };

        //EXITING
        private TBGFireTimeWatcher _ex_ending = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_1")
        {
            Map = "exiting",
            Description = "When reaching the effective end of the map"
        };

        //DOWN
        private TBGEntityDeletion _d_doorOpens = new TBGEntityDeletion(new Vector3f(44f, -4f, 6194f))
        {
            Map = "down",
            Description = "When the door in the cafe opens"
        };
        /*
        private TBGFireTimeWatcher _d_prisonOpens = new TBGFireTimeWatcher(OutputFireType.FinishedDelay, "jail_door_01")
        {
            Map = "down",
            Description = "When the first door of the prison opens"
        };
        */
        private TBGEntityDeletion _d_doorPuzzle = new TBGEntityDeletion(new Vector3f(6872f, 524f, -3840f))
        {
            Map = "down",
            Description = "When exiting the spiral staircase into the door puzzle"
        };
        private TBGFireTimeWatcher _d_talk1 = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_9")
        {
            Map = "down",
            Description = "When starting the first conversation"
        };
        private TBGEntityDeletion _d_talk2 = new TBGEntityDeletion(new Vector3f(8920f, 632f, -4252.79f))
        {
            Map = "down",
            Description = "When starting the second conversation"
        };
        private TBGEntityDeletion _d_spiral = new TBGEntityDeletion(new Vector3f(9302f, 730f, -4262f))
        {
            Map = "down",
            Description = "When entering the final spiral staircase"
        };
        private TBGFireTimeWatcher _d_finalArea = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_11")
        {
            Map = "down",
            Description = "When entering courtyard"
        };

        //NOTES
        private TBGEntityDeletion _n_beginCave = new TBGEntityDeletion(new Vector3f(-2184f, 6596f, 664f))
        {
            Map = "notes",
            Description = "When entering the Cave"
        };
        private TBGEntityDeletion _n_caveSlope = new TBGEntityDeletion(new Vector3f(-1274.94f, 1068.43f, -804f))
        {
            Map = "notes",
            Description = "When exiting beginning section of Cave into rough downward slopes"
        };
        private TBGEntityDeletion _n_painting = new TBGEntityDeletion(new Vector3f(-1368f, -2824f, -1509.71f))
        {
            Map = "notes",
            Description = "When entering painting area (where whispering sounds begin playing)"
        };
        private TBGEntityDeletion _n_finalDescent = new TBGEntityDeletion(new Vector3f(3231.36f, -2278.62f, -1616f))
        {
            Map = "notes",
            Description = "When entering final spiral path (where whispering sounds stop playing)"
        };
        private TBGFireTimeWatcher _n_finalArea = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "Crowd")
        {
            Map = "notes",
            Description = "When the door into the typewriter's room closes"
        };

        //ESCAPE
        private TBGPlayerViewEntity _e_blank = new TBGPlayerViewEntity(PlayerViewEntityChangeType.ChangedFrom, "cam_black")
        {
            Map = "escape",
            Description = "At the beginning of each section (when screen immediately transitions from black)"
        };
        private TBGFireTimeWatcher _e_intoWell = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_2")
        {
            Map = "escape",
            Description = "When reaching the well"
        };
        private TBGEntityState _e_tutTable = new TBGEntityState("tut_msg01", TBGEntityStateChangeType.Disabled, _pointMsgEnabledOffset)
        {
            Map = "escape",
            Description = "Tutorial, when clicking the table"
        };
        private TBGEntityState _e_tutPainting = new TBGEntityState("tut_msg02", TBGEntityStateChangeType.Disabled, _pointMsgEnabledOffset)
        {
            Map = "escape",
            Description = "Tutorial, when rotating the picture"
        };
        private TBGEntityState _e_tutLight = new TBGEntityState("tut_msg03", TBGEntityStateChangeType.Disabled, _pointMsgEnabledOffset)
        {
            Map = "escape",
            Description = "Tutorial, when turning the light off and on"
        };
        private TBGEntityState _e_tutSofa = new TBGEntityState("tut_msg04", TBGEntityStateChangeType.Disabled, _pointMsgEnabledOffset)
        {
            Map = "escape",
            Description = "Tutorial, when moving the sofa"
        };
        private TBGEntityState _e_tutShelf = new TBGEntityState("tut_msg05", TBGEntityStateChangeType.Disabled, _pointMsgEnabledOffset)
        {
            Map = "escape",
            Description = "Tutorial, when touching the shelves"
        };
        private TBGFireTimeWatcher _e_tutReturn = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_5")
        {
            Map = "escape",
            Description = "Tutorial, when returning to the start"
        };
        private TBGPlayerViewEntity _e_phoneCam = new TBGPlayerViewEntity(PlayerViewEntityChangeType.ChangedTo, "phonecam")
        {
            Map = "escape",
            Description = "When entering the phone booth at the end"
        };

        //HOUSE
        private TBGFireTimeWatcher _h_enter = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "music_inside", "FadeIn", "25")
        {
            Map = "house",
            Description = "When entering the house"
        };
        private TBGFireTimeWatcher _h_chores1Begin = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "cmd", "Command", "author_run trees/cleaning/*")
        {
            Map = "house",
            Description = "When finishing any of the chores"
        };
        private TBGFireTimeWatcher _h_houseDestroy = new TBGFireTimeWatcher(OutputFireType.FinishedDelay, "dots")
        {
            Map = "house",
            Description = "When the chore sequences finish and the walls of the house are deleted"
        };
        private TBGFireTimeWatcher _h_end = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_8")
        {
            Map = "house",
            Description = "When the door in the final section closes"
        };

        //THEATER
        private TBGFireTimeWatcher _t_showStart = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "cmd", "Command", "author_run trees/theater1.txt")
        {
            Map = "theater",
            Description = "When the show begins"
        };
        private TBGFireTimeWatcher _t_hallwayOpen = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "bumper22", "Open", "")
        {
            Map = "theater",
            Description = "When the curtain disappears to show the hallway"
        };
        private TBGFireTimeWatcher _t_end = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_2")
        {
            Map = "theater",
            Description = "When hitting the final trigger"
        };

        //MOBIUS
        private TBGFireTimeWatcher _mo_endTrigger = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_3")
        {
            Map = "mobius",
            Description = "When confessing the first time"
        };

        //PRESENCE (ISLANDS)
        private TBGFireTimeWatcher _i_initialFadeIn = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "island1_particlefog", "Stop", "")
        {
            Map = "presence",
            Description = "When triggering the first fade"
        };
        private TBGFireTimeWatcher _i_fadeIn = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "*a_particlefog", "Stop", "")
        {
            Map = "presence",
            Description = "When triggering a fade into a new island"
        };
        private TBGEntityDeletion _i_reachDoorPuzzle = new TBGEntityDeletion(new Vector3f(-194.5f, 1220.5f, 510.5f))
        {
            Map = "presence",
            Description = "When beginning door puzzle conversation"
        };
        private TBGFireTimeWatcher _i_reachTextRoom = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "whyisthisaportal")
        {
            Map = "presence",
            Description = "When the door in the final section closes"
        };
        private TBGPlayerViewEntity _i_reachPrison = new TBGPlayerViewEntity(PlayerViewEntityChangeType.ChangedTo, "cam_black")
        {
            Map = "presence",
            Description = "When the screen switches to black at the end"
        };

        //MACHINE
        private TBGFireTimeWatcher _m_machineDiag1 = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_1")
        {
            Map = "machine",
            Description = "When beginning negotiating with the machine"
        };
        private TBGFireTimeWatcher _m_crowd = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "cmd", "Command", "author_run trees/machine22.txt")
        {
            Map = "machine",
            Description = "When beginning speech to the crowd"
        };
        private TBGFireTimeWatcher _m_theatre = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "theatergodlight", "TurnOn", "")
        {
            Map = "machine",
            Description = "When entering the theater"
        };
        private TBGFireTimeWatcher _m_typewriter = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_3")
        {
            Map = "machine",
            Description = "When teleported to typewriters"
        };
        private TBGFireTimeWatcher _m_fall = new TBGFireTimeWatcher(OutputFireType.FinishedDelay, "to_stairs", "Trigger", "")
        {
            Map = "machine",
            Description = "When teleported to the room from Stairs"
        };
        private TBGFireTimeWatcher _m_fallen = new TBGFireTimeWatcher(OutputFireType.FinishedDelay, "feet", "PlaySound", "")
        {
            Map = "machine",
            Description = "When reaching the bottom of the fall"
        };

        //TOWER
        private TBGFireTimeWatcher _tow_mazeTouch = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_3")
        {
            Map = "tower",
            Description = "When failing the maze for the first time"
        };
        private TBGEntityDeletion _tow_mazeFinish = new TBGEntityDeletion(new Vector3f(1509.55f, -69.5f, -1776f))
        {
            Map = "tower",
            Description = "When finishing the maze / crossing the bridge"
        };
        private TBGFireTimeWatcher _tow_codeWheel = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_5")
        {
            Map = "tower",
            Description = "When reaching the code wheels"
        };
        private TBGFireTimeWatcher _tow_drop = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_6")
        {
            Map = "tower",
            Description = "When dropping into impossible room"
        };
        private TBGEntityDeletion _tow_ascent1 = new TBGEntityDeletion(new Vector3f(2880f, 155f, 90f))
        {
            Map = "tower",
            Description = "When beginning ascent after exiting impossible room"
        };
        private TBGEntityDeletion _tow_ascent2 = new TBGEntityDeletion(new Vector3f(1914.45f, 593.13f, 352))
        {
            Map = "tower",
            Description = "During ascent, on the ledge overlooking the maze"
        };
        private TBGFireTimeWatcher _tow_ascent3 = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_9")
        {
            Map = "tower",
            Description = "During ascent, in the room with the split, half-raised walkways"
        };
        private TBGFireTimeWatcher _tow_ascent4 = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_10")
        {
            Map = "tower",
            Description = "During ascent, heading toward final staircase"
        };
        private TBGFireTimeWatcher _tow_ltrRooms1 = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_11")
        {
            Map = "tower",
            Description = "Entering letter rooms"
        };
        private TBGFireTimeWatcher _tow_ltrRooms2 = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_12")
        {
            Map = "tower",
            Description = "In letter rooms, after first curved hallway"
        };
        private TBGFireTimeWatcher _tow_ltrRooms3 = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_13")
        {
            Map = "tower",
            Description = "In letter rooms, after 2nd curved hallway"
        };
        private TBGFireTimeWatcher _tow_ltrRooms4 = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_14")
        {
            Map = "tower",
            Description = "In letter rooms, after 3rd curved hallway"
        };
        private TBGEntityDeletion _tow_ltrRooms5 = new TBGEntityDeletion(new Vector3f(3832f, -2608f, 8186))
        {
            Map = "tower",
            Description = "In floating letter room, walking past 'I literally do not have it' text"
        };
        private TBGEntityDeletion _tow_ltrRooms6 = new TBGEntityDeletion(new Vector3f(5196f, -3344f, 8184))
        {
            Map = "tower",
            Description = "In brickwalled room, just past 'you're not my problem to solve' text"
        };
        private TBGFireTimeWatcher _tow_finalRoom = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "wall2", "Open", "")
        {
            Map = "tower",
            Description = "Beginning final room's seequence"
        };

        //EPILOGUE
        private TBGEntityDeletion _ep1_enteringStation = new TBGEntityDeletion(new Vector3f(3564f, 0f, -512f))
        {
            Map = "nomansland1",
            Description = "Entering the station"
        };
        private TBGEntityDeletion _ep1_enteringTrain = new TBGEntityDeletion(new Vector3f(2873.57f, 2852.9f, -28f))
        {
            Map = "nomansland1",
            Description = "Entering the traincar"
        };
        private TBGEntityDeletion _ep1_exitingTrain = new TBGEntityDeletion(new Vector3f(1327.94f, 2811.67f, -28))
        {
            Map = "nomansland1",
            Description = "Exiting the traincar"
        };
        private TBGFireTimeWatcher _ep1_enteringMansion = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "bg_2")
        {
            Map = "nomansland1",
            Description = "Entering the mansion"
        };
        private TBGFireTimeWatcher _ep2_exitInnerCave = new TBGFireTimeWatcher(OutputFireType.BeganDelay, "VO_7")
        {
            Map = "nomansland2",
            Description = "Entering cave pathway"
        };
        private TBGEntityDeletion _ep2_exitCavePathway = new TBGEntityDeletion(new Vector3f(-2112f, 10336f, 1192.23f))
        {
            Map = "nomansland2",
            Description = "Entering outdoors section"
        };
        private TBGEntityDeletion _ep2_exitOutdoors = new TBGEntityDeletion(new Vector3f(1152f, 7664f, 1188.17f))
        {
            Map = "nomansland2",
            Description = "Entering drop into elevator room"
        };
        private TBGEntityDeletion _ep2_exitElev = new TBGEntityDeletion(new Vector3f(14536f, 336f, -731.97f))
        {
            Map = "nomansland2",
            Description = "Entering final area"
        };
        private TBGEntityDeletion _ep2_finalTunnel1 = new TBGEntityDeletion(new Vector3f(13848f, 4336f, 0f))
        {
            Map = "nomansland2",
            Description = "Final tunnel, part-way through wide entrance"
        };
        private TBGEntityDeletion _ep2_finalTunnel2 = new TBGEntityDeletion(new Vector3f(13904f, 5573.78f, 62.94f))
        {
            Map = "nomansland2",
            Description = "Final tunnel, entering middle section, where it begins tapering"
        };
        private TBGEntityDeletion _ep2_finalTunnel3 = new TBGEntityDeletion(new Vector3f(13864f, 7183.59f, 25.59f))
        {
            Map = "nomansland2",
            Description = "Final tunnel, entering final section, where it tapers for the final time"
        };
    }
}
