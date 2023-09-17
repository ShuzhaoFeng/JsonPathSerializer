using JsonPathSerializer.Structs;
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
    public static string SerializeAll(IEnumerable<KeyValuePair<string, object>> jsonPathToValues)
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

        // Make a copy of root.
        JToken rootCopy = _root?.DeepClone()
                          ?? (JsonPathValidator.IsIndex(pathTokens[0]) ? new JArray() : new JObject());

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
                    case JArray when !JsonPathValidator.IsIndex(pathTokens[lastAvailableToken.Index]):
                        throw new ArgumentException
                        (
                            $"JSON element $.{lastAvailableToken.Token.Path} " +
                            "is a JArray, therefore cannot be taken as a JObject."
                        );

                    case JObject when JsonPathValidator.IsIndex(pathTokens[lastAvailableToken.Index]):
                        throw new ArgumentException
                        (
                            $"JSON element $.{lastAvailableToken.Token.Path} " +
                            "is a JObject, therefore cannot be taken as a JArray."
                        );
                }
            }

            rootCopy = JTokenGenerator.GenerateNewRoot(lastAvailableToken, pathTokens, rootCopy, value);
        }

        // assign root copy back to root.
        _root = rootCopy;
    }

    /// <summary>
    /// Force add a value to the JsonPathManager root, regardless of whether existing values will be overriden.
    /// </summary>
    /// <param name="path">The path where to add the value.</param>
    /// <param name="value">The value to be added.</param>
    public void Force(string path, object value)
    {
        // Verify the path is a valid JsonPath for the operation.
        JsonPathValidator.ValidateJsonPath((path ?? throw new ArgumentNullException(nameof(path))).Trim());

        // Tokenize the JsonPath.
        List<JsonPathToken> pathTokens = JsonPathTokenizer.Tokenize(path.Trim());

        // Make a copy of root.
        JToken rootCopy = _root?.DeepClone()
                          ?? (JsonPathValidator.IsIndex(pathTokens[0]) ? new JArray() : new JObject());

        // get the list of last available tokens that already exist within the root.
        foreach (JsonNodeToken lastAvailableToken in
                 JsonNodeTokenCollector.CollectLastAvailableTokens(rootCopy, pathTokens))
        {
            if (lastAvailableToken.Token is not JContainer)
            {
                JContainer emptyContainer = JsonPathValidator.IsIndex(pathTokens[lastAvailableToken.Index])
                    ? new JArray() : new JObject();

                lastAvailableToken.Token.Replace(emptyContainer);

                lastAvailableToken.Token = emptyContainer;
            }

            if (lastAvailableToken.Token.HasValues)
            {
                switch (lastAvailableToken.Token)
                {
                    case JArray when !JsonPathValidator.IsIndex(pathTokens[lastAvailableToken.Index]):

                        // replace the parent with a JObject and thus clearing all its children.
                        JObject emptyJObject = new();
                        lastAvailableToken.Token.Replace(emptyJObject);
                        lastAvailableToken.Token = emptyJObject;

                        break;

                    case JObject when JsonPathValidator.IsIndex(pathTokens[lastAvailableToken.Index]):

                        // replace the parent with a JArray and thus clearing all its children.
                        JArray emptyJArray = new();
                        lastAvailableToken.Token.Replace(emptyJArray);
                        lastAvailableToken.Token = emptyJArray;
                        break;
                }
            }

            rootCopy = JTokenGenerator.GenerateNewRoot(lastAvailableToken, pathTokens, rootCopy, value);
        }

        // assign root copy back to root.
        _root = rootCopy;
    }

    /// <summary>
    /// Remove a value or child from the JsonPathManager root and return it.
    /// </summary>
    /// <param name="path">The path where to remove the value or child.</param>
    /// <returns>The removed value or child.</returns>
    public JToken? Remove(string path)
    {
        // Verify the path is a valid JsonPath for the operation.
        JsonPathValidator.ValidateJsonPath((path ?? throw new ArgumentNullException(nameof(path))).Trim());

        // Tokenize the JsonPath.
        List<JsonPathToken> pathTokens = JsonPathTokenizer.Tokenize(path.Trim());

        List<JsonPathToken> rootTokens = pathTokens.GetRange(0, pathTokens.Count - 1);
        JsonPathToken leafToken = pathTokens[^1];

        JToken? parentRoot = _root?.SelectToken(JsonPathTokenizer.BuildPath(rootTokens));

        switch (parentRoot)
        {
            case JArray parentRootArray when JsonPathValidator.IsIndex(leafToken):

                return JTokenRemover.Remove(parentRootArray, leafToken);

            case JObject parentRootObject when !JsonPathValidator.IsIndex(leafToken):
                JToken? removed = parentRootObject[(string) leafToken.Value];

                parentRootObject.Remove((string) leafToken.Value);

                return removed;

            default:
                return null;
        }
    }
}