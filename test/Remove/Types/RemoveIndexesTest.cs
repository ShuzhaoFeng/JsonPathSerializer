namespace JsonPathSerializerTest.Remove.Types
{
    [TestClass]
    public class RemoveIndexesTest
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
        public void CanRemoveIndexes()
        {
            var removed = _loadedManager.Remove("name[1, 2]");

            // removed values are returned
            Assert.AreEqual("Feng", removed?[0].ToString());
            Assert.AreEqual("Shuzhao Feng", removed?[1].ToString());

            // smaller indexes remain untouched
            Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());

            // greater indexes are shifted
            Assert.AreEqual("SF", _loadedManager.Value["name"][1].ToString());

            // list count is reduced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][2]);
        }
    }
}
