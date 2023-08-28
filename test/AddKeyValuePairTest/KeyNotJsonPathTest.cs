using JsonPathSerializer;
using Newtonsoft.Json;

namespace JsonPathSerializerTest.AddKeyValuePairTest
{
    [TestClass]
    public class KeyNotJsonPathTest
    {
        private JsonPathManager _emptyManager = new();

        [TestInitialize]
        public void Initialize()
        {
            _emptyManager = new JsonPathManager();
        }
    }
}
