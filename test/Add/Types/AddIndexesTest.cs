namespace JsonPathSerializerTest.Add.Types
{
    [TestClass]
    public class AddIndexesTest
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

            // indexes that should be affected
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());

            // no extra indexes are added
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][2].ToString());
        }

        [TestMethod]
        public void CanAddIndexesAsRoot()
        {
            _emptyManager.Add("[0,1]", "Shuzhao Feng");

            // indexes that should be affected
            Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[0].ToString());
            Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[1].ToString());

            // no extra indexes are added
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value[2].ToString());
        }

        [TestMethod]
        public void CanAddIndexesNestedInIndex()
        {
            _emptyManager.Add("name[0][0,1]", "Shuzhao");

            // indexes that should be affected
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][1].ToString());

            // no extra indexes are added
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][2].ToString());
        }

        [TestMethod]
        public void CanAddIndexNestedInIndexes()
        {
            _emptyManager.Add("name[0,1][0]", "Shuzhao");

            // indexes that should be affected
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1][0].ToString());

            // no extra indexes are added
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][1].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1][1].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][2].ToString());
        }

        [TestMethod]
        public void CanAddIndexesNestedInIndexes()
        {
            _emptyManager.Add("name[0,1][0,1]", "Shuzhao");

            // indexes that should be affected
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][1].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1][1].ToString());

            // no extra indexes are added
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][2].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1][2].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][2].ToString());
        }

        [TestMethod]
        public void CanAddIndexesThatRequiresExpansion()
        {
            _emptyManager.Add("name[1,5]", "John Doe");

            // indexes that should be affected
            Assert.AreEqual("John Doe", _emptyManager.Value["name"][1].ToString());
            Assert.AreEqual("John Doe", _emptyManager.Value["name"][5].ToString());

            // empty indexes added to fill the gap
            Assert.AreEqual("{}", _emptyManager.Value["name"][0].ToString());
            Assert.AreEqual("{}", _emptyManager.Value["name"][2].ToString());
            Assert.AreEqual("{}", _emptyManager.Value["name"][3].ToString());
            Assert.AreEqual("{}", _emptyManager.Value["name"][4].ToString());

            // no extra indexes are added
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][6].ToString());
        }

        [TestMethod]
        public void CanAddIndexToExistingArray()
        {
            _loadedManager.Add("name[0,1]", "John Doe");

            // indexes that should be affected
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][0].ToString());
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][1].ToString());

            // indexes that should not be affected
            Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());

            // no extra indexes are added
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][3].ToString());
        }

        [TestMethod]
        public void CanAddIndexesToExistingArrayThatRequiresExpansion()
        {
            _loadedManager.Add("name[1,5]", "John Doe");

            // indexes that should be affected
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][1].ToString());
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][5].ToString());

            // indexes that should not be affected
            Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());
            Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());


            // empty indexes added to fill the gap
            Assert.AreEqual("{}", _loadedManager.Value["name"][3].ToString());
            Assert.AreEqual("{}", _loadedManager.Value["name"][4].ToString());

            // no extra indexes are added
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][6].ToString());
        }

        [TestMethod]
        public void CanAddNegativeIndexes()
        {
            _emptyManager.Add("name[-1, 2]", "Shuzhao");

            // empty indexes added to fill the gap
            Assert.AreEqual("{}", _emptyManager.Value["name"][0].ToString());

            // indexes that should be affected
            // C# array doesn't allow negative index value (but we do), so -1 is converted to 1.
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][2].ToString());

            // no extra indexes are added
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][3].ToString());
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
