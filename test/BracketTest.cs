﻿using JsonPathSerializer;
using Newtonsoft.Json;

namespace JsonPathSerializerTest
{
    [TestClass]
    public class BracketTest
    {
        private JsonPathManager _emptyManager = new();
        private JsonPathManager _loadedManager = new();
        private JsonPathManager _indexedManager = new();

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
        public void CanAddKey()
        {
            _emptyManager.Add("['name']", "Shuzhao Feng");

            Assert.AreEqual("Shuzhao Feng", _emptyManager.Value["name"].ToString());
        }

        [TestMethod]
        public void CanAddNestedKey()
        {
            _emptyManager.Add("name['first']", "Shuzhao");

            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"]["first"].ToString());
        }

        [TestMethod]
        public void CanAddNestedKeyWithMultipleBrackets()
        {
            _emptyManager.Add("['name']['first']", "Shuzhao");

            Assert.AreEqual("Shuzhao", _emptyManager.Value["name"]["first"].ToString());
        }

        [TestMethod]
        public void CanInsertKeyUnderExistingParentKey()
        {
            _loadedManager.Add("name['last']", "Feng");

            Assert.AreEqual("Shuzhao", _loadedManager.Value["name"]["first"].ToString());
            Assert.AreEqual("Feng", _loadedManager.Value["name"]["last"].ToString());
        }

        [TestMethod]
        public void ThrowsExceptionWhenAddingKeyWithStringInBracketWithoutQuote()
        {
            Assert.ThrowsException<JsonException>(() => _emptyManager.Add("name[last]", "Feng"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenAddingKeyWithUnclosedBracket()
        {
            Assert.ThrowsException<JsonException>(() => _emptyManager.Add("name['last'", "Feng"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenAddingKeyWithUnopenedBracket()
        {
            Assert.ThrowsException<JsonException>(() => _emptyManager.Add("name'last']", "Feng"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenInsertingValueToArray()
        {
            Assert.ThrowsException<ArgumentException>(() => _indexedManager.Add("name['last']", "Feng"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenInsertingValueToParentKey()
        {
            Assert.ThrowsException<ArgumentException>(() => _loadedManager.Add("['name']", "Shuzhao Feng"));
        }

        [TestMethod]
        public void ThrowsExceptionWhenInsertingValueAsChildUnderExistingValue()
        {
            Assert.ThrowsException<ArgumentException>(() => _loadedManager.Add("name.first['English']", "Shuzhao"));
        }
    }
}

