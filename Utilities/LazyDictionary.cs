using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.SourceSplit.Utils
{
    public class LazyDictionary<T> : IEnumerable<T>
    {
        public List<string> Keys = new();
        public List<T> Values = new();

        public Tuple<string, T> ElementAt(int index)
        {
            if (index < 0 || index >= Keys.Count)
                return null;

            return new Tuple<string, T>(Keys[index], Values[index]);
        }

        public void Add(string key, T value)
        {
            if (key == null || Values.Any(x => x.Equals(value)))
                return;

            Keys.Add(key);
            Values.Add(value);
        }

        public void Clear()
        {
            Keys.Clear();
            Values.Clear();
        }

        public T GetValue(string key)
        {
            for (int i = 0; i < Keys.Count; i++)
                if (key == Keys[i])
                    return Values[i];

            return default;
        }

        public string GetKey(T value)
        {
            for (int i = 0; i < Values.Count; i++)
                if (value.Equals(Values[i]))
                    return Keys[i];

            return null;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
