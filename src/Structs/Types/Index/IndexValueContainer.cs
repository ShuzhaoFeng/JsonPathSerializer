namespace JsonPathSerializer.Structs.Types.Index;

/// <summary>
///     Contains an index value from a JsonPath token.
/// </summary>
internal class IndexValueContainer(int index) : IValueContainer
{
    /// <summary>
    ///     The index value.
    /// </summary>
    public int Index { get; } = index;
}