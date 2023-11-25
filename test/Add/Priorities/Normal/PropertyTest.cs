namespace JsonPathSerializerTest.Add.Priorities.Normal;

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
        _manager.Add("name.last", "Smith", Priority.Normal);

        Assert.AreEqual("Smith", _manager.Value["name"]["last"].ToString());
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingPropertyToArray()
    {
        Assert.ThrowsException<ArgumentException>(() => _manager.Add("name.first.first", "John", Priority.Normal));
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingPropertyToObjectWithChildren()
    {
        Assert.ThrowsException<ArgumentException>(() => _manager.Add("name", "John Smith Doe", Priority.Normal));
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingPropertyAsChildUnderPropertyValue()
    {
        Assert.ThrowsException<ArgumentException>(() => _manager.Add("name.last.first", "Doe", Priority.Normal));
    }
}