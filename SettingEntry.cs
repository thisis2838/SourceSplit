using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.SourceSplit.Utils;

namespace LiveSplit.SourceSplit
{
    public abstract class SettingEntryBase
    {
        public abstract void InsertSettings(XmlDocument doc, XmlElement node);
        public abstract void GetValueFromSetting(XmlNode settings);
    }

    public class SettingEntry<T> : SettingEntryBase
    {
        string Name;
        public T Value { get; set; }
        T Default;

        public SettingEntry(
            string name, 
            T def)
        {
            Name = name;
            Value = Default = def;
        }

        public void Bind(Control control, string prop)
        {
            control.DataBindings.Add(
                prop,
                this,
                "Value",
                false,
                DataSourceUpdateMode.OnPropertyChanged);
        }

        public override void InsertSettings(XmlDocument doc, XmlElement node)
        {
            node.AppendChild(XMLOperations.ToElement(doc, Name, Value));
        }

        public override void GetValueFromSetting(XmlNode settings)
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter == null)
                converter = TypeDescriptor.GetConverter(typeof(Enum));
            string settingEntry = settings[Name]?.InnerText ?? null;
            try
            {
                Value = settingEntry == null ? Default : (T)converter.ConvertFromString(settingEntry);
            }
            catch (NotSupportedException)
            {
                Value = Default;
            }
        }
    }
}
