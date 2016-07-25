using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadFileDetector
{
    static class StringExtensions
    {
        public static bool Contains(this string value, string search, StringComparison options)
        {
            return value.IndexOf(search, StringComparison.OrdinalIgnoreCase) != -1;
        }
    }
}
