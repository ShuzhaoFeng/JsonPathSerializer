namespace JsonPathSerializerTest.Force
{
    public class ForceTest
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
        public void CanForceKeyValuePairToEmptyManager()
        {
            _emptyManager.Add("name", "John Doe");

            var expected = JToken.Parse(@"{
                ""name"": ""John Doe"",
            }");
            var actual = JToken.Parse(_emptyManager.Build());

            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void CanForceKeyValuePairToLoadedManager()
        {
            _loadedManager.Force("age", 42);

            var expected = JToken.Parse(@"{
                ""name"": ""John Doe"",
                ""age"": 42
            }");
            var actual = JToken.Parse(_loadedManager.Build());

            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void CanForceKeyValuePairToLoadedManagerWithExistingKey()
        {
            _loadedManager.Force("name", "Jane Doe");

            var expected = JToken.Parse(@"{
                ""name"": ""Jane Doe"",
            }");
            var actual = JToken.Parse(_loadedManager.Build());

            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void CanForceKeyValuePairToLoadedManagerWithExistingKeyAndValue()
        {
            _loadedManager.Force("name", "John Doe");

            var expected = JToken.Parse(@"{
                ""name"": ""John Doe"",
            }");
            var actual = JToken.Parse(_loadedManager.Build());

            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void ThrowsExceptionWhenForcingNullValueToManager()
        {
            Assert.ThrowsException<ArgumentNullException>(() => _emptyManager.Force("name", null));
        }
    }
}
