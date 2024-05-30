namespace JsonPathSerializer.Structs.Types.IndexSpan;

/// <summary>
///     Contains an index span value from a JsonPath token.
/// </summary>
internal class IndexSpanValueContainer(int startIndex, int? endIndex) : IValueContainer
{
    /// <summary>
    ///     Start Index of the IndexSpan.
    /// </summary>
    public int StartIndex { get; } = startIndex;

    /// <summary>
    ///     End Index of the IndexSpan. May be undefined due to the variable size of the JArray.
    /// </summary>
    public int? EndIndex { get; } = endIndex;
}