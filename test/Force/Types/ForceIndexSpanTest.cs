namespace JsonPathSerializerTest.Force.Types
{
    [TestClass]
    public class ForceIndexSpanTest
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
        public void CanForceIndexSpan()
        {
            _emptyManager.Force("name[0:2]", "Shuzhao");

            // indexes that should be affected
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][2].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][3].ToString());
        }

        [TestMethod]
        public void CanForceIndexSpanAsRoot()
        {
            _emptyManager.Force("[0:2]", "Shuzhao Feng");

            // indexes that should be affected
            Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[0].ToString());
            Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[1].ToString());
            Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[2].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value[3].ToString());
        }

        [TestMethod]
        public void CanForceIndexSpanNestedInIndex()
        {
            _emptyManager.Force("name[0][0:2]", "Shuzhao");

            // indexes that should be affected
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][1].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][2].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][3].ToString());
        }

        [TestMethod]
        public void CanForceIndexNestedInIndexSpan()
        {
            _emptyManager.Force("name[0:2][0]", "Shuzhao");

            // indexes that should be affected
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][2][0].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][3].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][1].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][2].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][3].ToString());
        }

        [TestMethod]
        public void CanForceIndexesNestedInIndexes()
        {
            _emptyManager.Force("name[0:2][0:2]", "Shuzhao");

            // indexes that should be affected
            // reduce 9 assertions to a for loop
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][i][0].ToString());
                Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][i][1].ToString());
                Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][i][2].ToString());
            }

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][3].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][3].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1][3].ToString());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][2][3].ToString());
        }

        [TestMethod]
        public void CanForceIndexToExistingArray()
        {
            _loadedManager.Force("name[0:2]", "John Doe");

            // indexes that should be affected
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][0].ToString());
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][1].ToString());
            Assert.AreEqual("John Doe", _loadedManager.Value["name"][2].ToString());

            // indexes that should not be affected
            Assert.AreEqual("SF", _loadedManager.Value["name"][3].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][4].ToString());
        }

        [TestMethod]
        public void CanForceIndexesToExistingArrayThatRequiresExpansion()
        {
            _loadedManager.Force("name[1:5]", "John Doe");

            // indexes that should be affected
            // reduce 9 assertions to a for loop
            for (int i = 1; i < 6; i++)
            {
                Assert.AreEqual("John Doe", _loadedManager.Value["name"][i].ToString());
            }

            // indexes that should not be affected
            Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][6].ToString());
        }

        [TestMethod]
        public void CanForceNegativeIndexSpan()
        {
            _emptyManager.Force("name[-5:-3]", "Shuzhao");

            // we need minimally 5 elements: 0 (-5), 1 (-4), 2 (-3), 3 (-2) and 4 (-1)

            // indexes that should be affected
            // C# array doesn't allow negative index value (but we do), so -5 is converted to 0 and -3 to 2.
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][2].ToString());

            // empty indexes forced to fill the gap
            Assert.AreEqual("{}", _emptyManager.Value["name"][3].ToString());
            Assert.AreEqual("{}", _emptyManager.Value["name"][4].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][5].ToString());
        }

        [TestMethod]
        public void CanForceNegativeAndPositiveIndexSpan()
        {
            _emptyManager.Force("name[-5:2]", "Shuzhao");

            // we need minimally 8 elements: 0, 1, 2, 3 (-5), 4 (-4), 5 (-3), 6 (-2) and 7 (-1)
            for (int i = 0; i < 8; i++)
            {
                Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][i].ToString());
            }

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][8].ToString());
        }

        [TestMethod]
        public void CanForceReverseIndexSpan()
        {
            _emptyManager.Force("name[5:2]", "Shuzhao");

            // empty indexes forced to fill the gap
            Assert.AreEqual("{}", _emptyManager.Value["name"][0].ToString());
            Assert.AreEqual("{}", _emptyManager.Value["name"][1].ToString());

            // indexes that should be affected
            // reduce 4 assertions to a for loop
            for (int i = 2; i < 6; i++)
            {
                Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][i].ToString());
            }

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][6].ToString());
        }

        [TestMethod]
        public void CanForceReverseNegativeIndexSpan()
        {
            _emptyManager.Force("name[-2:-5]", "Shuzhao");

            // we need minimally 5 elements: 0 (-5), 1 (-4), 2 (-3), 3 (-2) and 4 (-1)

            // indexes that should be affected
            // C# array doesn't allow negative index value (but we do), so -5 is converted to 0 and etc.
            // reduce 4 assertions to a for loop
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][i].ToString());
            }

            // empty indexes forced to fill the gap
            Assert.AreEqual("{}", _emptyManager.Value["name"][4].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][5].ToString());
        }

        [TestMethod]
        public void CanForceReversePositiveAndNegativeIndexSpan()
        {
            _emptyManager.Force("name[2:-5]", "Shuzhao");

            // we need minimally 8 elements: 0, 1, 2, 3 (-5), 4 (-4), 5 (-3), 6 (-2) and 7 (-1)

            // indexes that should be affected
            // reduce 8 assertions to a for loop
            for (int i = 0; i < 8; i++)
            {
                Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][i].ToString());
            }

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][8].ToString());
        }

        [TestMethod]
        public void CanForceIndexesToExistingPropertyObject()
        {
            _propertyManager.Force("name[0:2]", "Shuzhao");

            // indexes that should be affected
            Assert.AreEqual("Shuzhao", _propertyManager.Value["name"][0].ToString());
            Assert.AreEqual("Shuzhao", _propertyManager.Value["name"][1].ToString());
            Assert.AreEqual("Shuzhao", _propertyManager.Value["name"][2].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _propertyManager.Value["name"][3].ToString());
        }

        [TestMethod]
        public void ThrowsExceptionWhenForcingIndexesWithInvalidSeparator()
        {
            Assert.ThrowsException<JsonException>(() => _emptyManager.Force("name[0;:1]", "Shuzhao"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenIndexesAreNotInteger()
        {
            Assert.ThrowsException<JsonException>(() => _emptyManager.Force("name[1.5:7/4]", "Shuzhao"));
        }
    }
}
