using JsonPathSerializer.Structs;
using JsonPathSerializer.Structs.Path;
using JsonPathSerializer.Structs.Types;
using JsonPathSerializer.Structs.Types.Index;
using JsonPathSerializer.Structs.Types.IndexSpan;
using Newtonsoft.Json.Linq;
using static JsonPathSerializer.Globals;

namespace JsonPathSerializer.Utils;

/// <summary>
///     Collection of helper methods to generate a new JToken from a given path and value.
/// </summary>
internal class JTokenGenerator
{
    /// <summary>
    ///     Generate a new root JToken by adding a value at the specified path.
    /// </summary>
    /// <param name="lastAvailableToken">Specify the available tokens.</param>
    /// <param name="pathTokens">Specify the path to insert the value.</param>
    /// <param name="root">The current root.</param>
    /// <param name="value">The value to be inserted at the specified path.</param>
    /// <param name="priority">The priority of the operation.</param>
    /// <returns>A new JToken with the old root and the new value at the specified path.</returns>
    public static JToken GenerateNewRoot
(
    JsonNodeToken lastAvailableToken,
    List<IJsonPathToken> pathTokens,
    JToken root,
    object value,
    Priority priority
)
    {
        // identify the JsonPathToken on the split point.
        IJsonPathToken splitToken = pathTokens[lastAvailableToken.Index];

        // Generate a new JToken with all its child JsonPathTokens.

        List<IJsonPathToken> unavailableTokens = pathTokens.GetRange
        (
            lastAvailableToken.Index + 1,
            pathTokens.Count - lastAvailableToken.Index - 1
        );

        JToken newToken = GenerateToken(unavailableTokens, value
                                                           ?? throw new ArgumentNullException(nameof(value)));

        // merge the new JToken into the root copy using the split JsonPathToken.
        switch (splitToken)
        {
            case JsonPathPropertyToken propertyToken:

                // check if the split token is not a JObject,
                // which would fail the operation.
                if (lastAvailableToken.Token is not JObject)
                {
                    if (priority < Priority.High)
                    {
                        throw new ArgumentException("JSON element $." +
                                                    $"{lastAvailableToken.Token.Path} contains a value," +
                                                    "therefore cannot contain child elements.");
                    }

                    JObject emptyJObject = new();
                    lastAvailableToken.Token.Replace(emptyJObject);
                    lastAvailableToken.Token = emptyJObject;
                }

                // check if the corresponding location to add a value contains child elements,
                // which would be overwritten by the new value.
                if (unavailableTokens.Count == 0)
                {
                    // replacing a value that contains child elements
                    if (priority < Priority.High
                        && lastAvailableToken.Token[propertyToken.Property] is JContainer jContainer)
                    {
                        throw new ArgumentException("JSON element $." +
                                                    $"{jContainer.Path} contains a child element," +
                                                    "therefore cannot be replaced.");
                    }

                    // replacing an existing value
                    if (priority < Priority.Normal
                        && lastAvailableToken.Token[propertyToken.Property] is not null)
                    {
                        throw new ArgumentException("JSON element $." +
                            $"{lastAvailableToken.Token.Path}.{propertyToken.Property} contains an element," +
                            "therefore cannot be replaced.");
                    }
                }

                JObject lastJObject = (JObject)lastAvailableToken.Token;
                lastJObject[propertyToken.Property] = newToken;

                break;

            case JsonPathIndexToken indexToken:
                // check if the split token is not a JArray,
                // which would fail the operation.
                if (lastAvailableToken.Token is not JArray)
                {
                    if (priority < Priority.High)
                    {
                        throw new ArgumentException("JSON element $." +
                                                    $"{lastAvailableToken.Token.Path} contains a value," +
                                                    "therefore cannot contain child elements.");
                    }

                    JArray emptyJArray = new();
                    lastAvailableToken.Token.Replace(emptyJArray);
                    lastAvailableToken.Token = emptyJArray;
                }

                JArray lastJArray;

                if (lastAvailableToken.Token.HasValues)
                {
                    lastJArray = (JArray)lastAvailableToken.Token;
                }
                else // empty JObject that has been added as placeholder.
                {
                    lastJArray = new JArray();

                    if (lastAvailableToken.Index == 0) // if at root level, directly replace the root.
                        root = lastJArray;
                    else if (lastAvailableToken.Token.Parent is not null) lastAvailableToken.Token.Replace(lastJArray);
                }

                if (unavailableTokens.Count == 0)

                    for (int i = 0; i < lastJArray.Count; i++)
                    {
                        bool arrayContainsIndex = JsonPathValidator.ArrayContainsIndex(indexToken, i, lastJArray.Count);

                        // check if the corresponding location to add a value contains child elements,
                        // which would be overwritten by the new value.
                        if (priority < Priority.High && arrayContainsIndex
                                                     && lastAvailableToken.Token[i] is JContainer jContainer)
                        {
                            throw new ArgumentException("JSON element $." +
                                $"{jContainer.Path} contains a child element," +
                                "therefore cannot be replaced.");
                        }

                        if (priority < Priority.Normal && arrayContainsIndex
                                                       && lastAvailableToken.Token[i] is not null
                                                       && lastAvailableToken.Token[i]!.HasValues)
                        {
                            throw new ArgumentException("JSON element $." +
                                $"{lastAvailableToken.Token.Path}[{i}] contains an element," +
                                "therefore cannot be replaced.");
                        }
                    }

                // fill the JArray with empty elements up to the minimum index bound required.
                for (int i = lastJArray.Count; i < indexToken.Bound; i++) lastJArray.Add(new JObject());

                foreach (IValueContainer container in indexToken.Indexes)
                    // replace the element at the specified index/index span with the new value.

                    if (container is IndexValueContainer indexValueContainer)
                    {
                        int index = indexValueContainer.Index;

                        // if the index is negative, count the index from the end of the array.
                        lastJArray[index >= 0 ? index : lastJArray.Count + index] = newToken;
                    }
                    else if (container is IndexSpanValueContainer indexSpanValueContainer)
                    {
                        int start = indexSpanValueContainer.StartIndex;

                        // if the end index is not specified, it is defaulted the last index of the array.
                        int end = indexSpanValueContainer.EndIndex ??
                                  (start < 0 ? -1 : lastJArray.Count - 1);

                        int min = Math.Min(start, end);
                        int max = Math.Max(start, end);

                        // replace the value across the span.
                        for (int i = min; i <= max; i++) lastJArray[i >= 0 ? i : lastJArray.Count + i] = newToken;
                    }

                break;

            default:
                throw new NotSupportedException(ErrorMessage.UNSUPPORTED_TOKEN);
        }

        return root;
    }

