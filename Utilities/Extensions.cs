using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace LiveSplit.SourceSplit.Utilities
{
    public static class Extensions
    {
        public static XmlElement ToElement<E>(this XmlDocument document, string name, E value)
        {
            XmlElement str = document.CreateElement(name);
            str.InnerText = value.ToString();
            return str;
        }

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> func)
        {
            foreach (var item in enumerable)
            {
                func(item);
            }
        }

        public static string RemoveRepeats(this string input, params string[] targets)
        {
            return targets
                .OrderByDescending(x => x.Length)
                .Aggregate(input, (a, b) => Regex.Replace(a, $"({Regex.Escape(b)})(\\1+)", (e) => e.Groups[1].Value));
        }
    }
}
