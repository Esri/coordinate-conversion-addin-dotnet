// Copyright 2016 Esri 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace ProAppCoordConversionModule.Helpers
{
    class ImportDescriptor
    {
        public string name { get; set; }
        public int index { get; set; }
        public string type { get; set; }

        public object Convert(List<string> row, string dateformat = "")
        {
            if (index == -1)
                return null;

            if (string.IsNullOrEmpty(row[index]))
                return null;

            return ImportCSV.ConvertFromString(type.Replace("System.Nullable`1[", "").Replace("]", ""), row[index], dateformat);
        }

    }

    public class ImportCSV
    {
        public static List<string> GetHeaders(Stream stream)
        {
            if (stream == null)
                return null;

            using (StreamReader reader = new StreamReader(stream))
            {
                string line = reader.ReadLine();
                if (line.Contains("sep="))
                    line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    return null;

                return line.Split(GetSeparator(line)).ToList();
            }
        }

        private static char GetSeparator(string line)
        {
            Regex regexDD = new Regex(@"^(.+)(?<sep>[,;:| \t])(.+)");
            var matchSep = regexDD.Match(line);
            if (matchSep.Success && matchSep.Length == line.Length)
            {
                var sep = matchSep.Groups["sep"];
                if (sep.Success)
                    return char.Parse(sep.Value);
            }
            return '\0';
        }

        public static IEnumerable<T> Import<T>(Stream stream, string[] fieldNames, List<string> csvHeaders, List<Dictionary<string, Tuple<object,bool>>> lstDictionary) where T : new()
        {
            List<T> list = new List<T>();
            using (StreamReader reader = new StreamReader(stream))
            {
                string line = reader.ReadLine().Trim();
                if (line.Contains("sep="))
                    line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    return list;

                var charSep = GetSeparator(line);

                string[] row = line.Split(charSep);
                List<ImportDescriptor> headers = ParseHeader<T>(row, fieldNames);
                var fieldsDictionary = new Dictionary<string, Tuple<object,bool>>();
                var result = true;
                while (result)
                {
                    result = ImportLine(reader, headers, list, row.Count(), charSep, csvHeaders, out fieldsDictionary);
                    if (result)
                        lstDictionary.Add(fieldsDictionary);
                }
            }

            return list;
        }

        public static IEnumerable<T> Import<T>(Stream stream, string[] fieldNames) where T : new()
        {
            List<T> list = new List<T>();
            using (StreamReader reader = new StreamReader(stream))
            {
                string line = reader.ReadLine().Trim();
                if (line.Contains("sep="))
                    line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                    return list;

                var charSep = GetSeparator(line);

                string[] row = line.Split(charSep);
                List<ImportDescriptor> headers = ParseHeader<T>(row, fieldNames);
                while (ImportLine(reader, headers, list, row.Count(), charSep)) ;
            }

            return list;
        }

        internal static object ConvertFromString(string type, string value, string dateformat, string name = null)
        {
            try
            {
                switch (type)
                {
                    case "System.String":
                        return value;
                    case "System.DateTime":
                        if (string.IsNullOrEmpty(dateformat))
                            return DateTime.Parse(value);
                        return DateTime.ParseExact(value, dateformat, CultureInfo.InvariantCulture);
                    case "System.Int32":
                        return int.Parse(value);
                    case "System.UInt32":
                        return uint.Parse(value);
                    case "System.Int64":
                        return long.Parse(value);
                    case "System.UInt64":
                        return ulong.Parse(value);
                    case "System.Int16":
                        return short.Parse(value);
                    case "System.UInt16":
                        return ushort.Parse(value);
                    case "System.Single":
                        return float.Parse(value);
                    case "System.Double":
                        return double.Parse(value);
                    case "System.Decimal":
                        return decimal.Parse(value);
                    case "System.Char":
                        return char.Parse(value);
                    case "System.Byte":
                        return byte.Parse(value);
                    case "System.SByte":
                        return sbyte.Parse(value);
                    case "System.Boolean":
                        return bool.Parse(value);
                    case "System.Guid":
                        return Guid.Parse(value);
                }
            }
            catch (Exception ex)
            {
                if (string.IsNullOrEmpty(name))
                    name = "unknown";
                if (name.EndsWith("Id"))
                    return (int)0;
                throw new Exception("Unable to convert [" + name + "] string (" + value + ") to " + type, ex);
            }

            throw new Exception("Import was unable to convert [" + name + "] string (" + value + ") to " + type);
        }

        private static bool ImportLine<T>(StreamReader reader, List<ImportDescriptor> headers, List<T> list, int nColumns, char separator, List<string> csvHeaders,
            out Dictionary<string, Tuple<object,bool>> fieldsDictionary) where T : new()
        {
            List<string> row = new List<string>();
            string line = string.Empty;
            string temp = string.Empty;
            fieldsDictionary = new Dictionary<string, Tuple<object,bool>>();
            while (row.Count() < nColumns)
            {
                temp = reader.ReadLine();
                if (temp == null)
                {
                    if (row.Count() == nColumns - 1 && line.Length > 0)  // this is crappy!!
                    {
                        row.Add(line.Replace("\"\"", "\"")); // has embedded "qoated string"
                        break;
                    }
                    return false; // end of file
                }
                line = AddToRow(line + temp, row, separator, nColumns);
            }
            if (row.Count() == nColumns || LastColumnIsEmpty(nColumns, row))
            {
                list.Add(CreateItem<T>(headers, row));
                for (int i = 0; i < csvHeaders.Count; i++)
                    fieldsDictionary.Add(csvHeaders[i], Tuple.Create((object)row[i],false));
                return true;
            }
            throw new Exception("More than " + nColumns + " colunns in row " + list.Count() + ": " + Environment.NewLine + temp);
        }

        private static bool ImportLine<T>(StreamReader reader, List<ImportDescriptor> headers, List<T> list, int nColumns, char separator) where T : new()
        {
            List<string> row = new List<string>();
            string line = string.Empty;
            string temp = string.Empty;
            while (row.Count() < nColumns)
            {
                temp = reader.ReadLine();
                if (temp == null)
                {
                    if (row.Count() == nColumns - 1 && line.Length > 0)  // this is crappy!!
                    {
                        row.Add(line.Replace("\"\"", "\"")); // has embedded "qoated string"
                        break;
                    }

                    return false; // end of file
                }
                line = AddToRow(line + temp, row, separator, nColumns);
            }

            if (row.Count() == nColumns || LastColumnIsEmpty(nColumns, row))
            {
                list.Add(CreateItem<T>(headers, row));
                return true;
            }

            throw new Exception("More than " + nColumns + " colunns in row " + list.Count() + ": " + Environment.NewLine + temp);
        }

        private static bool LastColumnIsEmpty(int nColumns, List<string> row)
        {
            return row.Count() == nColumns + 1 && row.Last() == "";
        }

        // This function will split the string at ; and handle escape chars etc.
        // The output is put in row.
        // if the whole line was not passed, then the remaining line will be returned.
        private static string AddToRow(string line, List<string> row, char separator, int nColumns)
        {

            bool bSpecial = false;
            // while the line still has some chars, continue processing in a loop.
            while (line.Length > 0)
            {
                int pos;
                // only if the line does not start with NewLine or special shall we look for Escape sequence that will start a new quouted line.
                if (!line.StartsWith(Environment.NewLine) && !bSpecial && !line.StartsWith("\""))
                {
                    pos = line.IndexOf(separator + "\""); // start of string section
                    if (pos == -1)
                    {
                        row.AddRange(line.Split(separator));
                        return string.Empty;
                    }

                    if (pos > 0)
                    {
                        row.AddRange(line.Substring(0, pos).Split(separator));
                        line = line.Substring(pos + 2);
                    }
                    else if (pos == 0)
                    {
                        row.Add("");
                        line = line.Substring(2);
                    }
                }

                // now look for end of escape sequence.
                // we will only get here if start of a quoted sequence was found
                // now replace all double quotes with single quotes ...
                pos = line.IndexOf("\"" + separator); // end of string section.
                if (pos == -1)
                {
                    // special case when the "; is not found at the end of the line because the quoated string is the last column
                    if (line.EndsWith("\"") && !line.EndsWith("\"\"") && line.IndexOf(";") == -1)
                    {
                        if (row.Count() + 1 == nColumns)
                        {
                            line = line.Substring(0, line.Length - 1); // remove ending quote
                            if (line.StartsWith("\""))
                                line = line.Substring(1); // I am not sure this is always true.
                            row.Add(line);
                            return string.Empty;
                        }

                        return Environment.NewLine;
                    }

                    // if we end with new line, it is because there is new line inside a quoted sequense.
                    return Environment.NewLine + line;
                }

                // we know that an end "; was found.
                // now check if this is inside the string or the ned of it.
                bSpecial = false;
                if (!FindQuotedEnd(ref line, row, separator))
                {
                    line = line.Substring(0, pos).Replace("\"\"", "\"") + "\" " + separator + line.Substring(pos + 3);
                    bSpecial = true;
                }

            }

            return string.Empty;
        }

        // there may be quotes inside a quoted string. These are escaped by putting double quotes
        // there may also be ; inside a quotes string.
        // if double quotes are just before ; then it is not end of quotes string.
        // This method only return true if it finds the real end of the quoted string.
        // note when it finds the ned it also add the string to row
        private static bool FindQuotedEnd(ref string line, List<string> row, char separator)
        {
            int pos1 = line.IndexOf("\"" + separator);
            int pos2 = line.IndexOf("\"\"\"" + separator);
            int pos3 = line.IndexOf("\"" + separator + "\"" + separator);
            if (pos3 == 0)
            {
                row.Add(";");  // very special case/error when column contain just one separator (;) -- we handle it gracefully.
                line = line.Substring(4);
                return true;
            }
            else if (pos2 < pos1 && pos2 != -1)
            {
                string temp = line.Substring(0, pos2 + 2);
                if (!temp.EndsWith("\"\"\"") || temp.EndsWith("\"\"\"\""))
                {
                    row.Add(temp.Replace("\"\"", "\""));
                    line = line.Substring(pos2 + 4);
                    if (line.Length == 0)
                        row.Add("");
                    return true;
                }
            }
            else if ((pos1 < pos2 || pos2 == -1) && pos1 > 0)
            {
                string temp = line.Substring(0, pos1);
                if (line.StartsWith("\""))
                    temp = line.Substring(1, pos1 - 1);
                if (!temp.EndsWith("\""))
                {
                    row.Add(temp.Replace("\"\"", "\""));
                    line = line.Substring(pos1 + 2);
                    if (line.Length == 0)
                        row.Add("");
                    return true;
                }
            }

            return false;
        }

        private static T CreateItem<T>(List<ImportDescriptor> headers, List<string> row) where T : new()
        {
            T obj = new T();
            headers.ForEach(x => obj.GetType().GetProperty(x.name).SetValue(obj, x.Convert(row)));
            return obj;
        }

        private static List<ImportDescriptor> ParseHeader<T>(string[] row, string[] fieldNames)
        {
            List<ImportDescriptor> list = new List<ImportDescriptor>();

            //for (int i = 0; i < row.Length; i++) 
            for (int i = 0; i < fieldNames.Length; i++)
            {
                string temp = "lat";
                if (list.Any(p => p.name == temp))
                {
                    temp = "lon";
                }
                else if (list.Any(p => p.name == temp))
                    return list;

                var props = typeof(T).GetProperties();
                string header = fieldNames[i].Trim();
                if (header.StartsWith("\"") && header.EndsWith("\""))
                    header = header.Substring(1, header.Length - 2);
                header = header.Replace(" ", "_");
                int rowindex = Array.IndexOf(row, header);
                var prop = props.FirstOrDefault(x => temp.Equals(x.Name, StringComparison.CurrentCultureIgnoreCase));
                if (prop != null)
                    list.Add(new ImportDescriptor { name = prop.Name, index = rowindex, type = prop.PropertyType.ToString() });
            }

            return list;
        }
    }
}