﻿using Newtonsoft.Json.Linq;

namespace JsonPathSerializer
{
    internal interface IJsonPathManager
    {
        public IJEnumerable<JToken> Value { get; }

        void Add(string path, object value);

        void Force(string path, object value);

        void Remove(string path);

        string Build();

        void Clear();
    }
}
