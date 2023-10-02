using System.Text.RegularExpressions;

namespace JsonPathSerializer
{
    internal class SerializerGlobals
    {
        internal static readonly Uri JSON_PATH_SERIALIZER_REPOSITORY_URL =
            new("https://github.com/ShuzhaoFeng/JsonPathSerializer");

        internal class ErrorMessage
        {
            internal static readonly string UNSUPPORTED_TOKEN = "The JsonPath contains an unsupported token." +
                                                        " Please check the documentation at " +
                                                        JSON_PATH_SERIALIZER_REPOSITORY_URL +
                                                        " for supported tokens.";
        }
        internal class JsonPathRegex
        {
            // old regexes to be removed in v.0.2.0
            internal static readonly Regex PROPERTY_DOT = new Regex(@"\.([^\.\[]+)");
            internal static readonly Regex PROPERTY_BRACKET = new(@"\[\'([^']+)\'\]");

            internal static readonly Regex INDEX = new(@"\[(-?\d+)\]");
            internal static readonly Regex INDEXES = new(@"\[(-?\d+,\s{0,1})+(-?\d+)\]");
            internal static readonly Regex INDEX_SPAN =
                new(@"\[(-?\d*)\s{0,1}:\s{0,1}(-?\d*)\]");

            // new regexes to be used in v.0.2.0
            internal static readonly Regex NEW_PROPERTY = new(@"\.([^\.\[]+)|\[\'([^']+)\'\]");
            internal static readonly Regex NEW_INDEX = new(@"\[[-,:\s\d]+\]");
            internal static readonly Regex NEW_INDEX_TOKEN = new(@"(-?\d*\s{0,1}:\s{0,1}-?\d*)|(-?\d+)");
            internal static readonly Regex NEW_INDEX_SPAN = new(@"(-?\d*)\s{0,1}:\s{0,1}(-?\d*)");
            
        }
    }
}
