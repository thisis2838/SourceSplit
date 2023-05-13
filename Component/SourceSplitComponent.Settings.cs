using LiveSplit.Model;
using LiveSplit.SourceSplit.Common;
using LiveSplit.SourceSplit.Utilities;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static LiveSplit.SourceSplit.Common.SettingsProvider;
using System.Xml;
using LiveSplit.SourceSplit.Component.Forms;

namespace LiveSplit.SourceSplit.Component
{
    partial class SourceSplitComponent : IDisposable, IComponent
    {
        private DeferredSettingsControl _deferredSettings = null;
        private MainForm _settingsForm = null;

        public Control GetSettingsControl(LayoutMode mode)
        {
            return _deferredSettings;
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            XmlElement node = document.CreateElement("Settings");

            node.AppendChild(document.ToElement("Version", Globals.Version));
            Settings.ForEach(x =>
            {
                node.AppendChild(document.ToElement(x.Key, x.Value.Serialize()));
            });

            return node;
        }

        public void SetSettings(XmlNode settings)
        {
            settings.ChildNodes.Cast<XmlNode>().ForEach(x =>
            {
                if (SettingsProvider.Settings.ContainsKey(x.Name))
                {
                    Settings[x.Name].Deserialize(x.Value);
                }
                else BackCompatSetSettings(x.Name, x.Value);
            });
        }

        public void BackCompatSetSettings(string name, string serialized)
        {
            
        }
    }
}
