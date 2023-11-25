namespace JsonPathSerializerTest.Add;

[TestClass]
public class AddTest
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
    public void CanAddKeyValuePairToEmptyManager()
    {
        _emptyManager.Add("name", "John Doe", Priority.Normal);

        var expected = JToken.Parse(@"{
                ""name"": ""John Doe"",
            }");
        var actual = JToken.Parse(_emptyManager.Build());

        Assert.IsTrue(JToken.DeepEquals(expected, actual));
    }

    [TestMethod]
    public void CanAddKeyValuePairToLoadedManager()
    {
        _loadedManager.Add("age", 42, Priority.Normal);

        var expected = JToken.Parse(@"{
                ""name"": ""John Doe"",
                ""age"": 42
            }");
        var actual = JToken.Parse(_loadedManager.Build());

        Assert.IsTrue(JToken.DeepEquals(expected, actual));
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingNullValueToManager()
    {
        Assert.ThrowsException<ArgumentNullException>(() => _emptyManager.Add("name", null, Priority.Normal));
    }
}