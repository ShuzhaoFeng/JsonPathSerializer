namespace JsonPathSerializerTest
{
    [TestClass]
    public class SerializeAllTest
    {
        [TestMethod]
        public void CanSerializeKeyValuePair()
        {
            Dictionary<string, object> jsonPathToValues = new()
            {
                { "name", "John Doe" },
                { "age", "30" },
                { "address", "123 Main St." }
            };

            var output = JsonPathManager.SerializeAll(jsonPathToValues);
            Assert.IsNotNull(output);
        }

        [TestMethod]
        public void CanSerializeTuple()
        {
            (string, object)[] jsonPathToValues = {
                ("name", "John Doe"),
                ("age", "30"),
                ("address", "123 Main St.")
            };

            var output = JsonPathManager.SerializeAll(jsonPathToValues);
            Assert.IsNotNull(output);
        }

        [TestMethod]
        public void SerializeAllHasSameOutputAsManager()
        {
            Dictionary<string, object> jsonPathToValues = new()
            {
                { "name", "John Doe" },
                { "age", "30" },
                { "address", "123 Main St." }
            };

            var serializeAllOutput = JsonPathManager.SerializeAll(jsonPathToValues);

            var manager = new JsonPathManager();
            foreach (var (path, value) in jsonPathToValues)
            {
                manager.Add(path, value);
            }

            var managerOutput = manager.Build();
            Assert.AreEqual(serializeAllOutput, managerOutput);
        }
    }
}
