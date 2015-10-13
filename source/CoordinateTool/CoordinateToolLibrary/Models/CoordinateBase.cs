using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoordinateToolLibrary.Models
{
    public enum CoordinateType
    {
        DD,
        DDM,
        DMS,
        GARS,
        MGRS,
        Unknown,
        USNG,
        UTM
    }

    public class CoordinateBase
    {
        // TODO add required and optional group names?
        // only works with numeric values
        protected static bool ValidateNumericCoordinateMatch(Match m, string[] requiredGroupNames)
        {
            foreach (string gname in requiredGroupNames)
            {
                var temp = m.Groups[gname];
                if (temp.Success == false || temp.Captures.Count != 1)
                    return false;

                double result;
                if (double.TryParse(temp.Value, out result) == false)
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            return this.ToString(null);
        }

        public string ToString(string format)
        {
            return this.ToString(format, null);
        }

        public virtual string ToString(string format, IFormatProvider formatProvider)
        {
            if (formatProvider != null)
            {
                if (formatProvider is CoordinateFormatterBase && !format.Contains("{0:"))
                {
                    format = string.Format("{{0:{0}}}", format);
                }

                return string.Format(formatProvider, format, new object[] { this });
            }

            return string.Empty;
        }
    }
}
