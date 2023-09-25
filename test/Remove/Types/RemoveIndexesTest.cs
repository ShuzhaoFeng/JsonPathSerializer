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

        [TestMethod]
        public void CanRemoveNegativeIndexes()
        {
            var removed = _loadedManager.Remove("name[-2, -1]");

            // removed values are returned
            Assert.AreEqual("Shuzhao Feng", removed?[0]?.ToString());
            Assert.AreEqual("SF", removed?[1]?.ToString());

            // smaller indexes remain untouched
            Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());
            Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());

            // list count is reduced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][2]);
        }

        [TestMethod]
        public void CanRemovePositiveAndNegativeIndexes()
        {
            var removed = _loadedManager.Remove("name[1, -1]");

            // removed values are returned
            Assert.AreEqual("Feng", removed?[0]?.ToString());
            Assert.AreEqual("SF", removed?[1]?.ToString());

            // smaller indexes remain untouched
            Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());

            // greater indexes are shifted
            Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][1].ToString());

            // list count is reduced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][2]);
        }

        [TestMethod]
        public void CanRemoveExistingAndNonExistingIndexes()
        {
            var removed = _loadedManager.Remove("name[1, 5]");

            // removed values are returned
            Assert.AreEqual("Feng", removed?[0]?.ToString());

            // non-existing indexes are ignored
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => removed?[1]?.ToString());

            // smaller indexes remain untouched
            Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());

            // greater indexes are shifted
            Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][1].ToString());
            Assert.AreEqual("SF", _loadedManager.Value["name"][2].ToString());

            // list count is reduced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][3]);
        }

        [TestMethod]
        public void CanRemoveIndexesNestedInIndex()
        {
            var initialValue0 = JToken.Parse(_loadedBigManager.Build())["name"]?[0];
            var initialValue1 = JToken.Parse(_loadedBigManager.Build())["name"]?[1];
            var initialValue2 = JToken.Parse(_loadedBigManager.Build())["name"]?[2];
            var initialValue3 = JToken.Parse(_loadedBigManager.Build())["name"]?[3];

            var removed = _loadedBigManager.Remove("name[4][1, 2]");

            // removed values are returned
            Assert.AreEqual("Feng", removed?[0]?.ToString());
            Assert.AreEqual("Shuzhao Feng", removed?[1]?.ToString());

            // unrelated indexes remain untouched
            Assert.IsTrue(JToken.DeepEquals(initialValue0, JToken.Parse(_loadedBigManager.Build())?["name"]?[0]));
            Assert.IsTrue(JToken.DeepEquals(initialValue1, JToken.Parse(_loadedBigManager.Build())?["name"]?[1]));
            Assert.IsTrue(JToken.DeepEquals(initialValue2, JToken.Parse(_loadedBigManager.Build())?["name"]?[2]));
            Assert.IsTrue(JToken.DeepEquals(initialValue3, JToken.Parse(_loadedBigManager.Build())?["name"]?[3]));

            // smaller indexes remain untouched
            Assert.AreEqual("Shuzhao", _loadedBigManager.Value["name"][4][0].ToString());

            // greater indexes are shifted
            Assert.AreEqual("SF", _loadedBigManager.Value["name"][4][1].ToString());

            // list count is reduced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedBigManager.Value["name"][4][2]);
        }
    }
}
