using System.Text.RegularExpressions;

namespace JsonPathSerializer
{
    internal class PathGlobals
    {
        public class PathRegex
        {
            public static readonly Regex KEY = new(@"\[\'([^']+)\'\]"); // also as dot notation
            public static readonly Regex INDEX = new(@"\[(-?\d+)\]");
            public static readonly Regex INDEX_LIST = new(@"\[(\d+,\s{0,1})+\d+\]");
            public static readonly Regex INDEX_SPAN =
                new(@"(\[-?\d+\s{0,1}:\s{0,1}\])|(\[\s{0,1}:\s{0,1}\d+\])|(\[\d+\s{0,1}:\s{0,1}\d+\])");
            public static readonly Regex INDEX_START_FROM = new(@"\[(\d+)\s{0,1}:\s{0,1}\]");
            public static readonly Regex INDEX_END_AT = new(@"\[\s{0,1}:\s{0,1}(\d+)\]");
            public static readonly Regex INDEX_LAST = new(@"\[(-?\d+)\s{0,1}:\s{0,1}\]");
            public static readonly Regex WILDCARD = new(@"\[\s{0,1}(\*)\s{0,1}\]"); // also as plain *

            public static readonly Regex BRACKET = new(@"(\[[^\]]+\])");
            public static readonly Regex PARENTHESIS_WITHIN_BRACKET = new(@"(\[\([^\)]+\)\])");
        }
    }
}
