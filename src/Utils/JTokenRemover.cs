using JsonPathSerializer.Structs.Path;
using Newtonsoft.Json.Linq;

namespace JsonPathSerializer.Utils;

/// <summary>
///     Collection of methods to remove JToken from a Json object
/// </summary>
internal class JTokenRemover
{
    /// <summary>
    ///     Removes children from a JArray, based on the given token.
    /// </summary>
    /// <param name="parent">The parent JArray.</param>
    /// <param name="token">Specifies the children to remove.</param>
    /// <returns></returns>
    public static JArray? Remove(JArray parent, JsonPathIndexToken token)
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