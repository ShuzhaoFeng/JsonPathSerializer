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
            var removed = _loadedManager.Remove("name[1]");

            // removed value is returned
            Assert.AreEqual("Feng", removed?.ToString());

            // smaller indexes remain untouched
            Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());

            // greater indexes are shifted
            Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][1].ToString());
            Assert.AreEqual("SF", _loadedManager.Value["name"][2].ToString());

            // list count is reduced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][3]);
        }

        [TestMethod]
        public void CanRemoveFirstIndex()
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

        [TestMethod]
        public void CanRemoveLastIndex()
        {
            var removed = _loadedManager.Remove("name[3]");

            // removed value is returned
            Assert.AreEqual("SF", removed?.ToString());

            // smaller indexes remain untouched
            Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());
            Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());
            Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());

            // list count is reduced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][3]);
        }

        [TestMethod]
        public void CanRemoveIndexNestedInIndex()
        {
            var initialValue0 = JToken.Parse(_loadedBigManager.Build())["name"]?[0];
            var initialValue1 = JToken.Parse(_loadedBigManager.Build())["name"]?[1];
            var initialValue2 = JToken.Parse(_loadedBigManager.Build())["name"]?[2];
            var initialValue4 = JToken.Parse(_loadedBigManager.Build())["name"]?[4];


            var removed = _loadedBigManager.Remove("name[3][1]");

            // removed value is returned
            Assert.AreEqual("Feng", removed?.ToString());

            // unrelated indexes remain untouched
            Assert.IsTrue(JToken.DeepEquals(initialValue0, JToken.Parse(_loadedBigManager.Build())?["name"]?[0]));
            Assert.IsTrue(JToken.DeepEquals(initialValue1, JToken.Parse(_loadedBigManager.Build())?["name"]?[1]));
            Assert.IsTrue(JToken.DeepEquals(initialValue2, JToken.Parse(_loadedBigManager.Build())?["name"]?[2]));
            Assert.IsTrue(JToken.DeepEquals(initialValue4, JToken.Parse(_loadedBigManager.Build())?["name"]?[4]));

            // smaller indexes remain untouched
            Assert.AreEqual("Shuzhao", _loadedBigManager.Value["name"][3][0].ToString());

            // greater indexes are shifted
            Assert.AreEqual("Shuzhao Feng", _loadedBigManager.Value["name"][3][1].ToString());

            // list count is reduced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedBigManager.Value["name"][3][2]);

        }

        [TestMethod]
        public void CanRemoveNegativeIndex()
        {
            var removed = _loadedManager.Remove("name[-2]");

            // removed value is returned
            Assert.AreEqual("Shuzhao Feng", removed?.ToString());

            // smaller indexes remain untouched
            Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());
            Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());

            // greater indexes are shifted
            Assert.AreEqual("SF", _loadedManager.Value["name"][2].ToString());

            // list count is reduced
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][3]);
        }
    }
}
