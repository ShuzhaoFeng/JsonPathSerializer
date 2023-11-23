namespace JsonPathSerializerTest.Add.Types;

[TestClass]
public class IndexTest
{
    private JsonPathManager _emptyManager = new();
    private JsonPathManager _loadedManager = new();
    private JsonPathManager _propertyManager = new();

    [TestInitialize]
    public void Setup()
    {
        _emptyManager = new JsonPathManager();
        _loadedManager = new JsonPathManager(@"{
                ""name"": [
                    ""Shuzhao"",
                    ""Feng"",
                    ""Shuzhao Feng"",
                ],
            }");
        _propertyManager = new JsonPathManager(@"{
                ""name"": {
                    ""first"": ""Shuzhao"",
                },
            }");
    }

    [TestMethod]
    public void CanAddIndex()
    {
        _emptyManager.Add("name[0]", "Shuzhao", Priority.Normal);

        // index that should be affected
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1].ToString());
    }

    [TestMethod]
    public void CanAddIndexAsRoot()
    {
        _emptyManager.Add("[0]", "Shuzhao Feng", Priority.Normal);

        // index that should be affected
        Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[0].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value[1].ToString());
    }

    [TestMethod]
    public void CanAddIndexNestedInIndex()
    {
        _emptyManager.Add("name[0][0]", "Shuzhao", Priority.Normal);

        // index that should be affected
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1].ToString());
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][1].ToString());
    }

    [TestMethod]
    public void CanAddIndexToExistingArray()
    {
        _loadedManager.Add("name[0]", "John Doe", Priority.Normal);

        // index that should be affected
        Assert.AreEqual("John Doe", _loadedManager.Value["name"][0].ToString());

        // indexes that should not be affected
        Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());
        Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][3].ToString());
    }

    [TestMethod]
    public void CanAddIndexToExistingArrayToLastPosition()
    {
        _loadedManager.Add("name[2]", "John Doe", Priority.Normal);

        // indexes that should not be affected
        Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());
        Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());

        // index that should be affected
        Assert.AreEqual("John Doe", _loadedManager.Value["name"][2].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][3].ToString());
    }

    [TestMethod]
    public void CanAddIndexToExistingArrayThatRequiresExpansion()
    {
        _loadedManager.Add("name[5]", "John Doe", Priority.Normal);

        // indexes that should not be affected
        Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());
        Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());
        Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());

        // empty indexes added to fill the gap
        Assert.AreEqual("{}", _loadedManager.Value["name"][3].ToString());
        Assert.AreEqual("{}", _loadedManager.Value["name"][4].ToString());

        // index that should be affected
        Assert.AreEqual("John Doe", _loadedManager.Value["name"][5].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][6].ToString());
    }

    [TestMethod]
    public void CanAddIndexToExistingArrayThatRequiresLargeExpansion()
    {
        _loadedManager.Add("name[123]", "John Doe", Priority.Normal);

        // indexes that should not be affected
        Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());
        Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());
        Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());

        // empty indexes added to fill the gap
        Assert.AreEqual("{}", _loadedManager.Value["name"][3].ToString());
        Assert.AreEqual("{}", _loadedManager.Value["name"][122].ToString());

        // index that should be affected
        Assert.AreEqual("John Doe", _loadedManager.Value["name"][123].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][124].ToString());
    }

    [TestMethod]
    public void CanAddNegativeIndex()
    {
        _emptyManager.Add("name[-1]", "Shuzhao", Priority.Normal);

        // index that should be affected
        // C# array doesn't allow negative index value (but we do), so -1 is converted to 0.
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1].ToString());
    }

    [TestMethod]
    public void CanAddNegativeIndexToExistingArray()
    {
        _loadedManager.Add("name[-1]", "John Doe", Priority.Normal);

        // indexes that should not be affected
        Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());
        Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());

        // index that should be affected
        // C# array doesn't allow negative index value (but we do), so -1 is converted to 2.
        Assert.AreEqual("John Doe", _loadedManager.Value["name"][2].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][3].ToString());
    }

    [TestMethod]
    public void CanAddNegativeIndexToExistingArrayThatRequiresExpansion()
    {
        _loadedManager.Add("name[-5]", "John Doe", Priority.Normal);

        // index that should be affected
        // C# array doesn't allow negative index value (but we do), so -5 is converted to 0.
        Assert.AreEqual("John Doe", _loadedManager.Value["name"][0].ToString());

        // indexes that should not be affected
        Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());
        Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());

        // empty indexes added to fill the gap
        Assert.AreEqual("{}", _loadedManager.Value["name"][3].ToString());
        Assert.AreEqual("{}", _loadedManager.Value["name"][4].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][5].ToString());
    }

    [TestMethod]
    public void CanAddNegativeIndexToExistingArrayThatRequiresLargeExpansion()
    {
        _loadedManager.Add("name[-123]", "John Doe", Priority.Normal);

        // index that should be affected
        // C# array doesn't allow negative index value (but we do), so -5 is converted to 0.
        Assert.AreEqual("John Doe", _loadedManager.Value["name"][0].ToString());

        // indexes that should not be affected
        Assert.AreEqual("Feng", _loadedManager.Value["name"][1].ToString());
        Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());

        // empty indexes added to fill the gap
        Assert.AreEqual("{}", _loadedManager.Value["name"][3].ToString());
        Assert.AreEqual("{}", _loadedManager.Value["name"][122].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][123].ToString());
    }

    [TestMethod]
    public void CanAddPropertyNestedInIndex()
    {
        _emptyManager.Add("name[0].first", "Shuzhao", Priority.Normal);

        // index that should be affected
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0]["first"].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1].ToString());
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingIndexToExistingPropertyObject()
    {
        Assert.ThrowsException<ArgumentException>(() => _propertyManager.Add("name[0]", "Shuzhao", Priority.Normal));
    }

    [TestMethod]
    public void ThrowsExceptionWhenIndexIsNotInteger()
    {
        Assert.ThrowsException<JsonException>(() => _emptyManager.Add("name[1.5]", "Shuzhao", Priority.Normal));
    }

    [TestMethod]
    public void ThrowsExceptionWhenBracketIsEmpty()
    {
        Assert.ThrowsException<JsonException>(() => _emptyManager.Add("name[]", "Shuzhao", Priority.Normal));
    }
}