using JsonPathSerializer.Structs;
using JsonPathSerializer.Structs.Path;
using JsonPathSerializer.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static JsonPathSerializer.Globals;

namespace JsonPathSerializer;

/// <summary>
///     Entity that is the root manager for all JsonPathSerializer operations.
/// </summary>
public class JsonPathManager : IJsonPathManager
{
    /// <summary>
    ///     The root Json object.
    /// </summary>
    private JToken? _root;

    /// <summary>
    ///     Create a new JsonPathManager instance with an empty root.
    /// </summary>
    public JsonPathManager()
    {
    }

    /// <summary>
    ///     Create a new JsonPathManager instance with the input token as root.
    /// </summary>
    /// <param name="root">The root token.</param>
    public JsonPathManager(JToken root)
    {
        _root = root ?? throw new ArgumentNullException(nameof(root));
    }

    /// <summary>
    ///     Create a new JsonPathManager instance with the input string as root.
    ///     Will throw an exception if the root string is not a valid Json string.
    /// </summary>
    /// <param name="root">The root string.</param>
    public JsonPathManager(string root)
    {
        _root = JToken.Parse(root ?? throw new ArgumentNullException(nameof(root)));
    }

    /// <summary>
    ///     Returns a read-only instance of root.
    /// </summary>
    public IJEnumerable<JToken> Value => _root ?? new JObject();

    /// <summary>
    ///     Clear the JsonPathManager root.
    /// </summary>
    public void Clear()
    {
        _root = null;
    }

    /// <summary>
    ///     Build the Json string from the JsonPathManager root.
    /// </summary>
    /// <returns>A Json string representing the root.</returns>
    public string Build()
    {
        return JsonConvert.SerializeObject(_root ?? new JObject());
    }

    /// <summary>
    ///     Add a value to the JsonPathManager root.
    /// </summary>
    /// <param name="path">The path where to add the value.</param>
    /// <param name="value">The value to be added.</param>
    public void Add(string path, object value)
    {
        Add(path, value, Priority.Normal);
    }

    /// <summary>
    ///     Add a value to the JsonPathManager root.
    /// </summary>
    /// <param name="path">The path where to add the value.</param>
    /// <param name="value">The value to be added.</param>
    /// <param name="priority">The priority of the operation.</param>
    public void Add(string path, object value, Priority priority)
    {
        // Verify the path is a valid JsonPath for the operation.
        JsonValidator.ValidateJsonPath((path ?? throw new ArgumentNullException(nameof(path))).Trim());

        // Tokenize the JsonPath.
        List<IJsonPathToken> pathTokens = JsonPathTokenizer.Tokenize(path.Trim());

        // Make a copy of root.
        JToken rootCopy = _root?.DeepClone()
                          ?? (pathTokens[0] is JsonPathIndexToken ? new JArray() : new JObject());

        // get the list of last available tokens that already exist within the root.
        foreach (JsonNodeToken lastAvailableToken in
                 JsonNodeTokenCollector.CollectLastAvailableTokens(rootCopy, pathTokens, priority))
        {
            rootCopy = JTokenGenerator.GenerateNewRoot(lastAvailableToken, pathTokens, rootCopy, value, priority);
        }

        // assign root copy back to root.
        _root = rootCopy;
    }

    /// <summary>
    ///     Force add a value to the JsonPathManager root, regardless of whether existing values will be overriden.
    ///     This method is obsolete. Use Add(string, object, Priority.High) instead.
    /// </summary>
    /// <param name="path">The path where to add the value.</param>
    /// <param name="value">The value to be added.</param>

    [Obsolete("This method is obsolete. Use Add(string, object, Priority.High) instead.")]
    public void Force(string path, object value)
    {
        Add(path, value, Priority.High);
    }

