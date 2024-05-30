using JsonPathSerializer.Globals;
using Newtonsoft.Json.Linq;

namespace JsonPathSerializer;

/// <summary>
///     Describes the entity that is the root manager for all JsonPathSerializer operations.
/// </summary>
public interface IJsonPathManager
{
    /// <summary>
    ///     Gets the current root JSON object state.
    /// </summary>
    public IJEnumerable<JToken> Value { get; }

    /// <summary>
    ///     Add a value to the JsonPathManager root with default priority.
    /// </summary>
    /// <param name="path">The path where to add the value.</param>
    /// <param name="value">The value to be added.</param>
    void Add(string path, object value);

    /// <summary>
    ///     Add a value to the JsonPathManager root.
    /// </summary>
    /// <param name="path">The path where to add the value.</param>
    /// <param name="value">The value to be added.</param>
    /// <param name="priority">The priority of the operation.</param>
    void Add(string path, object value, Priority priority);

    /// <summary>
    ///     Force add a value to the JsonPathManager root, regardless of whether existing values will be overriden.
    ///     <br/>
    ///     This method is obsolete. Use Add(string, object, Priority.High) instead.
    /// </summary>
    /// <param name="path">The path where to add the value.</param>
    /// <param name="value">The value to be added.</param>
    [Obsolete("This method is obsolete. Use Add(string, object, Priority.High) instead.")]
    void Force(string path, object value);

    /// <summary>
    ///     Append a value to the end of an array in the JsonPathManager root.
    /// </summary>
    /// <param name="path">The path of the array(s) where to append the value.</param>
    /// <param name="value">The value to be appended at the end of the array.</param>
    /// <param name="priority">The priority of the operation.</param>
    void Append(string path, object value, Priority priority);

    /// <summary>
    ///     Remove a value or child from the JsonPathManager root and return it.
    /// </summary>
    /// <param name="path">The path where to remove the value or child.</param>
    /// <returns>The removed value or child.</returns>
    JToken? Remove(string path);

    /// <summary>
    ///    Builds the root JSON object into a string.
    /// </summary>
    /// <returns>The string representation of the root JSON object.</returns>
    string Build();

    /// <summary>
    ///     Clears the root.
    /// </summary>
    void Clear();
}