using System.Text.RegularExpressions;

namespace JsonPathSerializer.Globals;

internal class PathRegex
{
    internal static readonly Regex PROPERTY = new(@"\.([^\.\[]+)|\[\'([^']+)\'\]");
    internal static readonly Regex INDEX = new(@"\[[-,:\s\d]+\]");
    internal static readonly Regex INDEX_TOKEN = new(@"(-?\d*\s{0,1}:\s{0,1}-?\d*)|(-?\d+)");
    internal static readonly Regex INDEX_SPAN = new(@"(-?\d*)\s{0,1}:\s{0,1}(-?\d*)");
}
