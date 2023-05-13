using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.Common
{
    public class Setting
    {
        public Setting
        (
            string name,
            object @default = null,
            Func<object, string> serializer = null,
            Func<string, object> deserializer = null
        )
        {
            Name = name;
            _value = _default = @default;
            _serializer = serializer;
            _deserializer = deserializer;
        }

        public string Name;

        public EventHandler<EventArgs> ValueSet;
        public EventHandler<EventArgs> ValueLocked;
        public EventHandler<EventArgs> ValueUnlocked;

        private object _value;
        private object _default;
        private bool _locked = false;
        private Func<object, string> _serializer;
        private Func<string, object> _deserializer;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SetValue(object value)
        {
            if (_locked) return;

            _value = value;
            ValueSet?.Invoke(this, new EventArgs());
        }
        public object GetValue() => _value;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Lock()
        {
            _locked = true;
            ValueLocked?.Invoke(this, new EventArgs());
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Unlock()
        {
            _locked = true;
            ValueUnlocked?.Invoke(this, new EventArgs());
        }

        public string Serialize()
        {
            if (_serializer is null) return _value.ToString();
            else return _serializer(_value);
        }
        public void Deserialize(string serialized)
        {
            if (_deserializer is null)
            {
                SetValue(_default);
                //throw ErrorWindow.Exception($"Setting {Name} has no deserializer to use!");
            }
            SetValue(_deserializer(serialized));
        }
    }

    public static class SettingsProvider
    {
        public static Dictionary<string, Setting> Settings = null;
        public static void Init()
        {
            Settings = new Dictionary<string, Setting>();

            void add(Setting s)
            {
                Settings.Add(s.Name, s);
            }
        }
    }
}
