using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    /// <summary>
    /// Configure how the GraphSON utility treats edge and vertex properties.
    /// </summary>
    public class ElementPropertyConfig
    {
        public enum ElementPropertiesRule
        {
            Include, Exclude
        }

        readonly IEnumerable<string> _vertexPropertyKeys;
        readonly IEnumerable<string> _edgePropertyKeys;
        readonly ElementPropertiesRule _vertexPropertiesRule;
        readonly ElementPropertiesRule _edgePropertiesRule;

        /// <summary>
        /// A configuration that includes all properties of vertices and edges.
        /// </summary>
        public static readonly ElementPropertyConfig AllProperties = new ElementPropertyConfig(null, null,
            ElementPropertiesRule.Include, ElementPropertiesRule.Include);

        public ElementPropertyConfig(IEnumerable<string> vertexPropertyKeys, IEnumerable<string> edgePropertyKeys,
                                 ElementPropertiesRule vertexPropertiesRule, ElementPropertiesRule edgePropertiesRule)
        {
            _vertexPropertiesRule = vertexPropertiesRule;
            _vertexPropertyKeys = vertexPropertyKeys;
            _edgePropertiesRule = edgePropertiesRule;
            _edgePropertyKeys = edgePropertyKeys;
        }

        /// <summary>
        /// Construct a configuration that includes the specified properties from both vertices and edges.
        /// </summary>
        public static ElementPropertyConfig IncludeProperties(IEnumerable<string> vertexPropertyKeys,
                                                          IEnumerable<string> edgePropertyKeys)
        {
            return new ElementPropertyConfig(vertexPropertyKeys, edgePropertyKeys, ElementPropertiesRule.Include,
                    ElementPropertiesRule.Include);
        }

        /// <summary>
        /// Construct a configuration that excludes the specified properties from both vertices and edges.
        /// </summary>
        public static ElementPropertyConfig ExcludeProperties(IEnumerable<string> vertexPropertyKeys,
                                                          IEnumerable<string> edgePropertyKeys)
        {
            return new ElementPropertyConfig(vertexPropertyKeys, edgePropertyKeys, ElementPropertiesRule.Exclude,
                    ElementPropertiesRule.Exclude);
        }

        public IEnumerable<string> GetVertexPropertyKeys()
        {
            return _vertexPropertyKeys;
        }

        public IEnumerable<string> GetEdgePropertyKeys()
        {
            return _edgePropertyKeys;
        }

        public ElementPropertiesRule GetVertexPropertiesRule()
        {
            return _vertexPropertiesRule;
        }

        public ElementPropertiesRule GetEdgePropertiesRule()
        {
            return _edgePropertiesRule;
        }
    }
}
