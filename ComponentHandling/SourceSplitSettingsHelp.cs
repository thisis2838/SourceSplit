using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace LiveSplit.SourceSplit.ComponentHandling
{
    public partial class SourceSplitSettingsHelp : Form
    {
        public SourceSplitSettingsHelp()
        {
            InitializeComponent();

            this.VisibleChanged += SourceSplitSettingsHelp_VisibleChanged;
            this.FormClosing += SourceSplitSettingsHelp_FormClosing;

            this.Icon = SystemIcons.Information;

            UpdateDescription(null);
        }

        private void SourceSplitSettingsHelp_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }

        private void SourceSplitSettingsHelp_VisibleChanged(object sender, EventArgs e)
        {
            _highlightedControl?.Invalidate(); _highlightedControl?.Update();
        }

        private Dictionary<string, string> _descriptions = new Dictionary<string, string>();
        private Dictionary<string, string> _names = new Dictionary<string, string>();

        private string GetTitle(Control control)
        {
            if (_names.ContainsKey(control.Name))
            {
                return _names[control.Name];
            }

            if
            (
                control is TextBox ||
                control is NumericUpDown ||
                control is ComboBox
            )
                return control.Name;


            return string.IsNullOrWhiteSpace(control.Text) ? control.Name : control.Text;
        }

        private IEnumerable<Control> GetAncestors(Control control, bool onlyVisible = false)
        {
            var parent = control.Parent;
            while (parent != null && parent.Text != "root") // hack to prevent checking too far back in the settings window hierarchy
            {
                // hack to prevent including structural elements
                if (!onlyVisible || _descriptions.ContainsKey(parent.Name) || !(parent is TableLayoutPanel || (parent is Panel && !(parent is TabPage))))
                {
                    yield return parent;
                }
                parent = parent.Parent;
            }
        }

        public void SetName(Control control, string name)
        {
            _names[control.Name] = name;
        }
        public void SetDescription(Control control, string description)
        {
            _descriptions[control.Name] = description;
        }

        private Control _highlightedControl = null;
        private void DrawHighlight(object sender, PaintEventArgs e)
        {
            if (Visible)
            {
                var origRect = e.ClipRectangle;
                var color = Color.SkyBlue;
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(70, color)), origRect);
            }
        }
        private void HighlightControl(Control control)
        {
            if (control != null)
            {
                control.Paint += DrawHighlight;
                control.Invalidate(); control.Update();
                _highlightedControl = control;
            }
        }
        private void RemoveCurrentHighlight()
        {
            if (_highlightedControl != null)
            {
                _highlightedControl.Paint -= DrawHighlight;
                _highlightedControl.Invalidate(); _highlightedControl.Update();
                _highlightedControl = null;
            }
        }

        private Control _lastInput = null;

        public void UpdateDescription(Control control)
        {
            if (control == null) tlpMain.Visible = false;

            if (_lastInput == control) return;
            _lastInput = control;

            tlpMain.Visible = true;
            RemoveCurrentHighlight();

            if (!_descriptions.ContainsKey(control.Name))
            {
                var parent = control.Parent;
                if (GetAncestors(control).Any(x =>
                {
                    if (_descriptions.ContainsKey(x.Name))
                    {
                        labName.Text = GetTitle(x);
                        boxExplain.Text = _descriptions[x.Name];
                        boxPath.Text = string.Join(" > ", GetAncestors(x, true).Reverse().Select(y => GetTitle(y)));

                        HighlightControl(x);

                        return true;
                    }

                    return false;
                }))
                {
                    return;
                }

                tlpMain.Visible = false;
            }
            else
            {
                HighlightControl(control);

                labName.Text = GetTitle(control);
                boxExplain.Text = _descriptions[control.Name];
                boxPath.Text = string.Join(" > ", GetAncestors(control, true).Reverse().Select(x => GetTitle(x)));
            }
        }
    }
}
