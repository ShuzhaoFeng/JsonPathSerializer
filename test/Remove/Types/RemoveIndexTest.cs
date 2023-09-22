namespace JsonPathSerializerTest.Remove.Types
{
    [TestClass]
    public class RemoveIndexTest
    {
        private JsonPathManager _loadedManager = new();
        private JsonPathManager _loadedBigManager = new();

        [TestInitialize]
        public void Initialize()
        {
            _loadedManager = new JsonPathManager(@"{
                ""name"": [
                    ""Shuzhao"",
                    ""Feng"",
                    ""Shuzhao Feng"",
                    ""SF""
                ],
            }");

            _loadedBigManager = new JsonPathManager(@"{
                ""name"": [
                    [],
                    [""Shuzhao Feng""],
                    [""Shuzhao"", ""Feng""],
                    [""Shuzhao"", ""Feng"", ""Shuzhao Feng""],
                    [""Shuzhao"", ""Feng"", ""Shuzhao Feng"", ""SF""],
                ],
            }");
        }

        [TestMethod]
        public void CanRemoveIndex()
        {
            var removed = _loadedManager.Remove("name[0]");

            // removed value is returned
            Assert.AreEqual("Shuzhao", removed?.ToString());

            // greater indexes are shifted
            Assert.AreEqual("Feng", _loadedManager.Value["name"][0].ToString());
            Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][1].ToString());
            Assert.AreEqual("SF", _loadedManager.Value["name"][2].ToString());

            // list count is reduced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][3]);
        }
    }
}
