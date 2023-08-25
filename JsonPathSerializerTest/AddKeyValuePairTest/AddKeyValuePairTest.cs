using JsonPathSerializer;
using Newtonsoft.Json.Linq;

namespace JsonPathSerializerTest.AddKeyValuePairTest
{
    /// <summary>
    /// Test methods in this class should all throw an exception when adding a value to JsonPathManager with a invalid key.
    /// </summary>
    [TestClass]
    public class AddKeyValuePairTest
    {
        private JsonPathManager _emptyManager = new();
        private JsonPathManager _loadedManager = new();

        [TestInitialize]
        public void Initialize()
        {
            _emptyManager = new JsonPathManager();
            _loadedManager = new JsonPathManager(@"{
                ""name"": ""John Doe"",
            }");
        }

        [TestMethod]
        public void CanAddKeyValuePairToEmptyManager()
        {
            _emptyManager.Add("name", "John Doe");

            var expected = JToken.Parse(@"{
                ""name"": ""John Doe"",
            }");
            var actual = JToken.Parse(_emptyManager.Build());

            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void CanAddKeyValuePairToLoadedManager()
        {
            _loadedManager.Add("age", 42);

            var expected = JToken.Parse(@"{
                ""name"": ""John Doe"",
                ""age"": 42
            }");
            var actual = JToken.Parse(_loadedManager.Build());

            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void CanAddKeyValuePairToLoadedManagerWithExistingKey()
        {
            _loadedManager.Add("name", "Jane Doe");

            var expected = JToken.Parse(@"{
                ""name"": ""Jane Doe"",
            }");
            var actual = JToken.Parse(_loadedManager.Build());

            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void CanAddKeyValuePairToLoadedManagerWithExistingKeyAndValue()
        {
            _loadedManager.Add("name", "John Doe");

            var expected = JToken.Parse(@"{
                ""name"": ""John Doe"",
            }");
            var actual = JToken.Parse(_loadedManager.Build());

            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void ThrowsExceptionWhenAddingNullValueToManager()
        {
            Assert.ThrowsException<ArgumentNullException>(() => _emptyManager.Add("name", null));
        }
    }
}
