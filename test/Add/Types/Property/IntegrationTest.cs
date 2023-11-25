namespace JsonPathSerializerTest.Add.Types.Property;

public class IntegrationTest
{
    private JsonPathManager _emptyManager = new();

    [TestInitialize]
    public void Setup()
    {
        _emptyManager = new JsonPathManager();
    }

    [TestMethod]
    public void CanAddDotNotationThenBracketNotation()
    {
        _emptyManager.Add("name['first']", "Shuzhao", Priority.Normal);

        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"]["first"].ToString());
    }

    [TestMethod]
    public void CanAddBracketNotationThenDotNotation()
    {
        _emptyManager.Add("['name'].first", "Shuzhao", Priority.Normal);

        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"]["first"].ToString());
    }
}

