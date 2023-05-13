using LiveSplit.Model;
using LiveSplit.SourceSplit.Component.Forms;
using LiveSplit.SourceSplit.Utilities;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.SourceSplit.Component
{
    partial class SourceSplitComponent : IDisposable, IComponent
    {
        public string ComponentName => "SourceSplit";

        public IDictionary<string, Action> ContextMenuControls => null;

        public bool Disposed = false;
        public bool IsLayoutComponent = true;

        public SourceSplitComponent(LiveSplitState state, bool isLayoutComponent)
        {
            IsLayoutComponent = isLayoutComponent;

            _deferredSettings = new DeferredSettingsControl();
            _settingsForm = new MainForm();
            _deferredSettings.SetProperSettings(_settingsForm);
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            ;
        }

        public void Dispose()
        {
            Logging.Stop();
        }
    }
}
