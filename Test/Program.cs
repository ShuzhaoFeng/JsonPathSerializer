using JsonPathSerializer;

Dictionary<string, string> pathToValue = new Dictionary<string, string>()
{
    { "$.['say']['hello']['world']", "Hello world!" },
    { "$.say.hello.john.inFrench", "Salut Jean!" },
    { "$.say.hi.jane", "Hi jane!" },
    { "$.say.hi.montreal[0].French", "Salut Montréal!" },
    { "$.say.hi.montreal[1].English", "Hi Montreal!" },
    { "$.say.hi.montreal[2].Spanish", "Hola Montréal!" },
    { "$.note", "test values" },
};

JsonPathManager manager = new JsonPathManager();
foreach (var pair in pathToValue)
{
    manager.Add(pair.Value, pair.Key);
}
Console.WriteLine(manager.Build());
