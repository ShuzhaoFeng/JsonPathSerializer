using JsonPathSerializer.Structs.Path;
using Newtonsoft.Json.Linq;

namespace JsonPathSerializer.Utils;

internal class JTokenRemover
{
    public static JToken? Remove(JArray parent, JsonPathIndexToken token)
    {
        JArray arrayToKeep = new JArray();
        JArray arrayToRemove = new JArray();

        for (int i = 0; i < parent.Count; i++)
            if (JsonPathValidator.ArrayContainsIndex(token, i, parent.Count))
                arrayToRemove.Add(parent[i]);
            else
                arrayToKeep.Add(parent[i]);

        parent.Replace(arrayToKeep);

        if (arrayToRemove.Count > 0) return arrayToRemove;

        return null;
    }
}