namespace JsonPathSerializerTest.Append;

[TestClass]
public class AppendTest
{
    private JsonPathManager _manager = new();

    [TestInitialize]
    public void Setup()
    {
        _manager = new JsonPathManager(@"{
                ""name"": [
                    {
                        ""first"": ""Shuzhao"",
                        ""last"": ""Feng"",
                    },
                    {},
                    {
                        ""first"": ""John"",
                        ""last"": ""Doe"",
                    },
                    {
                        ""first"": ""Jane"",
                        ""last"": ""Doa"",
                    },
                ],
            }");
    }

    [TestMethod]
    public void CanAppendToArray()
    {
        _manager.Append("name", "John Smith", Priority.Normal);

        Assert.AreEqual("John Smith", _manager.Value["name"][4].ToString());
    }

    [TestMethod]
    public void CanAppendToExistingObjectIfHighPriority()
    {

        _manager.Append("name[0]", "John Smith", Priority.High);

        Assert.AreEqual("John Smith", _manager.Value["name"][0][0].ToString());
    }

    [TestMethod]
    public void ThrowsExceptionWhenAppendToExistingObjectIfNotHighPriority()
    {
        Assert.ThrowsException<ArgumentException>(() => _manager.Append("name[0]", "John Smith", Priority.Normal));
    }

    [TestMethod]
    public void CanAppendToExistingValueIfHighPriority()
    {

        _manager.Append("name.first[0]", "John Smith", Priority.High);

        Assert.AreEqual("John Smith", _manager.Value["name"]["first"][0][0].ToString());
    }
}

