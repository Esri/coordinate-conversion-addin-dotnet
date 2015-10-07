using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoordinateToolLibrary.Helpers;

namespace CoordinateToolLibrary.Models
{
    public class DefaultFormatModel
    {
        public DefaultFormatModel() 
        {
            CType = CoordinateType.Unknown;
            DefaultNameFormatDictionary = new SerializableDictionary<string, string>() {{"one","two"},{"three","four"} };
        }

        public CoordinateType CType { get; set; }
        public SerializableDictionary<string, string> DefaultNameFormatDictionary { get; set; }
    }
}
