namespace JsonPathSerializerTest.Add.Priorities.High;

[TestClass]
public class PropertyTest
{
    private JsonPathManager _manager = new();

    [TestInitialize]
    public void Initialize()
    {
        _manager = new JsonPathManager(@"{
                ""name"": {
                    ""first"": [""John"", ""Smith""],
                    ""last"": ""Doe""
                },
            }");
    }

    [TestMethod]
    public void CanAddPropertyToObjectWithExistingKey()
    {
        _manager.Add("name.last", "Smith", Priority.High);

        Assert.AreEqual("Smith", _manager.Value["name"]["last"].ToString());
    }

    [TestMethod]
    public void CanAddPropertyToArray()
    {
        _manager.Add("name.first.first", "John", Priority.High);

        Assert.AreEqual("John", _manager.Value["name"]["first"]["first"].ToString());
    }

    [TestMethod]
    public void CanAddPropertyToObjectWithChildren()
    {
        _manager.Add("name", "John Smith Doe", Priority.High);

        Assert.AreEqual("John Smith Doe", _manager.Value["name"].ToString());
    }

    [TestMethod]
    public void CanAddPropertyAsChildUnderPropertyValue()
    {
        _manager.Add("name.last.first", "Doe", Priority.High);

        Assert.AreEqual("Doe", _manager.Value["name"]["last"]["first"].ToString());
    }
}