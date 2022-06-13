using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter.Extensions
{
    public static class StringExtensions
    {
        public static string Replace(this string s, Dictionary<string, string> replacements)
        {
            StringBuilder sb = new StringBuilder(s);

            return replacements.Keys.Aggregate(sb, (current, toReplace) => current.Replace(toReplace, replacements[toReplace]))
                                    .ToString();
        }
    }
}
