using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI;

namespace Regex
{
    public class RegexFormat
    {
        public static string Rewrite(string format, object mapping)
        {
            Dictionary<String,String> vals = new Dictionary<String,String>();
            StringBuilder expr = new StringBuilder();
            foreach (PropertyInfo p in mapping.GetType().GetProperties())
            {
                if (expr.Length > 0)
                    expr.Append("|");
                expr.Append("(?<var>");
                expr.Append(p.Name);
                expr.Append(")");

                string val = (string)p.GetValue(mapping, null);
                vals.Add(p.Name, val);
            }

            System.Text.RegularExpressions.Regex r = 
                new System.Text.RegularExpressions.Regex(
                    expr.ToString(),
                    RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
                );

            string rewrittenFormat = r.Replace(format, delegate(Match m)
            {
                Group varGroup = m.Groups["var"];
                string name = varGroup.Value;
                return vals.ContainsKey(name) ? vals[name] : name;
            });

            return rewrittenFormat;
        }
    }
}
