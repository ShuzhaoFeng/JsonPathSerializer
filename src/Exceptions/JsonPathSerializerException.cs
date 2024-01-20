using JsonPathSerializer.Globals;
using Newtonsoft.Json.Linq;

namespace JsonPathSerializer.Exceptions;
public class JsonPathSerializerException : SystemException
{
    public string? Path { get; }

    public JToken? Token { get; }

    public object? Value { get; }

    public JsonPathSerializerException() : base(ErrorMessage.Generic)
    {
    }

    public JsonPathSerializerException(string message) : base(message)
    {
    }

    public JsonPathSerializerException(string message, string? path, JToken? token, object? value) : base(message)
    {
        Path = path;
        Token = token;
        Value = value;
    }
}
