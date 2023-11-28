namespace JsonPathSerializerTest.Add.Priorities.High;

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
        _manager.Add("name.first[0]", "Jane", Priority.High);

        Assert.AreEqual("Jane", _manager.Value["name"]["first"][0].ToString());
    }

    [TestMethod]
    public void CanAddIndexToObject()
    {
        _manager.Add("name[0]", "John", Priority.High);

        Assert.AreEqual("John", _manager.Value["name"][0].ToString());
    }

    [TestMethod]
    public void CanAddIndexToArrayWithChildren()
    {
        _manager.Add("name.first[1]", "Smith", Priority.High);

        Assert.AreEqual("Smith", _manager.Value["name"]["first"][1].ToString());
    }

    [TestMethod]
    public void CanAddIndexAsChildUnderIndexValue()
    {
        _manager.Add("name.last[0]", "John", Priority.High);

        Assert.AreEqual("John", _manager.Value["name"]["last"][0].ToString());
    }
}
