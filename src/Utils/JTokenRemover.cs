using JsonPathSerializer.Structs;
using JsonPathSerializer.Structs.Path;
using JsonPathSerializer.Structs.Types.IndexSpan;
using Newtonsoft.Json.Linq;

namespace JsonPathSerializer.Utils
{
    class JTokenRemover
    {
        public static JToken? Remove(JArray parent, JsonPathToken token)
        {
            JToken? removed;

            switch (token.Type)
            {
                case JsonPathToken.TokenType.Index:
                    int index = (int) token.Value;
                    int positiveIndex = index >= 0 ? index : parent.Count + index;

                    removed = parent[positiveIndex];
                    parent.RemoveAt(positiveIndex);
                    break;

                case JsonPathToken.TokenType.Indexes:
                    JArray arrayToKeep = new JArray();
                    JArray arrayToRemove = new JArray();
                    List<int> indexes = (List<int>) token.Value;

                    for (int i = 0; i < parent.Count; i++)
                    {
                        // If the index is negative, it is relative to the end of the array.
                        if (indexes.Contains(i) || indexes.Contains(i - parent.Count))
                        {
                            arrayToRemove.Add(parent[i]);
                        }
                        else
                        {
                            arrayToKeep.Add(parent[i]);
                        }
                    }

                    parent.Replace(arrayToKeep);

                    removed = arrayToRemove;

                    break;

                case JsonPathToken.TokenType.IndexSpan:
                    IndexSpanValueContainer indexSpan = (IndexSpanValueContainer) token.Value;

                    arrayToKeep = new JArray();
                    arrayToRemove = new JArray();

                    int start = indexSpan.StartIndex;
                    int end = indexSpan.EndIndex ?? parent.Count - 1;

                    for (int i = 0; i < parent.Count; i++)
                    {
                        bool isInRange = (start >= 0, end >= 0) switch
                        {
                            (true, true) when start < end => // spans from start to end
                                i >= start && i <= end,
                            (true, true) => // spans from end to start
                                i >= end && i <= start,
                            (false, true) => // spans from 0 to end, then from start to Count - 1
                                i >= 0 && i <= end || i >= parent.Count + start && i < parent.Count,
                            (true, false) => // spans from 0 to start, then from end to Count - 1
                                i >= 0 && i <= start || i >= parent.Count + end && i < parent.Count,
                            (false, false) when start < end => // spans from start to end
                                i >= parent.Count + start && i <= parent.Count + end,
                            (false, false) => // spans from end to start
                                i >= parent.Count + end && i <= parent.Count + start
                        };

                        if (isInRange)
                        {
                            arrayToRemove.Add(parent[i]);
                        }
                        else
                        {
                            arrayToKeep.Add(parent[i]);
                        }
                    }

                    parent.Replace(arrayToKeep);

                    removed = arrayToRemove;

                    break;

                default:
                    return null;
            }

            return removed;
        }

        public static JToken? Remove(JArray parent, IJsonPathToken token)
        {
            JToken? removed;

            throw new NotImplementedException();
        }
    }
}