    /// <summary>
    ///     Generate a nested JToken from a list of JsonPathTokens.
    /// </summary>
    /// <param name="jsonPathTokens">List of JsonPathTokens to generate.</param>
    /// <param name="value">Value of the leaf element.</param>
    /// <returns></returns>
    private static JToken GenerateToken(List<IJsonPathToken> jsonPathTokens, object value)
    {
        // Create leaf element with given value.
        JToken jToken = JToken.FromObject(value);

        // Build JToken bottom-up, starting from the leaf element.
        for (int i = jsonPathTokens.Count; i > 0; i--)
        {
            IJsonPathToken jsonPathToken = jsonPathTokens[i - 1];

            switch (jsonPathToken)
            {
                case JsonPathPropertyToken propertyToken:
                    jToken = new JObject { [propertyToken.Property] = jToken };

                    break;

                case JsonPathIndexToken indexToken:
                    JArray jArray = new JArray();

                    // fill the JArray with empty elements up to the minimum index bound required.
                    for (int j = 0; j < indexToken.Bound; j++) jArray.Add(new JObject());

                    foreach (IValueContainer container in indexToken.Indexes)
                        if (container is IndexValueContainer indexValueContainer)
                        {
                            int index = indexValueContainer.Index;

                            // if the index is negative, count the index from the end of the array.
                            jArray[index >= 0 ? index : jArray.Count + index] = jToken;
                        }
                        else if (container is IndexSpanValueContainer indexSpanValueContainer)
                        {
                            int start = indexSpanValueContainer.StartIndex;

                            // if the end index is not specified, it is defaulted the last index of the array.
                            int end = indexSpanValueContainer.EndIndex ??
                                      (start < 0 ? -1 : jArray.Count - 1);

                            int min = Math.Min(start, end);
                            int max = Math.Max(start, end);

                            // replace the value across the span.
                            for (int j = min; j <= max; j++) jArray[j >= 0 ? j : jArray.Count + j] = jToken;
                        }

                    jToken = jArray;

                    break;

                default:
                    throw new NotSupportedException(ErrorMessage.UNSUPPORTED_TOKEN);
            }
        }

        return jToken;
    }
}