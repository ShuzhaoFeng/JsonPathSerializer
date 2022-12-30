namespace JsonPathSerializer.Structs
{
    class JsonPathToken
    {
        /// <summary>
        /// Defines the type of information within a JsonPathToken.
        /// </summary>
        public enum TokenType
        {
            Unknown = 999,
            // JObject types
            Property,
            // JArray types
            Index,
            Indexes,
            IndexSpan
        }
        
        /// <summary>
        /// The value in a JsonPathToken.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// The type of information in a JsonPathToken.
        /// </summary>
        public TokenType Type { get; }

        /// <summary>
        /// A node in a JsonPath string.
        /// </summary>
        /// <param name="value">The value of the JsonPath node.</param>
        /// <param name="type">The type of JsonPath node.</param>
        public JsonPathToken(object value, TokenType type)
        {
            Value = value;
            Type = type;
        }
    }
}
