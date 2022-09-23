using System.Text.RegularExpressions;

namespace JsonPathSerializer
{
    internal class PathGlobals
    {
        public class PathRegex
        {
            public static readonly Regex KEY = new(@"\[\'([^']+)\'\]");
            public static readonly Regex INDEX = new(@"\[([0-9]+)\]");

            public static readonly Regex BRACKET = new(@"(\[[^\]]+\])");
            public static readonly Regex PARENTHESIS_WITHIN_BRACKET = new(@"(\[\([^\)]+\)\])");
        }
    }
}