    /// <summary>
    ///     Append a value to the end of an array in the JsonPathManager root.
    /// </summary>
    /// <param name="path">The path of the array(s) where to append the value.</param>
    /// <param name="value">The value to be appended at the end of the array.</param>
    /// <param name="priority">The priority of the operation.</param>
    public void Append(string path, object value, Priority priority)
    {
        // Verify the path is a valid JsonPath for the operation.
        JsonValidator.ValidateJsonPath((path ?? throw new ArgumentNullException(nameof(path))).Trim());

        // Tokenize the JsonPath.
        List<IJsonPathToken> pathTokens = JsonPathTokenizer.Tokenize(path.Trim());

        // Make a copy of root.
        JToken rootCopy = _root?.DeepClone()
                          ?? (pathTokens[0] is JsonPathIndexToken ? new JArray() : new JObject());

        // get the list of last available tokens that already exist within the root.
        foreach (JsonNodeToken lastAvailableToken in
                 JsonNodeTokenCollector.CollectLastAvailableTokens(rootCopy, pathTokens, priority))
        {
            foreach (JToken token in JsonNodeTokenCollector.GetExactTokens(lastAvailableToken.Token, pathTokens.Last()))
            {
                if (token is JArray array)
                {
                    array.Add(value);
                }
                else
                {
                    throw new ArgumentException("Cannot append to a non-array value.");
                }
            }
        }

        // assign root copy back to root.
        _root = rootCopy;
    }

    /// <summary>
    ///     Remove a value or child from the JsonPathManager root and return it.
    /// </summary>
    /// <param name="path">The path where to remove the value or child.</param>
    /// <returns>The removed value or child.</returns>
    public JToken? Remove(string path)
    {
        // Verify the path is a valid JsonPath for the operation.
        JsonValidator.ValidateJsonPath((path ?? throw new ArgumentNullException(nameof(path))).Trim());

        // Tokenize the JsonPath.
        (string, IJsonPathToken) splitPath = JsonPathTokenizer.SplitPathAtLeaf(path.Trim());

        string parentPath = splitPath.Item1;
        IJsonPathToken leafToken = splitPath.Item2;

        switch (_root?.SelectToken(parentPath))
        {
            case JArray parentRootArray when leafToken is JsonPathIndexToken indexToken:

                // use a helper method to remove the value.
                return JTokenRemover.Remove(parentRootArray, indexToken);

            case JObject parentRootObject when leafToken is JsonPathPropertyToken propertyToken:

                // operation is simpler. Can directly remove
                JToken? removed = parentRootObject[propertyToken.Property];

                parentRootObject.Remove(propertyToken.Property);

                return removed;

            default:
                return null;
        }
    }

    /// <summary>
    ///     Serialize a list of path-value pairs into a Json string.
    /// </summary>
    /// <param name="jsonPathToValues">The list of path-value pair to serialize.</param>
    /// <returns>The serialized string.</returns>
    public static string SerializeAll(IEnumerable<(string, object)> jsonPathToValues)
    {
        JsonPathManager manager = new();

        foreach ((string, object) jsonPathToValue in jsonPathToValues
                                                     ?? throw new ArgumentNullException(nameof(jsonPathToValues)))
            manager.Add(jsonPathToValue.Item1, jsonPathToValue.Item2);

        return manager.Build();
    }

    /// <summary>
    ///     Serialize a collection of key-value pairs into a Json string.
    /// </summary>
    /// <param name="jsonPathToValues">The key-value pairs to serialize.</param>
    /// <returns>The serialized string.</returns>
    public static string SerializeAll(IEnumerable<KeyValuePair<string, object>> jsonPathToValues)
    {
        JsonPathManager manager = new();

        foreach (KeyValuePair<string, object> jsonPathToValue in jsonPathToValues
                                                                 ?? throw new ArgumentNullException(
                                                                     nameof(jsonPathToValues)))
            manager.Add(jsonPathToValue.Key, jsonPathToValue.Value);

        return manager.Build();
    }
}