# JsonPathSerializer
Class library that helps putting data into the right place in a Json string, using JsonPath

## Description

Built with .NET and newtonsoft.Json, **JsonPathSerializer** is a class library that aims to help automated tests and simulations to build large JSON objects using only values and their respective JSON path.

As of version 0.1.0.0-beta, the project is more of a concept demo rather than a complete, deliverable .NET class library. See **Constaints** about known limitations of JsonPathSerializer.

## Quick-demo

After building the project, reference the class library in your program and import it as `using JsonPathSerializer;`. You will also need the nuget **Newtonsoft.Json**.

Then, try the following code:
```CSharp
Dictionary<string, string> pathToValue = new Dictionary<string, string>()
{
    { "$.say.hello.world", "Hello world!" },
    { "$.say.hello.john", "Hello John!" },
    { "$.say.hi.jane", "Hi jane!" },
    { "$.say.hi.montreal[0]", "Salut Montréal!" },
    { "$.say.hi.montreal[1]", "Hi Montreal!" },
};

JsonPathManager manager = new JsonPathManager();
foreach (var pair in pathToValue)
{
    manager.Add(pair.Key, pair.Value);
}
Console.WriteLine(manager.Build());
```
Which gives: `{"say":{"hello":{"world":"Hello world!","john":"Hello John!"},"hi":{"jane":"Hi jane!","montreal":["Salut Montréal!","Hi Montreal!"]}}}`, or:
```
{
  "say": {
    "hello": {
      "world": "Hello world!",
      "john": "Hello John!"
    },
    "hi": {
      "jane": "Hi jane!",
      "montreal": [
        "Salut Montréal!",
        "Hi Montreal!"
      ]
    }
  }
}
```

## Feature Summary

**JsonPathSerializer** supports JSON path in both **dot** and **bracket** notation, and currently support the following JSON path operation:

* Property, e.g. `$.foo.key` or `['foo']['key']`
* Index, e.g. `$.foo[1]`, `$[-2]`
* Multiple indexes, e.g. `$.foo[1,2]`, `$[-2, 3, 1]`
* Index range, e.g. `$.foo[1:3]`, `$[-2:3]`, `$[1:-1]`

**JsonPathManager** instance can be initialized with no argument (will create a new empty root), or with a JObject or JArray as its root.

## Constaints

There are several important known issues and limitations for **JsonPathSerializer** for future development:

* JsonPathSerializer currently cannot handle an indexed JsonPath with a combination of indexes and index range.
* Recursive descent is not allowed because it doesn't make sense in the context of building a JSON object.
* Supports for wildcard will be implemented in the future.
* Supports for JSON path operators will be gradually implemented.
