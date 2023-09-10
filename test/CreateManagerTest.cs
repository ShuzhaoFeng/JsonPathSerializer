namespace JsonPathSerializerTest
{
    [TestClass]
    public class CreateManagerTest
    {
        [TestMethod]
        public void CanCreateEmptyManager()
        {
            var manager = new JsonPathManager();
            Assert.IsNotNull(manager);
        }

        [TestMethod]
        public void CanCreateManagerWithEmptyJsonString()
        {
            var manager = new JsonPathManager("{}");
            Assert.IsNotNull(manager);
        }

        [TestMethod]
        public void CanCreateManagerWithEmptyJsonToken()
        {
            var token = JToken.Parse("{}");
            var manager = new JsonPathManager(token);
            Assert.IsNotNull(manager);
        }

        [TestMethod]
        public void CanCreateManagerWithLoadedJsonString()
        {
            string json = @"{
                ""name"": ""John Doe"",
            }";

            var manager = new JsonPathManager(json);
            Assert.IsNotNull(manager);

            var expected = JToken.Parse(json);
            var actual = JToken.Parse(manager.Build());

            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void CanCreateManagerWithLoadedJsonToken()
        {
            string json = @"{
                ""name"": ""John Doe"",
            }";

            var token = JToken.Parse(json);
            var manager = new JsonPathManager(token);
            Assert.IsNotNull(manager);

            var expected = JToken.Parse(json);
            var actual = JToken.Parse(manager.Build());

            Assert.IsTrue(JToken.DeepEquals(expected, actual));
        }

        [TestMethod]
        public void ThrowsExceptionWhenCreatingManagerWithInvalidJsonString()
        {
            string json = @"{
                ""name"": ""John Doe"",
            ";

            Assert.ThrowsException<Newtonsoft.Json.JsonReaderException>(() => new JsonPathManager(json));
        }
    }
}