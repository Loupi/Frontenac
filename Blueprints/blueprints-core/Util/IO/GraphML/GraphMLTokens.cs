using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO.GraphML
{
    /// <summary>
    /// A collection of tokens used for GraphML related data.
    /// </summary>
    public static class GraphMLTokens
    {
        public const string XML_SCHEMA_NAMESPACE_TAG = "xsi";
        public const string DEFAULT_GRAPHML_SCHEMA_LOCATION = "http://graphml.graphdrawing.org/xmlns/1.1/graphml.xsd";
        public const string XML_SCHEMA_LOCATION_ATTRIBUTE = "schemaLocation";
        public const string GRAPHML = "graphml";
        public const string XMLNS = "xmlns";
        public const string GRAPHML_XMLNS = "http://graphml.graphdrawing.org/xmlns";
        public const string G = "G";
        public const string EDGEDEFAULT = "edgedefault";
        public const string DIRECTED = "directed";
        public const string KEY = "key";
        public const string FOR = "for";
        public const string ID = "id";
        public const string ATTR_NAME = "attr.name";
        public const string ATTR_TYPE = "attr.type";
        public const string GRAPH = "graph";
        public const string NODE = "node";
        public const string EDGE = "edge";
        public const string SOURCE = "source";
        public const string TARGET = "target";
        public const string DATA = "data";
        public const string LABEL = "label";
        public const string STRING = "string";
        public const string FLOAT = "float";
        public const string DOUBLE = "double";
        public const string LONG = "long";
        public const string BOOLEAN = "boolean";
        public const string INT = "int";
        public const string _DEFAULT = "_default";
    }
}
