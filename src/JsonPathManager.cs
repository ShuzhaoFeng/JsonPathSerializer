using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace JsonPathSerializer
{
    public class JsonPathManager
    {
        private JToken? _root;

        public IJEnumerable<JToken> Values => (_root ?? new JObject()).AsJEnumerable();

        public enum PathTokenType
        {
            Unknown = 999,
            Key,
            Index
        }

        public struct PathToken
        {
            private object _value;
            private PathTokenType _type;

            public object Value => _value;
            public PathTokenType Type => _type;

            public PathToken(object value, PathTokenType type)
            {
                _value = value;
                _type = type;
            }
        }

        public struct JsonToken
        {
            private JToken _token;
            private int _index;
            private bool _isLastAvailableToken = false;

            public JToken Token => _token;
            public int Index => _index;
            public bool IsLastAvailableToken => _isLastAvailableToken;

            public JsonToken(JToken token, int index)
            {
                _token = token;
                _index = index;
            }

            public JsonToken AsLastAvailable()
            {
                _isLastAvailableToken = true;
                return this;
            }
        }

        public JsonPathManager()
        {
        }

        public JsonPathManager(JObject root)
        {
            _root = root;
        }

        public JsonPathManager(JArray root)
        {
            _root = root;
        }

        public string Build()
        {
            return JsonConvert.SerializeObject(_root);
        }

        public void Add(object value, string path)
        {
            List<string> paths = ParsePathToList(path);
            PathToken[] pathTokens = new PathToken[paths.Count];

            for (int i = 0; i < paths.Count; i++)
            {
                string pathElement = paths[i];

                Match keyMatch = PathGlobals.PathRegex.KEY.Match(pathElement);

                if (keyMatch.Success)
                {
                    pathTokens[i] = new PathToken(keyMatch.Groups[1].Value, PathTokenType.Key);

                    continue;
                }

                Match indexMatch = PathGlobals.PathRegex.INDEX.Match(pathElement);

                if (indexMatch.Success)
                {
                    pathTokens[i] = new PathToken(int.Parse(indexMatch.Groups[1].Value), PathTokenType.Index);

                    continue;
                }

                if (pathElement.Contains('[') || pathElement.Contains(']'))
                {
                    pathTokens[i] = new PathToken(pathElement, PathTokenType.Unknown);
                }
                else
                {
                    pathTokens[i] = new PathToken(pathElement, PathTokenType.Key);
                }
            }

            SetValueToPath(value, pathTokens);
        }

        private List<string> ParsePathToList(string path)
        {
            string trimmedPath = path.Trim();

            if (trimmedPath.Length < 1)
            {
                throw new ArgumentException("Path cannot be empty");
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
                    else
                    {
                        trimmedPath = trimmedPath.Substring(2);
                    }
                }
                else
                {
                    trimmedPath = trimmedPath.Substring(1);
                }
            }

            IEnumerable<string> pathFirstParsedByBrackets =
                PathGlobals.PathRegex.BRACKET.Split(trimmedPath).Where(x => x.Length > 0);

            List<string> pathSecondParsedByDotsAndParenthesis = new();

            foreach (string pathFirstMatchByBracket in pathFirstParsedByBrackets)
            {
                if (PathGlobals.PathRegex.PARENTHESIS_WITHIN_BRACKET.IsMatch(pathFirstMatchByBracket))
                {
                    pathSecondParsedByDotsAndParenthesis.Add(pathFirstMatchByBracket);
                }
                else
                {
                    IEnumerable<string> pathFirstMatchSecondParsedByDots =
                        pathFirstMatchByBracket.Split('.').Where(x => x.Length > 0);

                    foreach (string pathSecondMatchByDot in pathFirstMatchSecondParsedByDots)
                    {
                        pathSecondParsedByDotsAndParenthesis.Add(pathSecondMatchByDot);
                    }
                }
            }

            return pathSecondParsedByDotsAndParenthesis;
        }

        private void SetValueToPath(object value, PathToken[] pathTokens)
        {
            if (pathTokens.Length < 1)
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
                        $"contains a value, therefore cannot contain other child elements."
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
                        $"cannot be taken both as JObject and JArray."
                    );
                }

                JToken newToken = CreateNewToken(value, pathTokens, lastToken.Index);

                switch (pathTokens[lastToken.Index].Type)
                {
                    case PathTokenType.Key:
                        ((JObject) lastToken.Token)
                            .Add
                            (
                                (string) pathTokens[lastToken.Index].Value,
                                newToken[pathTokens[lastToken.Index].Value]
                            );
                        newToken = lastToken.Token;

                        break;
                    case PathTokenType.Index:
                        JArray token = (JArray) lastToken.Token;
                        int index = (int) pathTokens[lastToken.Index].Value;

                        if (index < token.Count)
                        {
                            token[index] = newToken[index] ?? throw new NullReferenceException();
                            newToken = token;
                        }
                        else
                        {
                            for (int i = 0; i < index - token.Count; i++)
                            {
                                token.Add(new JObject());
                            }

                            token.Add(newToken[index] ?? throw new NullReferenceException());
                            newToken = token;
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

        private List<JsonToken> TravelToLastAvailableToken(PathToken[] pathTokens)
        {
            if (_root is null)
            {
                _root = (pathTokens[0].Type is PathTokenType.Index) ? new JArray() : new JObject();
            }

            int pathIndex = 0;
            List<JsonToken> currentTokens = new() { new (_root, pathIndex) };

            while (true)
            {
                if (pathIndex >= pathTokens.Length - 1)
                {
                    return currentTokens.Select(x => x.AsLastAvailable()).ToList();
                }

                PathToken token = pathTokens[pathIndex];

                switch (token.Type)
                {
                    case PathTokenType.Key:
                        string key = (string) token.Value;

                        for (int i = 0; i < currentTokens.Count; i++)
                        {
                            JsonToken currentToken = currentTokens[i];
                            JToken jToken = currentToken.Token;

                            if (jToken is JObject && ((JObject) jToken).ContainsKey(key))
                            {
                                JObject currentJObject = (JObject) jToken;
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

                            if (jToken is JArray && ((JArray) jToken).Count > index)
                            {
                                JObject currentJObject = (JObject)jToken;
                                currentTokens[currentTokens.IndexOf(currentToken)] =
                                    new JsonToken
                                    (
                                        currentJObject[index] ?? throw new NullReferenceException(),
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

        private JToken CreateNewToken(object value, PathToken[] pathTokens, int startIndex)
        {
            JToken newToken = new JObject();

            for (int i = pathTokens.Length; i > startIndex; i--)
            {
                PathToken token = pathTokens[i - 1];

                switch (token.Type)
                {
                    case PathTokenType.Key:
                        newToken = new JObject()
                        {
                            [token.Value] =
                                (i == pathTokens.Length) ? JToken.FromObject(value) : newToken
                        };

                        break;
                    case PathTokenType.Index:
                        JArray arrToken = new JArray();

                        for (int j = 0; j < (int) token.Value; j++)
                        {
                            arrToken.Add(new JObject());
                        }

                        arrToken.Add((i == pathTokens.Length) ? JToken.FromObject(value) : newToken);
                        newToken = arrToken;

                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return newToken;
        }
    }
}
