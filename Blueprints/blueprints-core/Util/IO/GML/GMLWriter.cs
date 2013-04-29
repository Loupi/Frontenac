using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Frontenac.Blueprints.Util.IO.GML
{
    /// <summary>
    /// GMLWriter writes a Graph to a GML OutputStream.
    /// <p/>
    /// GML definition taken from
    /// (http://www.fim.uni-passau.de/fileadmin/files/lehrstuhl/brandenburg/projekte/gml/gml-documentation.tar.gz)
    /// </summary>
    public class GmlWriter
    {
        const string Delimiter = " ";
        const string Tab = "\t";
        const string NewLine = "\r\n";
        static readonly string OpenList = string.Concat(" [", NewLine);
        static readonly string CloseList = string.Concat("]", NewLine);
        readonly IGraph _graph;
        bool _normalize;
        bool _useId;
        bool _strict;
        string _vertexIdKey = GmlTokens.BlueprintsId;
        string _edgeIdKey = GmlTokens.BlueprintsId;

        /// <note>
        /// Property keys must be alphanumeric and not exceed 254 characters. They must start with an alpha character.
        /// </note>
        const string GmlPropertyKeyRegex = "[a-zA-Z][a-zA-Z0-9]{0,253}";
        static readonly Regex Regex = new Regex(GmlPropertyKeyRegex, RegexOptions.Compiled);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph">he Graph to pull the data from</param>
        public GmlWriter(IGraph graph)
        {
            _graph = graph;
        }

        /// <summary>
        /// when set to true, property keys in the graph that do not meet the exact guidelines of the GML
        /// specification are ignored.  By default this value is false.
        /// </summary>
        /// <param name="strict"></param>
        public void SetStrict(bool strict)
        {
            _strict = strict;
        }

        /// <summary>
        /// whether to normalize the output. Normalized output is deterministic with respect to the order of
        /// elements and properties in the resulting XML document, and is compatible with line diff-based tools
        /// such as Git. Note: normalized output is memory-intensive and is not appropriate for very large graphs.
        /// </summary>
        /// <param name="normalize"></param>
        public void SetNormalize(bool normalize)
        {
            _normalize = normalize;
        }

        /// <summary>
        /// whether to use the blueprints id directly or substitute with a generated integer. To use this option
        /// the blueprints ids must all be Integers of string representations of integers
        /// </summary>
        /// <param name="useId"></param>
        public void SetUseId(bool useId)
        {
            _useId = useId;
        }

        /// <summary>
        /// gml property to use for the blueprints vertex id, defaults to GMLTokens.BLUEPRINTS_ID
        /// </summary>
        /// <param name="vertexIdKey"></param>
        public void SetVertexIdKey(string vertexIdKey)
        {
            _vertexIdKey = vertexIdKey;
        }

        /// <summary>
        /// gml property to use for the blueprints edges id, defaults to GMLTokens.BLUEPRINTS_ID
        /// </summary>
        /// <param name="edgeIdKey"></param>
        public void SetEdgeIdKey(string edgeIdKey)
        {
            _edgeIdKey = edgeIdKey;
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
        /// <param name="gMlOutputStream">the GML OutputStream to write the Graph data to</param>
        public void OutputGraph(Stream gMlOutputStream)
        {
            // ISO 8859-1 as specified in the GML documentation
            var writer = new StreamWriter(gMlOutputStream, Encoding.GetEncoding("ISO-8859-1"));
            var vertices = new List<IVertex>();
            var edges = new List<IEdge>();

            PopulateLists(vertices, edges);

            if (_normalize)
            {
                var comparator = new LexicographicalElementComparator();
                vertices.Sort(comparator);
                edges.Sort(comparator);
            }

            WriteGraph(writer, vertices, edges);
            writer.Flush();
        }

        void WriteGraph(StreamWriter writer, IEnumerable<IVertex> vertices, IEnumerable<IEdge> edges)
        {
            var ids = new Dictionary<IVertex, int>();

            writer.Write(GmlTokens.Graph);
            writer.Write(OpenList);
            WriteVertices(writer, vertices, ids);
            WriteEdges(writer, edges, ids);
            writer.Write(CloseList);
        }

        void WriteVertices(StreamWriter writer, IEnumerable<IVertex> vertices, Dictionary<IVertex, int> ids)
        {
            int count = 1;
            foreach (IVertex v in vertices)
            {
                if (_useId)
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

        void WriteVertex(StreamWriter writer, IVertex v, int id)
        {
            writer.Write(Tab);
            writer.Write(GmlTokens.Node);
            writer.Write(OpenList);
            WriteKey(writer, GmlTokens.Id);
            WriteNumberProperty(writer, id);
            WriteVertexProperties(writer, v);
            writer.Write(Tab);
            writer.Write(CloseList);
        }

        void WriteEdges(StreamWriter writer, IEnumerable<IEdge> edges, Dictionary<IVertex, int> ids)
        {
            foreach (IEdge e in edges)
                WriteEdgeProperties(writer, e, ids.Get(e.GetVertex(Direction.Out)), ids.Get(e.GetVertex(Direction.In)));
        }

        void WriteEdgeProperties(StreamWriter writer, IEdge e, int source, int target)
        {
            writer.Write(Tab);
            writer.Write(GmlTokens.Edge);
            writer.Write(OpenList);
            WriteKey(writer, GmlTokens.Source);
            WriteNumberProperty(writer, source);
            WriteKey(writer, GmlTokens.Target);
            WriteNumberProperty(writer, target);
            WriteKey(writer, GmlTokens.Label);
            WriteStringProperty(writer, e.GetLabel());
            WriteEdgeProperties(writer, e);
            writer.Write(Tab);
            writer.Write(CloseList);
        }

        void WriteVertexProperties(StreamWriter writer, IVertex e)
        {
            object blueprintsId = e.GetId();
            if (!_useId)
            {
                WriteKey(writer, _vertexIdKey);
                if (Portability.IsNumber(blueprintsId))
                    WriteNumberProperty(writer, blueprintsId);
                else
                    WriteStringProperty(writer, blueprintsId);
            }
            WriteProperties(writer, e);
        }

        void WriteEdgeProperties(StreamWriter writer, IEdge e)
        {
            object blueprintsId = e.GetId();
            if (!_useId)
            {
                WriteKey(writer, _edgeIdKey);
                if (Portability.IsNumber(blueprintsId))
                    WriteNumberProperty(writer, blueprintsId);
                else
                    WriteStringProperty(writer, blueprintsId);
            }
            WriteProperties(writer, e);
        }

        void WriteProperties(StreamWriter writer, IElement e)
        {
            foreach (string key in e.GetPropertyKeys())
            {
                Match m = Regex.Match(key);
                if (!_strict || m.Length > 0 && m.Value == key)
                {
                    object property = e.GetProperty(key);
                    WriteKey(writer, key);
                    WriteProperty(writer, property, 0);
                }
            }
        }

        void WriteProperty(StreamWriter writer, object property, int tab)
        {
            if (Portability.IsNumber(property))
                WriteNumberProperty(writer, property);
            else if (property is IDictionary)
                WriteMapProperty(writer, property as IDictionary, tab);
            else
                WriteStringProperty(writer, property.ToString());
        }

        void WriteMapProperty(StreamWriter writer, IDictionary map, int tabs)
        {
            writer.Write(OpenList);
            tabs++;
            foreach (DictionaryEntry entry in map)
            {
                WriteTabs(writer, tabs);
                WriteKey(writer, entry.Key.ToString());
                WriteProperty(writer, entry.Value, tabs);
            }
            WriteTabs(writer, tabs - 1);
            writer.Write(CloseList);
        }

        static void WriteTabs(StreamWriter writer, int tabs)
        {
            for (int i = 0; i <= tabs; i++)
                writer.Write(Tab);
        }

        static void WriteNumberProperty(StreamWriter writer, object number)
        {
            writer.Write(Convert.ToString(number, CultureInfo.InvariantCulture));
            writer.Write(NewLine);
        }

        static void WriteStringProperty(StreamWriter writer, object string_)
        {
            writer.Write("\"");
            writer.Write(string_.ToString());
            writer.Write("\"");
            writer.Write(NewLine);
        }

        static void WriteKey(StreamWriter writer, string command)
        {
            writer.Write(Tab);
            writer.Write(Tab);
            writer.Write(command);
            writer.Write(Delimiter);
        }

        void PopulateLists(List<IVertex> vertices, List<IEdge> edges)
        {
            vertices.AddRange(_graph.GetVertices());
            edges.AddRange(_graph.GetEdges());
        }

        /// <summary>
        /// Write the data in a Graph to a GML OutputStream.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="graphMlOutputStream">the GML OutputStream to write the Graph data to</param>
        public static void OutputGraph(IGraph graph, Stream graphMlOutputStream)
        {
            var writer = new GmlWriter(graph);
            writer.OutputGraph(graphMlOutputStream);
        }

        /// <summary>
        /// Write the data in a Graph to a GML OutputStream.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="filename">the GML file to write the Graph data to</param>
        public static void OutputGraph(IGraph graph, string filename)
        {
            var writer = new GmlWriter(graph);
            writer.OutputGraph(filename);
        }
    }
}
