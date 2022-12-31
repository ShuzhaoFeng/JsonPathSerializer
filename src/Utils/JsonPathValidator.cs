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

            if (path.StartsWith("$.") && path.Length < 3)
            {
                throw new ArgumentException("Path starting with $. cannot be empty");
            }

            // Check for unsupported operations
            if (path.Contains(".."))
            {
                throw new ArgumentException("Deep scan \'..\' is unsupported.");
            }

            // Check for invalid JsonPath string format
            new JObject().SelectToken(path);
        }
    }
}
