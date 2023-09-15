namespace JsonPathSerializerTest.Force.Types
{
    [TestClass]
    public class ForceIndexesTest
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
        public void CanForceIndexes()
        {
            _emptyManager.Force("name[0,1]", "Shuzhao");

            // indexes that should be affected
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][2].ToString());
        }

        [TestMethod]
        public void CanForceIndexesAsRoot()
        {
            _emptyManager.Force("[0,1]", "Shuzhao Feng");

            // indexes that should be affected
            Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[0].ToString());
            Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[1].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value[2].ToString());
        }

        [TestMethod]
        public void CanForceIndexesNestedInIndex()
        {
            _emptyManager.Force("name[0][0,1]", "Shuzhao");

            // indexes that should be affected
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][1].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][2].ToString());
        }

        [TestMethod]
        public void CanForceIndexNestedInIndexes()
        {
            _emptyManager.Force("name[0,1][0]", "Shuzhao");

            // indexes that should be affected
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1][0].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][1].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1][1].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][2].ToString());
        }

        [TestMethod]
        public void CanForceIndexesNestedInIndexes()
        {
            _emptyManager.Force("name[0,1][0,1]", "Shuzhao");

            // indexes that should be affected
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][1].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1][1].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][2].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1][2].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][2].ToString());
        }

        [TestMethod]
        public void CanForceIndexesThatRequiresExpansion()
        {
            _emptyManager.Force("name[1,5]", "John Doe");

            // indexes that should be affected
            Assert.AreEqual("John Doe", _emptyManager.Value["name"][1].ToString());
            Assert.AreEqual("John Doe", _emptyManager.Value["name"][5].ToString());

            // empty indexes forced to fill the gap
            Assert.AreEqual("{}", _emptyManager.Value["name"][0].ToString());
            Assert.AreEqual("{}", _emptyManager.Value["name"][2].ToString());
            Assert.AreEqual("{}", _emptyManager.Value["name"][3].ToString());
            Assert.AreEqual("{}", _emptyManager.Value["name"][4].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][6].ToString());
        }

        [TestMethod]
        public void CanForceIndexToExistingArray()
        {
            _loadedManager.Force("name[0,1]", "John Doe");

            // indexes that should be affected
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][0].ToString());
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][1].ToString());

            // indexes that should not be affected
            Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][3].ToString());
        }

        [TestMethod]
        public void CanForceIndexesToExistingArrayThatRequiresExpansion()
        {
            _loadedManager.Force("name[1,5]", "John Doe");

            // indexes that should be affected
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][1].ToString());
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][5].ToString());

            // indexes that should not be affected
            Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());
            Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());


            // empty indexes forced to fill the gap
            Assert.AreEqual("{}", _loadedManager.Value["name"][3].ToString());
            Assert.AreEqual("{}", _loadedManager.Value["name"][4].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][6].ToString());
        }

        [TestMethod]
        public void CanForceNegativeIndexes()
        {
            _emptyManager.Force("name[-1, 2]", "Shuzhao");

            // empty indexes forced to fill the gap
            Assert.AreEqual("{}", _emptyManager.Value["name"][0].ToString());

            // indexes that should be affected
            // C# array doesn't allow negative index value (but we do), so -1 is converted to 1.
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][2].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][3].ToString());
        }

        [TestMethod]
        public void CanForceIndexesToExistingPropertyObject()
        {
            _propertyManager.Force("name[0, 1]", "Shuzhao");

            // indexes that should be affected
            Assert.AreEqual("Shuzhao", _propertyManager.Value["name"][0].ToString());
            Assert.AreEqual("Shuzhao", _propertyManager.Value["name"][1].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _propertyManager.Value["name"][2].ToString());
        }

        [TestMethod]
        public void ThrowsExceptionWhenForcingIndexesWithInvalidSeparator()
        {
            Assert.ThrowsException<JsonException>(() => _emptyManager.Force("name[0 1]", "Shuzhao"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenIndexesAreNotInteger()
        {
            Assert.ThrowsException<JsonException>(() => _emptyManager.Force("name[1.5, 7/4]", "Shuzhao"));
        }
    }
}
