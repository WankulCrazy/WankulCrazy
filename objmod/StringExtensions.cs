using System;
using System.Collections.Generic;
using System.Text;

namespace WankulCrazyPlugin.objmod
{
    public static class StringExtensions
    {
        public static string Clean(this string str)
        {
            string str1 = str.Replace('\t', ' ');
            while (str1.Contains("  "))
                str1 = str1.Replace("  ", " ");
            return str1.Trim();
        }
    }
}
