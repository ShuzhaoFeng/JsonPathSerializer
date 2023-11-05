namespace JsonPathSerializer.Structs.Path;

/// <summary>
///    JsonPathToken that contains a property name of an object.
/// </summary>
internal class JsonPathPropertyToken : IJsonPathToken
{
    public JsonPathPropertyToken(string property)
    {
        Property = property;
    }

    /// <summary>
    ///    The property name.
    /// </summary>
    public string Property { get; }
}