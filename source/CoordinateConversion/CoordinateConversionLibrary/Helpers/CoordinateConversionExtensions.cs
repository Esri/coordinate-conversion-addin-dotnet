using System.Text.RegularExpressions;

namespace CoordinateConversionLibrary.Helpers
{
    public static class CoordinateConversionExtensions
    {
        public static bool ValidatePrefix(this Group group, bool showHyphen, bool showPlus)
        {
            if (group.ToString().Contains("+"))
                return !(showPlus && group.ToString().Contains("+"));
            else if (group.ToString().Contains("-"))
                return !(showHyphen && group.ToString().Contains("-"));
            return group.Success;
        }
    }
}
