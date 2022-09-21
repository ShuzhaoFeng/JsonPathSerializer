using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonPathSerializer
{
    public class JsonPathManager
    {
        private dynamic? _root;
        private int _index;

        public IJEnumerable<JToken> Values =>
            (_root is null) ? new JObject().AsJEnumerable() : _root.AsJEnumerable();

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

        public JsonPathManager()
        {
            _index = 0;
        }

        public JsonPathManager(JObject root)
        {
            _root = root;
            _index = 0;
        }

        public JsonPathManager(JArray root)
        {
            _root = root;
            _index = 0;
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

                if (PathGlobals.PathRegex.KEY.IsMatch(pathElement))
                {
                    string key = pathElement.Substring(2, pathElement.Length - 4);
                    pathTokens[i] = new PathToken(key, PathTokenType.Key);
                }
                else if (PathGlobals.PathRegex.INDEX.IsMatch(pathElement))
                {
                    int index = int.Parse(pathElement.Substring(1, pathElement.Length - 2));
                    pathTokens[i] = new PathToken(index, PathTokenType.Index);
                }
                else
                {
                    if (pathElement.Contains('[') || pathElement.Contains(']'))
                    {
                        pathTokens[i] = new PathToken(pathElement, PathTokenType.Unknown);
                    }
                    else
                    {
                        pathTokens[i] = new PathToken(pathElement, PathTokenType.Key);
                    }

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

            dynamic lastToken = TravelToLastAvailableToken(pathTokens);

            if
            (!(
                (lastToken is JArray && pathTokens[_index].Type == PathTokenType.Index)
                || (lastToken is JObject && pathTokens[_index].Type != PathTokenType.Index)
            ))
            {
                string errorPath = "$";

                for (int i = 0; i < _index; i++)
                {
                    PathToken errorToken = pathTokens[i];

                    switch (errorToken.Type)
                    {
                        case PathTokenType.Key:
                            errorPath += $".{errorToken.Value}";
                            break;
                        case PathTokenType.Index:
                            errorPath += $"[{errorToken.Value}]";
                            break;
                        default:
                            errorPath += errorToken.Value.ToString();
                            break;
                    }
                }

                throw new ArgumentException
                (
                    $"JSON element {errorPath} cannot be taken both as JObject and JArray."
                );
            }

            dynamic newToken = CreateNewToken(value, pathTokens);

            switch (pathTokens[_index].Type)
            {
                case PathTokenType.Key:
                    newToken = new JObject(lastToken)
                    {
                        [pathTokens[_index].Value] = newToken[pathTokens[_index].Value]
                    };

                    break;
                case PathTokenType.Index:
                    int index = (int)pathTokens[_index].Value;
                    if (index < lastToken.Count)
                    {
                        lastToken[index] = newToken[index];
                        newToken = lastToken;
                    }
                    else
                    {
                        for (int i = 0; i < index - lastToken.Count; i++)
                        {
                            lastToken.Add(new JObject());
                        }

                        lastToken.Add(newToken[index]);
                        newToken = lastToken;
                    }

                    break;
            }

            if (_index == 0)
            {
                _root = newToken;
            }
            else
            {
                lastToken.Replace(newToken);
            }

            _index = 0;
        }

        private dynamic TravelToLastAvailableToken(PathToken[] pathTokens)
        {
            if (_root is null)
            {
                _root = (pathTokens[0].Type is PathTokenType.Index) ? new JArray() : new JObject();
            }

            dynamic currentToken = _root;
            int pathIndex = 0;

            while (true)
            {
                if (pathIndex >= pathTokens.Length - 1)
                {
                    _index = pathIndex;
                    return currentToken ?? throw new NullReferenceException();
                }

                PathToken token = pathTokens[pathIndex];

                switch (token.Type)
                {
                    case PathTokenType.Key:
                        string key = (string)token.Value;

                        if (currentToken is JObject && ((JObject)currentToken).ContainsKey(key))
                        {
                            JObject currentJObject = (JObject)currentToken;
                            currentToken = currentJObject[key] ?? throw new NullReferenceException();

                            break;
                        }
                        else
                        {
                            _index = pathIndex;
                            return currentToken;
                        }

                    case PathTokenType.Index:
                        int index = (int)token.Value;

                        if (currentToken is JArray && !((JArray)currentToken)[index].Equals(null))
                        {
                            JArray currentJArray = (JArray)currentToken;
                            currentToken = currentJArray[index];

                            break;
                        }
                        else
                        {
                            _index = pathIndex;
                            return currentToken;
                        }

                    default:
                        throw new NotImplementedException();
                }

                pathIndex++;
            }
        }

        private dynamic CreateNewToken(object value, PathToken[] pathTokens)
        {
            dynamic newToken = new JObject();

            for (int i = pathTokens.Length; i > _index; i--)
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

                        for (int j = 0; j < (int)token.Value; j++)
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
