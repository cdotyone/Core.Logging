using System;
using System.Collections.Generic;
using System.Linq;

namespace Stack.Core.Logging.Configuration
{
    internal static class Cloner
    {
        public static IEnumerable<T> Clone<T>(this IEnumerable<T> collection) where T : ICloneable
        {
            return collection.Select(item => (T)item.Clone());
        }

        public static Dictionary<string,string> Clone(this Dictionary<string, string> collection)
        {
            var dict = new Dictionary<string, string>();

            foreach (var pair in collection)
            {
                dict[pair.Key] = pair.Value;
            }

            return dict;
        }
    }
}