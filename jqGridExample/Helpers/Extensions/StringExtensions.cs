using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jqGridExample.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static string ToDelimitedString<T>(this IEnumerable<T> list, string separator)
        {
            string[] array = list.Cast<object>().Where(n => n != null).Select(n => n.ToString()).ToArray();
            if (array.Length > 0 && array != null)
                return string.Join(separator, array);
            else
                return string.Empty;
        }
    }
}