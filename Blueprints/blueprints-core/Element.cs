using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints
{
    /// <summary>
    /// An Element is the base class for both vertices and edges.
    /// An element has an identifier that must be unique to its inheriting classes (vertex or edges).
    /// An element can maintain a collection of key/value properties.
    /// Keys are always Strings and values can be any object.
    /// Particular implementations can reduce the space of objects that can be used as values.
    /// Typically, objects are C# primitives (e.g. string, long, int, boolean, etc.)
    /// </summary>
    public interface Element
    {
        /// <summary>
        /// Return the object value associated with the provided string key.
        /// If no value exists for that key, return null.
        /// </summary>
        /// <param name="key">the key of the key/value property</param>
        /// <returns>the object value related to the string key</returns>
        object GetProperty(string key);

        /// <summary>
        /// Return all the keys associated with the element.
        /// </summary>
        /// <returns>the set of all string keys associated with the element</returns>
        IEnumerable<string> GetPropertyKeys();

        /// <summary>
        /// Assign a key/value property to the element.
        /// If a value already exists for this key, then the previous key/value is overwritten.
        /// </summary>
        /// <param name="key">the string key of the property</param>
        /// <param name="value">the object value o the property</param>
        void SetProperty(string key, object value);

        /// <summary>
        /// Un-assigns a key/value property from the element.
        /// The object value of the removed property is returned.
        /// </summary>
        /// <param name="key">the key of the property to remove from the element</param>
        /// <returns>the object value associated with that key prior to removal</returns>
        object RemoveProperty(string key);

        /// <summary>
        /// Remove the element from the graph.
        /// </summary>
        void Remove();

        /// <summary>
        /// An identifier that is unique to its inheriting class.
        /// All vertices of a graph must have unique identifiers.
        /// All edges of a graph must have unique identifiers.
        /// </summary>
        /// <returns>the identifier of the element</returns>
        object GetId();
    }
}
