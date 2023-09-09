using JsonPathSerializer.Structs;
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
            List<JsonPathToken> pathTokens,
            JToken root,
            object value
        )
        {
            // identify the JsonPathToken on the split point.
            JsonPathToken splitToken = pathTokens[lastAvailableToken.Index];

            // Generate a new JToken with all its child JsonPathTokens.

            List<JsonPathToken> unavailableTokens = pathTokens.GetRange
            (
                lastAvailableToken.Index + 1,
                pathTokens.Count - lastAvailableToken.Index - 1
            );

            JToken newToken = GenerateToken(unavailableTokens, value
                            ?? throw new ArgumentNullException(nameof(value)));

            // merge the new JToken into the root copy using the split JsonPathToken.
            switch (splitToken.Type)
            {
                case JsonPathToken.TokenType.Property:
                    JObject lastJObject = (JObject)lastAvailableToken.Token;
                    lastJObject[(string)splitToken.Value] = newToken;

                    break;

                case JsonPathToken.TokenType.Index:
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
                        else
                        {
                            lastAvailableToken.Token.Replace(lastJArray);
                        }
                    }

                    int index = (int)splitToken.Value;

                    if (index > 0)
                    {
                        for (int i = 0; i <= index - lastJArray.Count + 2; i++)
                        {
                            lastJArray.Add(new JObject());
                        }
                    }
                    else
                    {
                        for (int i = 0; i <= Math.Abs(index) - lastJArray.Count; i++)
                        {
                            lastJArray.Add(new JObject());
                        }
                    }

                    lastJArray[index < 0 ? lastJArray.Count + index : index] = newToken;

                    break;

                case JsonPathToken.TokenType.Indexes:

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

                    List<int> indexes = (List<int>)splitToken.Value;

                    for (int i = 0; i <= indexes.Select(Math.Abs).Max(); i++)
                    {
                        if (i >= lastJArray.Count)
                        {
                            lastJArray.Add(new JObject());
                        }
                    }

                    foreach (int i in indexes.Select(i => i < 0 ? lastJArray.Count + i : i))
                    {
                        lastJArray[i] = newToken;
                    }

                    break;

                case JsonPathToken.TokenType.IndexSpan:

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

                    IndexSpanValueContainer indexSpan = (IndexSpanValueContainer)splitToken.Value;

                    int realEnd = indexSpan.EndIndex ?? lastJArray.Count;

                    int max = Math.Max(indexSpan.StartIndex, realEnd);
                    int min = Math.Min(indexSpan.StartIndex, realEnd);

                    if (lastJArray.Count < max - min)
                    {
                        for (int i = lastJArray.Count; i <= max - min; i++)
                        {
                            lastJArray.Add(new JObject());
                        }
                    }

                    if (lastJArray.Count < Math.Max(Math.Abs(max), Math.Abs(min)))
                    {
                        for (int i = lastJArray.Count; i <= Math.Max(Math.Abs(max), Math.Abs(min)); i++)
                        {
                            lastJArray.Add(new JObject());
                        }
                    }

                    for (int i = indexSpan.StartIndex;
                         indexSpan.StartIndex > realEnd ? i >= realEnd : i <= realEnd;
                         i += indexSpan.StartIndex > realEnd ? -1 : 1)
                    {
                        lastJArray[i < 0 ? lastJArray.Count + i : i] = newToken;
                    }

                    break;

                default:
                    throw new NotSupportedException(SerializerGlobals.ErrorMessage.UNSUPPORTED_TOKEN);
            }

            return root;
        }

        /// <summary>
        /// Generate a nested JToken from a list of JsonPathTokens.
        /// </summary>
        /// <param name="jsonPathTokens">List of JsonPathTokens to generate.</param>
        /// <param name="value">Value of the leaf element.</param>
        /// <returns></returns>
        private static JToken GenerateToken(List<JsonPathToken> jsonPathTokens, object value)
        {
            // Create leaf element with given value.
            JToken jToken = JToken.FromObject(value);

            // Build JToken bottom-up, starting from the leaf element.
            for (int i = jsonPathTokens.Count; i > 0; i--)
            {
                JsonPathToken jsonPathToken = jsonPathTokens[i - 1];

                switch (jsonPathToken.Type)
                {
                    case JsonPathToken.TokenType.Property:
                        jToken = new JObject { [(string)jsonPathToken.Value] = jToken };

                        break;

                    case JsonPathToken.TokenType.Index:
                        int index = (int)jsonPathToken.Value;

                        // if index is positive (e.g. [3]), then we need to insert {index} empty elements before inserting the value.
                        // if index is negative (e.g. [-3]), then we need to insert -{index} - 1 empty elements after inserting the value.
                        int numOfEmptyElements = index >= 0 ? index : - index - 1;

                        JArray jArray = new JArray(Enumerable.Repeat(new JObject(), numOfEmptyElements));

                        if (index > 0)
                        {
                            jArray.Add(jToken);
                        }
                        else
                        {
                            jArray.Insert(0, jToken);
                        }

                        jToken = jArray;

                        break;

                    case JsonPathToken.TokenType.Indexes:

                        jArray = new JArray();
                        List<int> indexes = (List<int>)jsonPathToken.Value;

                        // if index is positive (e.g. [3]), then the minimum index required is {index}
                        // if index is negative (e.g. [-3]), then the minimum index required -{index} - 1.
                        // the minimum index required for all indexes is thus the maximum of those.
                        int bound = indexes.Select(ind => ind >= 0 ? ind : - ind - 1).Max();

                        // convert the list of indexes into positive indexes.
                        indexes = indexes.Select(ind => ind >= 0 ? ind : bound + ind).ToList();

                        // insert jToken into all slots specified by the indexes.
                        for (int j = 0; j <= bound; j++)
                        {
                            jArray.Add(indexes.Contains(j) ? jToken : new JObject());
                        }

                        jToken = jArray;

                        break;

                    case JsonPathToken.TokenType.IndexSpan:
                        IndexSpanValueContainer indexSpan = (IndexSpanValueContainer)jsonPathToken.Value;

                        // if the end value is not provided (e.g. [1:], which to be fair doesn't make too much sense in the context of building a new JSON)
                        // then it is defined to be 0.
                        // start value is simpler (always 0 if not provided), so it is already handled.
                        int start = indexSpan.StartIndex;
                        int end = indexSpan.EndIndex ?? 0;

                        // if the two values are positive (e.g. [3:5]), then we need Count > max(start, end), where the first min(start, end) elements are empty.
                        // if the two values are negative (e.g. [-5:-3]), then we need Count >= max(-start, -end), where the last min(-start, -end) - 1 elements are empty.
                        // if the two values have different signs (e.g. [-5:3]), then we need Count > their difference + 1, with no empty elements.
                        if (start >= 0 && end >= 0) // both positive
                        {
                            jArray = new JArray(Enumerable.Repeat(new JObject(), Math.Min(start, end)));

                            for (int j = Math.Min(start, end); j < Math.Max(start, end) + 1; j++)
                            {
                                jArray.Add(jToken);
                            }
                        }
                        else if (start < 0 && end < 0) // both negative
                        {
                            jArray = new JArray(Enumerable.Repeat(new JObject(), Math.Min(-start, -end) - 1));

                            for (int j = Math.Min(-start, -end); j <= Math.Max(-start, -end); j++)
                            {
                                jArray.Insert(0, jToken);
                            }
                        }
                        else // different signs
                        {
                            jArray = new JArray(Enumerable.Repeat(jToken, Math.Abs(start - end) + 1));
                        }

                        jToken = jArray;

                        break;

                    default:
                        throw new NotSupportedException(SerializerGlobals.ErrorMessage.UNSUPPORTED_TOKEN);
                }
            }

            return jToken;
        }
    }
}
