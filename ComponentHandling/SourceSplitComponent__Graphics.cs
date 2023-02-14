using System.Diagnostics;
using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.TimeFormatters;
using LiveSplit.UI.Components;
using LiveSplit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml;
using System.Windows.Forms;
using LiveSplit.SourceSplit.GameSpecific;
using static LiveSplit.SourceSplit.GameHandling.GameMemory;
using LiveSplit.SourceSplit.GameHandling;
using System.Reflection;
using LiveSplit.SourceSplit.Utilities;
using LiveSplit.SourceSplit.ComponentHandling;
using LiveSplit.View;

namespace LiveSplit.SourceSplit.ComponentHandling
{
    partial class SourceSplitComponent : IComponent
    {
        private InternalTimeComponent _altTimingComponent;
        private InternalTextComponent _tickCountComponent;
        private InternalTextComponent _curDemoComponent;
        private InternalComponentRenderer _componentRenderer = new InternalComponentRenderer();

        private void InitGraphics(LiveSplitState state)
        {
            InternalTextComponent.Components.Clear();

            _altTimingComponent = new InternalTimeComponent(Settings.ShowGameTime.Value, "Game Time", "");
            _tickCountComponent = new InternalTextComponent(Settings.ShowTickCount.Value, "Tick Count", "");
            _curDemoComponent = new InternalTextComponent(Settings.ShowCurDemo.Value, "Current Demo", "");

            this.ContextMenuControls = new Dictionary<String, Action>();
            this.ContextMenuControls.Add("SourceSplit: Times", () => SettingControl.SessionsForm.Show());
            this.ContextMenuControls.Add("SourceSplit: Settings", () =>
            {
                var dialog = new ComponentSettingsDialog(this);
                dialog.Size = new Size(dialog.MaximumSize.Width, SettingControl.Size.Height + 100);
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (IsLayoutComponent) _timer.CurrentState.Layout.HasChanged = true;
                    else
                    {
                        var run = _timer.CurrentState.Run;
                        var document = new XmlDocument();
                        var autoSplitterSettings = document.CreateElement("AutoSplitterSettings");
                        autoSplitterSettings.InnerXml = this.GetSettings(document).InnerXml;
                        autoSplitterSettings.Attributes.Append(SettingsHelper.ToAttribute(document, "gameName", run.GameName));
                        run.AutoSplitterSettings = autoSplitterSettings;
                        run.HasChanged = true;
                    }
                }
            });

#if DEBUG
            this.ContextMenuControls.Add("SourceSplit: Debug Logs", () => SettingControl.butOpenDebug_Click(null, null));
#endif
        }

        private void UpdateComponentEnabledState()
        {
            _altTimingComponent.Enabled = Settings.ShowGameTime.Value;
            _tickCountComponent.Enabled = Settings.ShowTickCount.Value;
            _curDemoComponent.Enabled = Settings.ShowCurDemo.Value;
        }

        private void UpdateInternalComponents(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            UpdateComponentEnabledState();

            if (_componentRenderer.VisibleComponents.Any())
            {
                if (Settings.ShowGameTime.Value)
                {
                    // change this if we ever have new timing methods
                    TimingMethod method = state.CurrentTimingMethod;
                    if (Settings.ShowAltTime.Value)
                        method = (TimingMethod)(((int)method + 1) % 2);

                    this._altTimingComponent.SetTime(state.CurrentTime[method]?? TimeSpan.Zero, (int)Settings.GameTimeDecimalPlaces.Value);
                    this._altTimingComponent.SetName(method == TimingMethod.RealTime ? "Real Time" : "Game Time");

                    _altTimingComponent.Update(invalidator, state, width, height, mode);
                }

                if (Settings.ShowTickCount.Value)
                {
                    _tickCountComponent.SetText
                    (
                        $"{(long)(GameTime.TotalSeconds / _intervalPerTick)} | " +
                        $"{StringUtils.NumberAlign(_sessions.Current?.TotalTicks ?? 0, 5)}"
                    );
                    _tickCountComponent.Update(invalidator, state, width, height, mode);
                }

                if (Settings.ShowCurDemo.Value)
                {
                    _curDemoComponent.Update(invalidator, state, width, height, mode);
                }
            }

            _componentRenderer.Update(invalidator, state, width, height, mode);
            _componentRenderer.CalculateOverallSize(mode);
        }

        public float MinimumWidth => _componentRenderer.MinimumWidth;
        public float MinimumHeight => _componentRenderer.MinimumHeight;
        public float VerticalHeight => _componentRenderer.VerticalHeight;
        public float HorizontalWidth => _componentRenderer.HorizontalWidth;
        public float PaddingLeft => _componentRenderer.PaddingLeft;
        public float PaddingRight => _componentRenderer.PaddingRight;
        public float PaddingTop => _componentRenderer.PaddingTop;
        public float PaddingBottom => _componentRenderer.PaddingBottom;

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region region)
        {
            this.PrepareDraw(state);
            _componentRenderer.Render(g, state, width, 0, LayoutMode.Vertical, region);
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region region)
        {
            this.PrepareDraw(state);
            _componentRenderer.Render(g, state, 0, height, LayoutMode.Horizontal, region);
        }

        void PrepareDraw(LiveSplitState state)
        {
            UpdateComponentEnabledState();

            _componentRenderer.VisibleComponents.ToList().ForEach(x =>
            {
                x.NameLabel.ForeColor = state.LayoutSettings.TextColor;
                x.ValueLabel.ForeColor = state.LayoutSettings.TextColor;
                x.NameLabel.HasShadow = x.ValueLabel.HasShadow = state.LayoutSettings.DropShadows;
            });
        }
    }
}
