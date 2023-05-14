using System.Text.RegularExpressions;

namespace JsonPathSerializer
{
    internal class SerializerGlobals
    {
        public static readonly Uri JSON_PATH_SERIALIZER_REPOSITORY_URL =
            new("https://github.com/ShuzhaoFeng/JsonPathSerializer");

        public class ErrorMessage
        {
            public static readonly string UNSUPPORTED_TOKEN = "The JsonPath contains an unsupported token." +
                                                        " Please check the documentation at " +
                                                        JSON_PATH_SERIALIZER_REPOSITORY_URL +
                                                        " for supported tokens.";
        }
        public class JsonPathRegex
        {
            public static readonly Regex PROPERTY_DOT = new Regex(@"\.([^\.\[]+)");
            public static readonly Regex PROPERTY_BRACKET = new(@"\[\'([^']+)\'\]");
            public static readonly Regex INDEX = new(@"\[(-?\d+)\]");
            public static readonly Regex INDEXES = new(@"\[(-?\d+,\s{0,1})+(-?\d+)\]");
            public static readonly Regex INDEX_SPAN =
                new(@"\[(-?\d*)\s{0,1}:\s{0,1}(-?\d*)\]");
        }
    }
}
