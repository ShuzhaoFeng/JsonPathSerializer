namespace JsonPathSerializer.Structs.Path;

internal class JsonPathPropertyToken : IJsonPathToken
{
    public JsonPathPropertyToken(string property)
    {
        Property = property;
    }

    public string Property { get; }
}