using JsonPathSerializer;

Dictionary<string, string> pathToValue = new Dictionary<string, string>()
{
    { "$.['say']['hello']['world']", "Hello world!" }, // bracket notation
    { "$.say.hello.john.inFrench", "Salut Jean!" }, // dot notation, deeper node
    { "$.say.hi.jane", "Hi jane!" }, // dot notation
    { "$.say.hi.montreal[0].French", "Salut Montréal!" }, // index
    { "$.say.hi.montreal[2].Spanish", "Hola Montréal!" }, // index gap
    { "$.say.hi.montreal[-2].English", "Hi Montreal!" }, // negative index
    { "$.note", "test values" },
};

JsonPathManager manager = new JsonPathManager();
foreach (var pair in pathToValue)
{
    manager.Add(pair.Value, pair.Key);
}
manager.Add("Hello World!", "$.say.hello.world"); // overwrite, dot notation
Console.WriteLine(manager.Build());
