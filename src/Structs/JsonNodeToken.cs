using Newtonsoft.Json.Linq;

namespace JsonPathSerializer.Structs
{
    class JsonNodeToken
    {
        /// <summary>
        /// Json node.
        /// </summary>
        public JToken Token { get; }

        /// <summary>
        /// Depth of the Json node.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Specify whether the token is the last node that already exists in the root.
        /// </summary>
        public bool IsLastAvailableToken { get; private set; } = false;

        public JsonNodeToken(JToken token, int index)
        {
            Token = token;
            Index = index;
        }

        /// <summary>
        /// Set the token to the last node that already exists in the root.
        /// </summary>
        public JsonNodeToken AsLastAvailable()
        {
            IsLastAvailableToken = true;
            return this;
        }
    }
}
