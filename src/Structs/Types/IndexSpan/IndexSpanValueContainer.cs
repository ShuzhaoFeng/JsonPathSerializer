namespace JsonPathSerializer.Structs.Types.IndexSpan;

/// <summary>
///     Contains a index span value from a JsonPath token.
/// </summary>
internal class IndexSpanValueContainer : IValueContainer
{
    public IndexSpanValueContainer(int startIndex, int? endIndex)
    {
        StartIndex = startIndex;
        EndIndex = endIndex;
    }

    /// <summary>
    ///     Start Index of the IndexSpan.
    /// </summary>
    public int StartIndex { get; }

    /// <summary>
    ///     End Index of the IndexSpan. May be undefined due to the variable size of the JArray.
    /// </summary>
    public int? EndIndex { get; }
}