using System;
using System.Collections.Generic;
using System.Globalization;
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
        readonly Graph _graph;
        bool _normalize = false;
        Dictionary<string, string> _vertexKeyTypes = null;
        Dictionary<string, string> _edgeKeyTypes = null;

        string _xmlSchemaLocation = null;
        string _edgeLabelKey = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        public GraphMLWriter(Graph graph)
        {
            _graph = graph;
        }

        /// <summary>
        /// the location of the GraphML XML Schema instance
        /// </summary>
        /// <param name="xmlSchemaLocation"></param>
        public void setXmlSchemaLocation(string xmlSchemaLocation)
        {
            _xmlSchemaLocation = xmlSchemaLocation;
        }

        /// <summary>
        /// Set the name of the edge label in the GraphML. When this value is not set the value of the Edge.getLabel()
        /// is written as a "label" attribute on the edge element.  This does not validate against the GraphML schema.
        /// If this value is set then the the value of Edge.getLabel() is written as a data element on the edge and
        /// the appropriate key element is added to define it in the GraphML
        /// </summary>
        /// <param name="edgeLabelKey">if the label of an edge will be handled by the data property.</param>
        public void setEdgeLabelKey(string edgeLabelKey)
        {
            _edgeLabelKey = edgeLabelKey;
        }

        /// <summary>
        /// whether to normalize the output. Normalized output is deterministic with respect to the order of
        /// elements and properties in the resulting XML document, and is compatible with line diff-based tools
        /// such as Git. Note: normalized output is memory-intensive and is not appropriate for very large graphs.
        /// </summary>
        /// <param name="normalize"></param>
        public void setNormalize(bool normalize)
        {
            _normalize = normalize;
        }

        /// <summary>
        /// a IDictionary<string, string> of the data types of the vertex keys
        /// </summary>
        /// <param name="vertexKeyTypes"></param>
        public void setVertexKeyTypes(Dictionary<string, string> vertexKeyTypes)
        {
            _vertexKeyTypes = vertexKeyTypes;
        }

        /// <summary>
        /// a IDictionary<string, string> of the data types of the edge keys
        /// </summary>
        /// <param name="edgeKeyTypes"></param>
        public void setEdgeKeyTypes(Dictionary<string, string> edgeKeyTypes)
        {
            _edgeKeyTypes = edgeKeyTypes;
        }

        /// <summary>
        /// Write the data in a Graph to a GraphML file.
        /// </summary>
        /// <param name="filename">the name of the file write the Graph data (as GraphML) to</param>
        public void outputGraph(string filename)
        {
            using (var fos = File.Open(filename, FileMode.Create))
            {
                outputGraph(fos);
            }
        }

        const string XMLNS_ATTRIBUTE = "xmlns";
        const string W3C_XML_SCHEMA_INSTANCE_NS_URI = "http://www.w3.org/2001/XMLSchema-instance";


        /// <summary>
        /// Write the data in a Graph to a GraphML OutputStream.
        /// </summary>
        /// <param name="graphMLOutputStream">the GraphML OutputStream to write the Graph data to</param>
        public void outputGraph(Stream graphMLOutputStream)
        {
            if (null == _vertexKeyTypes || null == _edgeKeyTypes)
            {
                Dictionary<string, string> vertexKeyTypes = new Dictionary<string, string>();
                Dictionary<string, string> edgeKeyTypes = new Dictionary<string, string>();

                foreach (Vertex vertex in _graph.getVertices())
                {
                    foreach (string key in vertex.getPropertyKeys())
                    {
                        if (!vertexKeyTypes.ContainsKey(key))
                            vertexKeyTypes[key] = GraphMLWriter.getStringType(vertex.getProperty(key));
                    }
                    foreach (Edge edge in vertex.getEdges(Direction.OUT))
                    {
                        foreach (string key in edge.getPropertyKeys())
                        {
                            if (!edgeKeyTypes.ContainsKey(key))
                                edgeKeyTypes[key] = GraphMLWriter.getStringType(edge.getProperty(key));
                        }
                    }
                }

                if (null == _vertexKeyTypes)
                    _vertexKeyTypes = vertexKeyTypes;

                if (null == _edgeKeyTypes)
                    _edgeKeyTypes = edgeKeyTypes;
            }

            // adding the edge label key will push the label into the data portion of the graphml otherwise it
            // will live with the edge data itself (which won't validate against the graphml schema)
            if (null != _edgeLabelKey && null != _edgeKeyTypes && null == _edgeKeyTypes.get(_edgeLabelKey))
                _edgeKeyTypes[_edgeLabelKey] = GraphMLTokens.STRING;

            XmlTextWriter writer = new XmlTextWriter(new EncodingStreamWriter(graphMLOutputStream, null));
            
            if (_normalize)
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 2;
            }

            writer.WriteStartDocument();
            writer.WriteStartElement(GraphMLTokens.GRAPHML);
            writer.WriteAttributeString(GraphMLTokens.XMLNS, GraphMLTokens.GRAPHML_XMLNS);

            //XML Schema instance namespace definition (xsi)
            writer.WriteAttributeString(string.Concat(XMLNS_ATTRIBUTE, ":", GraphMLTokens.XML_SCHEMA_NAMESPACE_TAG), W3C_XML_SCHEMA_INSTANCE_NS_URI);

            //XML Schema location
            writer.WriteAttributeString(string.Concat(GraphMLTokens.XML_SCHEMA_NAMESPACE_TAG, ":", GraphMLTokens.XML_SCHEMA_LOCATION_ATTRIBUTE),
                    string.Concat(GraphMLTokens.GRAPHML_XMLNS, " ", (_xmlSchemaLocation == null ?
                            GraphMLTokens.DEFAULT_GRAPHML_SCHEMA_LOCATION : _xmlSchemaLocation)));

            // <key id="weight" for="edge" attr.name="weight" attr.type="float"/>
            IEnumerable<string> keyset;

            if (_normalize)
            {
                List<string> sortedKeyset = new List<string>(_vertexKeyTypes.Keys);
                sortedKeyset.Sort();
                keyset = sortedKeyset;
            }
            else
                keyset = _vertexKeyTypes.Keys.ToList();

            foreach (string key in keyset)
            {
                writer.WriteStartElement(GraphMLTokens.KEY);
                writer.WriteAttributeString(GraphMLTokens.ID, key);
                writer.WriteAttributeString(GraphMLTokens.FOR, GraphMLTokens.NODE);
                writer.WriteAttributeString(GraphMLTokens.ATTR_NAME, key);
                writer.WriteAttributeString(GraphMLTokens.ATTR_TYPE, _vertexKeyTypes.get(key));
                Formatting oldFormating = writer.Formatting;
                writer.Formatting = Formatting.None;
                writer.WriteFullEndElement();
                writer.Formatting = oldFormating;
            }

            if (_normalize)
            {
                List<string> sortedKeyset = new List<string>(_edgeKeyTypes.Keys);
                sortedKeyset.Sort();
                keyset = sortedKeyset;
            }
            else
                keyset = _edgeKeyTypes.Keys;

            foreach (string key in keyset)
            {
                writer.WriteStartElement(GraphMLTokens.KEY);
                writer.WriteAttributeString(GraphMLTokens.ID, key);
                writer.WriteAttributeString(GraphMLTokens.FOR, GraphMLTokens.EDGE);
                writer.WriteAttributeString(GraphMLTokens.ATTR_NAME, key);
                writer.WriteAttributeString(GraphMLTokens.ATTR_TYPE, _edgeKeyTypes.get(key));
                Formatting oldFormating = writer.Formatting;
                writer.Formatting = Formatting.None;
                writer.WriteFullEndElement();
                writer.Formatting = oldFormating;
            }

            writer.WriteStartElement(GraphMLTokens.GRAPH);
            writer.WriteAttributeString(GraphMLTokens.ID, GraphMLTokens.G);
            writer.WriteAttributeString(GraphMLTokens.EDGEDEFAULT, GraphMLTokens.DIRECTED);

            IEnumerable<Vertex> vertices;
            if (_normalize)
            {
                List<Vertex> sortedVertices = new List<Vertex>(_graph.getVertices());
                sortedVertices.Sort(new LexicographicalElementComparator());
                vertices = sortedVertices;
            }
            else
                vertices = _graph.getVertices();

            foreach (Vertex vertex in vertices)
            {
                writer.WriteStartElement(GraphMLTokens.NODE);
                writer.WriteAttributeString(GraphMLTokens.ID, vertex.getId().ToString());
                IEnumerable<string> keys;
                if (_normalize)
                {
                    List<string> sortedKeys = new List<string>(vertex.getPropertyKeys());
                    sortedKeys.Sort();
                    keys = sortedKeys;
                }
                else
                    keys = vertex.getPropertyKeys();

                foreach (string key in keys)
                {
                    writer.WriteStartElement(GraphMLTokens.DATA);
                    writer.WriteAttributeString(GraphMLTokens.KEY, key);
                    object value = vertex.getProperty(key);
                    if (null != value)
                        writer.WriteString(Convert.ToString(value, CultureInfo.InvariantCulture));

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }

            if (_normalize)
            {
                List<Edge> edges = new List<Edge>();
                foreach (Vertex vertex in _graph.getVertices())
                    edges.AddRange(vertex.getEdges(Direction.OUT));
                edges.Sort(new LexicographicalElementComparator());

                foreach (Edge edge in edges)
                {
                    writer.WriteStartElement(GraphMLTokens.EDGE);
                    writer.WriteAttributeString(GraphMLTokens.ID, edge.getId().ToString());
                    writer.WriteAttributeString(GraphMLTokens.SOURCE, edge.getVertex(Direction.OUT).getId().ToString());
                    writer.WriteAttributeString(GraphMLTokens.TARGET, edge.getVertex(Direction.IN).getId().ToString());

                    if (_edgeLabelKey == null)
                    {
                        // this will not comply with the graphml schema but is here so that the label is not
                        // mixed up with properties.
                        writer.WriteAttributeString(GraphMLTokens.LABEL, edge.getLabel());
                    }
                    else
                    {
                        writer.WriteStartElement(GraphMLTokens.DATA);
                        writer.WriteAttributeString(GraphMLTokens.KEY, _edgeLabelKey);
                        writer.WriteString(edge.getLabel());
                        writer.WriteEndElement();
                    }

                    List<string> keys = new List<string>(edge.getPropertyKeys());
                    keys.Sort();

                    foreach (string key in keys)
                    {
                        writer.WriteStartElement(GraphMLTokens.DATA);
                        writer.WriteAttributeString(GraphMLTokens.KEY, key);
                        object value = edge.getProperty(key);
                        if (null != value)
                            writer.WriteString(Convert.ToString(value, CultureInfo.InvariantCulture));

                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
            }
            else
            {
                foreach (Vertex vertex in _graph.getVertices())
                {
                    foreach (Edge edge in vertex.getEdges(Direction.OUT))
                    {
                        writer.WriteStartElement(GraphMLTokens.EDGE);
                        writer.WriteAttributeString(GraphMLTokens.ID, edge.getId().ToString());
                        writer.WriteAttributeString(GraphMLTokens.SOURCE, edge.getVertex(Direction.OUT).getId().ToString());
                        writer.WriteAttributeString(GraphMLTokens.TARGET, edge.getVertex(Direction.IN).getId().ToString());
                        writer.WriteAttributeString(GraphMLTokens.LABEL, edge.getLabel());

                        foreach (string key in edge.getPropertyKeys())
                        {
                            writer.WriteStartElement(GraphMLTokens.DATA);
                            writer.WriteAttributeString(GraphMLTokens.KEY, key);
                            object value = edge.getProperty(key);
                            if (null != value)
                                writer.WriteString(Convert.ToString(value, CultureInfo.InvariantCulture));

                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                }
            }

            writer.WriteEndElement(); // graph
            writer.WriteEndElement(); // graphml
            writer.WriteEndDocument();
            writer.Flush();
        }

        /// <summary>
        /// Write the data in a Graph to a GraphML OutputStream.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="graphMLOutputStream">the GraphML OutputStream to write the Graph data to</param>
        public static void outputGraph(Graph graph, Stream graphMLOutputStream)
        {
            GraphMLWriter writer = new GraphMLWriter(graph);
            writer.outputGraph(graphMLOutputStream);
        }

        /// <summary>
        /// Write the data in a Graph to a GraphML file.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="filename">the name of the file write the Graph data (as GraphML) to</param>
        public static void outputGraph(Graph graph, string filename)
        {
            GraphMLWriter writer = new GraphMLWriter(graph);
            writer.outputGraph(filename);
        }

        /// <summary>
        /// Write the data in a Graph to a GraphML file.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="filename">the name of the file write the Graph data (as GraphML) to</param>
        /// <param name="vertexKeyTypes">a IDictionary<string, string> of the data types of the vertex keys</param>
        /// <param name="edgeKeyTypes">a IDictionary<string, string> of the data types of the edge keys</param>
        public static void outputGraph(Graph graph, string filename,
                                   Dictionary<string, string> vertexKeyTypes, Dictionary<string, string> edgeKeyTypes)
        {
            GraphMLWriter writer = new GraphMLWriter(graph);
            writer.setVertexKeyTypes(vertexKeyTypes);
            writer.setEdgeKeyTypes(edgeKeyTypes);
            writer.outputGraph(filename);
        }

        /// <summary>
        /// Write the data in a Graph to a GraphML OutputStream.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="graphMLOutputStream">the GraphML OutputStream to write the Graph data to</param>
        /// <param name="vertexKeyTypes">a IDictionary<string, string> of the data types of the vertex keys</param>
        /// <param name="edgeKeyTypes">a IDictionary<string, string> of the data types of the edge keys</param>
        public static void outputGraph(Graph graph, Stream graphMLOutputStream,
                                   Dictionary<string, string> vertexKeyTypes, Dictionary<string, string> edgeKeyTypes)
        {
            GraphMLWriter writer = new GraphMLWriter(graph);
            writer.setVertexKeyTypes(vertexKeyTypes);
            writer.setEdgeKeyTypes(edgeKeyTypes);
            writer.outputGraph(graphMLOutputStream);
        }

        static string getStringType(object object_)
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
