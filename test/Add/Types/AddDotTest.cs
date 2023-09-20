namespace JsonPathSerializerTest.Add.Types
{
    [TestClass]
    public class AddDotTest
    {
        private JsonPathManager _emptyManager = new();
        private JsonPathManager _loadedManager = new();
        private JsonPathManager _indexedManager = new();

        [TestInitialize]
        public void Setup()
        {
            _emptyManager = new JsonPathManager();
            _loadedManager = new JsonPathManager(@"{
                ""name"": {
                    ""first"" : ""Shuzhao"",
                },
            }");
            _indexedManager = new JsonPathManager(@"{
                ""name"": [
                    ""Shuzhao"",
                ],
            }");
        }

        [TestMethod]
        public void CanAddProperty()
        {
            _emptyManager.Add("name", "Shuzhao Feng");

            Assert.AreEqual("Shuzhao Feng", _emptyManager.Value["name"].ToString());
        }

        [TestMethod]
        public void CanAddNestedProperty()
        {
            _emptyManager.Add("name.first", "Shuzhao");

            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"]["first"].ToString());
        }

        [TestMethod]
        public void CanInsertPropertyUnderExistingParentProperty()
        {
            _loadedManager.Add("name.last", "Feng");

            Assert.AreEqual("Shuzhao", _loadedManager.Value["name"]["first"].ToString());
            Assert.AreEqual("Feng", _loadedManager.Value["name"]["last"].ToString());
        }

        [TestMethod]
        public void ThrowsExceptionWhenAddingPropertyWithDoubleDots()
        {
            Assert.ThrowsException<ArgumentException>(() => _emptyManager.Add("name..last", "Feng"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenAddingPropertyWithEndingDot()
        {
            Assert.ThrowsException<JsonException>(() => _emptyManager.Add("name.last.", "Feng"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenInsertingValueToArray()
        {
            Assert.ThrowsException<ArgumentException>(() => _indexedManager.Add("name.last", "Feng"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenInsertingValueToParentProperty()
        {
            Assert.ThrowsException<ArgumentException>(() => _loadedManager.Add("name", "Shuzhao Feng"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenInsertingValueAsChildUnderExistingValue()
        {
            Assert.ThrowsException<ArgumentException>(() => _loadedManager.Add("name.first.English", "Shuzhao"));
        }
    }
}

