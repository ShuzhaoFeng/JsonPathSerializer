using JsonPathSerializer.Structs.Types;
using JsonPathSerializer.Structs.Types.Index;
using JsonPathSerializer.Structs.Types.IndexSpan;

namespace JsonPathSerializer.Structs.Path;

/// <summary>
///     JsonPathToken that contains indexes and index spans of an array.
/// </summary>
internal class JsonPathIndexToken : IJsonPathToken
{
    /// <summary>
    ///     The indexes of the array.
    /// </summary>
    public List<IValueContainer> Indexes { get; } = new();

    /// <summary>
    ///     The minimum number of elements that an array must have so that all Indexes are valid.
    /// </summary>
    public int Bound { get; private set; }

    /// <summary>
    ///    Add a new index or index span to the token, and update the bound.
    /// </summary>
    /// <param name="container"></param>
    public void Add(IValueContainer container)
    {
        Indexes.Add(container);

        // update bound
        if (container is IndexValueContainer index)
        {
            // if a index x is positive (e.g. [3]), then we need at least x + 1 elements.
            // if a index x is negative (e.g. [-3]), then we need at least -x elements.
            int absoluteBound = index.Index < 0 ? -index.Index : index.Index + 1;

            Bound = Math.Max(Bound, absoluteBound);
        }
        else if (container is IndexSpanValueContainer indexSpan)
        {
            // same as index, but we consider both ends
            int absoluteStartBound = indexSpan.StartIndex < 0
                ? -indexSpan.StartIndex
                : indexSpan.StartIndex + 1;

            Bound = Math.Max(Bound, absoluteStartBound);

            // if end index is null, whatever the current bound will be used
            if (indexSpan.EndIndex is not null)
            {
                int endIndex = (int)indexSpan.EndIndex;
                int absoluteEndBound = endIndex < 0 ? -endIndex : endIndex + 1;

                Bound = Math.Max(Bound, absoluteEndBound);
            }
        }
    }
}