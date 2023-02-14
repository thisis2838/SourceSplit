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

            this.ShowInTaskbar = false;
            UpdateDescription(null);

            this.FormClosing += (s, e) =>
            {
                this.Hide();
                e.Cancel = true;
            };
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

        public void UpdateDescription(Control control)
        {
            tlpMain.Visible = true;

            if (control is null || !_descriptions.ContainsKey(control.Name))
            {
                if (control != null)
                {
                    var parent = control.Parent;
                    if (GetAncestors(control).Any(x =>
                    {
                        if (_descriptions.ContainsKey(x.Name))
                        {
                            labName.Text = GetTitle(x);
                            boxExplain.Text = _descriptions[x.Name];
                            boxPath.Text = string.Join(" > ", GetAncestors(x, true).Reverse().Select(y => GetTitle(y)));

                            return true;
                        }

                        return false;
                    }))
                    {
                        return;
                    }
                }

                tlpMain.Visible = false;

            }
            else
            {
                labName.Text = GetTitle(control);
                boxExplain.Text = _descriptions[control.Name];
                boxPath.Text = string.Join(" > ", GetAncestors(control, true).Reverse().Select(x => GetTitle(x)));
            }
        }
    }
}
