using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO.GML
{
    /// <summary>
    /// GMLWriter writes a Graph to a GML OutputStream.
    /// <p/>
    /// GML definition taken from
    /// (http://www.fim.uni-passau.de/fileadmin/files/lehrstuhl/brandenburg/projekte/gml/gml-documentation.tar.gz)
    /// </summary>
    public class GMLWriter
    {
        const string DELIMITER = " ";
        const string TAB = "\t";
        const string NEW_LINE = "\r\n";
        static readonly string OPEN_LIST = string.Concat(" [", NEW_LINE);
        static readonly string CLOSE_LIST = string.Concat("]", NEW_LINE);
        readonly Graph _graph;
        bool _normalize = false;
        bool _useId = false;
        bool _strict = false;
        string _vertexIdKey = GMLTokens.BLUEPRINTS_ID;
        string _edgeIdKey = GMLTokens.BLUEPRINTS_ID;

        /// <note>
        /// Property keys must be alphanumeric and not exceed 254 characters. They must start with an alpha character.
        /// </note>
        const string GML_PROPERTY_KEY_REGEX = "[a-zA-Z][a-zA-Z0-9]{0,253}";
        static readonly Regex regex = new Regex(GML_PROPERTY_KEY_REGEX, RegexOptions.Compiled);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph">he Graph to pull the data from</param>
        public GMLWriter(Graph graph)
        {
            _graph = graph;
        }

        /// <summary>
        /// when set to true, property keys in the graph that do not meet the exact guidelines of the GML
        /// specification are ignored.  By default this value is false.
        /// </summary>
        /// <param name="strict"></param>
        public void setStrict(bool strict)
        {
            _strict = strict;
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
        /// whether to use the blueprints id directly or substitute with a generated integer. To use this option
        /// the blueprints ids must all be Integers of string representations of integers
        /// </summary>
        /// <param name="useId"></param>
        public void setUseId(bool useId)
        {
            _useId = useId;
        }

        /// <summary>
        /// gml property to use for the blueprints vertex id, defaults to GMLTokens.BLUEPRINTS_ID
        /// </summary>
        /// <param name="vertexIdKey"></param>
        public void setVertexIdKey(string vertexIdKey)
        {
            _vertexIdKey = vertexIdKey;
        }

        /// <summary>
        /// gml property to use for the blueprints edges id, defaults to GMLTokens.BLUEPRINTS_ID
        /// </summary>
        /// <param name="edgeIdKey"></param>
        public void setEdgeIdKey(string edgeIdKey)
        {
            _edgeIdKey = edgeIdKey;
        }

        /// <summary>
        /// Write the data in a Graph to a GML OutputStream.
        /// </summary>
        /// <param name="filename">the GML file to write the Graph data to</param>
        public void outputGraph(string filename)
        {
            using (var fos = File.Open(filename, FileMode.Create))
            {
                outputGraph(fos);
            }
        }

        /// <summary>
        /// Write the data in a Graph to a GML OutputStream.
        /// </summary>
        /// <param name="gMLOutputStream">the GML OutputStream to write the Graph data to</param>
        public void outputGraph(Stream gMLOutputStream)
        {
            // ISO 8859-1 as specified in the GML documentation
            var writer = new StreamWriter(gMLOutputStream, Encoding.GetEncoding("ISO-8859-1"));
            List<Vertex> vertices = new List<Vertex>();
            List<Edge> edges = new List<Edge>();

            populateLists(vertices, edges);

            if (_normalize)
            {
                LexicographicalElementComparator comparator = new LexicographicalElementComparator();
                vertices.Sort(comparator);
                edges.Sort(comparator);
            }

            writeGraph(writer, vertices, edges);
            writer.Flush();
        }

        void writeGraph(StreamWriter writer, List<Vertex> vertices, List<Edge> edges)
        {
            Dictionary<Vertex, int> ids = new Dictionary<Vertex, int>();

            writer.Write(GMLTokens.GRAPH);
            writer.Write(OPEN_LIST);
            writeVertices(writer, vertices, ids);
            writeEdges(writer, edges, ids);
            writer.Write(CLOSE_LIST);
        }

        void writeVertices(StreamWriter writer, List<Vertex> vertices, Dictionary<Vertex, int> ids)
        {
            int count = 1;
            foreach (Vertex v in vertices)
            {
                if (_useId)
                {
                    int id = int.Parse(v.getId().ToString());
                    writeVertex(writer, v, id);
                    ids[v] = id;
                }
                else
                {
                    writeVertex(writer, v, count);
                    ids[v] = count++;
                }
            }
        }

        void writeVertex(StreamWriter writer, Vertex v, int id)
        {
            writer.Write(TAB);
            writer.Write(GMLTokens.NODE);
            writer.Write(OPEN_LIST);
            writeKey(writer, GMLTokens.ID);
            writeNumberProperty(writer, id);
            writeVertexProperties(writer, v);
            writer.Write(TAB);
            writer.Write(CLOSE_LIST);
        }

        void writeEdges(StreamWriter writer, List<Edge> edges,
                        Dictionary<Vertex, int> ids)
        {
            foreach (Edge e in edges)
                writeEdgeProperties(writer, e, ids.get(e.getVertex(Direction.OUT)), ids.get(e.getVertex(Direction.IN)));
        }

        void writeEdgeProperties(StreamWriter writer, Edge e, int source, int target)
        {
            writer.Write(TAB);
            writer.Write(GMLTokens.EDGE);
            writer.Write(OPEN_LIST);
            writeKey(writer, GMLTokens.SOURCE);
            writeNumberProperty(writer, source);
            writeKey(writer, GMLTokens.TARGET);
            writeNumberProperty(writer, target);
            writeKey(writer, GMLTokens.LABEL);
            writeStringProperty(writer, e.getLabel());
            writeEdgeProperties(writer, e);
            writer.Write(TAB);
            writer.Write(CLOSE_LIST);
        }

        void writeVertexProperties(StreamWriter writer, Vertex e)
        {
            object blueprintsId = e.getId();
            if (!_useId)
            {
                writeKey(writer, _vertexIdKey);
                if (Portability.isNumber(blueprintsId))
                    writeNumberProperty(writer, blueprintsId);
                else
                    writeStringProperty(writer, blueprintsId);
            }
            WriteProperties(writer, e);
        }

        void writeEdgeProperties(StreamWriter writer, Edge e)
        {
            object blueprintsId = e.getId();
            if (!_useId)
            {
                writeKey(writer, _edgeIdKey);
                if (Portability.isNumber(blueprintsId))
                    writeNumberProperty(writer, blueprintsId);
                else
                    writeStringProperty(writer, blueprintsId);
            }
            WriteProperties(writer, e);
        }

        void WriteProperties(StreamWriter writer, Element e)
        {
            foreach (string key in e.getPropertyKeys())
            {
                Match m = regex.Match(key);
                if (!_strict || m.Length > 0 && m.Value == key)
                {
                    object property = e.getProperty(key);
                    writeKey(writer, key);
                    writeProperty(writer, property, 0);
                }
            }
        }

        void writeProperty(StreamWriter writer, object property, int tab)
        {
            if (Portability.isNumber(property))
                writeNumberProperty(writer, property);
            else if (property is IDictionary)
                writeMapProperty(writer, property as IDictionary, tab);
            else
                writeStringProperty(writer, property.ToString());
        }

        void writeMapProperty(StreamWriter writer, IDictionary map, int tabs)
        {
            writer.Write(OPEN_LIST);
            tabs++;
            foreach (DictionaryEntry entry in map)
            {
                writeTabs(writer, tabs);
                writeKey(writer, entry.Key.ToString());
                writeProperty(writer, entry.Value, tabs);
            }
            writeTabs(writer, tabs - 1);
            writer.Write(CLOSE_LIST);
        }

        void writeTabs(StreamWriter writer, int tabs)
        {
            for (int i = 0; i <= tabs; i++)
                writer.Write(TAB);
        }

        void writeNumberProperty(StreamWriter writer, object number)
        {
            writer.Write(Convert.ToString(number, CultureInfo.InvariantCulture));
            writer.Write(NEW_LINE);
        }

        void writeStringProperty(StreamWriter writer, object string_)
        {
            writer.Write("\"");
            writer.Write(string_.ToString());
            writer.Write("\"");
            writer.Write(NEW_LINE);
        }

        void writeKey(StreamWriter writer, string command)
        {
            writer.Write(TAB);
            writer.Write(TAB);
            writer.Write(command);
            writer.Write(DELIMITER);
        }

        void populateLists(List<Vertex> vertices, List<Edge> edges)
        {
            vertices.AddRange(_graph.getVertices());
            edges.AddRange(_graph.getEdges());
        }

        /// <summary>
        /// Write the data in a Graph to a GML OutputStream.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="graphMLOutputStream">the GML OutputStream to write the Graph data to</param>
        public static void outputGraph(Graph graph, Stream graphMLOutputStream)
        {
            GMLWriter writer = new GMLWriter(graph);
            writer.outputGraph(graphMLOutputStream);
        }

        /// <summary>
        /// Write the data in a Graph to a GML OutputStream.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="filename">the GML file to write the Graph data to</param>
        public static void outputGraph(Graph graph, string filename)
        {
            GMLWriter writer = new GMLWriter(graph);
            writer.outputGraph(filename);
        }
    }
}
