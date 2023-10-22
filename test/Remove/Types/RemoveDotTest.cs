namespace JsonPathSerializerTest.Remove.Types;

[TestClass]
public class RemoveDotTest
{
    private JsonPathManager _loadedBigManager = new();
    private JsonPathManager _loadedManager = new();


    [TestInitialize]
    public void Initialize()
    {
        _loadedManager = new JsonPathManager(@"{
                ""name"": ""Shuzhao Feng"",
            }");

        _loadedBigManager = new JsonPathManager(@"{
                ""name"": {
                    ""first"": {
                        ""whole"": ""Shuzhao"",
                        ""split"": ""Shu Zhao"",
                    },
                    ""last"": ""Feng"",
                },
            }");
    }

    [TestMethod]
    public void CanRemoveProperty()
    {
        var removed = _loadedManager.Remove("name");

        Assert.AreEqual("Shuzhao Feng", removed?.ToString());
        Assert.AreEqual(null, _loadedManager.Value["name"]);
    }

    [TestMethod]
    public void CanRemovePropertyWithChildren()
    {
        var removed = _loadedBigManager.Remove("name.first");

        // children are removed
        Assert.AreEqual("Shuzhao", removed?["whole"]);
        Assert.AreEqual("Shu Zhao", removed?["split"]);

        // key is removed from root
        Assert.AreEqual(null, _loadedBigManager.Value["name"]["first"]);

        // other keys are untouched
        Assert.AreEqual("Feng", _loadedBigManager.Value["name"]["last"].ToString());

        // removed children are not accessible
        Assert.ThrowsException<InvalidOperationException>(() => _loadedManager.Value["name"]["first"]["whole"]);
        Assert.ThrowsException<InvalidOperationException>(() => _loadedManager.Value["name"]["first"]["split"]);
    }

    [TestMethod]
    public void ThrowsExceptionWhenRemovingPropertyWithDoubleDots()
    {
        Assert.ThrowsException<ArgumentException>(() => _loadedManager.Remove("name..last'"));
    }

    [TestMethod]
    public void ThrowsExceptionWhenRemovingPropertyWithWithEndingDot()
    {
        Assert.ThrowsException<JsonException>(() => _loadedManager.Remove("name.last."));
    }
}