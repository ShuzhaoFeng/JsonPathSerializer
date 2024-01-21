using System.Text.RegularExpressions;

namespace JsonPathSerializer.Globals;

internal class PathRegex
{
    internal static readonly Regex Property = new(@"\.([^\.\[]+)|\[\'([^']+)\'\]");
    internal static readonly Regex Index = new(@"\[[-,:\s\d]+\]");
    internal static readonly Regex IndexSpan = new(@"(-?\d*)\s{0,1}:\s{0,1}(-?\d*)");
    internal static readonly Regex IndexToken = new(@"(-?\d*\s{0,1}:\s{0,1}-?\d*)|(-?\d+)");
}
