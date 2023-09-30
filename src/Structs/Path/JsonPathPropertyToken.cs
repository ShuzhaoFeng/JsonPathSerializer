namespace JsonPathSerializer.Structs.Path
{
    class JsonPathPropertyToken : NewJsonPathToken
    {
        public string Property { get; }

        public JsonPathPropertyToken(string property)
        {
            Property = property;
        }
    }
}
