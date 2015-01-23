﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Frontenac.Blueprints.Util.IO.GML
{
    /// <summary>
    ///     GMLWriter writes a Graph to a GML OutputStream.
    ///     <p />
    ///     GML definition taken from
    ///     (http://www.fim.uni-passau.de/fileadmin/files/lehrstuhl/brandenburg/projekte/gml/gml-documentation.tar.gz)
    /// </summary>
    public class GmlWriter
    {
        private const string Delimiter = " ";
        private const string Tab = "\t";
        private const string NewLine = "\r\n";

        /// <note>
        ///     Property keys must be alphanumeric and not exceed 254 characters. They must start with an alpha character.
        /// </note>
        private const string GmlPropertyKeyRegex = "[a-zA-Z][a-zA-Z0-9]{0,253}";

        private static readonly string OpenList = string.Concat(" [", NewLine);
        private static readonly string CloseList = string.Concat("]", NewLine);
        private static readonly Regex Regex = new Regex(GmlPropertyKeyRegex, RegexOptions.Compiled);
        private readonly IGraph _graph;
        private string _edgeIdKey = GmlTokens.BlueprintsId;
        private string _vertexIdKey = GmlTokens.BlueprintsId;

        /// <summary>
        /// </summary>
        /// <param name="graph">he Graph to pull the data from</param>
        public GmlWriter(IGraph graph)
        {
            Contract.Requires(graph != null);

            _graph = graph;
        }

        /// <summary>
        ///     when set to true, property keys in the Graph that do not meet the exact guidelines of the GML
        ///     specification are ignored.  By default this value is false.
        /// </summary>
        /// <value></value>
        public bool Strict { get; set; }

        /// <summary>
        ///     whether to normalize the output. Normalized output is deterministic with respect to the order of
        ///     elements and properties in the resulting XML document, and is compatible with line diff-based tools
        ///     such as Git. Note: normalized output is memory-intensive and is not appropriate for very large graphs.
        /// </summary>
        /// <value></value>
        public bool Normalize { get; set; }

        /// <summary>
        ///     whether to use the blueprints id directly or substitute with a generated integer. To use this option
        ///     the blueprints ids must all be Integers of string representations of integers
        /// </summary>
        /// <value></value>
        public bool UseId { get; set; }

        /// <summary>
        ///     gml property to use for the blueprints vertex id, defaults to GMLTokens.BLUEPRINTS_ID
        /// </summary>
        /// <value></value>
        public string VertexIdKey
        {
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));
                return _vertexIdKey;
            }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));
                _vertexIdKey = value;
            }
        }

        /// <summary>
        ///     gml property to use for the blueprints edges id, defaults to GMLTokens.BLUEPRINTS_ID
        /// </summary>
        /// <value></value>
        public string EdgeIdKey
        {
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));
                return _edgeIdKey;
            }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));
                _edgeIdKey = value;
            }
        }

        /// <summary>
        ///     Write the data in a Graph to a GML OutputStream.
        /// </summary>
        /// <param name="filename">the GML file to write the Graph data to</param>
        public void OutputGraph(string filename)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));

            using (var fos = File.Open(filename, FileMode.Create))
            {
                OutputGraph(fos);
            }
        }

        /// <summary>
        ///     Write the data in a Graph to a GML OutputStream.
        /// </summary>
        /// <param name="gMlOutputStream">the GML OutputStream to write the Graph data to</param>
        public void OutputGraph(Stream gMlOutputStream)
        {
            Contract.Requires(gMlOutputStream != null);

            // ISO 8859-1 as specified in the GML documentation
            var writer = new StreamWriter(gMlOutputStream, Encoding.GetEncoding("ISO-8859-1"));
            var vertices = new List<IVertex>();
            var edges = new List<IEdge>();

            PopulateLists(vertices, edges);

            if (Normalize)
            {
                var comparator = new LexicographicalElementComparator();
                vertices.Sort(comparator);
                edges.Sort(comparator);
            }

            WriteGraph(writer, vertices, edges);
            writer.Flush();
        }

        private void WriteGraph(StreamWriter writer, IEnumerable<IVertex> vertices, IEnumerable<IEdge> edges)
        {
            Contract.Requires(writer != null);
            Contract.Requires(vertices != null);
            Contract.Requires(edges != null);

            var ids = new Dictionary<IVertex, int>();

            writer.Write(GmlTokens.Graph);
            writer.Write(OpenList);
            WriteVertices(writer, vertices, ids);
            WriteEdges(writer, edges, ids);
            writer.Write(CloseList);
        }

        private void WriteVertices(StreamWriter writer, IEnumerable<IVertex> vertices, Dictionary<IVertex, int> ids)
        {
            Contract.Requires(writer != null);
            Contract.Requires(vertices != null);
            Contract.Requires(ids != null);

            var count = 1;
            foreach (var v in vertices)
            {
                if (UseId)
                {
                    var id = int.Parse(v.Id.ToString());
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

        private void WriteVertex(StreamWriter writer, IVertex v, int id)
        {
            Contract.Requires(writer != null);
            Contract.Requires(v != null);

            writer.Write(Tab);
            writer.Write(GmlTokens.Node);
            writer.Write(OpenList);
            WriteKey(writer, GmlTokens.Id);
            WriteNumberProperty(writer, id);
            WriteElementProperties(writer, v);
            writer.Write(Tab);
            writer.Write(CloseList);
        }

        private void WriteEdges(StreamWriter writer, IEnumerable<IEdge> edges, Dictionary<IVertex, int> ids)
        {
            Contract.Requires(writer != null);
            Contract.Requires(edges != null);
            Contract.Requires(ids != null);

            foreach (var e in edges)
                WriteEdgeProperties(writer, e, ids.Get(e.GetVertex(Direction.Out)), ids.Get(e.GetVertex(Direction.In)));
        }

        private void WriteEdgeProperties(StreamWriter writer, IEdge e, int source, int target)
        {
            Contract.Requires(writer != null);
            Contract.Requires(e != null);

            writer.Write(Tab);
            writer.Write(GmlTokens.Edge);
            writer.Write(OpenList);
            WriteKey(writer, GmlTokens.Source);
            WriteNumberProperty(writer, source);
            WriteKey(writer, GmlTokens.Target);
            WriteNumberProperty(writer, target);
            WriteKey(writer, GmlTokens.Label);
            WriteStringProperty(writer, e.Label);
            WriteElementProperties(writer, e);
            writer.Write(Tab);
            writer.Write(CloseList);
        }

        private void WriteElementProperties(StreamWriter writer, IElement e)
        {
            Contract.Requires(writer != null);
            Contract.Requires(e != null);

            var blueprintsId = e.Id;
            if (!UseId)
            {
                WriteKey(writer, _vertexIdKey);
                if (Blueprints.GraphHelpers.IsNumber(blueprintsId))
                    WriteNumberProperty(writer, blueprintsId);
                else
                    WriteStringProperty(writer, blueprintsId);
            }
            WriteProperties(writer, e);
        }

        private void WriteProperties(StreamWriter writer, IElement e)
        {
            Contract.Requires(writer != null);
            Contract.Requires(e != null);

            foreach (var key in e.GetPropertyKeys())
            {
                var m = Regex.Match(key);
                if (!Strict || m.Length > 0 && m.Value == key)
                {
                    var property = e.GetProperty(key);
                    WriteKey(writer, key);
                    WriteProperty(writer, property, 0);
                }
            }
        }

        private void WriteProperty(StreamWriter writer, object property, int tab)
        {
            Contract.Requires(writer != null);

            if (Blueprints.GraphHelpers.IsNumber(property))
                WriteNumberProperty(writer, property);
            else if (property is IDictionary)
                WriteMapProperty(writer, property as IDictionary, tab);
            else
                WriteStringProperty(writer, property.ToString());
        }

        private void WriteMapProperty(StreamWriter writer, IDictionary map, int tabs)
        {
            Contract.Requires(writer != null);
            Contract.Requires(map != null);

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

        private static void WriteTabs(TextWriter writer, int tabs)
        {
            Contract.Requires(writer != null);

            for (var i = 0; i <= tabs; i++)
                writer.Write(Tab);
        }

        private static void WriteNumberProperty(TextWriter writer, object number)
        {
            Contract.Requires(writer != null);
            Contract.Requires(number != null);

            writer.Write(Convert.ToString(number, CultureInfo.InvariantCulture));
            writer.Write(NewLine);
        }

        private static void WriteStringProperty(TextWriter writer, object string_)
        {
            Contract.Requires(writer != null);
            Contract.Requires(string_ != null);

            writer.Write("\"");
            writer.Write(string_.ToString());
            writer.Write("\"");
            writer.Write(NewLine);
        }

        private static void WriteKey(TextWriter writer, string command)
        {
            Contract.Requires(writer != null);
            Contract.Requires(command != null);

            writer.Write(Tab);
            writer.Write(Tab);
            writer.Write(command);
            writer.Write(Delimiter);
        }

        private void PopulateLists(List<IVertex> vertices, List<IEdge> edges)
        {
            Contract.Requires(vertices != null);
            Contract.Requires(edges != null);

            vertices.AddRange(_graph.GetVertices());
            edges.AddRange(_graph.GetEdges());
        }

        /// <summary>
        ///     Write the data in a Graph to a GML OutputStream.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="graphMlOutputStream">the GML OutputStream to write the Graph data to</param>
        public static void OutputGraph(IGraph graph, Stream graphMlOutputStream)
        {
            Contract.Requires(graph != null);
            Contract.Requires(graphMlOutputStream != null);

            var writer = new GmlWriter(graph);
            writer.OutputGraph(graphMlOutputStream);
        }

        /// <summary>
        ///     Write the data in a Graph to a GML OutputStream.
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        /// <param name="filename">the GML file to write the Graph data to</param>
        public static void OutputGraph(IGraph graph, string filename)
        {
            Contract.Requires(graph != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));

            var writer = new GmlWriter(graph);
            writer.OutputGraph(filename);
        }
    }
}