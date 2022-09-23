using JsonPathSerializer;

Dictionary<string, string> pathToValue = new Dictionary<string, string>()
{
    { "$.['say'].['hello'].['world']", "Hello world!" },
    { "$.say.hello.john", "Hello John!" },
    { "$.say.hi.jane", "Hi jane!" },
    { "$.say.hi.montreal[0].French", "Salut Montréal!" },
    { "$.say.hi.montreal[2].English", "Hi Montreal!" },
    { "$.say.hi.montreal[1].Spanish", "Hola Montreal!" }
};

JsonPathManager manager = new JsonPathManager();
foreach (var pair in pathToValue)
{
    manager.Add(pair.Value, pair.Key);
}
Console.WriteLine(manager.Build());
