using JsonPathSerializer;
using Newtonsoft.Json;

namespace JsonPathSerializerTest.AddKeyValuePairTest.OperationSpecificTest
{
    [TestClass]
    public class IndexesTest
    {
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
        public void CanAddIndexes()
        {
            _emptyManager.Add("name[0,1]", "Shuzhao");

            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());
        }

        [TestMethod]
        public void CanAddIndexesAsRoot()
        {
            _emptyManager.Add("[0,1]", "Shuzhao Feng");

            Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[0].ToString());
            Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[1].ToString());
        }

        [TestMethod]
        public void CanAddIndexesNestedInIndex()
        {
            _emptyManager.Add("name[0][0,1]", "Shuzhao");

            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][1].ToString());
        }

        [TestMethod]
        public void CanAddIndexNestedInIndexes()
        {
            _emptyManager.Add("name[0,1][0]", "Shuzhao");

            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1][0].ToString());
        }

        [TestMethod]
        public void CanAddIndexesNestedInIndexes()
        {
            _emptyManager.Add("name[0,1][0,1]", "Shuzhao");

            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][1].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1][1].ToString());
        }

        [TestMethod]
        public void CanAddIndexToExistingArray()
        {
            _loadedManager.Add("name[0,1]", "John Doe");

            Assert.AreEqual("John Doe", _loadedManager.Value["name"][0].ToString());
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][1].ToString());
        }

        [TestMethod]
        public void CanAddIndexesToExistingArrayThatRequiresExpansion()
        {
            _loadedManager.Add("name[1,5]", "John Doe");

            Assert.AreEqual("John Doe", _loadedManager.Value["name"][1].ToString());
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][5].ToString());
        }

        [TestMethod]
        public void CanAddNegativeIndexes()
        {
            _emptyManager.Add("name[-1, 1]", "Shuzhao");

            // C# array doesn't allow negative index value (but we do), so -1 is converted to 0.
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());
        }

        [TestMethod]
        public void ThrowsExceptionWhenAddingIndexesWithInvalidSeparator()
        {
            Assert.ThrowsException<JsonException>(() => _emptyManager.Add("name[0 1]", "Shuzhao"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenAddingIndexesToExistingPropertyObject()
        {
            Assert.ThrowsException<ArgumentException>(() => _propertyManager.Add("name[0, 1]", "Shuzhao"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenIndexesAreNotInteger()
        {
            Assert.ThrowsException<JsonException>(() => _emptyManager.Add("name[1.5, 7/4]", "Shuzhao"));
        }
    }
}
