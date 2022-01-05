using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using LiveSplit.SourceSplit;
using LiveSplit.UI.Components;
using System;
using LiveSplit.Model;
using System.Linq;

[assembly: ComponentFactory(typeof(SourceSplitFactory))]

namespace LiveSplit.SourceSplit
{
    public class SourceSplitFactory : IComponentFactory
    {
        private SourceSplitComponent _instance;

        public string ComponentName => "SourceSplit";
        public string Description => "Game Time / Auto-splitting for Source engine games.";
        public ComponentCategory Category => ComponentCategory.Control;

        public IComponent Create(LiveSplitState state)
        {
            // hack to prevent double loading
            string caller = new StackFrame(1).GetMethod().Name;
            bool createAsLayoutComponent = (caller == "LoadLayoutComponent" || caller == "AddComponent");

            // if component is already loaded somewhere else
            // layout editor takes precedent because its just better & i can't figure out how to unload
            // components from layout editor >:(
            if (_instance != null && !_instance.Disposed)
            {
                void unloadSplitsInstance(bool fromLayout)
                {
                    string msg = (fromLayout) ?
                        "Deactivated SourceSplit from Splits Editor!" :
                        "Cannot activate SourceSplit when it is already loaded in the Layout Editor!";

                    string source = fromLayout ?
                        "Layout Editor" :
                        "Splits Editor";

                    MessageBox.Show(msg, $"SourceSplit Warning | {source}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Debug.WriteLine("unload splits instance");
                    state.Run.AutoSplitter.Component?.Dispose();
                    state.Run.AutoSplitter.Deactivate();
                    state.Settings.ActiveAutoSplitters.Remove(state.Run.GameName);
                }

                if (createAsLayoutComponent)
                {
                    if (!_instance.IsLayoutComponent)
                        unloadSplitsInstance(true);
                    else
                        throw new Exception($"Component already loaded");
                } 
                else
                {
                    if (_instance.IsLayoutComponent)
                    {
                        unloadSplitsInstance(false);
                        return null;
                    }
                    else
                        _instance.Dispose();
                }
            }

/*
            // if component is already loaded somewhere else
            if (_instance != null && !_instance.Disposed)
                throw new Exception($">:( )\n(Component already loaded in the {(_instance.IsLayoutComponent ? "Layout Editor" : "Splits Editor")}");
*/

            return (_instance = new SourceSplitComponent(state, createAsLayoutComponent));
        }

        public string UpdateName => this.ComponentName;
        public string UpdateURL => "https://raw.githubusercontent.com/thisis2838/SourceSplit/master/update/";
        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public string XMLURL => "https://raw.githubusercontent.com/thisis2838/SourceSplit/master/update/update.xml";
    }
}
