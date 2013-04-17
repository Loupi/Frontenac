using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Frontenac.Blueprints.Util.IO.GraphML
{
    /// <summary>
    /// GraphMLWriter writes a Graph to a GraphML OutputStream.
    /// </summary>
    public class GraphMLWriter
    {
        readonly Graph _Graph;
        bool _Normalize = false;
        Dictionary<string, string> _VertexKeyTypes = null;
        Dictionary<string, string> _EdgeKeyTypes = null;

        string _XmlSchemaLocation = null;
        string _EdgeLabelKey = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        public GraphMLWriter(Graph graph)
        {
            _Graph = graph;
        }

        /// <summary>
        /// the location of the GraphML XML Schema instance
        /// </summary>
        /// <param name="xmlSchemaLocation"></param>
        public void SetXmlSchemaLocation(string xmlSchemaLocation)
        {
            _XmlSchemaLocation = xmlSchemaLocation;
        }

        /// <summary>
        /// Set the name of the edge label in the GraphML. When this value is not set the value of the Edge.getLabel()
        /// is written as a "label" attribute on the edge element.  This does not validate against the GraphML schema.
        /// If this value is set then the the value of Edge.getLabel() is written as a data element on the edge and
        /// the appropriate key element is added to define it in the GraphML
        /// </summary>
        /// <param name="edgeLabelKey">if the label of an edge will be handled by the data property.</param>
        public void SetEdgeLabelKey(string edgeLabelKey)
        {
            _EdgeLabelKey = edgeLabelKey;
        }

        /// <summary>
        /// whether to normalize the output. Normalized output is deterministic with respect to the order of
        /// elements and properties in the resulting XML document, and is compatible with line diff-based tools
        /// such as Git. Note: normalized output is memory-intensive and is not appropriate for very large graphs.
        /// </summary>
        /// <param name="normalize"></param>
        public void SetNormalize(bool normalize)
        {
            _Normalize = normalize;
        }

        /// <summary>
        /// a IDictionary<string, string> of the data types of the vertex keys
        /// </summary>
        /// <param name="vertexKeyTypes"></param>
        public void SetVertexKeyTypes(Dictionary<string, string> vertexKeyTypes)
        {
            _VertexKeyTypes = vertexKeyTypes;
        }

        /// <summary>
        /// a IDictionary<string, string> of the data types of the edge keys
        /// </summary>
        /// <param name="edgeKeyTypes"></param>
        public void SetEdgeKeyTypes(Dictionary<string, string> edgeKeyTypes)
        {
            _EdgeKeyTypes = edgeKeyTypes;
        }

        /// <summary>
        /// Write the data in a Graph to a GraphML file.
        /// </summary>
        /// <param name="filename">the name of the file write the Graph data (as GraphML) to</param>
        public void OutputGraph(string filename)
        {
            using (var fos = File.Open(filename, FileMode.Create))
            {
                OutputGraph(fos);
            }
        }

        const string XMLNS_ATTRIBUTE = "xmlns";
        const string W3C_XML_SCHEMA_INSTANCE_NS_URI = "http://www.w3.org/2001/XMLSchema-instance";


        /// <summary>
        /// Write the data in a Graph to a GraphML OutputStream.
        /// </summary>
        /// <param name="graphMLOutputStream">the GraphML OutputStream to write the Graph data to</param>
        public void OutputGraph(Stream graphMLOutputStream)
        {
            if (null == _VertexKeyTypes || null == _EdgeKeyTypes)
            {
                Dictionary<string, string> vertexKeyTypes = new Dictionary<string, string>();
                Dictionary<string, string> edgeKeyTypes = new Dictionary<string, string>();

                foreach (Vertex vertex in _Graph.GetVertices())
                {
                    foreach (string key in vertex.GetPropertyKeys())
                    {
                        if (!vertexKeyTypes.ContainsKey(key))
                            vertexKeyTypes[key] = GraphMLWriter.GetStringType(vertex.GetProperty(key));
                    }
                    foreach (Edge edge in vertex.GetEdges(Direction.OUT))
                    {
                        foreach (string key in edge.GetPropertyKeys())
                        {
                            if (!edgeKeyTypes.ContainsKey(key))
                                edgeKeyTypes[key] = GraphMLWriter.GetStringType(edge.GetProperty(key));
                        }
                    }
                }

                if (null == _VertexKeyTypes)
                    _VertexKeyTypes = vertexKeyTypes;

                if (null == _EdgeKeyTypes)
                    _EdgeKeyTypes = edgeKeyTypes;
            }

            // adding the edge label key will push the label into the data portion of the graphml otherwise it
            // will live with the edge data itself (which won't validate against the graphml schema)
            if (null != _EdgeLabelKey && null != _EdgeKeyTypes && null == _EdgeKeyTypes.Get(_EdgeLabelKey))
                _EdgeKeyTypes[_EdgeLabelKey] = GraphMLTokens.STRING;

            using (XmlTextWriter writer = new XmlTextWriter(graphMLOutputStream, Encoding.UTF8))
            {
                if (_Normalize)
                {
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 4;
                }

                writer.WriteStartDocument();
                writer.WriteStartElement(GraphMLTokens.GRAPHML);
                writer.WriteAttributeString(GraphMLTokens.XMLNS, GraphMLTokens.GRAPHML_XMLNS);

                //XML Schema instance namespace definition (xsi)
                writer.WriteAttributeString(string.Concat(XMLNS_ATTRIBUTE, ":", GraphMLTokens.XML_SCHEMA_NAMESPACE_TAG), W3C_XML_SCHEMA_INSTANCE_NS_URI);

                //XML Schema location
                writer.WriteAttributeString(string.Concat(GraphMLTokens.XML_SCHEMA_NAMESPACE_TAG, ":", GraphMLTokens.XML_SCHEMA_LOCATION_ATTRIBUTE),
                        string.Concat(GraphMLTokens.GRAPHML_XMLNS, " ", (_XmlSchemaLocation == null ?
                                GraphMLTokens.DEFAULT_GRAPHML_SCHEMA_LOCATION : _XmlSchemaLocation)));

                // <key id="weight" for="edge" attr.name="weight" attr.type="float"/>
                IEnumerable<string> keyset;

                if (_Normalize)
                {
                    List<string> sortedKeyset = new List<string>(_VertexKeyTypes.Keys);
                    sortedKeyset.Sort();
                    keyset = sortedKeyset;
                }
                else
                    keyset = _VertexKeyTypes.Keys.ToList();

                foreach (string key in keyset)
                {
                    writer.WriteStartElement(GraphMLTokens.KEY);
                    writer.WriteAttributeString(GraphMLTokens.ID, key);
                    writer.WriteAttributeString(GraphMLTokens.FOR, GraphMLTokens.NODE);
                    writer.WriteAttributeString(GraphMLTokens.ATTR_NAME, key);
                    writer.WriteAttributeString(GraphMLTokens.ATTR_TYPE, _VertexKeyTypes.Get(key));
                    writer.WriteEndElement();
                }

                if (_Normalize)
                {
                    List<string> sortedKeyset = new List<string>(_EdgeKeyTypes.Keys);
                    sortedKeyset.Sort();
                    keyset = sortedKeyset;
                }
                else
                    keyset = _EdgeKeyTypes.Keys;

                foreach (string key in keyset)
                {
                    writer.WriteStartElement(GraphMLTokens.KEY);
                    writer.WriteAttributeString(GraphMLTokens.ID, key);
                    writer.WriteAttributeString(GraphMLTokens.FOR, GraphMLTokens.EDGE);
                    writer.WriteAttributeString(GraphMLTokens.ATTR_NAME, key);
                    writer.WriteAttributeString(GraphMLTokens.ATTR_TYPE, _EdgeKeyTypes.Get(key));
                    writer.WriteEndElement();
                }

                writer.WriteStartElement(GraphMLTokens.GRAPH);
                writer.WriteAttributeString(GraphMLTokens.ID, GraphMLTokens.G);
                writer.WriteAttributeString(GraphMLTokens.EDGEDEFAULT, GraphMLTokens.DIRECTED);

                IEnumerable<Vertex> vertices;
                if (_Normalize)
                {
                    List<Vertex> sortedVertices = new List<Vertex>(_Graph.GetVertices());
                    sortedVertices.Sort(new LexicographicalElementComparator());
                    vertices = sortedVertices;
                }
                else
                    vertices = _Graph.GetVertices();

                foreach (Vertex vertex in vertices)
                {
                    writer.WriteStartElement(GraphMLTokens.NODE);
                    writer.WriteAttributeString(GraphMLTokens.ID, vertex.GetId().ToString());
                    IEnumerable<string> keys;
                    if (_Normalize)
                    {
                        List<string> sortedKeys = new List<string>(vertex.GetPropertyKeys());
                        sortedKeys.Sort();
                        keys = sortedKeys;
                    }
                    else
                        keys = vertex.GetPropertyKeys();

                    foreach (string key in keys)
                    {
                        writer.WriteStartElement(GraphMLTokens.DATA);
                        writer.WriteAttributeString(GraphMLTokens.KEY, key);
                        object value = vertex.GetProperty(key);
                        if (null != value)
                            writer.WriteString(value.ToString());

                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                if (_Normalize)
                {
                    List<Edge> edges = new List<Edge>();
                    foreach (Vertex vertex in _Graph.GetVertices())
                        edges.AddRange(vertex.GetEdges(Direction.OUT));
                    edges.Sort(new LexicographicalElementComparator());

                    foreach (Edge edge in edges)
                    {
                        writer.WriteStartElement(GraphMLTokens.EDGE);
                        writer.WriteAttributeString(GraphMLTokens.ID, edge.GetId().ToString());
                        writer.WriteAttributeString(GraphMLTokens.SOURCE, edge.GetVertex(Direction.OUT).GetId().ToString());
                        writer.WriteAttributeString(GraphMLTokens.TARGET, edge.GetVertex(Direction.IN).GetId().ToString());

                        if (_EdgeLabelKey == null)
                        {
                            // this will not comply with the graphml schema but is here so that the label is not
                            // mixed up with properties.
                            writer.WriteAttributeString(GraphMLTokens.LABEL, edge.GetLabel());
                        }
                        else
                        {
                            writer.WriteStartElement(GraphMLTokens.DATA);
                            writer.WriteAttributeString(GraphMLTokens.KEY, _EdgeLabelKey);
                            writer.WriteString(edge.GetLabel());
                            writer.WriteEndElement();
                        }

                        List<string> keys = new List<string>(edge.GetPropertyKeys());
                        keys.Sort();

                        foreach (string key in keys)
                        {
                            writer.WriteStartElement(GraphMLTokens.DATA);
                            writer.WriteAttributeString(GraphMLTokens.KEY, key);
                            object value = edge.GetProperty(key);
                            if (null != value)
                                writer.WriteString(value.ToString());

                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                }
                else
                {
                    foreach (Vertex vertex in _Graph.GetVertices())
                    {
                        foreach (Edge edge in vertex.GetEdges(Direction.OUT))
                        {
                            writer.WriteStartElement(GraphMLTokens.EDGE);
                            writer.WriteAttributeString(GraphMLTokens.ID, edge.GetId().ToString());
                            writer.WriteAttributeString(GraphMLTokens.SOURCE, edge.GetVertex(Direction.OUT).GetId().ToString());
                            writer.WriteAttributeString(GraphMLTokens.TARGET, edge.GetVertex(Direction.IN).GetId().ToString());
                            writer.WriteAttributeString(GraphMLTokens.LABEL, edge.GetLabel());

                            foreach (string key in edge.GetPropertyKeys())
                            {
                                writer.WriteStartElement(GraphMLTokens.DATA);
                                writer.WriteAttributeString(GraphMLTokens.KEY, key);
                                object value = edge.GetProperty(key);
                                if (null != value)
                                    writer.WriteString(value.ToString());

                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();
                        }
                    }
                }

                writer.WriteEndElement(); // graph
                writer.WriteEndElement(); // graphml
                writer.WriteEndDocument();
            }
        }

        /// <summary>
        /// Write the data in a Graph to a GraphML OutputStream.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="graphMLOutputStream">the GraphML OutputStream to write the Graph data to</param>
        public static void OutputGraph(Graph graph, Stream graphMLOutputStream)
        {
            GraphMLWriter writer = new GraphMLWriter(graph);
            writer.OutputGraph(graphMLOutputStream);
        }

        /// <summary>
        /// Write the data in a Graph to a GraphML file.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="filename">the name of the file write the Graph data (as GraphML) to</param>
        public static void OutputGraph(Graph graph, string filename)
        {
            GraphMLWriter writer = new GraphMLWriter(graph);
            writer.OutputGraph(filename);
        }

        /// <summary>
        /// Write the data in a Graph to a GraphML file.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="filename">the name of the file write the Graph data (as GraphML) to</param>
        /// <param name="vertexKeyTypes">a IDictionary<string, string> of the data types of the vertex keys</param>
        /// <param name="edgeKeyTypes">a IDictionary<string, string> of the data types of the edge keys</param>
        public static void OutputGraph(Graph graph, string filename,
                                   Dictionary<string, string> vertexKeyTypes, Dictionary<string, string> edgeKeyTypes)
        {
            GraphMLWriter writer = new GraphMLWriter(graph);
            writer.SetVertexKeyTypes(vertexKeyTypes);
            writer.SetEdgeKeyTypes(edgeKeyTypes);
            writer.OutputGraph(filename);
        }

        /// <summary>
        /// Write the data in a Graph to a GraphML OutputStream.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="graphMLOutputStream">the GraphML OutputStream to write the Graph data to</param>
        /// <param name="vertexKeyTypes">a IDictionary<string, string> of the data types of the vertex keys</param>
        /// <param name="edgeKeyTypes">a IDictionary<string, string> of the data types of the edge keys</param>
        public static void OutputGraph(Graph graph, Stream graphMLOutputStream,
                                   Dictionary<string, string> vertexKeyTypes, Dictionary<string, string> edgeKeyTypes)
        {
            GraphMLWriter writer = new GraphMLWriter(graph);
            writer.SetVertexKeyTypes(vertexKeyTypes);
            writer.SetEdgeKeyTypes(edgeKeyTypes);
            writer.OutputGraph(graphMLOutputStream);
        }

        static string GetStringType(object object_)
        {
            if (object_ is string)
                return GraphMLTokens.STRING;
            else if (object_ is int)
                return GraphMLTokens.INT;
            else if (object_ is long)
                return GraphMLTokens.LONG;
            else if (object_ is float)
                return GraphMLTokens.FLOAT;
            else if (object_ is double)
                return GraphMLTokens.DOUBLE;
            else if (object_ is bool)
                return GraphMLTokens.BOOLEAN;
            else
                return GraphMLTokens.STRING;
        }
    }
}
