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
            internal static readonly Regex PROPERTY = new(@"\.([^\.\[]+)|\[\'([^']+)\'\]");
            internal static readonly Regex INDEX = new(@"\[[-,:\s\d]+\]");
            internal static readonly Regex INDEX_TOKEN = new(@"(-?\d*\s{0,1}:\s{0,1}-?\d*)|(-?\d+)");
            internal static readonly Regex INDEX_SPAN = new(@"(-?\d*)\s{0,1}:\s{0,1}(-?\d*)");
        }
    }
}
