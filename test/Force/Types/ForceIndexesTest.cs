namespace JsonPathSerializerTest.Force.Types
{
    [TestClass]
    public class ForceIndexesTest
    {
        private JsonPathManager _emptyManager = new();
        private JsonPathManager _loadedManager = new();
        private JsonPathManager _propertyManager = new();
        private JsonPathManager _loadedPropertyManager = new();

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
            _loadedPropertyManager = new JsonPathManager(@"{
                ""name"": [
                    {
                        ""first"": ""Shuzhao"",
                        ""last"": ""Feng"",
                    },
                    {},
                    {
                        ""first"": ""John"",
                        ""last"": ""Doe"",
                    },
                    {
                        ""first"": ""Jane"",
                        ""last"": ""Doa"",
                    },
                ],
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
        public void CanForceIndexesToExistingArray()
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
        public void CanForcePropertyNestedInIndexes()
        {
            _loadedPropertyManager.Force("name[1, 2, 3].middle", "A.");

            // indexes that should be affected
            Assert.AreEqual("A.", _loadedPropertyManager.Value["name"][1]["middle"].ToString());
            Assert.AreEqual("A.", _loadedPropertyManager.Value["name"][2]["middle"].ToString());
            Assert.AreEqual("A.", _loadedPropertyManager.Value["name"][3]["middle"].ToString());

            // indexes that should not be affected
            Assert.AreEqual("Shuzhao", _loadedPropertyManager.Value["name"][0]["first"].ToString());
            Assert.AreEqual("Feng", _loadedPropertyManager.Value["name"][0]["last"].ToString());
            Assert.ThrowsException<NullReferenceException>(() => _loadedPropertyManager.Value["name"][0]["middle"].ToString());

            // other values in the same index should not be affected
            Assert.AreEqual("John", _loadedPropertyManager.Value["name"][2]["first"].ToString());
            Assert.AreEqual("Doe", _loadedPropertyManager.Value["name"][2]["last"].ToString());
            Assert.AreEqual("Jane", _loadedPropertyManager.Value["name"][3]["first"].ToString());
            Assert.AreEqual("Doa", _loadedPropertyManager.Value["name"][3]["last"].ToString());

            // no extra properties are forced
            Assert.ThrowsException<NullReferenceException>(() => _loadedPropertyManager.Value["name"][1]["first"].ToString());
            Assert.ThrowsException<NullReferenceException>(() => _loadedPropertyManager.Value["name"][1]["last"].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedPropertyManager.Value["name"][4].ToString());
        }

        [TestMethod]
        public void CanForcePropertyNestedInIndexesUnderExistingKey()
        {
            _loadedPropertyManager.Force("name[2, 3].last", "Smith");

            // indexes that should be affected
            Assert.AreEqual("Smith", _loadedPropertyManager.Value["name"][2]["last"].ToString());
            Assert.AreEqual("Smith", _loadedPropertyManager.Value["name"][3]["last"].ToString());

            // indexes that should not be affected
            Assert.AreEqual("Shuzhao", _loadedPropertyManager.Value["name"][0]["first"].ToString());
            Assert.AreEqual("Feng", _loadedPropertyManager.Value["name"][0]["last"].ToString());

            // other values in the same index should not be affected
            Assert.AreEqual("John", _loadedPropertyManager.Value["name"][2]["first"].ToString());
            Assert.AreEqual("Jane", _loadedPropertyManager.Value["name"][3]["first"].ToString());

            // no extra properties are forced
            Assert.ThrowsException<NullReferenceException>(() => _loadedPropertyManager.Value["name"][1]["first"].ToString());
            Assert.ThrowsException<NullReferenceException>(() => _loadedPropertyManager.Value["name"][1]["last"].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedPropertyManager.Value["name"][4].ToString());
        }

        [TestMethod]
        public void CanForcePropertyNestedInIndexesUnderNewAndExistingKey()
        {
            _loadedPropertyManager.Force("name[1, 2, 3].last", "Smith");

            // indexes that should be affected
            Assert.AreEqual("Smith", _loadedPropertyManager.Value["name"][1]["last"].ToString());
            Assert.AreEqual("Smith", _loadedPropertyManager.Value["name"][2]["last"].ToString());
            Assert.AreEqual("Smith", _loadedPropertyManager.Value["name"][3]["last"].ToString());

            // indexes that should not be affected
            Assert.AreEqual("Shuzhao", _loadedPropertyManager.Value["name"][0]["first"].ToString());
            Assert.AreEqual("Feng", _loadedPropertyManager.Value["name"][0]["last"].ToString());

            // other values in the same index should not be affected
            Assert.AreEqual("John", _loadedPropertyManager.Value["name"][2]["first"].ToString());
            Assert.AreEqual("Jane", _loadedPropertyManager.Value["name"][3]["first"].ToString());

            // no extra properties are forced
            Assert.ThrowsException<NullReferenceException>(() => _loadedPropertyManager.Value["name"][1]["first"].ToString());

            // no extra indexes are forced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedPropertyManager.Value["name"][4].ToString());
        }

        [TestMethod]
        public void CanForceNegativeIndexes()
        {
            _emptyManager.Force("name[-2, 2]", "Shuzhao");

            // empty indexes forced to fill the gap
            Assert.AreEqual("{}", _emptyManager.Value["name"][0].ToString());

            // indexes that should be affected
            // C# array doesn't allow negative index value (but we do), so -2 is converted to 1.
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
