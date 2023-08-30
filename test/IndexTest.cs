using JsonPathSerializer;
using Newtonsoft.Json;

namespace JsonPathSerializerTest
{
    [TestClass]
    public class IndexTest {
        private JsonPathManager _emptyManager = new();
        private JsonPathManager _loadedManager = new();
        private JsonPathManager _propertyManager = new();

        [TestInitialize]
        public void Setup()
        {
            _emptyManager = new JsonPathManager();
            _loadedManager = new JsonPathManager(@"{
                ""name"": [
                    ""Shuzhao"",
                    ""Feng"",
                    ""Shuzhao Feng"",
                ],
            }");
            _propertyManager = new JsonPathManager(@"{
                ""name"": {
                    ""first"": ""Shuzhao"",
                },
            }");
        }

        [TestMethod]
        public void CanAddIndex()
        {
            _emptyManager.Add("name[0]", "Shuzhao");

            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
        }

        [TestMethod]
        public void CanAddIndexAsRoot()
        {
            _emptyManager.Add("[0]", "Shuzhao Feng");

            Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[0].ToString());
        }

        [TestMethod]
        public void CanAddIndexNestedInIndex()
        {
            _emptyManager.Add("name[0][0]", "Shuzhao");

            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
        }

        [TestMethod]
        public void CanAddIndexToExistingArray()
        {
            _loadedManager.Add("name[0]", "John Doe");

            Assert.AreEqual("John Doe", _loadedManager.Value["name"][0].ToString());
        }

        [TestMethod]
        public void CanAddIndexToExistingArrayThatRequiresExpansion()
        {
            _loadedManager.Add("name[5]", "John Doe");

            Assert.AreEqual("John Doe", _loadedManager.Value["name"][5].ToString());
        }

        [TestMethod]
        public void CanAddNegativeIndex()
        {
            _emptyManager.Add("name[-1]", "Shuzhao");

            // C# array doesn't allow negative index value (but we do), so -1 is converted to 0.
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
        }

        [TestMethod]
        public void CanAddNegativeIndexToExistingArray()
        {
            _loadedManager.Add("name[-1]", "John Doe");

            // C# array doesn't allow negative index value (but we do), so -1 is converted to 2.
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][2].ToString());
        }

        [TestMethod]
        public void CanAddNegativeIndexToExistingArrayThatRequiresExpansion()
        {
            _loadedManager.Add("name[-5]", "John Doe");

            // C# array doesn't allow negative index value (but we do), so -5 is converted to 0.
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][-5].ToString());
        }

        [TestMethod]
        public void CanAddPropertyNestedInIndex()
        {
            _emptyManager.Add("name[0].first", "Shuzhao");

            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0]["first"].ToString());
        }

        [TestMethod]
        public void ThrowsExceptionWhenAddingIndexToExistingPropertyObject()
        {
            Assert.ThrowsException<ArgumentException>(() => _propertyManager.Add("name[0]", "Shuzhao"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenIndexIsNotInteger()
        {
            Assert.ThrowsException<JsonException>(() => _emptyManager.Add("name[1.5]", "Shuzhao"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenBracketIsEmpty()
        {
            Assert.ThrowsException<JsonException>(() => _emptyManager.Add("name[]", "Shuzhao"));
        }
    }
}