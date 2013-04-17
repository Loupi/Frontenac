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

        readonly IEnumerable<string> _VertexPropertyKeys;
        readonly IEnumerable<string> _EdgePropertyKeys;
        readonly ElementPropertiesRule _VertexPropertiesRule;
        readonly ElementPropertiesRule _EdgePropertiesRule;

        /// <summary>
        /// A configuration that includes all properties of vertices and edges.
        /// </summary>
        public static readonly ElementPropertyConfig AllProperties = new ElementPropertyConfig(null, null,
            ElementPropertiesRule.INCLUDE, ElementPropertiesRule.INCLUDE);

        public ElementPropertyConfig(IEnumerable<string> vertexPropertyKeys, IEnumerable<string> edgePropertyKeys,
                                 ElementPropertiesRule vertexPropertiesRule, ElementPropertiesRule edgePropertiesRule)
        {
            _VertexPropertiesRule = vertexPropertiesRule;
            _VertexPropertyKeys = vertexPropertyKeys;
            _EdgePropertiesRule = edgePropertiesRule;
            _EdgePropertyKeys = edgePropertyKeys;
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

        public IEnumerable<string> GetVertexPropertyKeys()
        {
            return _VertexPropertyKeys;
        }

        public IEnumerable<string> GetEdgePropertyKeys()
        {
            return _EdgePropertyKeys;
        }

        public ElementPropertiesRule GetVertexPropertiesRule()
        {
            return _VertexPropertiesRule;
        }

        public ElementPropertiesRule GetEdgePropertiesRule()
        {
            return _EdgePropertiesRule;
        }
    }
}
