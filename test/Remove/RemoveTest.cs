namespace JsonPathSerializerTest.Remove
{
    [TestClass]
    public class RemoveTest
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
        public void CanRemoveKeyFromEmptyManager()
        {
            var removed = _emptyManager.Remove("name");

            var expected = JToken.Parse(@"{}");
            var actual = JToken.Parse(_emptyManager.Build());

            Assert.AreEqual(null, removed);
            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void CanRemoveKeyFromLoadedManager()
        {
            var removed = _loadedManager.Remove("name");

            var expected = JToken.Parse(@"{}");
            var actual = JToken.Parse(_loadedManager.Build());

            Assert.AreEqual("John Doe", removed?.ToString());
            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void CanRemoveEmptyKeyFromLoadedManager()
        {
            var removed = _loadedManager.Remove("name.first");

            var expected = JToken.Parse(@"{
                ""name"": ""John Doe"",
            }");
            var actual = JToken.Parse(_loadedManager.Build());

            Assert.AreEqual(null, removed);
            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }
    }
}