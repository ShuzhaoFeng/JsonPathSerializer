using JsonPathSerializer.Structs;
using JsonPathSerializer.Structs.Path;
using JsonPathSerializer.Structs.Types;
using JsonPathSerializer.Structs.Types.Index;
using JsonPathSerializer.Structs.Types.IndexSpan;
using Newtonsoft.Json.Linq;

namespace JsonPathSerializer.Utils
{
    class JTokenGenerator
    {
        /// <summary>
        /// Generate a new root JToken by adding a value at the specified path.
        /// </summary>
        /// <param name="lastAvailableToken">Specify the available tokens.</param>
        /// <param name="pathTokens">Specify the path to insert the value.</param>
        /// <param name="root">The current root.</param>
        /// <param name="value">The value to be inserted at the specified path.</param>
        /// <returns>A new JToken with the old root and the new value at the specified path.</returns>
        public static JToken GenerateNewRoot
        (
            JsonNodeToken lastAvailableToken,
            List<IJsonPathToken> pathTokens,
            JToken root,
            object value
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
                    JObject lastJObject = (JObject)lastAvailableToken.Token;
                    lastJObject[propertyToken.Property] = newToken;

                    break;

                case JsonPathIndexToken indexToken:
                    JArray lastJArray;

                    if (lastAvailableToken.Token.HasValues)
                    {
                        lastJArray = (JArray)lastAvailableToken.Token;
                    }
                    else // empty JObject
                    {
                        lastJArray = new JArray();

                        if (lastAvailableToken.Index == 0)
                        {
                            root = lastJArray;
                        }
                        else if (lastAvailableToken.Token.Parent is not null)
                        {
                            lastAvailableToken.Token.Replace(lastJArray);
                        }
                    }

                    // fill the JArray with empty elements up to the minimum index bound required.
                    for (int i = lastJArray.Count; i < indexToken.Bound; i++)
                    {
                        lastJArray.Add(new JObject());
                    }

                    foreach (IValueContainer container in indexToken.Indexes)
                    {
                        if (container is IndexValueContainer indexValueContainer)
                        {
                            int index = indexValueContainer.Index;

                            lastJArray[index >= 0 ? index : lastJArray.Count + index] = newToken;
                        }
                        else if (container is IndexSpanValueContainer indexSpanValueContainer)
                        {
                            int start = indexSpanValueContainer.StartIndex;
                            int end = indexSpanValueContainer.EndIndex ??
                                      (start < 0 ? -1 : lastJArray.Count - 1);

                            int min = Math.Min(start, end);
                            int max = Math.Max(start, end);

                            for (int i = min; i <= max; i++)
                            {
                                lastJArray[i >= 0 ? i : lastJArray.Count + i] = newToken;
                            }
                        }
                    }

                    break;

                default:
                    throw new NotSupportedException(Globals.ErrorMessage.UNSUPPORTED_TOKEN);
            }

            return root;
        }

        /// <summary>
        /// Generate a nested JToken from a list of JsonPathTokens.
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
                        for (int j = 0; j < indexToken.Bound; j++)
                        {
                            jArray.Add(new JObject());
                        }

                        foreach (IValueContainer container in indexToken.Indexes)
                        {
                            if (container is IndexValueContainer indexValueContainer)
                            {
                                int index = indexValueContainer.Index;

                                jArray[index >= 0 ? index : jArray.Count + index] = jToken;
                            }
                            else if (container is IndexSpanValueContainer indexSpanValueContainer)
                            {
                                int start = indexSpanValueContainer.StartIndex;
                                int end = indexSpanValueContainer.EndIndex ??
                                          (start < 0 ? -1 : jArray.Count - 1);

                                int min = Math.Min(start, end);
                                int max = Math.Max(start, end);

                                for (int j = min; j <= max; j++)
                                {
                                    jArray[j >= 0 ? j : jArray.Count + j] = jToken;
                                }
                            }
                        }

                        jToken = jArray;

                        break;

                    default:
                        throw new NotSupportedException(Globals.ErrorMessage.UNSUPPORTED_TOKEN);
                }
            }

            return jToken;
        }
    }
}
