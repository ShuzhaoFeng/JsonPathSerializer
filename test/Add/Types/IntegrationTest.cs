namespace JsonPathSerializerTest.Add.Types;
public class IntegrationTest
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
    public void CanAddPropertyNestedInIndexSpan()
    {
        _manager.Add("name[1:3].middle", "A.", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("A.", _manager.Value["name"][1]["middle"].ToString());
        Assert.AreEqual("A.", _manager.Value["name"][2]["middle"].ToString());
        Assert.AreEqual("A.", _manager.Value["name"][3]["middle"].ToString());

        // indexes that should not be affected
        Assert.AreEqual("Shuzhao", _manager.Value["name"][0]["first"].ToString());
        Assert.AreEqual("Feng", _manager.Value["name"][0]["last"].ToString());
        Assert.ThrowsException<NullReferenceException>(() =>
            _manager.Value["name"][0]["middle"].ToString());

        // other values in the same index should not be affected
        Assert.AreEqual("John", _manager.Value["name"][2]["first"].ToString());
        Assert.AreEqual("Doe", _manager.Value["name"][2]["last"].ToString());
        Assert.AreEqual("Jane", _manager.Value["name"][3]["first"].ToString());
        Assert.AreEqual("Doa", _manager.Value["name"][3]["last"].ToString());

        // no extra properties are added
        Assert.ThrowsException<NullReferenceException>(
            () => _manager.Value["name"][1]["first"].ToString());
        Assert.ThrowsException<NullReferenceException>(() =>
            _manager.Value["name"][1]["last"].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _manager.Value["name"][4].ToString());
    }

    [TestMethod]
    public void CanAddPropertyNestedInIndexSpanUnderExistingKey()
    {
        _manager.Add("name[2:3].last", "Smith", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Smith", _manager.Value["name"][2]["last"].ToString());
        Assert.AreEqual("Smith", _manager.Value["name"][3]["last"].ToString());

        // indexes that should not be affected
        Assert.AreEqual("Shuzhao", _manager.Value["name"][0]["first"].ToString());
        Assert.AreEqual("Feng", _manager.Value["name"][0]["last"].ToString());

        // other values in the same index should not be affected
        Assert.AreEqual("John", _manager.Value["name"][2]["first"].ToString());
        Assert.AreEqual("Jane", _manager.Value["name"][3]["first"].ToString());

        // no extra properties are added
        Assert.ThrowsException<NullReferenceException>(
            () => _manager.Value["name"][1]["first"].ToString());
        Assert.ThrowsException<NullReferenceException>(() =>
            _manager.Value["name"][1]["last"].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _manager.Value["name"][4].ToString());
    }

    [TestMethod]
    public void CanAddPropertyNestedInIndexSpanUnderNewAndExistingKey()
    {
        _manager.Add("name[1:3].last", "Smith", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Smith", _manager.Value["name"][1]["last"].ToString());
        Assert.AreEqual("Smith", _manager.Value["name"][2]["last"].ToString());
        Assert.AreEqual("Smith", _manager.Value["name"][3]["last"].ToString());

        // indexes that should not be affected
        Assert.AreEqual("Shuzhao", _manager.Value["name"][0]["first"].ToString());
        Assert.AreEqual("Feng", _manager.Value["name"][0]["last"].ToString());

        // other values in the same index should not be affected
        Assert.AreEqual("John", _manager.Value["name"][2]["first"].ToString());
        Assert.AreEqual("Jane", _manager.Value["name"][3]["first"].ToString());

        // no extra properties are added
        Assert.ThrowsException<NullReferenceException>(
            () => _manager.Value["name"][1]["first"].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _manager.Value["name"][4].ToString());
    }
}
