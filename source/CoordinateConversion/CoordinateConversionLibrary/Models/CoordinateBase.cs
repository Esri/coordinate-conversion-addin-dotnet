/******************************************************************************* 
  * Copyright 2015 Esri 
  *  
  *  Licensed under the Apache License, Version 2.0 (the "License"); 
  *  you may not use this file except in compliance with the License. 
  *  You may obtain a copy of the License at 
  *  
  *  http://www.apache.org/licenses/LICENSE-2.0 
  *   
  *   Unless required by applicable law or agreed to in writing, software 
  *   distributed under the License is distributed on an "AS IS" BASIS, 
  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
  *   See the License for the specific language governing permissions and 
  *   limitations under the License. 
  ******************************************************************************/ 

using System;
using System.Text.RegularExpressions;

namespace CoordinateConversionLibrary.Models
{
    public enum CoordinateType
    {
        DD,
        DDM,
        DMS,
        //GARS,
        MGRS,
        Unknown,
        USNG,
        UTM
    }

    public class CoordinateBase
    {
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

                // Support Tabs
                format = format.Replace(@"\t", "\t");

                return string.Format(formatProvider, format, new object[] { this });
            }

            return string.Empty;
        }
    }
}
