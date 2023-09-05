﻿using JsonPathSerializer.Structs;
using JsonPathSerializer.Structs.Types.IndexSpan;
using JsonPathSerializer.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        foreach ((string, object) jsonPathToValue in jsonPathToValues
                                                     ?? throw new ArgumentNullException(nameof(jsonPathToValues)))
        {
            manager.Add(jsonPathToValue.Item1, jsonPathToValue.Item2);
        }

        return manager.Build();
    }

    /// <summary>
    /// Serialize a collection of key-value pairs into a Json string.
    /// </summary>
    /// <param name="jsonPathToValues">The key-value pairs to serialize.</param>
    /// <returns>The serialized string.</returns>
    public static string SerializeAll(ICollection<KeyValuePair<string, object>> jsonPathToValues)
    {
        JsonPathManager manager = new();

        foreach (KeyValuePair<string, object> jsonPathToValue in jsonPathToValues
                ?? throw new ArgumentNullException(nameof(jsonPathToValues)))
        {
            manager.Add(jsonPathToValue.Key, jsonPathToValue.Value);
        }

        return manager.Build();
    }

    /// <summary>
    /// Create a new JsonPathManager instance with an empty root.
    /// </summary>
    public JsonPathManager()
    {
    }

    /// <summary>
    /// Create a new JsonPathManager instance with the input token as root.
    /// </summary>
    /// <param name="root">The root token.</param>
    public JsonPathManager(JToken root)
    {
        _root = root ?? throw new ArgumentNullException(nameof(root));
    }

    /// <summary>
    /// Create a new JsonPathManager instance with the input string as root.
    /// Will throw an exception if the root string is not a valid Json string.
    /// </summary>
    /// <param name="root">The root string.</param>
    public JsonPathManager(string root)
    {
        _root = JToken.Parse(root ?? throw new ArgumentNullException(nameof(root)));
    }

    /// <summary>
    /// Clear the JsonPathManager root.
    /// </summary>
    public void Clear()
    {
        _root = null;
    }

    /// <summary>
    /// Build the Json string from the JsonPathManager root.
    /// </summary>
    /// <returns>A Json string representing the root.</returns>
    public string Build()
    {
        return JsonConvert.SerializeObject(_root ?? new JObject());
    }

    /// <summary>
    /// Add a value to the JsonPathManager root.
    /// </summary>
    /// <param name="path">The path where to add the value.</param>
    /// <param name="value">The value to be added.</param>
    public void Add(string path, object value)
    {
        // Verify the path is a valid JsonPath for the operation.
        JsonPathValidator.ValidateJsonPath((path ?? throw new ArgumentNullException(nameof(path))).Trim());

        try
        {
            var result = _root?.SelectTokens(path);

            if (result != null)
            {
                foreach (var resultView in result)
                {
                    if (resultView is JContainer) // there exists a JSON child at the location that will be overriden.
                    {
                        throw new ArgumentException($"JSON element {path} contains a JSON child element," +
                                                    $"therefore cannot contain a value.");
                    }
                }
            }
        }
        catch (ArgumentOutOfRangeException) // indicates a negative index, which SelectTokens can't handle
        {
            // TODO: find a way to validate negative index
        }

        // Tokenize the JsonPath.
        List<JsonPathToken> pathTokens = JsonPathTokenizer.Tokenize(path.Trim());

        if (pathTokens.Count < 1)
        {
            throw new ArgumentException("There is no valid JsonPath element in the string.");
        }

        // Make a copy of root.
        JToken rootCopy = _root?.DeepClone()
                          ?? (JsonPathValidator.IsArray(pathTokens[0]) ? new JArray() : new JObject());

        // get the list of last available tokens that already exist within the root.
        foreach (JsonNodeToken lastAvailableToken in
                 JsonNodeTokenCollector.CollectLastAvailableTokens(rootCopy, pathTokens))
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

            // identify the JsonPathToken on the split point.
            JsonPathToken splitToken = pathTokens[lastAvailableToken.Index];

            // Generate a new JToken with all its child JsonPathTokens.

            List<JsonPathToken> unavailableTokens = pathTokens.GetRange
            (
                lastAvailableToken.Index + 1,
                pathTokens.Count - lastAvailableToken.Index - 1
            );

            JToken newToken = JTokenGenerator.GenerateToken(unavailableTokens, value
                            ?? throw new ArgumentNullException(nameof(value)));

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

                        if (lastAvailableToken.Index == 0)
                        {
                            rootCopy = lastJArray;
                        }
                        else
                        {
                            lastAvailableToken.Token.Replace(lastJArray);
                        }
                    }

                    int index = (int) splitToken.Value;

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
                        lastJArray = (JArray) lastAvailableToken.Token;
                    }
                    else // empty JObject
                    {
                        lastJArray = new JArray();

                        if (lastAvailableToken.Index == 0)
                        {
                            rootCopy = lastJArray;
                        }
                        else if (lastAvailableToken.Token.Parent is not null)
                        {
                            lastAvailableToken.Token.Replace(lastJArray);
                        }
                    }

                    List<int> indexes = (List<int>) splitToken.Value;

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
                            rootCopy = lastJArray;
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
        }

        // assign root copy back to root.
        _root = rootCopy;
    }
}