﻿using JsonPathSerializer.Structs;
using Newtonsoft.Json.Linq;

namespace JsonPathSerializer.Utils
{
    class JsonPathValidator
    {
        /// <summary>
        /// Validate a JsonPath string and throws an exception whenever the string is invalid for the operation.
        /// </summary>
        /// <param name="path">JsonPath to validate.</param>
        public static void ValidateJsonPath(string path)
        {

            // Check for empty path
            if (path.Length < 1)
            {
                throw new ArgumentException("Path cannot be empty");
            }

            // Check for unsupported operations
            if (path.Contains(".."))
            {
                throw new ArgumentException("Deep scan \'..\' is unsupported.");
            }

            // Check for invalid JsonPath string format
            new JObject().SelectToken(path);
        }

        /// <summary>
        /// Check if a JsonPathToken has array type, i.e. it will contain indexed values.
        /// </summary>
        /// <param name="token">The JsonPathToken to check.</param>
        /// <returns>True if the token is an array type, false otherwise.</returns>
        public static bool IsArray(JsonPathToken token)
        {
            switch (token.Type)
            {
                case JsonPathToken.TokenType.Index:
                case JsonPathToken.TokenType.Indexes:
                case JsonPathToken.TokenType.IndexSpan:
                    return true;
                default:
                    return false;
            }
        }
    }
}
