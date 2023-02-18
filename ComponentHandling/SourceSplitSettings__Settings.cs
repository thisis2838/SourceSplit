using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using LiveSplit.SourceSplit.Utilities;
using System.Xml;
using System.Drawing;
using LiveSplit.SourceSplit.ComponentHandling;


namespace LiveSplit.SourceSplit.ComponentHandling
{
    public partial class SourceSplitSettings : UserControl
    {
        private void InitSettings()
        {
            var p = SourceSplitComponent.Settings;

            p.AutoSplitEnabled = new("AutoSplitEnabled", true, chkAutoSplitEnabled);
            p.SplitInterval = new("SplitInterval", 1, dmnSplitInterval);
            p.UseSplitInterval = new("UseSplitInterval", true, chkUseInterval);
            p.UseMTL = new("UseMTL", false, chkUseMTL);
            p.MTLModeSetting = new("MTLModeSetting", MTLMode.Disallow, cmbMTLMode);
            
            p.AutoSplitOnLevelTrans = new("AutoSplitOnLevelTrans", true, chkSplitLevelTrans);
            p.AutoSplitOnGenericMap = new("AutoSplitOnGenericMap", false, chkSplitGenericMap);
            p.AutoSplitOnSpecial = new("AutoSplitOnSpecial", true, chkSplitSpecial);
            
            p.AutoStartEnabled = new("AutoStartEnabled", true, chkAutoStart);
            p.AutoStopEnabled = new("AutoStopEnabled", true, chkAutoStop);
            p.AutoResetEnabled = new("AutoResetEnabled", true, chkAutoReset);
            p.FirstMapAutoReset = new("FirstMapAutoReset", true, chkFirstMapReset);
            p.AllowAdditionalAutoStart = new("AllowAdditionalAutoStart", false, chkAllowAddAutoStart);
            p.AddAutoStartName = new("StartMap", "", boxAddAutoStartName, true);
            p.AddAutoStartType = new("AddAutoStartType", AdditionalAutoStartType.NewGame, cmbAddAutoStartMode);
            
            p.ShowGameTime = new("ShowGameTime", false, chkShowGameTime);
            p.ShowAltTime = new("ShowAltTime", false, chkShowAlt);
            p.ShowTickCount = new("ShowTickCount", false, chkShowTickCount);
            p.ShowCurDemo = new("ShowCurDemo", false, chkShowCurDemo);
            p.GameTimeDecimalPlaces = new("GameTimeDecimalPlaces", 6, nudDecimalPlaces);
            
            p.HoldUntilPause = new("HoldUntilPause", true, chkHoldUntilPause);
            p.RTAStartOffset = new("RTAStartOffset", true, chkRTAStartOffset);
            p.ServerInitialTicks = new("ServerInitialTicks", false, chkServerInitialTicks);
            p.SLPenalty = new("SLPenalty", 0, nudSLPenalty);
            p.SplitInstead = new("SplitInstead", false, boxSplitInstead);
            p.ResetMapTransitions = new("ResetMapTransitions", false, chkResetMapTransitions);
            
            p.CountEngineTicks = new("CountEngineTicks", true, chkCountEngineTicks);
            p.CountPauses = new("CountPauses", true, chkCountPauses);
            p.CountDisconnects = new("CountDisconnects", false, chkCountDisconnect);
            p.CountInactive = new("CountInactive", false, chkNoGamePresent);
            p.CountAutomatic = new("CountAutomatic", false, chkAutomatic);
            p.CountDemoInterop = new("CountDemoInterop", false, chkDemoInterop);
            
            p.PrintDemoInfo = new("PrintDemoInfo", false, chkPrintDemoInfo);
            p.DemoParserPath = new("DemoParserPath", "", boxDemoParserPath);

            p.MapTransitionList = new Settings.SettingUIRepresented<string[][]>
            (
                "MapTransitionList",
                new string[][] { new string[] { } },
                dgvMapTransitions.GetValues,
                (e) =>
                {
                    dgvMapTransitions.Rows.Clear();
                    dgvMapTransitions.SetValues(e);
                },
                dgvMapTransitions,
                (e) => string.Join("|", e.Select(x => string.Join(",", x))),
                (e) =>
                {
                    List<string[]> entries = new List<string[]>();
                    e.Split('|').ToList().ForEach(x => entries.Add(x.Split(',')));
                    return entries.ToArray();
                }
            );

            dgvMapTransitions.CellValueChanged += (s, e) => p.MapTransitionList.UIValueChangedCallback();
            dgvMapTransitions.CurrentCellDirtyStateChanged += (s, e) => p.MapTransitionList.UIValueChangedCallback();
            dgvMapTransitions.CellBeginEdit += (s, e) => p.MapTransitionList.UIValueChangedCallback();
            dgvMapTransitions.CellEndEdit += (s, e) => p.MapTransitionList.UIValueChangedCallback();
            dgvMapTransitions.UserDeletedRow += (s, e) => p.MapTransitionList.UIValueChangedCallback();
            dgvMapTransitions.UserAddedRow += (s, e) => p.MapTransitionList.UIValueChangedCallback();
        }
    }
}
