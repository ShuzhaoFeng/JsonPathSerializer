using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace JsonPathSerializer;

public class JsonPathManager : IJsonPathManager
{
    private JToken? _root;

    public IJEnumerable<JToken> Value => _root ?? new JObject();

    public enum PathTokenType
    {
        Unknown = 999,
        Property,
        Index,
        IndexList,
        IndexSpan,
        IndexSpanStartOnly,
        IndexSpanEndOnly,
        IndexSpanReverse,
        WildCard
    }

    public struct PathToken
    {
        public object Value { get; }

        public PathTokenType Type { get; }

        public PathToken(object value, PathTokenType type)
        {
            Value = value;
            Type = type;
        }
    }

    internal struct JsonToken
    {
        private JToken _token;
        private int _index;
        private bool _isLastAvailableToken = false;

        internal JToken Token => _token;
        internal int Index => _index;
        internal bool IsLastAvailableToken => _isLastAvailableToken;

        internal JsonToken(JToken token, int index)
        {
            _token = token;
            _index = index;
        }

        internal JsonToken AsLastAvailable()
        {
            _isLastAvailableToken = true;
            return this;
        }
    }

    public static string SerializeAllJsonPaths(IEnumerable<(string, object)> jsonPathToValues)
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
        ValidatePath(path);
        List<string> paths = ParsePathToList(path.Trim());
        List<PathToken> pathTokens = new();

        foreach (string pathToken in paths)
        {
            if ("$.".Contains(pathToken))
            {
                continue;
            }

            Match indexMatch = PathGlobals.PathRegex.INDEX.Match(pathToken);

            if (indexMatch.Success)
            {
                pathTokens.Add(new PathToken(int.Parse(indexMatch.Groups[1].Value), PathTokenType.Index));
                continue;
            }

            Match propertyBracketMatch = PathGlobals.PathRegex.PROPERTY_BRACKET.Match(pathToken);

            if (propertyBracketMatch.Success)
            {
                pathTokens.Add(new PathToken(propertyBracketMatch.Groups[1].Value, PathTokenType.Property));
                continue;
            }

            Match propertyDotMatch = PathGlobals.PathRegex.PROPERTY_DOT.Match(pathToken);

            if (propertyDotMatch.Success)
            {
                pathTokens.Add(new PathToken(propertyDotMatch.Groups[1].Value, PathTokenType.Property));
                continue;
            }

            throw new NotImplementedException();
        }

