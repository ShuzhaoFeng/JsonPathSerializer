using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathSerializer
{
    internal interface IJsonPathManager
    {
        public IJEnumerable<JToken> Value { get; }

        void Add(string path, object value);
        
        string Build();

        void Clear();
    }
}
