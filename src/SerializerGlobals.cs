using System.Text.RegularExpressions;

namespace JsonPathSerializer
{
    class SerializerGlobals
    {
        public class JsonPathRegex
        {
            public static readonly Regex PROPERTY_DOT = new Regex(@"\.([^\.\[]+)");
            public static readonly Regex PROPERTY_BRACKET = new(@"\[\'([^']+)\'\]");
            public static readonly Regex INDEX = new(@"\[(-?\d+)\]");
            public static readonly Regex INDEXES = new(@"\[(-?\d+,\s{0,1})+(-?\d+)\]");
            public static readonly Regex INDEX_SPAN =
                new(@"\[(-?\d+)\s{0,1}:\s{0,1}(-?\d+)\]");
            public static readonly Regex INDEX_START_FROM = new(@"\[(-?\d+)\s{0,1}:\s{0,1}\]");
            public static readonly Regex INDEX_END_AT = new(@"\[\s{0,1}:\s{0,1}(-?\d+)\]");
        }
    }
}
