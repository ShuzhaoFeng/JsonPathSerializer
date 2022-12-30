using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonPathSerializer.Structs;
using JsonPathSerializer.Utils;

namespace JsonPathSerializer;

public class JsonPathManager : IJsonPathManager
{
    /// <summary>
    /// The root Json object.
    /// </summary>
    private JToken? _root;

    /// <summary>
    /// Returns a read-only instance of root.
    /// </summary>
    public IJEnumerable<JToken> Value => _root ?? new JObject();

    /// <summary>
    /// Serialize a list of path-value pairs into a Json string.
    /// </summary>
    /// <param name="jsonPathToValues">The list of path-value pair to serialize.</param>
    /// <returns>The serialized string.</returns>
    public static string SerializeAll(IEnumerable<(string, object)> jsonPathToValues)
    {
        JsonPathManager manager = new();

        foreach ((string, object) jsonPathToValue in jsonPathToValues)
        {
            manager.Add(jsonPathToValue.Item1, jsonPathToValue.Item2);
        }

        return manager.Build();
    }

    public JsonPathManager()
    {
    }

    public JsonPathManager(JToken root)
    {
        _root = root;
    }

    public JsonPathManager(string root)
    {
        _root = JToken.Parse(root);
    }

    public void Clear()
    {
        _root = null;
    }

    public string Build()
    {
        return JsonConvert.SerializeObject(_root ?? new JObject());
    }

    public void Add(string path, object value)
    {
        // Verify the path is a valid JsonPath for the operation.
        JsonPathValidator.ValidateJsonPath(path.Trim());

        // Tokenize the JsonPath.

        List<JsonPathToken> pathTokens = JsonPathTokenizer.Tokenize(path.Trim());

        if (pathTokens.Count < 1)
        {
            throw new ArgumentException("There is no valid JsonPath element in the string.");
        }

        // Make a copy of root.
        JToken rootCopy = _root?.DeepClone()
                          ?? (pathTokens[0].Type is JsonPathToken.TokenType.Index ? new JArray() : new JObject());

        // get the list of last available tokens that already exist within the root.
        foreach (JsonNodeToken lastAvailableToken in JsonNodeTokenCollector.CollectLastAvailableTokens(rootCopy, pathTokens))
        {
            // Check for conflicting types.
            
            if (lastAvailableToken.Token is not JContainer)
            {
                throw new ArgumentException
                (
                    $"JSON element $.{lastAvailableToken.Token.Path} " +
                    "contains a value, therefore cannot contain other child elements."
                );
            }

            if (lastAvailableToken.Token.HasValues)
            {
                switch (lastAvailableToken.Token)
                {
                    case JArray when pathTokens[lastAvailableToken.Index].Type < JsonPathToken.TokenType.Index:
                        throw new ArgumentException
                        (
                            $"JSON element $.{lastAvailableToken.Token.Path} " +
                            "is a JArray, therefore cannot be taken as a JObject."
                        );

                    case JObject when pathTokens[lastAvailableToken.Index].Type >= JsonPathToken.TokenType.Index:
                        throw new ArgumentException
                        (
                            $"JSON element $.{lastAvailableToken.Token.Path} " +
                            "is a JObject, therefore cannot be taken as a JArray."
                        );
                }
            }

            // identify the split JsonPathToken.
            JsonPathToken splitToken = pathTokens[lastAvailableToken.Index];

            // Generate a new JToken with the rest of JsonPathTokens.

            List<JsonPathToken> unavailableTokens = pathTokens.GetRange
            (
                lastAvailableToken.Index + 1,
                pathTokens.Count - lastAvailableToken.Index - 1
            );

            JToken newToken = JTokenGenerator.GenerateToken(unavailableTokens, value);

            // merge the new JToken into the root copy using the split JsonPathToken.
            switch (splitToken.Type)
            {
                case JsonPathToken.TokenType.Property:
                    JObject lastJObject = (JObject) lastAvailableToken.Token;
                    lastJObject[(string) splitToken.Value] = newToken;

                    break;

                case JsonPathToken.TokenType.Index:
                    JArray lastJArray;
                    
                    if (lastAvailableToken.Token.HasValues)
                    {
                        lastJArray = (JArray) lastAvailableToken.Token;
                    }
                    else // empty JObject
                    {
                        lastJArray = new JArray();
                        lastAvailableToken.Token.Replace(lastJArray);
                    }
                    
                    int index = (int) splitToken.Value;
                    
                    for (int i = 0; i < index - lastJArray.Count + 2; i++)
                    {
                        lastJArray.Add(new JObject());
                    }
                    
                    lastJArray[index] = newToken;

                    break;

                case JsonPathToken.TokenType.Indexes: case JsonPathToken.TokenType.IndexSpan:

                    if (lastAvailableToken.Token.HasValues)
                    {
                        lastJArray = (JArray) lastAvailableToken.Token;
                    }
                    else // empty JObject
                    {
                        lastJArray = new JArray();

                        if (lastAvailableToken.Token.Parent is not null)
                        {
                            lastAvailableToken.Token.Replace(lastJArray);
                        }
                    }
                    
                    List<int> indexes = (List<int>) splitToken.Value;
                    
                    for (int i = 0; i < indexes.Max() + 1; i++)
                    {
                        if (i < lastJArray.Count) // Array already contains the index i.
                        {
                            if (indexes.Contains(i)) lastJArray[i] = newToken;
                        }
                        else // Array does not contain the index i.
                        {
                            lastJArray.Add(indexes.Contains(i) ? newToken : new JObject());
                        }
                    }

                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        // assign root copy back to root.
        _root = rootCopy;
    }
}