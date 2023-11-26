﻿namespace JsonPathSerializerTest.Add.Types;

[TestClass]
public class IndexesTest
{
    private JsonPathManager _emptyManager = new();
    private JsonPathManager _loadedManager = new();
    private JsonPathManager _loadedPropertyManager = new();
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
        _loadedPropertyManager = new JsonPathManager(@"{
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
    public void CanAddIndexes()
    {
        _emptyManager.Add("name[0,1]", "Shuzhao", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][0].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][2].ToString());
    }

    [TestMethod]
    public void CanAddIndexesAsRoot()
    {
        _emptyManager.Add("[0,1]", "Shuzhao Feng", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[0].ToString());
        Assert.AreEqual("Shuzhao Feng", _emptyManager.Value[1].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value[2].ToString());
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
    public void CanAddIndexesThatRequiresExpansion()
    {
        _emptyManager.Add("name[1,5]", "John Doe", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("John Doe", _emptyManager.Value["name"][1].ToString());
        Assert.AreEqual("John Doe", _emptyManager.Value["name"][5].ToString());

        // empty indexes added to fill the gap
        Assert.AreEqual("{}", _emptyManager.Value["name"][0].ToString());
        Assert.AreEqual("{}", _emptyManager.Value["name"][2].ToString());
        Assert.AreEqual("{}", _emptyManager.Value["name"][3].ToString());
        Assert.AreEqual("{}", _emptyManager.Value["name"][4].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][6].ToString());
    }

    [TestMethod]
    public void CanAddIndexToExistingArray()
    {
        _loadedManager.Add("name[0,1]", "John Doe", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("John Doe", _loadedManager.Value["name"][0].ToString());
        Assert.AreEqual("John Doe", _loadedManager.Value["name"][1].ToString());

        // indexes that should not be affected
        Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][3].ToString());
    }

    [TestMethod]
    public void CanAddIndexesToExistingArrayThatRequiresExpansion()
    {
        _loadedManager.Add("name[1,5]", "John Doe", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("John Doe", _loadedManager.Value["name"][1].ToString());
        Assert.AreEqual("John Doe", _loadedManager.Value["name"][5].ToString());

        // indexes that should not be affected
        Assert.AreEqual("Shuzhao", _loadedManager.Value["name"][0].ToString());
        Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"][2].ToString());


        // empty indexes added to fill the gap
        Assert.AreEqual("{}", _loadedManager.Value["name"][3].ToString());
        Assert.AreEqual("{}", _loadedManager.Value["name"][4].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedManager.Value["name"][6].ToString());
    }

    [TestMethod]
    public void CanAddPropertyNestedInIndexes()
    {
        _loadedPropertyManager.Add("name[1, 2, 3].middle", "A.", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("A.", _loadedPropertyManager.Value["name"][1]["middle"].ToString());
        Assert.AreEqual("A.", _loadedPropertyManager.Value["name"][2]["middle"].ToString());
        Assert.AreEqual("A.", _loadedPropertyManager.Value["name"][3]["middle"].ToString());

        // indexes that should not be affected
        Assert.AreEqual("Shuzhao", _loadedPropertyManager.Value["name"][0]["first"].ToString());
        Assert.AreEqual("Feng", _loadedPropertyManager.Value["name"][0]["last"].ToString());
        Assert.ThrowsException<NullReferenceException>(() =>
            _loadedPropertyManager.Value["name"][0]["middle"].ToString());

        // other values in the same index should not be affected
        Assert.AreEqual("John", _loadedPropertyManager.Value["name"][2]["first"].ToString());
        Assert.AreEqual("Doe", _loadedPropertyManager.Value["name"][2]["last"].ToString());
        Assert.AreEqual("Jane", _loadedPropertyManager.Value["name"][3]["first"].ToString());
        Assert.AreEqual("Doa", _loadedPropertyManager.Value["name"][3]["last"].ToString());

        // no extra properties are added
        Assert.ThrowsException<NullReferenceException>(
            () => _loadedPropertyManager.Value["name"][1]["first"].ToString());
        Assert.ThrowsException<NullReferenceException>(() =>
            _loadedPropertyManager.Value["name"][1]["last"].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedPropertyManager.Value["name"][4].ToString());
    }

    [TestMethod]
    public void CanAddPropertyNestedInIndexesUnderExistingKey()
    {
        _loadedPropertyManager.Add("name[2, 3].last", "Smith", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Smith", _loadedPropertyManager.Value["name"][2]["last"].ToString());
        Assert.AreEqual("Smith", _loadedPropertyManager.Value["name"][3]["last"].ToString());

        // indexes that should not be affected
        Assert.AreEqual("Shuzhao", _loadedPropertyManager.Value["name"][0]["first"].ToString());
        Assert.AreEqual("Feng", _loadedPropertyManager.Value["name"][0]["last"].ToString());

        // other values in the same index should not be affected
        Assert.AreEqual("John", _loadedPropertyManager.Value["name"][2]["first"].ToString());
        Assert.AreEqual("Jane", _loadedPropertyManager.Value["name"][3]["first"].ToString());

        // no extra properties are added
        Assert.ThrowsException<NullReferenceException>(
            () => _loadedPropertyManager.Value["name"][1]["first"].ToString());
        Assert.ThrowsException<NullReferenceException>(() =>
            _loadedPropertyManager.Value["name"][1]["last"].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedPropertyManager.Value["name"][4].ToString());
    }

    [TestMethod]
    public void CanAddPropertyNestedInIndexesUnderNewAndExistingKey()
    {
        _loadedPropertyManager.Add("name[1, 2, 3].last", "Smith", Priority.Normal);

        // indexes that should be affected
        Assert.AreEqual("Smith", _loadedPropertyManager.Value["name"][1]["last"].ToString());
        Assert.AreEqual("Smith", _loadedPropertyManager.Value["name"][2]["last"].ToString());
        Assert.AreEqual("Smith", _loadedPropertyManager.Value["name"][3]["last"].ToString());

        // indexes that should not be affected
        Assert.AreEqual("Shuzhao", _loadedPropertyManager.Value["name"][0]["first"].ToString());
        Assert.AreEqual("Feng", _loadedPropertyManager.Value["name"][0]["last"].ToString());

        // other values in the same index should not be affected
        Assert.AreEqual("John", _loadedPropertyManager.Value["name"][2]["first"].ToString());
        Assert.AreEqual("Jane", _loadedPropertyManager.Value["name"][3]["first"].ToString());

        // no extra properties are added
        Assert.ThrowsException<NullReferenceException>(
            () => _loadedPropertyManager.Value["name"][1]["first"].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _loadedPropertyManager.Value["name"][4].ToString());
    }

    [TestMethod]
    public void CanAddNegativeIndexes()
    {
        _emptyManager.Add("name[-2, 2]", "Shuzhao", Priority.Normal);

        // empty indexes added to fill the gap
        Assert.AreEqual("{}", _emptyManager.Value["name"][0].ToString());

        // indexes that should be affected
        // C# array doesn't allow negative index value (but we do), so -2 is converted to 1.
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][1].ToString());
        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"][2].ToString());

        // no extra indexes are added
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _emptyManager.Value["name"][3].ToString());
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingIndexesWithInvalidSeparator()
    {
        Assert.ThrowsException<JsonException>(() => _emptyManager.Add("name[0 1]", "Shuzhao", Priority.Normal));
    }

    [TestMethod]
    public void ThrowsExceptionWhenAddingIndexesToExistingPropertyObject()
    {
        Assert.ThrowsException<ArgumentException>(() => _propertyManager.Add("name[0, 1]", "Shuzhao", Priority.Normal));
    }

    [TestMethod]
    public void ThrowsExceptionWhenIndexesAreNotInteger()
    {
        Assert.ThrowsException<JsonException>(() => _emptyManager.Add("name[1.5, 7/4]", "Shuzhao", Priority.Normal));
    }
}