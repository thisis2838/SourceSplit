using System.Collections.Generic;

namespace LiveSplit.SourceSplit.ComponentHandling.Settings
{
    public class SettingProvider
    {
        public SettingCheckBox AutoSplitEnabled;
        public SettingNumericUpDown SplitInterval;
        public SettingCheckBox UseSplitInterval;
        public SettingCheckBox UseMTL;
        public SettingComboBoxEnum<MTLMode> MTLModeSetting;

        public SettingCheckBox AutoSplitOnLevelTrans;
        public SettingCheckBox AutoSplitOnGenericMap;
        public SettingCheckBox AutoSplitOnSpecial;

        public SettingCheckBox AutoStartEnabled;
        public SettingCheckBox AutoStopEnabled;
        public SettingCheckBox AutoResetEnabled;
        public SettingCheckBox FirstMapAutoReset;
        public SettingTextBox AddAutoStartName;
        public SettingComboBoxEnum<AdditionalAutoStartType> AddAutoStartType;
        public SettingCheckBox AllowAdditionalAutoStart;

        public SettingCheckBox ShowGameTime;
        public SettingCheckBox ShowAltTime;
        public SettingCheckBox ShowTickCount;
        public SettingCheckBox ShowCurDemo;
        public SettingNumericUpDown GameTimeDecimalPlaces;

        public SettingCheckBox HoldUntilPause;
        public SettingCheckBox RTAStartOffset;
        public SettingCheckBox ServerInitialTicks;
        public SettingNumericUpDown SLPenalty;
        public SettingCheckBox SplitInstead;
        public SettingCheckBox ResetMapTransitions;

        public SettingCheckBox CountEngineTicks;
        public SettingCheckBox CountPauses;
        public SettingCheckBox CountDisconnects;
        public SettingCheckBox CountInactive;
        public SettingCheckBox CountAutomatic;
        public SettingCheckBox CountDemoInterop;

        public SettingCheckBox PrintDemoInfo;
        public SettingTextBox DemoParserPath;

        public SettingCheckBox SetGameTimeOnLaunch;

        public SettingUIRepresented<string[][]> MapTransitionList;

        public List<SettingUIRepresented> GetUIRepresented()
        {
            var list = new List<SettingUIRepresented>();

            var fields = typeof(SettingProvider).GetFields();
            foreach (var field in fields)
            {
                var val = field.GetValue(this);
                if (val is SettingUIRepresented)
                    list.Add(val as SettingUIRepresented);
            }

            return list;
        }

        public Dictionary<string, string> Miscellaneous = new Dictionary<string, string>();
        public string GetMiscSetting(string key)
        {
            if (Miscellaneous.TryGetValue(key, out var val)) return val;
            else return null;
        }
        public void SetMiscSetting(string key, string value)
        {
            if (Miscellaneous.TryGetValue(key, out _)) Miscellaneous[key] = value;
            else Miscellaneous.Add(key, value);
        }
    }
}
