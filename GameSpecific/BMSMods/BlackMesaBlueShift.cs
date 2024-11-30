namespace LiveSplit.SourceSplit.GameSpecific.BMSMods;

class BlackMesaBlueShift : BMSBase
{
    public BlackMesaBlueShift()
    {
        // Insecurity: first map
        // Could be changed to Duty Calls instead if we decide cutscenes
        // are too boring but probably not.
        this.AddFirstMap("bs_c1m0a");
        // Focal Point: last map
        // TODO: Change this as more chapters are released.
        this.AddLastMap("bs_c4m2c");

        this.StartOnFirstLoadMaps.AddRange(this.FirstMaps);

        // End when player enters the portal in bs_c4m2c.
        // TODO: Remove this as more chapters are released.
        this.WhenOutputIsQueued(ActionType.AutoEnd, "interdim_shaft_rel");
    }
}
