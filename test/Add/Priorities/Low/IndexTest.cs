namespace JsonPathSerializerTest.Add.Priorities.Low;

[TestClass]
public class IndexTest
{
    private JsonPathManager _manager = new();

    [TestInitialize]
    public void Initialize()
    {
        _manager = new JsonPathManager(@"{
                ""name"": {
                    ""first"": [""John"", [""William"", ""Will""]],
                    ""last"": ""Doe""
                },
            }");
    }

    [TestMethod]
    public void ThrowExceptionWhenAddingIndexToArrayWithExistingIndex()
    {
        Assert.ThrowsException<JsonPathSerializerException>(() => _manager.Add("name.first[0]", "Jane", Priority.Low));
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingIndexToObject()
    {
        Assert.ThrowsException<JsonPathSerializerException>(() => _manager.Add("name[0]", "John", Priority.Low));
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingIndexToArrayWithChildren()
    {
        Assert.ThrowsException<JsonPathSerializerException>(() => _manager.Add("name.first[1]", "Smith", Priority.Low));
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingIndexAsChildUnderIndexValue()
    {
        Assert.ThrowsException<JsonPathSerializerException>(() => _manager.Add("name.last[0]", "John", Priority.Low));
    }
}
