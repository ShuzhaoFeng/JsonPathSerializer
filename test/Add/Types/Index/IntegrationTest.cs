namespace JsonPathSerializerTest.Add.Types.Index;

[TestClass]
public class IntegrationTest
{
    private JsonPathManager _emptyManager = new();

    [TestInitialize]
    public void Setup()
    {
        _emptyManager = new JsonPathManager();
    }

    [TestMethod]
    public void CanAddIndexAndIndexSpan()
    {
        _emptyManager.Add("name[0,1:2]", "Shuzhao", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][2].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][3].ToString());
    }

    [TestMethod]
    public void CanAddOverlappingIndexAndIndexSpan()
    {
        _emptyManager.Add("name[2,1:3]", "Shuzhao", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][2].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][3].ToString());

        // empty indexes added to fill the gap
        Assert.AreEqual("{}", _emptyManager.Value["name"][0].ToString());


        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][4].ToString());
    }

    [TestMethod]
    public void CanAddIndexAndIndexSpanWithGap()
    {
        _emptyManager.Add("name[0,2:3]", "Shuzhao", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][2].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][3].ToString());

        // empty indexes added to fill the gap
        Assert.AreEqual("{}", _emptyManager.Value["name"][1].ToString());


        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][4].ToString());
    }

    [TestMethod]
    public void CanAddIndexesNestedInIndex()
    {
        _emptyManager.Add("name[0][0,1]", "Shuzhao", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][1].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1].ToString());
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][2].ToString());
    }

    [TestMethod]
    public void CanAddIndexNestedInIndexes()
    {
        _emptyManager.Add("name[0,1][0]", "Shuzhao", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1][0].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][1].ToString());
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1][1].ToString());
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][2].ToString());
    }

    [TestMethod]
    public void CanAddIndexesNestedInIndexes()
    {
        _emptyManager.Add("name[0,1][0,1]", "Shuzhao", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][1].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1][0].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1][1].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][2].ToString());
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1][2].ToString());
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][2].ToString());
    }

    [TestMethod]
    public void CanAddIndexSpanNestedInIndex()
    {
        _emptyManager.Add("name[0][0:2]", "Shuzhao", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][1].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][2].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1].ToString());
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][3].ToString());
    }

    [TestMethod]
    public void CanAddIndexNestedInIndexSpan()
    {
        _emptyManager.Add("name[0:2][0]", "Shuzhao", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0][0].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1][0].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][2][0].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][3].ToString());
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][1].ToString());
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][2].ToString());
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][3].ToString());
    }

    [TestMethod]
    public void CanAddIndexSpanNestedInIndexSpan()
    {
        _emptyManager.Add("name[0:2][0:2]", "Shuzhao", Priority.Normal);

        // indexes that should be affected
        // reduce 9 assertions to a for loop
        for (var i = 0; i < 3; i++)
        {
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][i][0].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][i][1].ToString());
            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][i][2].ToString());
        }

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][3].ToString());
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][0][3].ToString());
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][1][3].ToString());
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][2][3].ToString());
    }
}