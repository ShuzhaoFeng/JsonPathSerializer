namespace JsonPathSerializer.Globals;
    internal class ErrorMessage
    {
        internal static readonly string Generic = "An error occurred while performing the operation.";

        internal static readonly string UnsupportedToken = "The JsonPath contains an unsupported token." +
                                                            " Please check the documentation at " +
                                                            JsonPathUri.JsonPathSerializerRepositoryUri +
                                                            " for supported tokens.";

        internal static readonly string AppendToNonArray = "Cannot append value to a non-array token.";

        internal static string TypeConflict(string expected, string actual) =>
            $"Expected type {expected}, found {actual} instead.";

        internal static string Override(string path) =>
            $"The token at at $.{path} contains other elements, overriding it may cause loss of information.";
}
