using JsonPathSerializer.Structs.Path;
using JsonPathSerializer.Structs.Types;
using JsonPathSerializer.Structs.Types.Index;
using JsonPathSerializer.Structs.Types.IndexSpan;
using Newtonsoft.Json.Linq;

namespace JsonPathSerializer.Utils;

internal class JsonPathValidator
{
    /// <summary>
    ///     Validate a JsonPath string and throws an exception whenever the string is invalid for the operation.
    /// </summary>
    /// <param name="path">JsonPath to validate.</param>
    public static void ValidateJsonPath(string path)
    {
        // Check for empty path
        if (path.Length < 1) throw new ArgumentException("Path cannot be empty");

        // Check for unsupported operations
        if (path.Contains("..")) throw new ArgumentException("Deep scan \'..\' is unsupported.");

        // If the string is not valid JsonPath, the following will throw a JsonException
        new JObject().SelectToken(path);
    }

    public static bool ArrayContainsIndex(JsonPathIndexToken token, int i, int count)
    {
        foreach (IValueContainer index in token.Indexes)
            if (index is IndexValueContainer indexValueContainer)
            {
                if (indexValueContainer.Index >= 0)
                {
                    if (indexValueContainer.Index == i) return true;
                }
                else
                {
                    if (indexValueContainer.Index + count == i) return true;
                }
            }
            else if (index is IndexSpanValueContainer indexSpanValueContainer)
            {
                int start = indexSpanValueContainer.StartIndex;
                int end = indexSpanValueContainer.EndIndex ??
                          (start < 0 ? -1 : count - 1);

                int min = Math.Min(start, end);
                int max = Math.Max(start, end);

                // if start >= 0 and end >= 0
                // then the acceptable range is [min, max]
                if (min >= 0)
                    if (i >= min && i <= max)
                        return true;

                // if start < 0 and end < 0
                // then the acceptable range is [count + min, count + max]
                if (max < 0)
                    if (i >= count + min && i <= count + max)
                        return true;

                // if start < 0 and end >= 0
                // then the acceptable range is [count + start, count - 1]
                // and [0, end]
                if (start < 0 && end >= 0)
                    if (i >= count + start || i <= end)
                        return true;

                // if start >= 0 and end < 0
                // then the acceptable range is [0, start]
                // and [count + end, count - 1]
                if (start >= 0 && end < 0)
                    if (i <= start || i >= count + end)
                        return true;
            }

        return false;
    }
}