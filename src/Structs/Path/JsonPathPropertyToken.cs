namespace JsonPathSerializer.Structs.Path
{
    class JsonPathPropertyToken : IJsonPathToken
    {
        public string Property { get; }

        public JsonPathPropertyToken(string property)
        {
            Property = property;
        }
    }
}
