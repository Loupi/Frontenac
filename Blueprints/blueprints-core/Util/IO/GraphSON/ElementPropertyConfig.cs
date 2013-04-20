using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    /// <summary>
    /// Configure how the GraphSON utility treats edge and vertex properties.
    /// </summary>
    public class ElementPropertyConfig
    {
        public enum ElementPropertiesRule
        {
            INCLUDE, EXCLUDE
        }

        readonly IEnumerable<string> _vertexPropertyKeys;
        readonly IEnumerable<string> _edgePropertyKeys;
        readonly ElementPropertiesRule _vertexPropertiesRule;
        readonly ElementPropertiesRule _edgePropertiesRule;

        /// <summary>
        /// A configuration that includes all properties of vertices and edges.
        /// </summary>
        public static readonly ElementPropertyConfig allProperties = new ElementPropertyConfig(null, null,
            ElementPropertiesRule.INCLUDE, ElementPropertiesRule.INCLUDE);

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
            return new ElementPropertyConfig(vertexPropertyKeys, edgePropertyKeys, ElementPropertiesRule.INCLUDE,
                    ElementPropertiesRule.INCLUDE);
        }

        /// <summary>
        /// Construct a configuration that excludes the specified properties from both vertices and edges.
        /// </summary>
        public static ElementPropertyConfig ExcludeProperties(IEnumerable<string> vertexPropertyKeys,
                                                          IEnumerable<string> edgePropertyKeys)
        {
            return new ElementPropertyConfig(vertexPropertyKeys, edgePropertyKeys, ElementPropertiesRule.EXCLUDE,
                    ElementPropertiesRule.EXCLUDE);
        }

        public IEnumerable<string> getVertexPropertyKeys()
        {
            return _vertexPropertyKeys;
        }

        public IEnumerable<string> getEdgePropertyKeys()
        {
            return _edgePropertyKeys;
        }

        public ElementPropertiesRule getVertexPropertiesRule()
        {
            return _vertexPropertiesRule;
        }

        public ElementPropertiesRule getEdgePropertiesRule()
        {
            return _edgePropertiesRule;
        }
    }
}
