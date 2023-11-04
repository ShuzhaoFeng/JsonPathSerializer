using Newtonsoft.Json.Linq;

namespace JsonPathSerializer.Structs;

/// <summary>
///     A JsonNodeToken describes a node of a JsonPath, which corresponds to a level of depth in the Json tree.
///     It is used to keep track of the last available nodes in the root.
/// </summary>
internal class JsonNodeToken
{
    /// <summary>
    ///     The JsonNodeToken constructor.
    /// </summary>
    public JsonNodeToken(JToken token, int index)
    {
        Token = token;
        Index = index;
    }

    /// <summary>
    ///     Json node.
    /// </summary>
    public JToken Token { get; set; }

    /// <summary>
    ///     Depth of the Json node.
    /// </summary>
    public int Index { get; }

    /// <summary>
    ///     Specify whether the token is the last node that already exists in the root.
    /// </summary>
    public bool IsLastAvailableToken { get; private set; }

    /// <summary>
    ///     Set the token to the last node that already exists in the root.
    /// </summary>
    public JsonNodeToken AsLastAvailable()
    {
        IsLastAvailableToken = true;
        return this;
    }
}