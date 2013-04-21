using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    /// <summary>
    /// GraphSONWriter writes a Graph to a TinkerPop JSON OutputStream.
    /// </summary>
    public class GraphSONWriter
    {
        readonly Graph _graph;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        public GraphSONWriter(Graph graph)
        {
            _graph = graph;
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream.
        /// </summary>
        /// <param name="filename">the JSON file to write the Graph data to</param>
        /// <param name="vertexPropertyKeys">the keys of the vertex elements to write to JSON</param>
        /// <param name="edgePropertyKeys">the keys of the edge elements to write to JSON</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public void outputGraph(string filename, IEnumerable<string> vertexPropertyKeys,
                            IEnumerable<string> edgePropertyKeys, GraphSONMode mode)
        {
            using (FileStream fos = File.Open(filename, FileMode.Create))
            {
                outputGraph(fos, vertexPropertyKeys, edgePropertyKeys, mode);
            }
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream.
        /// </summary>
        /// <param name="jsonOutputStream">the JSON OutputStream to write the Graph data to</param>
        /// <param name="vertexPropertyKeys">the keys of the vertex elements to write to JSON</param>
        /// <param name="edgePropertyKeys">the keys of the edge elements to write to JSON</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public void outputGraph(Stream jsonOutputStream, IEnumerable<string> vertexPropertyKeys,
                            IEnumerable<string> edgePropertyKeys, GraphSONMode mode)
        {
            StreamWriter sw = new StreamWriter(jsonOutputStream);
            JsonTextWriter jg = new JsonTextWriter(sw);
            
            GraphSONUtility graphson = new GraphSONUtility(mode, null, vertexPropertyKeys, edgePropertyKeys);

            jg.WriteStartObject();

            jg.WritePropertyName(GraphSONTokens.MODE);
            jg.WriteValue(mode.ToString());

            jg.WritePropertyName(GraphSONTokens.VERTICES);
            jg.WriteStartArray();
            foreach (Vertex v in _graph.getVertices())
                jg.WriteRawValue(graphson.jsonFromElement(v).ToString());

            jg.WriteEndArray();

            jg.WritePropertyName(GraphSONTokens.EDGES);
            jg.WriteStartArray();
            foreach (Edge e in _graph.getEdges())
                jg.WriteRawValue(graphson.jsonFromElement(e).ToString());

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
        public static void outputGraph(Graph graph, Stream jsonOutputStream)
        {
            GraphSONWriter writer = new GraphSONWriter(graph);
            writer.outputGraph(jsonOutputStream, null, null, GraphSONMode.NORMAL);
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream. All keys are written to JSON. Utilizing
        /// GraphSONMode.NORMAL.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="filename">the JSON file to write the Graph data to</param>
        public static void outputGraph(Graph graph, string filename)
        {
            GraphSONWriter writer = new GraphSONWriter(graph);
            writer.outputGraph(filename, null, null, GraphSONMode.NORMAL);
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream. All keys are written to JSON.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="jsonOutputStream">the JSON OutputStream to write the Graph data to</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public static void outputGraph(Graph graph, Stream jsonOutputStream, GraphSONMode mode)
        {
            GraphSONWriter writer = new GraphSONWriter(graph);
            writer.outputGraph(jsonOutputStream, null, null, mode);
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream. All keys are written to JSON.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="filename">the JSON file to write the Graph data to</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public static void outputGraph(Graph graph, string filename, GraphSONMode mode)
        {
            GraphSONWriter writer = new GraphSONWriter(graph);
            writer.outputGraph(filename, null, null, mode);
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="jsonOutputStream">the JSON OutputStream to write the Graph data to</param>
        /// <param name="vertexPropertyKeys">the keys of the vertex elements to write to JSON</param>
        /// <param name="edgePropertyKeys">the keys of the edge elements to write to JSON</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public static void outputGraph(Graph graph, Stream jsonOutputStream,
                                   IEnumerable<string> vertexPropertyKeys, IEnumerable<string> edgePropertyKeys,
                                   GraphSONMode mode)
        {
            GraphSONWriter writer = new GraphSONWriter(graph);
            writer.outputGraph(jsonOutputStream, vertexPropertyKeys, edgePropertyKeys, mode);
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="filename">the JSON file to write the Graph data to</param>
        /// <param name="vertexPropertyKeys">the keys of the vertex elements to write to JSON</param>
        /// <param name="edgePropertyKeys">the keys of the edge elements to write to JSON</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public static void outputGraph(Graph graph, string filename,
                                   IEnumerable<string> vertexPropertyKeys, IEnumerable<string> edgePropertyKeys,
                                   GraphSONMode mode)
        {
            GraphSONWriter writer = new GraphSONWriter(graph);
            writer.outputGraph(filename, vertexPropertyKeys, edgePropertyKeys, mode);
        }
    }
}
