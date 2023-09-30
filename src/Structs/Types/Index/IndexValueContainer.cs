namespace JsonPathSerializer.Structs.Types.Index
{
    class IndexValueContainer : IValueContainer
    {
        /// <summary>
        /// Index.
        /// </summary>
        public int Index { get; }

        public IndexValueContainer(int index)
        {
            Index = index;
        }
    }
}

