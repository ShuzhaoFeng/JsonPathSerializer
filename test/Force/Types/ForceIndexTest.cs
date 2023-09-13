namespace JsonPathSerializerTest.Force.Types
{
    [TestClass]
    public class ForceIndexTest
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
                    [
                        ""SF"",
                    ],
                ],
            }");
            _propertyManager = new JsonPathManager(@"{
                ""name"": {
                    ""first"": ""Shuzhao"",
                },
            }");
        }

        [TestMethod]
        public void CanForceIndex()
        {
            _emptyManager.Force("name[0]", "Shuzhao");

            // index that should be affected
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1].ToString());
        }

        [TestMethod]
        public void CanForceIndexAsRoot()
        {
            _emptyManager.Force("[0]", "Shuzhao Feng");
            
            // index that should be affected
            Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[0].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value[1].ToString());
        }

        [TestMethod]
        public void CanForceIndexNestedInIndex()
        {
            _emptyManager.Force("name[0][0]", "Shuzhao");

            // index that should be affected
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][1].ToString());
        }

        [TestMethod]
        public void CanForceIndexToExistingArray()
        {
            _loadedManager.Force("name[0]", "John Doe");

            // index that should be affected
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][0].ToString());

            // indexes that should not be affected
            Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());
            Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());
            Assert.AreEqual("SF", _loadedManager.Value["name"][3][0].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][3][1].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][4].ToString());
        }

        [TestMethod]
        public void CanForceIndexToExistingArrayToLastPosition()
        {
            _loadedManager.Force("name[3][0]", "John Doe");

            // indexes that should not be affected
            Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());
            Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());
            Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());

            // index that should be affected
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][3][0].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][3][1].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][4].ToString());
        }

        [TestMethod]
        public void CanForceIndexToExistingArrayThatRequiresExpansion()
        {
            _loadedManager.Force("name[5]", "John Doe");

            // indexes that should not be affected
            Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());
            Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());
            Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());
            Assert.AreEqual("SF", _loadedManager.Value["name"][3][0].ToString());

            // empty indexes forced to fill the gap
            Assert.AreEqual("{}", _loadedManager.Value["name"][4].ToString());

            // index that should be affected
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][5].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][3][1].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][6].ToString());
        }

        [TestMethod]
        public void CanForceIndexToExistingArrayThatRequiresLargeExpansion()
        {
            _loadedManager.Force("name[123]", "John Doe");

            // indexes that should not be affected
            Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());
            Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());
            Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());
            Assert.AreEqual("SF", _loadedManager.Value["name"][3][0].ToString());

            // empty indexes forced to fill the gap
            Assert.AreEqual("{}", _loadedManager.Value["name"][4].ToString());
            Assert.AreEqual("{}", _loadedManager.Value["name"][122].ToString());

            // index that should be affected
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][123].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][124].ToString());
        }

        [TestMethod]
        public void CanForceNegativeIndex()
        {
            _emptyManager.Force("name[-1]", "Shuzhao");

            // index that should be affected
            // C# array doesn't allow negative index value (but we do), so -1 is converted to 0.
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1].ToString());
        }

        [TestMethod]
        public void CanForceNegativeIndexToExistingArray()
        {
            _loadedManager.Force("name[-2]", "John Doe");

            // indexes that should not be affected
            Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());
            Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());

            // index that should be affected
            // C# array doesn't allow negative index value (but we do), so -1 is converted to 2.
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][2].ToString());
            Assert.AreEqual("SF", _loadedManager.Value["name"][3][0].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][3][1].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][4].ToString());
        }

        [TestMethod]
        public void CanForceNegativeIndexToExistingArrayThatRequiresExpansion()
        {
            _loadedManager.Force("name[-5]", "John Doe");

            // index that should be affected
            // C# array doesn't allow negative index value (but we do), so -5 is converted to 0.
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][0].ToString());

            // indexes that should not be affected
            Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());
            Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());
            Assert.AreEqual("SF", _loadedManager.Value["name"][3][0].ToString());

            // empty indexes forced to fill the gap
            Assert.AreEqual("{}", _loadedManager.Value["name"][4].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][3][1].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][5].ToString());
        }

        [TestMethod]
        public void CanForceNegativeIndexToExistingArrayThatRequiresLargeExpansion()
        {
            _loadedManager.Force("name[-123]", "John Doe");

            // index that should be affected
            // C# array doesn't allow negative index value (but we do), so -5 is converted to 0.
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][0].ToString());

            // indexes that should not be affected
            Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());
            Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());
            Assert.AreEqual("SF", _loadedManager.Value["name"][3][0].ToString());

            // empty indexes forced to fill the gap
            Assert.AreEqual("{}", _loadedManager.Value["name"][4].ToString());
            Assert.AreEqual("{}", _loadedManager.Value["name"][122].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][3][1].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][123].ToString());
        }

        [TestMethod]
        public void CanForcePropertyNestedInIndex()
        {
            _emptyManager.Force("name[0].first", "Shuzhao");

            // index that should be affected
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0]["first"].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1].ToString());
        }

        [TestMethod]
        public void CanForceIndexToParent()
        {
            _loadedManager.Force("name[3]", "John Doe");

            // indexes that should not be affected
            Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());
            Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());
            Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());

            // index that should be affected
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][3].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][4].ToString());
        }

        [TestMethod]
        public void CanForceIndexUnderExistingValue()
        {
            _loadedManager.Force("name[0][0]", "John Doe");

            // index that should be affected
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][0][0].ToString());

            // indexes that should not be affected
            Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());
            Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());
            Assert.AreEqual("SF", _loadedManager.Value["name"][3][0].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][3][1].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][4].ToString());
        }

        [TestMethod]
        public void CanForceIndexToExistingPropertyObject()
        {
            _propertyManager.Force("name[0]", "Shuzhao");

            // index that should be affected
            Assert.AreEqual("Shuzhao", _propertyManager.Value["name"][0].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _propertyManager.Value["name"][1].ToString());
        }

        [TestMethod]
        public void ThrowsExceptionWhenIndexIsNotInteger()
        {
            Assert.ThrowsException<JsonException>(() => _emptyManager.Force("name[1.5]", "Shuzhao"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenBracketIsEmpty()
        {
            Assert.ThrowsException<JsonException>(() => _emptyManager.Force("name[]", "Shuzhao"));
        }
    }
}