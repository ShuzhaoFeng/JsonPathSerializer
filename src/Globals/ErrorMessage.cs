namespace JsonPathSerializer.Globals;
    internal class ErrorMessage
    {
        internal static readonly string GENERIC = "An error occurred while performing the operation.";

        internal static readonly string UNSUPPORTED_TOKEN = "The JsonPath contains an unsupported token." +
                                                            " Please check the documentation at " +
                                                            JsonPathUri.JSON_PATH_SERIALIZER_REPOSITORY_URI +
                                                            " for supported tokens.";
    }
