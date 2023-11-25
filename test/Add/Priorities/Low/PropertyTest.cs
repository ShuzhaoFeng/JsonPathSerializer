namespace JsonPathSerializerTest.Add.Priorities.Low;

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
    public void ThrowExceptionWhenAddingPropertyToObjectWithExistingKey()
    {
        Assert.ThrowsException<ArgumentException>(() => _manager.Add("name.last", "Smith", Priority.Low));
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingPropertyToArray()
    {
        Assert.ThrowsException<ArgumentException>(() => _manager.Add("name.first.first", "John", Priority.Low));
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingPropertyToObjectWithChildren()
    {
        Assert.ThrowsException<ArgumentException>(() => _manager.Add("name", "John Smith Doe", Priority.Low));
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingPropertyAsChildUnderPropertyValue()
    {
        Assert.ThrowsException<ArgumentException>(() => _manager.Add("name.last.first", "Doe", Priority.Low));
    }
}