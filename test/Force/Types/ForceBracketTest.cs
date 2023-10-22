﻿namespace JsonPathSerializerTest.Force.Types;

[TestClass]
public class ForceBracketTest
{
    private JsonPathManager _emptyManager = new();
    private JsonPathManager _indexedManager = new();
    private JsonPathManager _loadedManager = new();

    [TestInitialize]
    public void Initialize()
    {
        _emptyManager = new JsonPathManager();
        _loadedManager = new JsonPathManager(@"{
                ""name"": {
                    ""first"": ""Shuzhao"",
                },
            }");
        _indexedManager = new JsonPathManager(@"{
                ""name"": [
                    ""Shuzhao"",
                ],
            }");
    }

    [TestMethod]
    public void CanForceProperty()
    {
        _emptyManager.Force("['name']", "Shuzhao Feng");

        Assert.AreEqual("Shuzhao Feng", _emptyManager.Value["name"].ToString());
    }

    [TestMethod]
    public void CanForceNestedProperty()
    {
        _emptyManager.Force("name['first']", "Shuzhao");

        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"]["first"].ToString());
    }

    [TestMethod]
    public void CanForceNestedPropertyWithMultipleBrackets()
    {
        _emptyManager.Force("['name']['first']", "Shuzhao");

        Assert.AreEqual("Shuzhao", _emptyManager.Value["name"]["first"].ToString());
    }

    [TestMethod]
    public void CanForceInsertPropertyUnderExistingParentProperty()
    {
        _loadedManager.Force("name['last']", "Feng");

        Assert.AreEqual("Shuzhao", _loadedManager.Value["name"]["first"].ToString());
        Assert.AreEqual("Feng", _loadedManager.Value["name"]["last"].ToString());
    }

    [TestMethod]
    public void CanForceInsertValueToParentProperty()
    {
        _loadedManager.Force("['name']", "Shuzhao Feng");

        Assert.AreEqual("Shuzhao Feng", _loadedManager.Value["name"].ToString());

        Assert.ThrowsException<InvalidOperationException>(() => _loadedManager.Value["name"]["first"].ToString());
    }

    [TestMethod]
    public void CanForceInsertValueToArray()
    {
        _indexedManager.Force("['name']['last']", "Feng");

        Assert.AreEqual("Feng", _indexedManager.Value["name"]["last"].ToString());

        Assert.ThrowsException<ArgumentException>(() => _indexedManager.Value["name"][0].ToString());
    }

    [TestMethod]
    public void CanForceInsertValueAsChildUnderExistingValue()
    {
        _loadedManager.Force("['name']['first']['English']", "Shuzhao");

        Assert.AreEqual("Shuzhao", _loadedManager.Value["name"]["first"]["English"].ToString());
    }

    [TestMethod]
    public void ThrowsExceptionWhenForcingPropertyWithStringInBracketWithoutQuote()
    {
        Assert.ThrowsException<JsonException>(() => _emptyManager.Force("name[last]", "Feng"));
    }

    [TestMethod]
    public void ThrowsExceptionWhenForcingPropertyWithUnclosedBracket()
    {
        Assert.ThrowsException<JsonException>(() => _emptyManager.Force("name['last'", "Feng"));
    }

    [TestMethod]
    public void ThrowsExceptionWhenForceingPropertyWithUnopenedBracket()
    {
        Assert.ThrowsException<JsonException>(() => _emptyManager.Force("name'last']", "Feng"));
    }
}