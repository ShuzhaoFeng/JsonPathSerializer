namespace JsonPathSerializerTest.Add.Types.Index;

[TestClass]
public class IndexSpanTest
{
    private JsonPathManager _emptyManager = new();
    private JsonPathManager _loadedManager = new();

    [TestInitialize]
    public void Setup()
    {
        _emptyManager = new JsonPathManager();
        _loadedManager = new JsonPathManager(@"{
                ""name"": [
                    ""Shuzhao"",
                    ""Feng"",
                    ""Shuzhao Feng"",
                    ""SF"",
                ],
            }");
    }

    [TestMethod]
    public void CanAddIndexSpan()
    {
        _emptyManager.Add("name[0:2]", "Shuzhao", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][2].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][3].ToString());
    }

    [TestMethod]
    public void CanAddMultipleIndexSpans()
    {
        _emptyManager.Add("name[0:1, 2:3]", "Shuzhao", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][2].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][3].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][4].ToString());
    }

    [TestMethod]
    public void CanAddMultipleOverlappingIndexSpans()
    {
        _emptyManager.Add("name[0:2, 1:3]", "Shuzhao", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][2].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][3].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][4].ToString());
    }

    [TestMethod]
    public void CanAddMultipleIndexSpansWithGap()
    {
        _emptyManager.Add("name[0:1, 3:4]", "Shuzhao", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][3].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][4].ToString());

        // empty indexes added to fill the gap
        Assert.AreEqual("{}", _emptyManager.Value["name"][2].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][5].ToString());
    }

    [TestMethod]
    public void CanAddIndexSpanAsRoot()
    {
        _emptyManager.Add("[0:2]", "Shuzhao Feng", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[0].ToString());
        Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[1].ToString());
        Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[2].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value[3].ToString());
    }

    [TestMethod]
    public void CanAddIndexSpanThatRequiresExpansion()
    {
        _loadedManager.Add("name[1:5]", "John Doe", Priority.Normal);

        // indexes that should be affected
        // reduce 9 assertions to a for loop
        for (var i = 1; i < 6; i++) Assert.AreEqual("John Doe", _loadedManager.Value["name"][i].ToString());

        // indexes that should not be affected
        Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][6].ToString());
    }

    [TestMethod]
    public void CanAddIndexSpanThatRequiresLargeExpansion()
    {
        _loadedManager.Add("name[1:123]", "John Doe", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("John Doe", _loadedManager.Value["name"][1].ToString());
        Assert.AreEqual("John Doe", _loadedManager.Value["name"][2].ToString());

        // middle ones are skipped

        Assert.AreEqual("John Doe", _loadedManager.Value["name"][122].ToString());
        Assert.AreEqual("John Doe", _loadedManager.Value["name"][123].ToString());

        // indexes that should not be affected
        Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][124].ToString());
    }

    [TestMethod]
    public void CanAddNegativeIndexSpan()
    {
        _emptyManager.Add("name[-5:-3]", "Shuzhao", Priority.Normal);

        // we need minimally 5 elements: 0 (-5), 1 (-4), 2 (-3), 3 (-2) and 4 (-1)

        // indexes that should be affected
        // C# array doesn't allow negative index value (but we do), so -5 is converted to 0 and -3 to 2.
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][2].ToString());

        // empty indexes added to fill the gap
        Assert.AreEqual("{}", _emptyManager.Value["name"][3].ToString());
        Assert.AreEqual("{}", _emptyManager.Value["name"][4].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][5].ToString());
    }

    [TestMethod]
    public void CanAddNegativeAndPositiveIndexSpan()
    {
        _emptyManager.Add("name[-5:2]", "Shuzhao", Priority.Normal);

        // we need minimally 5 elements: 0 (-5), 1 (-4), 2 (-3), 3 (-2) and 4 (-1)
        for (var i = 0; i < 5; i++) Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][i].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][5].ToString());
    }

    [TestMethod]
    public void CanAddReverseIndexSpan()
    {
        _emptyManager.Add("name[5:2]", "Shuzhao", Priority.Normal);

        // empty indexes added to fill the gap
        Assert.AreEqual("{}", _emptyManager.Value["name"][0].ToString());
        Assert.AreEqual("{}", _emptyManager.Value["name"][1].ToString());

        // indexes that should be affected
        // reduce 4 assertions to a for loop
        for (var i = 2; i < 6; i++) Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][i].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][6].ToString());
    }

    [TestMethod]
    public void CanAddReverseNegativeIndexSpan()
    {
        _emptyManager.Add("name[-2:-5]", "Shuzhao", Priority.Normal);

        // we need minimally 5 elements: 0 (-5), 1 (-4), 2 (-3), 3 (-2) and 4 (-1)

        // indexes that should be affected
        // C# array doesn't allow negative index value (but we do), so -5 is converted to 0 and etc.
        // reduce 4 assertions to a for loop
        for (var i = 0; i < 4; i++) Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][i].ToString());

        // empty indexes added to fill the gap
        Assert.AreEqual("{}", _emptyManager.Value["name"][4].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][5].ToString());
    }

    [TestMethod]
    public void CanAddReversePositiveAndNegativeIndexSpan()
    {
        _emptyManager.Add("name[2:-5]", "Shuzhao", Priority.Normal);

        // we need minimally 5 elements: 0 (-5), 1 (-4), 2 (-3), 3 (-2) and 4 (-1)

        // indexes that should be affected
        // reduce 8 assertions to a for loop
        for (var i = 0; i < 5; i++) Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][i].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][5].ToString());
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingIndexSpanWithInvalidSeparator()
    {
        Assert.ThrowsException<JsonException>(() => _emptyManager.Add("name[0;:1]", "Shuzhao", Priority.Normal));
    }

    [TestMethod]
    public void ThrowsExceptionWhenIndexSpanIsNotInteger()
    {
        Assert.ThrowsException<JsonException>(() => _emptyManager.Add("name[1.5:7/4]", "Shuzhao", Priority.Normal));
    }
}