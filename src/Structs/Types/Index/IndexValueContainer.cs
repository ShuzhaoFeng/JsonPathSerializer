namespace JsonPathSerializer.Structs.Types.Index;

internal class IndexValueContainer : IValueContainer
{
    public IndexValueContainer(int index)
    {
        Index = index;
    }

    /// <summary>
    ///     Index.
    /// </summary>
    public int Index { get; }
}