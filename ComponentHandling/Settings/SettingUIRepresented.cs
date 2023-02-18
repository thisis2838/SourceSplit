using LiveSplit.SourceSplit.GameSpecific;
using LiveSplit.SourceSplit.Utilities;
using LiveSplit.SourceSplit.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.SourceSplit.ComponentHandling.Settings
{
    /// <summary>
    /// Setting entry that is represented by the setting UI...
    /// </summary>
    public abstract class SettingUIRepresented
    {
        public string Name { get; set; }
        //protected object _accessLocker = new object();

        public SettingUIRepresented(string name)
        {
            Name = name;
        }

        public abstract string GetStorageValue();
        public abstract void InitFromStorageValue(string value);
        public abstract void Unlock();
    }

    public class SettingUIRepresented<T> : SettingUIRepresented
    {
        private Action<T> _setFunc;
        private Func<T> _getFunc;
        protected Control _control;

        private Func<T, string> _getStorageValue = (e) => e.ToString();
        private Func<string, T> _parseStorageValue = (e) =>
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter == null) throw ErrorDialog.Exception($"No converter found for data type {typeof(T).FullName} while parsing setting");

            return (T)converter.ConvertFromString(e);
        };

        private T _default;
        private T _preLockValue;
        private bool _locked;
        private T _value;
        private object _valueLock = new object();
        public T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        public SettingUIRepresented
        (
            string name, T @default, 
            Func<T> getFunc, Action<T> setFunc, 
            Control control,
            Func<T, string> getStorageValue = null, Func<string, T> parseStorageValue = null) 
            : base(name)
        {
            _control = control;
            _setFunc = setFunc;
            _getFunc = getFunc;
            _default = @default;

            if (getStorageValue != null) _getStorageValue = getStorageValue;
            if (parseStorageValue != null) _parseStorageValue = parseStorageValue;

            _value = @default;
            SetValue(@default);
        }

        // this must always be called by the control.
        public void UIValueChangedCallback()
        {
            lock (_valueLock) _value = _getFunc.Invoke();
        }

        public T GetValue()
        {
            lock (_valueLock) return _value;
        }

        public void SetValue(T value)
        {
            if (_locked) _preLockValue = value;
            else
            {
                _control.AttemptInvoke(() => _setFunc.Invoke(value), 100, 10);
                lock (_valueLock) _value = value;
            }
        }

        public void Lock(T value)
        {
            if (_locked) return;

            Debug.WriteLine($"Locking {Name} to {value}...");
            _preLockValue = GetValue();

            SetValue(value);
            _locked = true;
            _control.AttemptInvoke(() => _control.Enabled = false, 100, 10);
        }

        public override void Unlock()
        {
            if (!_locked) return;

            Debug.WriteLine($"Unlocking {Name}...");
            _control.AttemptInvoke(() => _control.Enabled = true, 100, 10);

            _locked = false;
            SetValue(_preLockValue);
        }

        public override string GetStorageValue()
        {
            if (_locked) return _getStorageValue(_preLockValue);
            return _getStorageValue(GetValue());
        }

        public override void InitFromStorageValue(string value)
        {
            T val = _default;
            try { val = _parseStorageValue(value); }
            catch {; }
            
            SetValue(val);
        }
    }

    public class SettingCheckBox : SettingUIRepresented<bool>
    {
        public SettingCheckBox(string name, bool value, CheckBox box) : base
        (
            name,
            value,
            () => { return box.Checked; },
            (e) => { box.Checked = e; },
            box
        )

        {
            box.CheckedChanged += (s, e) => this.UIValueChangedCallback();
        }
    }

    public class SettingNumericUpDown : SettingUIRepresented<decimal>
    {
        public SettingNumericUpDown(string name, decimal value, NumericUpDown nud) : base
        (
            name,
            value,
            () => { return nud.Value; },
            (e) => { nud.Value = decimal.Parse(e.ToString()); },
            nud
        )

        {
            nud.ValueChanged += (s, e) => this.UIValueChangedCallback();
        }
    }

    public class SettingTextBox : SettingUIRepresented<string>
    {
        public SettingTextBox(string name, string value, TextBox box, bool alwaysLowerCase = false) : base
        (
            name,
            value,
            () => { return alwaysLowerCase ? box.Text.ToLower() : box.Text; },
            (e) => { box.Text = alwaysLowerCase ? e.ToLower() : e; },
            box
        )

        {
            box.TextChanged += (s, e) => this.UIValueChangedCallback();
        }
    }

    public class SettingComboBoxEnum<T> : SettingUIRepresented<T> where T : Enum
    {
        public SettingComboBoxEnum(string name, T value, ComboBox box) : base
        (
            name,
            value,
            () => EnumUtils.EnumValueFromDescription<T>(box.Text),
            (e) => { box.Text = e.GetDescription(); },
            box
        )
        {
            box.SelectedIndexChanged += (s, e) => this.UIValueChangedCallback();
            box.TextChanged += (s, e) => this.UIValueChangedCallback();
        }
    }
}
