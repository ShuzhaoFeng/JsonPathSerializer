# JsonPathSerializer
Class library that helps putting data incrementally to build a JSON string.

[![NuGet](https://img.shields.io/nuget/v/JsonPathSerializer.svg)](https://www.nuget.org/packages/JsonPathSerializer/)
[![NuGet](https://img.shields.io/nuget/dt/JsonPathSerializer.svg)](https://www.nuget.org/packages/JsonPathSerializer/)

## Summary

**JsonPathSerializer** is a class library that help C# code to build JSON objects incrementally, each time with a JsonPath and a value.

If you're familiar with JSON, you may know that you can use a JsonPath to query a JSON object:

```json
{
  "name": [
	{
	  "first": "John",
	  "last": "Doe"
	},
	{
	  "first": "Jane",
	  "last": "Doe"
	}
  ]
}
```

If you want to access the value `Jane` in the above JSON object, you can use the JsonPath `$.name[1].first`. The JsonPath is a string that describes the path to the value you want to access.

But what if you're given raw data incrementally, and you want to build a structured JSON object from it?

You can use **JsonPathSerializer** to do that.

This class library can help:

* Automated tests to generate mock data.
* IoT devices to dynamically send back digested data on an interval.
* Web applications to build structured JSON objects from user input.
* And many more!

## Requirements

* [.NET](https://github.com/dotnet/sdk) 6.0 or higher.

* [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) 13.0.1 or higher.

## Setup

### Install from NuGet

```bash
dotnet add package JsonPathSerializer
```

### Build from source

```bash
git clone
dotnet build
```

## Quick-demo

After downloading the class library, add the following `using` statement to your code:
```CSharp
using JsonPathSerializer;
```

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
Which gives:
```
{"say":{"hello":{"world":"Hello world!","john":"Hello John!"},"hi":{"jane":"Hi jane!","montreal":["Salut Montréal!","Hi Montreal!"]}}}
```

Or:
```json
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

**JsonPathSerializer** supports a combination of the following JsonPath tokens:

* Property, e.g. `foo.key` or `['foo']['key']`
* Index, e.g. `foo[1]`, `[-2]`
* Multiple indexes, e.g. `[1,2]`, `[-2, 3, 1]`
* Index span, e.g. `[1:3]`, `[-1:]`, `[:2]`

For more use case information, please consult the [wiki](https://github.com/ShuzhaoFeng/JsonPathSerializer/wiki).

## Constaints

There are several important known issues and limitations for **JsonPathSerializer**:

* The library is missing a validation method on type conflict in case the path contains a negative value.
* Supports for wildcard will be implemented in the future.
* Supports for JSON path operators will be gradually implemented.

## Release Notes

### 0.2.0

See [v0.2.0 Changelog](https://github.com/ShuzhaoFeng/JsonPathSerializer/wiki/v0.2.0-Changelog).

### 0.1.2

See [v0.1.2 Changelog](https://github.com/ShuzhaoFeng/JsonPathSerializer/wiki/v0.1.2-Changelog).

### 0.1.0 (Unstable)

* Initial release.

## Future Plans for v0.3.0

* Implement a priority mechanism to handle conflicts when adding a value, effectively replacing the `Force()` method.
There will be 3 levels of priority: `Low`, `Medium`, and `High`, with `Medium` being the default equivalent to the current `Add()`,
`High` being equivalent to the current `Force()`, and `Low` to be implemented.

* Implement an `Append()` method that allows user to append a value at the end of the array specified by the path.

* Implement a validation method on type conflict in case the path contains a negative value.
