using JsonPathSerializer;
using Newtonsoft.Json;

namespace JsonPathSerializerTest
{
    [TestClass]
    public class IndexSpanTest
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
                    ""SF"",
                ],
            }");
            _propertyManager = new JsonPathManager(@"{
                ""name"": {
                    ""first"": ""Shuzhao"",
                },
            }");
        }

        [TestMethod]
        public void CanAddIndexSpan()
        {
            _emptyManager.Add("name[0:2]", "Shuzhao");

            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][2].ToString());
        }

        [TestMethod]
        public void CanAddIndexSpanAsRoot()
        {
            _emptyManager.Add("[0:2]", "Shuzhao Feng");

            Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[0].ToString());
            Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[1].ToString());
            Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[2].ToString());
        }

        [TestMethod]
        public void CanAddIndexSpanNestedInIndex()
        {
            _emptyManager.Add("name[0][0:2]", "Shuzhao");

            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][1].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][2].ToString());
        }

        [TestMethod]
        public void CanAddIndexNestedInIndexSpan()
        {
            _emptyManager.Add("name[0:2][0]", "Shuzhao");

            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][2][0].ToString());
        }

        [TestMethod]
        public void CanAddIndexesNestedInIndexes()
        {
            _emptyManager.Add("name[0:2][0:2]", "Shuzhao");

            // reduce 9 assertions to a for loop
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][i][0].ToString());
                Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][i][1].ToString());
                Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][i][2].ToString());
            }
        }

        [TestMethod]
        public void CanAddIndexToExistingArray()
        {
            _loadedManager.Add("name[0:2]", "John Doe");

            Assert.AreEqual("John Doe", _loadedManager.Value["name"][0].ToString());
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][1].ToString());
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][2].ToString());

        }

        [TestMethod]
        public void CanAddIndexesToExistingArrayThatRequiresExpansion()
        {
            _loadedManager.Add("name[1:5]", "John Doe");

            // reduce 9 assertions to a for loop
            for (int i = 1; i < 6; i++)
            {
                Assert.AreEqual("John Doe", _loadedManager.Value["name"][i].ToString());
            }
        }

        [TestMethod]
        public void CanAddNegativeIndexSpan()
        {
            _emptyManager.Add("name[-2:-1]", "Shuzhao");

            // C# array doesn't allow negative index value (but we do), so -2 is converted to 0.
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());
        }

        [TestMethod]
        public void CanAddNegativeAndPositiveIndexSpan()
        {
            _emptyManager.Add("name[-1:1]", "Shuzhao");

            // C# array doesn't allow negative index value (but we do), so -2 is converted to 0.
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());
        }

        [TestMethod]
        public void ThrowsExceptionWhenAddingIndexesWithInvalidSeparator()
        {
            Assert.ThrowsException<JsonException>(() => _emptyManager.Add("name[0,:1]", "Shuzhao"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenAddingIndexesToExistingPropertyObject()
        {
            Assert.ThrowsException<ArgumentException>(() => _propertyManager.Add("name[0:2]", "Shuzhao"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenIndexesAreNotInteger()
        {
            Assert.ThrowsException<JsonException>(() => _emptyManager.Add("name[1.5:7/4]", "Shuzhao"));
        }
    }
}
