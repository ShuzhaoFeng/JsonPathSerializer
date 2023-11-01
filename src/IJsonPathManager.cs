using Newtonsoft.Json.Linq;

namespace JsonPathSerializer;

/// <summary>
///     Describes the entity that is the root manager for all JsonPathSerializer operations.
/// </summary>
internal interface IJsonPathManager
{
    public IJEnumerable<JToken> Value { get; }

    void Add(string path, object value);

    void Force(string path, object value);

    JToken? Remove(string path);

    string Build();

    void Clear();
}