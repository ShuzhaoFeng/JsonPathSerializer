using JsonPathSerializer.Structs;
using Newtonsoft.Json.Linq;

namespace JsonPathSerializer.Utils
{
    class JTokenGenerator
    {
        /// <summary>
        /// Generate a nested JToken from a list of JsonPathTokens.
        /// </summary>
        /// <param name="jsonPathTokens">List of JsonPathTokens to generate.</param>
        /// <param name="value">Value of the leaf element.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static JToken GenerateToken(List<JsonPathToken> jsonPathTokens, object value)
        {
            // Create leaf element with given value.
            JToken jToken = JToken.FromObject(value);

            // Generate JToken bottom-up, starting from the leaf element.
            for (int i = jsonPathTokens.Count; i > 0; i--)
            {
                JsonPathToken jsonPathToken = jsonPathTokens[i - 1];

                switch (jsonPathToken.Type)
                {
                    case JsonPathToken.TokenType.Property:
                        jToken = new JObject { [(string)jsonPathToken.Value] = jToken };

                        break;

                    case JsonPathToken.TokenType.Index:
                        JArray jArray = new JArray { jToken };

                        // insert empty slots.
                        for (int j = 0; j < (int)jsonPathToken.Value; j++)
                        {
                            jArray.Insert(0, new JObject());
                        }

                        jToken = jArray;

                        break;

                    case JsonPathToken.TokenType.Indexes:

                        jArray = new JArray();
                        List<int> indexes = (List<int>)jsonPathToken.Value;

                        // insert jToken into all slots specified by the indexes.
                        for (int j = 0; j < indexes.Max() + 1; j++)
                        {
                            jArray.Add(indexes.Contains(j) ? jToken : new JObject());
                        }

                        jToken = jArray;

                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            return jToken;
        }
    }
}
