using JsonPathSerializer.Structs;
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
                    removed = parent[(int) token.Value];
                    parent.RemoveAt((int) token.Value);
                    break;

                case JsonPathToken.TokenType.Indexes:
                    JArray arrayToKeep = new JArray();
                    JArray arrayToRemove = new JArray();
                    List<int> indexes = (List<int>) token.Value;

                    for (int i = 0; i < indexes.Count; i++)
                    {
                        // If the index is negative, it is relative to the end of the array.
                        if (indexes.Contains(i) || indexes.Contains(i - indexes.Count))
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
                        bool isInRange;

                        switch (start >= 0, end >= 0)
                        {
                            case (true, true) when start < end: // spans from start to end
                                isInRange = i >= start && i <= end;
                                break;

                            case (true, true): // spans from end to start
                                isInRange = i >= end && i <= start;
                                break;

                            case (false, true): // spans from 0 to end, then from start to Count - 1
                                isInRange = i >= 0 && i <= end || i >= parent.Count + start && i < parent.Count;
                                break;

                            case (true, false): // spans from 0 to start, then from end to Count - 1
                                isInRange = i >= 0 && i <= start || i >= parent.Count + end && i < parent.Count;
                                break;

                            case (false, false) when start < end: // spans from start to end
                                isInRange = i >= parent.Count + start && i <= parent.Count + end;
                                break;

                            case (false, false): // spans from end to start
                                isInRange = i >= parent.Count + end && i <= parent.Count + start;
                                break;
                        }
                        


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
    }
}
