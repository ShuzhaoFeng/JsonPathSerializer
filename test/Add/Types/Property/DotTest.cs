namespace JsonPathSerializerTest.Add.Types.Property;

[TestClass]
public class DotTest
{
    private JsonPathManager _emptyManager = new();
    private JsonPathManager _loadedManager = new();

    [TestInitialize]
    public void Setup()
    {
        _emptyManager = new JsonPathManager();
        _loadedManager = new JsonPathManager(@"{
                ""name"": {
                    ""first"" : ""Shuzhao"",
                },
            }");
    }

    [TestMethod]
    public void CanAddProperty()
    {
        _emptyManager.Add("name", "Shuzhao Feng", Priority.Normal);

        Assert.AreEqual("Shuzhao Feng", _emptyManager.Value["name"].ToString());
    }

    [TestMethod]
    public void CanAddNestedProperty()
    {
        _emptyManager.Add("name.first", "Shuzhao", Priority.Normal);

        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"]["first"].ToString());
    }

    [TestMethod]
    public void CanAddPropertyUnderExistingObject()
    {
        _loadedManager.Add("name.last", "Feng", Priority.Normal);

        Assert.AreEqual("Shuzhao", _loadedManager.Value["name"]["first"].ToString());
        Assert.AreEqual("Feng", _loadedManager.Value["name"]["last"].ToString());
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingPropertyWithDoubleDots()
    {
        Assert.ThrowsException<ArgumentException>(() => _emptyManager.Add("name..last", "Feng", Priority.Normal));
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingPropertyWithEndingDot()
    {
        Assert.ThrowsException<JsonException>(() => _emptyManager.Add("name.last.", "Feng", Priority.Normal));
    }
}