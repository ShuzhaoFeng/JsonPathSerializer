using JsonPathSerializer.Structs.Types;

namespace JsonPathSerializer.Structs.Path
{
    class JsonPathIndexToken : NewJsonPathToken
    {
        public List<IValueContainer> Indexes { get; }

        public JsonPathIndexToken()
        {
            Indexes = new();
        }

        public void Add(IValueContainer container)
        {
            Indexes.Add(container);
        }
    }
}
