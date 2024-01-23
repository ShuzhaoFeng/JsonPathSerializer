using JsonPathSerializer.Globals;
using Newtonsoft.Json.Linq;

namespace JsonPathSerializer.Exceptions;

/// <summary>
///   The exception thrown by the JsonPathSerializer library.
/// </summary>
public class JsonPathSerializerException : SystemException
{
    /// <summary>
    ///   The path where the exception occurred.
    /// </summary>
    public string? Path { get; }

    /// <summary>
    ///     The token that caused the exception.
    /// </summary>
    public JToken? Token { get; }

    /// <summary>
    ///    The value that was being inserted.
    /// </summary>
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
