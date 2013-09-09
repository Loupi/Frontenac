using System.Diagnostics.Contracts;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    /// <summary>
    /// GraphSONWriter writes a Graph to a TinkerPop JSON OutputStream.
    /// </summary>
    public class GraphSonWriter
    {
        readonly IGraph _graph;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        public GraphSonWriter(IGraph graph)
        {
            Contract.Requires(graph != null);

            _graph = graph;
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream.
        /// </summary>
        /// <param name="filename">the JSON file to write the Graph data to</param>
        /// <param name="vertexPropertyKeys">the keys of the vertex elements to write to JSON</param>
        /// <param name="edgePropertyKeys">the keys of the edge elements to write to JSON</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public void OutputGraph(string filename, IEnumerable<string> vertexPropertyKeys,
                            IEnumerable<string> edgePropertyKeys, GraphSONMode mode)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));

            using (var fos = File.Open(filename, FileMode.Create))
            {
                OutputGraph(fos, vertexPropertyKeys, edgePropertyKeys, mode);
            }
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream.
        /// </summary>
        /// <param name="jsonOutputStream">the JSON OutputStream to write the Graph data to</param>
        /// <param name="vertexPropertyKeys">the keys of the vertex elements to write to JSON</param>
        /// <param name="edgePropertyKeys">the keys of the edge elements to write to JSON</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public void OutputGraph(Stream jsonOutputStream, IEnumerable<string> vertexPropertyKeys,
                            IEnumerable<string> edgePropertyKeys, GraphSONMode mode)
        {
            Contract.Requires(jsonOutputStream != null);

            var sw = new StreamWriter(jsonOutputStream);
            var jg = new JsonTextWriter(sw);
            
            var graphson = new GraphSonUtility(mode, null, vertexPropertyKeys, edgePropertyKeys);

            jg.WriteStartObject();

            jg.WritePropertyName(GraphSonTokens.Mode);
            jg.WriteValue(mode.ToString());

            jg.WritePropertyName(GraphSonTokens.Vertices);
            jg.WriteStartArray();
            foreach (var v in _graph.GetVertices())
                jg.WriteRawValue(graphson.JsonFromElement(v).ToString());

            jg.WriteEndArray();

            jg.WritePropertyName(GraphSonTokens.Edges);
            jg.WriteStartArray();
            foreach (var e in _graph.GetEdges())
                jg.WriteRawValue(graphson.JsonFromElement(e).ToString());

            jg.WriteEndArray();

            jg.WriteEndObject();
            jg.Flush();
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream. All keys are written to JSON. Utilizing
        /// GraphSONMode.NORMAL.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="jsonOutputStream">the JSON OutputStream to write the Graph data to</param>
        public static void OutputGraph(IGraph graph, Stream jsonOutputStream)
        {
            Contract.Requires(graph != null);
            Contract.Requires(jsonOutputStream != null);

            var writer = new GraphSonWriter(graph);
            writer.OutputGraph(jsonOutputStream, null, null, GraphSONMode.NORMAL);
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream. All keys are written to JSON. Utilizing
        /// GraphSONMode.NORMAL.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="filename">the JSON file to write the Graph data to</param>
        public static void OutputGraph(IGraph graph, string filename)
        {
            Contract.Requires(graph != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));

            var writer = new GraphSonWriter(graph);
            writer.OutputGraph(filename, null, null, GraphSONMode.NORMAL);
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream. All keys are written to JSON.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="jsonOutputStream">the JSON OutputStream to write the Graph data to</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public static void OutputGraph(IGraph graph, Stream jsonOutputStream, GraphSONMode mode)
        {
            Contract.Requires(graph != null);
            Contract.Requires(jsonOutputStream != null);

            var writer = new GraphSonWriter(graph);
            writer.OutputGraph(jsonOutputStream, null, null, mode);
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream. All keys are written to JSON.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="filename">the JSON file to write the Graph data to</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public static void OutputGraph(IGraph graph, string filename, GraphSONMode mode)
        {
            Contract.Requires(graph != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));

            var writer = new GraphSonWriter(graph);
            writer.OutputGraph(filename, null, null, mode);
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="jsonOutputStream">the JSON OutputStream to write the Graph data to</param>
        /// <param name="vertexPropertyKeys">the keys of the vertex elements to write to JSON</param>
        /// <param name="edgePropertyKeys">the keys of the edge elements to write to JSON</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public static void OutputGraph(IGraph graph, Stream jsonOutputStream,
                                   IEnumerable<string> vertexPropertyKeys, IEnumerable<string> edgePropertyKeys,
                                   GraphSONMode mode)
        {
            Contract.Requires(graph != null);
            Contract.Requires(jsonOutputStream != null);

            var writer = new GraphSonWriter(graph);
            writer.OutputGraph(jsonOutputStream, vertexPropertyKeys, edgePropertyKeys, mode);
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="filename">the JSON file to write the Graph data to</param>
        /// <param name="vertexPropertyKeys">the keys of the vertex elements to write to JSON</param>
        /// <param name="edgePropertyKeys">the keys of the edge elements to write to JSON</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public static void OutputGraph(IGraph graph, string filename,
                                   IEnumerable<string> vertexPropertyKeys, IEnumerable<string> edgePropertyKeys,
                                   GraphSONMode mode)
        {
            Contract.Requires(graph != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));

            var writer = new GraphSonWriter(graph);
            writer.OutputGraph(filename, vertexPropertyKeys, edgePropertyKeys, mode);
        }
    }
}
