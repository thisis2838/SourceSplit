using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.SourceSplit.Utils;
using LiveSplit.SourceSplit.Extensions;
using System.Diagnostics;

namespace LiveSplit.SourceSplit.Utils
{
    public interface ISourceSplitSetting
    {
        public string Name { get; internal set; }
        public bool Forced { get; internal set; }
        public void SetForcedValue(bool forced, object value);
        public void InsertSettings(XmlDocument doc, XmlElement node);
        public void GetValueFromSetting(XmlNode settings);
    }

    public class SourceSplitSetting<T> : ISourceSplitSetting
    {
        public string Name { get; set; }
        public bool Forced { get; set; } = false;
        public T Value { get => _get(); set => _set.Invoke(value); }

        protected Func<T> _get = null;
        protected Action<T> _set = null;
        protected T _default;
        protected T? _forcedBefore;
        protected Control _control;

        public SourceSplitSetting() {; }

        public SourceSplitSetting(string name, T value, Func<T> get, Action<T> set, Control control)
        {
            Name = name;
            _get = get;
            _set = set;
            _forcedBefore = _default = value;
            _control = control;
            Value = value;
        }

        public void SetForcedValue(bool forced, object value)
        {
            if (Forced != forced)
                Debug.WriteLine($"Set forced state of {this.Name} to {forced}, value to \"{value}\"");

            if (forced)
            {
                _forcedBefore = _get();;
                _set(Unbox(value));
            }
            else if (Forced) _set(_forcedBefore);

            _control.Enabled = !forced;
            Forced = forced;
        }

        internal virtual T Unbox(object value)
        {
            return (T)value;
        }

        public void InsertSettings(XmlDocument doc, XmlElement node)
        {
            node.AppendChild(XMLExtensions.ToElement(doc, Name, Forced ? _forcedBefore : _get()));
        }

        public void GetValueFromSetting(XmlNode settings)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter == null)
                converter = TypeDescriptor.GetConverter(typeof(Enum));
            string settingEntry = settings[Name]?.InnerText ?? null;

            T val;
            try
            {
                val = settingEntry == null ? _default : (T)converter.ConvertFromString(settingEntry);
            }
            catch (NotSupportedException)
            {
                val = _default;
            }

            if (Forced) _forcedBefore = val;
            else Value = val;
        }
    }

    public class SourceSplitSettingCheckBox : SourceSplitSetting<bool>
    {
        public SourceSplitSettingCheckBox(string name, bool value, CheckBox box) : base(
            name,
            value,
            () => { return box.Checked; },
            (e) => { box.Checked = e; },
            box
            )
        { }
    }

    public class SourceSplitSettingNumericUpDown : SourceSplitSetting<decimal>
    {
        public SourceSplitSettingNumericUpDown(string name, decimal value, NumericUpDown nud) : base(
            name,
            value,
            () => { return nud.Value; },
            (e) => { nud.Value = e; },
            nud
            )
        { }

        internal override decimal Unbox(object value)
        {
            return decimal.Parse(value.ToString());
        }
    }

    public class SourceSplitSettingTextBox : SourceSplitSetting<string>
    {
        public SourceSplitSettingTextBox(string name, string value, TextBox box) : base(
            name,
            value,
            () => { return box.Text; },
            (e) => { box.Text = e; },
            box
            )
        { }
    }

    public class SourceSplitSettingEnumComboBox<T> : SourceSplitSetting<T> where T : Enum
    {
        public SourceSplitSettingEnumComboBox(string name, T value, ComboBox box) : base(
            name,
            value,
            () => Util.EnumValueFromDescription<T>(box.Text),
            (e) => { box.Text = e.GetDescription(); },
            box
            )
        { }
    }

    public struct EnforceSettingInfo
    {
        public string Name;
        public object Value;

        public EnforceSettingInfo(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }

    public class EnforceSettingInfoCollection : List<EnforceSettingInfo>
    {
        public void Add(string name, object value)
        {
            if (!this.Any(x => x.Name == name))
                this.Add(new EnforceSettingInfo(name, value));
        }
    }
}
