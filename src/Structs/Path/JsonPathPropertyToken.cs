namespace JsonPathSerializer.Structs.Path;

/// <summary>
///    JsonPathToken that contains a property name of an object.
/// </summary>
internal class JsonPathPropertyToken(string property) : IJsonPathToken
{
    /// <summary>
    ///    The property name.
    /// </summary>
    public string Property { get; } = property;
}