using JsonPathSerializer;

Dictionary<string, string> pathToValue = new Dictionary<string, string>()
{
    { "$.['say']['hello']['world']", "Hello world!" }, // bracket notation
    { "$.say.hello.john.inFrench", "Salut Jean!" }, // dot notation, deeper node
    { "$.say.hi.jane", "Hi jane!" }, // dot notation
    { "$.say.hi.montreal[0].French", "Salut Montréal!" }, // index
    { "$.say.hi.montreal[2].Spanish", "Hola Montréal!" }, // index gap
    { "$.say.hi.montreal[-2].English", "Hi Montreal!" }, // negative index
    { "$.say.hi.four.times[0]", "Hey!" },
    { "$.say.hi.four.times[1,2,3]", "Hi!" }, // index list
    { "$.say.hi.nine.times[0,1,2][0].node", "Hi!" }, // index list
    { "$.say.hi.nine.times[0,1,2][1,2].tree", "Hey!" }, // index list
    { "$.say.hi.nine.times[0,1,2][0,2].node", "Hello!" }, // index list
    { "$.note", "test values" },
};

JsonPathManager manager = new JsonPathManager();
foreach (var pair in pathToValue)
{
    manager.Add(pair.Key, pair.Value);
}
manager.Add("$.say.hello.world", "Hello World!"); // overwrite, dot notation
Console.WriteLine(manager.Build());
