using System;
using System.Collections;
using System.Collections.Generic;
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
        readonly Graph _Graph;
        bool _Normalize = false;
        bool _UseId = false;
        bool _Strict = false;
        string _VertexIdKey = GMLTokens.BLUEPRINTS_ID;
        string _EdgeIdKey = GMLTokens.BLUEPRINTS_ID;

        /// <note>
        /// Property keys must be alphanumeric and not exceed 254 characters. They must start with an alpha character.
        /// </note>
        const string GML_PROPERTY_KEY_REGEX = "[a-zA-Z][a-zA-Z0-9]{0,253}";
        static Regex regex = new Regex(GML_PROPERTY_KEY_REGEX, RegexOptions.Compiled);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph">he Graph to pull the data from</param>
        public GMLWriter(Graph graph)
        {
            _Graph = graph;
        }

        /// <summary>
        /// when set to true, property keys in the graph that do not meet the exact guidelines of the GML
        /// specification are ignored.  By default this value is false.
        /// </summary>
        /// <param name="strict"></param>
        public void SetStrict(bool strict)
        {
            _Strict = strict;
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
        /// whether to use the blueprints id directly or substitute with a generated integer. To use this option
        /// the blueprints ids must all be Integers of string representations of integers
        /// </summary>
        /// <param name="useId"></param>
        public void SetUseId(bool useId)
        {
            _UseId = useId;
        }

        /// <summary>
        /// gml property to use for the blueprints vertex id, defaults to GMLTokens.BLUEPRINTS_ID
        /// </summary>
        /// <param name="vertexIdKey"></param>
        public void SetVertexIdKey(string vertexIdKey)
        {
            _VertexIdKey = vertexIdKey;
        }

        /// <summary>
        /// gml property to use for the blueprints edges id, defaults to GMLTokens.BLUEPRINTS_ID
        /// </summary>
        /// <param name="edgeIdKey"></param>
        public void SetEdgeIdKey(string edgeIdKey)
        {
            _EdgeIdKey = edgeIdKey;
        }

        /// <summary>
        /// Write the data in a Graph to a GML OutputStream.
        /// </summary>
        /// <param name="filename">the GML file to write the Graph data to</param>
        public void OutputGraph(string filename)
        {
            using (var fos = File.Open(filename, FileMode.Create))
            {
                OutputGraph(fos);
            }
        }

        /// <summary>
        /// Write the data in a Graph to a GML OutputStream.
        /// </summary>
        /// <param name="gMLOutputStream">the GML OutputStream to write the Graph data to</param>
        public void OutputGraph(Stream gMLOutputStream)
        {
            // ISO 8859-1 as specified in the GML documentation
            using (var writer = new StreamWriter(gMLOutputStream, Encoding.GetEncoding("ISO-8859-1")))
            {
                List<Vertex> vertices = new List<Vertex>();
                List<Edge> edges = new List<Edge>();

                PopulateLists(vertices, edges);

                if (_Normalize)
                {
                    LexicographicalElementComparator comparator = new LexicographicalElementComparator();
                    vertices.Sort(comparator);
                    edges.Sort(comparator);
                }

                WriteGraph(writer, vertices, edges);
            }
        }

        void WriteGraph(StreamWriter writer, List<Vertex> vertices, List<Edge> edges)
        {
            Dictionary<Vertex, int> ids = new Dictionary<Vertex, int>();

            writer.Write(GMLTokens.GRAPH);
            writer.Write(OPEN_LIST);
            WriteVertices(writer, vertices, ids);
            WriteEdges(writer, edges, ids);
            writer.Write(CLOSE_LIST);
        }

        void WriteVertices(StreamWriter writer, List<Vertex> vertices, Dictionary<Vertex, int> ids)
        {
            int count = 1;
            foreach (Vertex v in vertices)
            {
                if (_UseId)
                {
                    int id = int.Parse(v.GetId().ToString());
                    WriteVertex(writer, v, id);
                    ids[v] = id;
                }
                else
                {
                    WriteVertex(writer, v, count);
                    ids[v] = count++;
                }
            }
        }

        void WriteVertex(StreamWriter writer, Vertex v, int id)
        {
            writer.Write(TAB);
            writer.Write(GMLTokens.NODE);
            writer.Write(OPEN_LIST);
            WriteKey(writer, GMLTokens.ID);
            WriteNumberProperty(writer, id);
            WriteVertexProperties(writer, v);
            writer.Write(TAB);
            writer.Write(CLOSE_LIST);
        }

        void WriteEdges(StreamWriter writer, List<Edge> edges,
                        Dictionary<Vertex, int> ids)
        {
            foreach (Edge e in edges)
                WriteEdgeProperties(writer, e, ids.Get(e.GetVertex(Direction.OUT)), ids.Get(e.GetVertex(Direction.IN)));
        }

        void WriteEdgeProperties(StreamWriter writer, Edge e, int source, int target)
        {
            writer.Write(TAB);
            writer.Write(GMLTokens.EDGE);
            writer.Write(OPEN_LIST);
            WriteKey(writer, GMLTokens.SOURCE);
            WriteNumberProperty(writer, source);
            WriteKey(writer, GMLTokens.TARGET);
            WriteNumberProperty(writer, target);
            WriteKey(writer, GMLTokens.LABEL);
            WriteStringProperty(writer, e.GetLabel());
            WriteEdgeProperties(writer, e);
            writer.Write(TAB);
            writer.Write(CLOSE_LIST);
        }

        void WriteVertexProperties(StreamWriter writer, Vertex e)
        {
            object blueprintsId = e.GetId();
            if (!_UseId)
            {
                WriteKey(writer, _VertexIdKey);
                if (Portability.IsNumeric(blueprintsId))
                    WriteNumberProperty(writer, Convert.ToInt64(blueprintsId));
                else
                    WriteStringProperty(writer, blueprintsId);
            }
            WriteProperties(writer, e);
        }

        void WriteEdgeProperties(StreamWriter writer, Edge e)
        {
            object blueprintsId = e.GetId();
            if (!_UseId)
            {
                WriteKey(writer, _EdgeIdKey);
                if (Portability.IsNumeric(blueprintsId))
                    WriteNumberProperty(writer, Convert.ToInt64(blueprintsId));
                else
                    WriteStringProperty(writer, blueprintsId);
            }
            WriteProperties(writer, e);
        }

        void WriteProperties(StreamWriter writer, Element e)
        {
            foreach (string key in e.GetPropertyKeys())
            {
                if (!_Strict || regex.Match(key).Length > 0)
                {
                    object property = e.GetProperty(key);
                    WriteKey(writer, key);
                    WriteProperty(writer, property, 0);
                }
            }
        }

        void WriteProperty(StreamWriter writer, object property, int tab)
        {
            if (Portability.IsNumeric(property))
                WriteNumberProperty(writer, Convert.ToInt64(property));
            else if (property is IDictionary)
                WriteMapProperty(writer, property as IDictionary, tab);
            else
                WriteStringProperty(writer, property.ToString());
        }

        void WriteMapProperty(StreamWriter writer, IDictionary map, int tabs)
        {
            writer.Write(OPEN_LIST);
            tabs++;
            foreach (DictionaryEntry entry in map)
            {
                WriteTabs(writer, tabs);
                WriteKey(writer, entry.Key.ToString());
                WriteProperty(writer, entry.Value, tabs);
            }
            WriteTabs(writer, tabs - 1);
            writer.Write(CLOSE_LIST);
        }

        void WriteTabs(StreamWriter writer, int tabs)
        {
            for (int i = 0; i <= tabs; i++)
                writer.Write(TAB);
        }

        void WriteNumberProperty(StreamWriter writer, long integer)
        {
            writer.Write(integer.ToString());
            writer.Write(NEW_LINE);
        }

        void WriteStringProperty(StreamWriter writer, object string_)
        {
            writer.Write("\"");
            writer.Write(string_.ToString());
            writer.Write("\"");
            writer.Write(NEW_LINE);
        }

        void WriteKey(StreamWriter writer, string command)
        {
            writer.Write(TAB);
            writer.Write(TAB);
            writer.Write(command);
            writer.Write(DELIMITER);
        }

        void PopulateLists(List<Vertex> vertices, List<Edge> edges)
        {
            vertices.AddRange(_Graph.GetVertices());
            edges.AddRange(_Graph.GetEdges());
        }

        /// <summary>
        /// Write the data in a Graph to a GML OutputStream.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="graphMLOutputStream">the GML OutputStream to write the Graph data to</param>
        public static void OutputGraph(Graph graph, Stream graphMLOutputStream)
        {
            GMLWriter writer = new GMLWriter(graph);
            writer.OutputGraph(graphMLOutputStream);
        }

        /// <summary>
        /// Write the data in a Graph to a GML OutputStream.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="filename">the GML file to write the Graph data to</param>
        public static void OutputGraph(Graph graph, string filename)
        {
            GMLWriter writer = new GMLWriter(graph);
            writer.OutputGraph(filename);
        }
    }
}
