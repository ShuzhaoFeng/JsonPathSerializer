namespace JsonPathSerializerTest.Add.Priorities.Normal;

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
    public void CanAddIndexToArrayWithExistingIndex()
    {
        _manager.Add("name.first[0]", "Jane", Priority.Normal);

        Assert.AreEqual("Jane", _manager.Value["name"]["first"][0].ToString());
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingIndexToObject()
    {
        Assert.ThrowsException<JsonPathSerializerException>(() => _manager.Add("name[0]", "John", Priority.Normal));
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingIndexToArrayWithChildren()
    {
        Assert.ThrowsException<JsonPathSerializerException>(() => _manager.Add("name.first[1]", "Smith", Priority.Normal));
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingIndexAsChildUnderIndexValue()
    {
        Assert.ThrowsException<JsonPathSerializerException>(() => _manager.Add("name.last[0]", "John", Priority.Normal));
    }
}

