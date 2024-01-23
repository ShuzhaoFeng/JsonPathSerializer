using System.Text.RegularExpressions;

namespace JsonPathSerializer.Globals;

/// <summary>
///    Stores the regular expressions used in the library.
/// </summary>
internal class PathRegex
{
    /// <summary>
    ///     Extracts a property token from a JsonPath.
    /// </summary>
    internal static readonly Regex Property = new(@"\.([^\.\[]+)|\[\'([^']+)\'\]");

    /// <summary>
    ///     Extracts an index token from a JsonPath.
    /// </summary>
    internal static readonly Regex Index = new(@"(-?\d*\s{0,1}:\s{0,1}-?\d*)|(-?\d+)");

    /// <summary>
    ///     Extracts a single index from an index token.
    /// </summary>
    internal static readonly Regex SingleIndex = new(@"\[[-,:\s\d]+\]");

    /// <summary>
    ///     Extracts an index span from an index token.
    /// </summary>
    internal static readonly Regex IndexSpan = new(@"(-?\d*)\s{0,1}:\s{0,1}(-?\d*)");
}
