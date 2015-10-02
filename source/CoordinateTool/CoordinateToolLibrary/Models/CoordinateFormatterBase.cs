using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoordinateToolLibrary.Models
{
    public class CoordinateFormatterBase : IFormatProvider, ICustomFormatter
    {
        public CoordinateFormatterBase() { }

        public object GetFormat(Type formatType)
        {
            return (formatType == typeof(ICustomFormatter)) ? this : null;
        }

        public virtual string Format(string format, object arg, IFormatProvider formatProvider)
        {
            // override this
            throw new Exception("Must override CoordinateFormatterBase.Format!");
        }
    }
}
