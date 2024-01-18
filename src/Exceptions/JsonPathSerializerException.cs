using JsonPathSerializer.Globals;
using Newtonsoft.Json.Linq;

namespace JsonPathSerializer.Exceptions;
public class JsonPathSerializerException : SystemException
{
    public string? Path { get; }

    public JToken? Token { get; }

    public object? Value { get; }

    public JsonPathSerializerException() : base(ErrorMessage.GENERIC)
    {
    }

    public JsonPathSerializerException(string message) : base(message)
    {
    }
}
