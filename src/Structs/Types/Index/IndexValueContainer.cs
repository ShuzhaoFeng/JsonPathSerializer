namespace JsonPathSerializer.Structs.Types.Index;

/// <summary>
///     Contains a index value from a JsonPath token.
/// </summary>
internal class IndexValueContainer : IValueContainer
{
    public IndexValueContainer(int index)
    {
        Index = index;
    }

    /// <summary>
    ///     The index value.
    /// </summary>
    public int Index { get; }
}