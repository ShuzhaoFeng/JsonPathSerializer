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

        [TestMethod]
        public void ThrowsExceptionWhenAddingEmptyKey()
        {
            Assert.ThrowsException<ArgumentException>(() => _emptyManager.Add("", "John Doe"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenAddingKeyWithStringInBracketWithoutQuote()
        {
            Assert.ThrowsException<JsonException>(() => _emptyManager.Add("John[Doe]", "John Doe"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenAddingKeyWithUnclosedBracket()
        {
            Assert.ThrowsException<JsonException>(() => _emptyManager.Add("John['Doe'", "John Doe"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenAddingNullKey()
        {
            Assert.ThrowsException<ArgumentNullException>(() => _emptyManager.Add(null, "John Doe"));
        }
    }
}
