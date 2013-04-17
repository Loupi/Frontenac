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
        readonly Graph _Graph;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph">the Graph to pull the data from</param>
        public GraphSONWriter(Graph graph)
        {
            _Graph = graph;
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
            using (FileStream fos = File.Open(filename, FileMode.Create))
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
            using (StreamWriter sw = new StreamWriter(jsonOutputStream))
            {
                using (JsonTextWriter jg = new JsonTextWriter(sw))
                {
                    GraphSONUtility graphson = new GraphSONUtility(mode, null, vertexPropertyKeys, edgePropertyKeys);

                    jg.WriteStartObject();

                    jg.WritePropertyName(GraphSONTokens.MODE);
                    jg.WriteValue(mode.ToString());

                    jg.WritePropertyName(GraphSONTokens.VERTICES);
                    jg.WriteStartArray();
                    foreach (Vertex v in _Graph.GetVertices())
                        jg.WriteRawValue(graphson.ObjectNodeFromElement(v).ToString());

                    jg.WriteEndArray();

                    jg.WritePropertyName(GraphSONTokens.EDGES);
                    jg.WriteStartArray();
                    foreach (Edge e in _Graph.GetEdges())
                        jg.WriteRawValue(graphson.ObjectNodeFromElement(e).ToString());

                    jg.WriteEndArray();

                    jg.WriteEndObject();
                }
            }
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream. All keys are written to JSON. Utilizing
        /// GraphSONMode.NORMAL.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="jsonOutputStream">the JSON OutputStream to write the Graph data to</param>
        public static void OutputGraph(Graph graph, Stream jsonOutputStream)
        {
            GraphSONWriter writer = new GraphSONWriter(graph);
            writer.OutputGraph(jsonOutputStream, null, null, GraphSONMode.NORMAL);
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream. All keys are written to JSON. Utilizing
        /// GraphSONMode.NORMAL.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="filename">the JSON file to write the Graph data to</param>
        public static void OutputGraph(Graph graph, string filename)
        {
            GraphSONWriter writer = new GraphSONWriter(graph);
            writer.OutputGraph(filename, null, null, GraphSONMode.NORMAL);
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream. All keys are written to JSON.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="jsonOutputStream">the JSON OutputStream to write the Graph data to</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public static void OutputGraph(Graph graph, Stream jsonOutputStream, GraphSONMode mode)
        {
            GraphSONWriter writer = new GraphSONWriter(graph);
            writer.OutputGraph(jsonOutputStream, null, null, mode);
        }

        /// <summary>
        /// Write the data in a Graph to a JSON OutputStream. All keys are written to JSON.
        /// </summary>
        /// <param name="graph">the graph to serialize to JSON</param>
        /// <param name="filename">the JSON file to write the Graph data to</param>
        /// <param name="mode">determines the format of the GraphSON</param>
        public static void OutputGraph(Graph graph, string filename, GraphSONMode mode)
        {
            GraphSONWriter writer = new GraphSONWriter(graph);
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
        public static void OutputGraph(Graph graph, Stream jsonOutputStream,
                                   IEnumerable<string> vertexPropertyKeys, IEnumerable<string> edgePropertyKeys,
                                   GraphSONMode mode)
        {
            GraphSONWriter writer = new GraphSONWriter(graph);
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
        public static void OutputGraph(Graph graph, string filename,
                                   IEnumerable<string> vertexPropertyKeys, IEnumerable<string> edgePropertyKeys,
                                   GraphSONMode mode)
        {
            GraphSONWriter writer = new GraphSONWriter(graph);
            writer.OutputGraph(filename, vertexPropertyKeys, edgePropertyKeys, mode);
        }
    }
}
