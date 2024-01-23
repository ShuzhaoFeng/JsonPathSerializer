namespace JsonPathSerializer.Globals;

/// <summary>
///    Stores the error messages used in the library.
/// </summary>
    internal class ErrorMessage
    {
        /// <summary>
        ///    The error message for a generic JsonPathSerializerException.
        /// </summary>
        internal static readonly string Generic = "An error occurred while performing the operation.";

        /// <summary>
        ///   A token in the path cannot be identified to a supported token.
        /// </summary>
        internal static readonly string UnsupportedToken = "The JsonPath contains an unsupported token." +
                                                            " Please check the documentation at " +
                                                            JsonPathUri.JsonPathSerializerRepositoryUri +
                                                            " for supported tokens.";
        /// <summary>
        ///     Attempted to perform the append operation on a non-array token.
        /// </summary>
        internal static readonly string AppendToNonArray = "Cannot append value to a non-array token.";

        /// <summary>
        ///    The operation requires a certain token to be of a certain type,
        ///    but the token at the location specified is of a different type.
        /// </summary>
        /// <param name="expected">The expected type.</param>
        /// <param name="actual">The observed type.</param>
        /// <returns></returns>
        internal static string TypeConflict(string expected, string actual) =>
            $"Expected type {expected}, found {actual} instead.";
        /// <summary>
        ///    The operation would potentially replace a token,
        ///    causing it and its children to be lost.
        /// </summary>
        /// <param name="path">The path that will be overridden.</param>
        /// <returns></returns>
        internal static string Override(string path) =>
            $"The token at at $.{path} contains other elements, overriding it may cause loss of information.";
}