        SetValueToPath(value, pathTokens);
    }

    private void ValidatePath(string path)
    {
        string trimmedPath = path.Trim();

        if (trimmedPath.Length < 1)
        {
            throw new ArgumentException("Path cannot be empty");
        }

        if (trimmedPath.Contains(".."))
        {
            throw new ArgumentException("Deep scan \'..\' is unsupported.");
        }

        if (trimmedPath[0].Equals('$'))
        {
            if (trimmedPath.Length < 2)
            {
                throw new ArgumentException("Path starting with $ cannot be empty");
            }

            if (trimmedPath[1].Equals('.'))
            {
                if (trimmedPath.Length < 3)
                {
                    throw new ArgumentException("Path starting with $. cannot be empty");
                }
            }
        }
            
        new JObject().SelectToken(trimmedPath);
    }

    private List<string> ParsePathToList(string path)
    {
        List<string> paths = new();
        
        int index = 0;

        while (index >= 0 && index < path.Length)
        {
            int newIndex = path[index].Equals('[')
                ? path.IndexOf(']', index) + 1
                : FindNextDotOrOpenBracket(path, index + 1);

            paths.Add(newIndex > 0 ? path.Substring(index, newIndex - index) : path.Substring(index));
            index = newIndex;
        }

        return paths;
    }

    private void SetValueToPath(object value, List<PathToken> pathTokens)
    {
        if (pathTokens.Count < 1)
        {
            throw new ArgumentException();
        }

        List<JsonToken> lastTokens = TravelToLastAvailableToken(pathTokens);

        foreach (JsonToken lastToken in lastTokens)
        {
            if (lastToken.Token is not JContainer)
            {
                throw new ArgumentException
                (
                    $"JSON element $.{lastToken.Token.Path} " +
                    "contains a value, therefore cannot contain other child elements."
                );
            }
                
            if
                (!(
                     (
                         lastToken.Token is JArray
                         && pathTokens[lastToken.Index].Type == PathTokenType.Index)
                     || (
                         lastToken.Token is JObject
                         && pathTokens[lastToken.Index].Type != PathTokenType.Index)
                 ))
            {
                throw new ArgumentException
                (
                    $"JSON element $.{lastToken.Token.Path} " +
                    "cannot be taken both as JObject and JArray."
                );
            }

            JToken newToken = CreateNewToken(value, pathTokens, lastToken.Index);

            switch (pathTokens[lastToken.Index].Type)
            {
                case PathTokenType.Property:
                    JObject lastJObject = (JObject)lastToken.Token;
                    if
                    (   
                        lastToken.Index == pathTokens.Count - 1
                        && lastJObject.ContainsKey((string)pathTokens.Last().Value)
                    )
                    {
                        lastJObject[(string)pathTokens.Last().Value] = JToken.FromObject(value);
                    }
                    else
                    {
                        lastJObject.Add
                        (
                            (string)pathTokens[lastToken.Index].Value,
                            newToken[pathTokens[lastToken.Index].Value]
                        );
                    }
                        
                    newToken = lastToken.Token;

                    break;
                case PathTokenType.Index:
                    JArray lastJArray = (JArray) lastToken.Token;
                    int index = (int) pathTokens[lastToken.Index].Value;

                    if (index < lastJArray.Count)
                    {
                        lastJArray[index] = newToken[index] ?? throw new NullReferenceException();
                        newToken = lastJArray;
                    }
                    else
                    {
                        for (int i = 0; i < index - lastJArray.Count; i++)
                        {
                            lastJArray.Add(new JObject());
                        }

                        lastJArray.Add(newToken[index] ?? throw new NullReferenceException());
                        newToken = lastJArray;
                    }

                    break;
                default:
                    throw new NotImplementedException();
            }

            if (lastToken.Index == 0)
            {
                _root = newToken;
            }
            else
            {
                lastToken.Token.Replace(newToken);
            }
        }
    }

    private List<JsonToken> TravelToLastAvailableToken(List<PathToken> pathTokens)
    {
        _root ??= (pathTokens[0].Type is PathTokenType.Index) ? new JArray() : new JObject();

        int pathIndex = 0;
        List<JsonToken> currentTokens = new() { new (_root, pathIndex) };

        while (true)
        {
            if (pathIndex >= pathTokens.Count - 1)
            {
                return currentTokens.Select(x => x.AsLastAvailable()).ToList();
            }

            PathToken token = pathTokens[pathIndex];

            switch (token.Type)
            {
                case PathTokenType.Property:
                    string key = (string) token.Value;

                    for (int i = 0; i < currentTokens.Count; i++)
                    {
                        JsonToken currentToken = currentTokens[i];
                        JToken jToken = currentToken.Token;

                        if (jToken is JObject currentJObject && currentJObject.ContainsKey(key))
                        {
                            currentTokens[currentTokens.IndexOf(currentToken)] =
                                new JsonToken
                                (
                                    currentJObject[key] ?? throw new NullReferenceException(),
                                    currentToken.Index + 1
                                );
                        } else
                        {
                            currentTokens[currentTokens.IndexOf(currentToken)] =
                                currentToken.AsLastAvailable();
                            break;
                        }
                    }

                    break;

                case PathTokenType.Index:
                    int index = (int) token.Value;

                    for (int i = 0; i < currentTokens.Count; i++)
                    {
                        JsonToken currentToken = currentTokens[i];
                        JToken jToken = currentToken.Token;

                        if (jToken is JArray currentJArray && currentJArray.Count > (index < 0 ? Math.Abs(index + 1) : index))
                        {
                            int absoluteIndex = index < 0 ? currentJArray.Count + index : index;
                            currentTokens[currentTokens.IndexOf(currentToken)] =
                                new JsonToken
                                (
                                    currentJArray[absoluteIndex] ?? throw new NullReferenceException(),
                                    currentToken.Index + 1
                                );
                        }
                        else
                        {
                            currentTokens[currentTokens.IndexOf(currentToken)] =
                                currentToken.AsLastAvailable();
                            break;
                        }
                    }

                    break;

                default:
                    throw new NotImplementedException();
            }

            if (currentTokens.Any(x => !x.IsLastAvailableToken))
            {
                pathIndex++;
            }
            else
            {
                return currentTokens;
            }
        }
    }

    private JToken CreateNewToken(object value, List<PathToken> pathTokens, int startIndex)
    {
        JToken newToken = new JObject();

        for (int i = pathTokens.Count; i > startIndex; i--)
        {
            PathToken token = pathTokens[i - 1];

            switch (token.Type)
            {
                case PathTokenType.Property:
                    newToken = new JObject()
                    {
                        [token.Value] =
                            i == pathTokens.Count ? JToken.FromObject(value) : newToken
                    };

                    break;
                case PathTokenType.Index:
                    JArray arrToken = new JArray();

                    for (int j = 0; j < (int) token.Value; j++)
                    {
                        arrToken.Add(new JObject());
                    }

                    arrToken.Add(i == pathTokens.Count ? JToken.FromObject(value) : newToken);
                    newToken = arrToken;

                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        return newToken;
    }

    private int FindNextDotOrOpenBracket(string path, int index)
    {
        int dotIndex = path.IndexOf('.', index);
        int bracketIndex = path.IndexOf('[', index);

        if (dotIndex == -1)
        {
            return bracketIndex;
        }

        if (bracketIndex == -1)
        {
            return dotIndex;
        }
        
        return Math.Min(dotIndex, bracketIndex);
    }
}