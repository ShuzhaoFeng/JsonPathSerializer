namespace JsonPathSerializerTest.Remove.Types;

[TestClass]
public class RemoveIndexSpanTest
{
    private JsonPathManager _loadedBigManager = new();
    private JsonPathManager _loadedManager = new();

    [TestInitialize]
    public void Initialize()
    {
        _loadedManager = new JsonPathManager(@"{
                ""name"": [
                    ""Shuzhao"",
                    ""Feng"",
                    ""Shuzhao Feng"",
                    ""Shu Zhao Feng"",
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
                    [""Shuzhao"", ""Feng"", ""Shuzhao Feng"", ""Shu Zhao Feng"", ""SF""],
                ],
            }");
    }

    [TestMethod]
    public void CanRemoveIndexSpan()
    {
        var removed = _loadedManager.Remove("name[1:3]");

        // removed value is returned
        Assert.AreEqual("Feng", removed?[0]?.ToString());
        Assert.AreEqual("Shuzhao Feng", removed?[1]?.ToString());
        Assert.AreEqual("Shu Zhao Feng", removed?[2]?.ToString());

        // smaller indexes remain untouched
        Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());

        // greater indexes are shifted
        Assert.AreEqual("SF", _loadedManager.Value["name"][1].ToString());

        // list count is reduced
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][2]);
    }

    [TestMethod]
    public void CanRemoveReverseIndexSpan()
    {
        var removed = _loadedManager.Remove("name[3:1]");

        // removed value is returned
        Assert.AreEqual("Feng", removed?[0]?.ToString());
        Assert.AreEqual("Shuzhao Feng", removed?[1]?.ToString());
        Assert.AreEqual("Shu Zhao Feng", removed?[2]?.ToString());

        // smaller indexes remain untouched
        Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());

        // greater indexes are shifted
        Assert.AreEqual("SF", _loadedManager.Value["name"][1].ToString());

        // list count is reduced
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][2]);
    }

    [TestMethod]
    public void CanRemoveExistingAndOutOfRangeIndexSpan()
    {
        var removed = _loadedManager.Remove("name[1:10]");

        // removed value is returned
        Assert.AreEqual("Feng", removed?[0]?.ToString());
        Assert.AreEqual("Shuzhao Feng", removed?[1]?.ToString());
        Assert.AreEqual("Shu Zhao Feng", removed?[2]?.ToString());
        Assert.AreEqual("SF", removed?[3]?.ToString());

        // non-existing indexes are ignored
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => removed?[4]?.ToString());

        // smaller indexes remain untouched
        Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());

        // list count is reduced
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][1]);
    }

    [TestMethod]
    public void CanRemoveNegativeIndexSpan()
    {
        var removed = _loadedManager.Remove("name[-4:-2]");

        // removed value is returned
        Assert.AreEqual("Feng", removed?[0]?.ToString());
        Assert.AreEqual("Shuzhao Feng", removed?[1]?.ToString());
        Assert.AreEqual("Shu Zhao Feng", removed?[2]?.ToString());

        // smaller indexes remain untouched
        Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());

        // greater indexes are shifted
        Assert.AreEqual("SF", _loadedManager.Value["name"][1].ToString());

        // list count is reduced
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][2]);
    }

    [TestMethod]
    public void CanRemoveNegativeReverseIndexSpan()
    {
        var removed = _loadedManager.Remove("name[-2:-4]");

        // removed value is returned
        Assert.AreEqual("Feng", removed?[0]?.ToString());
        Assert.AreEqual("Shuzhao Feng", removed?[1]?.ToString());
        Assert.AreEqual("Shu Zhao Feng", removed?[2]?.ToString());

        // smaller indexes remain untouched
        Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());

        // greater indexes are shifted
        Assert.AreEqual("SF", _loadedManager.Value["name"][1].ToString());

        // list count is reduced
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][2]);
    }

    [TestMethod]
    public void CanRemoveNegativeExistingAndOutOfRangeIndexSpan()
    {
        var removed = _loadedManager.Remove("name[-10:-2]");

        // removed value is returned
        Assert.AreEqual("Shuzhao", removed?[0]?.ToString());
        Assert.AreEqual("Feng", removed?[1]?.ToString());
        Assert.AreEqual("Shuzhao Feng", removed?[2]?.ToString());
        Assert.AreEqual("Shu Zhao Feng", removed?[3]?.ToString());

        // non-existing indexes are ignored
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => removed?[4]?.ToString());

        // greater indexes are shifted
        Assert.AreEqual("SF", _loadedManager.Value["name"][0].ToString());

        // list count is reduced
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][1]);
    }

    [TestMethod]
    public void CanRemovePositiveAndNegativeIndexSpan()
    {
        var removed = _loadedManager.Remove("name[-1:1]");

        // removed value is returned
        Assert.AreEqual("Shuzhao", removed?[0]?.ToString());
        Assert.AreEqual("Feng", removed?[1]?.ToString());
        Assert.AreEqual("SF", removed?[2]?.ToString());

        // greater indexes are shifted
        Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][0].ToString());
        Assert.AreEqual("Shu Zhao Feng", _loadedManager.Value["name"][1].ToString());

        // list count is reduced
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][2]);
    }

    [TestMethod]
    public void CanRemovePositiveAndNegativeReverseIndexSpan()
    {
        var removed = _loadedManager.Remove("name[1:-1]");

        // removed value is returned
        Assert.AreEqual("Shuzhao", removed?[0]?.ToString());
        Assert.AreEqual("Feng", removed?[1]?.ToString());
        Assert.AreEqual("SF", removed?[2]?.ToString());

        // greater indexes are shifted
        Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][0].ToString());
        Assert.AreEqual("Shu Zhao Feng", _loadedManager.Value["name"][1].ToString());

        // list count is reduced
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][2]);
    }

    [TestMethod]
    public void CanRemoveIndexSpanNestedInIndex()
    {
        var initialValue0 = JToken.Parse(_loadedBigManager.Build())["name"]?[0];
        var initialValue1 = JToken.Parse(_loadedBigManager.Build())["name"]?[1];
        var initialValue2 = JToken.Parse(_loadedBigManager.Build())["name"]?[2];
        var initialValue3 = JToken.Parse(_loadedBigManager.Build())["name"]?[3];

        var removed = _loadedBigManager.Remove("name[4][1:2]");

        // removed values are returned
        Assert.AreEqual("Feng", removed?[0]?.ToString());
        Assert.AreEqual("Shuzhao Feng", removed?[1]?.ToString());

        // unrelated indexes remain untouched
        Assert.IsTrue(JToken.DeepEquals(initialValue0, JToken.Parse(_loadedBigManager.Build())["name"]?[0]));
        Assert.IsTrue(JToken.DeepEquals(initialValue1, JToken.Parse(_loadedBigManager.Build())["name"]?[1]));
        Assert.IsTrue(JToken.DeepEquals(initialValue2, JToken.Parse(_loadedBigManager.Build())["name"]?[2]));
        Assert.IsTrue(JToken.DeepEquals(initialValue3, JToken.Parse(_loadedBigManager.Build())["name"]?[3]));

        // smaller indexes remain untouched
        Assert.AreEqual("Shuzhao", _loadedBigManager.Value["name"][4][0].ToString());

        // greater indexes are shifted
        Assert.AreEqual("SF", _loadedBigManager.Value["name"][4][1].ToString());

        // list count is reduced
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedBigManager.Value["name"][4][2]);
    }
}