using Newtonsoft.Json.Linq;

namespace JsonPathSerializer
{
    internal interface IJsonPathManager
    {
        public IJEnumerable<JToken> Value { get; }

        void Add(string path, object value);

        string Build();

        void Clear();
    }
}
