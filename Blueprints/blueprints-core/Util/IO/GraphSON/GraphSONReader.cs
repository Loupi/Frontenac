using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Util.Wrappers.Batch;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    /// <summary>
    /// GraphSONReader reads the data from a TinkerPop JSON stream to a graph.
    /// </summary>
    public class GraphSONReader
    {
        readonly Graph _Graph;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph">the graph to populate with the JSON data</param>
        public GraphSONReader(Graph graph)
        {
            _Graph = graph;
        }

        /// <summary>
        /// Input the JSON stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="jsonInputStream">a Stream of JSON data</param>
        public void InputGraph(Stream jsonInputStream)
        {
            GraphSONReader.InputGraph(_Graph, jsonInputStream, 1000);
        }

        /// <summary>
        /// Input the JSON stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="filename">name of a file of JSON data</param>
        public void InputGraph(string filename)
        {
            GraphSONReader.InputGraph(_Graph, filename, 1000);
        }

        /// <summary>
        /// Input the JSON stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="jsonInputStream">an Stream of JSON data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        public void InputGraph(Stream jsonInputStream, int bufferSize)
        {
            GraphSONReader.InputGraph(_Graph, jsonInputStream, bufferSize);
        }

        /// <summary>
        /// Input the JSON stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="filename">name of a file of JSON data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        public void InputGraph(string filename, int bufferSize)
        {
            GraphSONReader.InputGraph(_Graph, filename, bufferSize);
        }

        /// <summary>
        /// Input the JSON stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="graph">the graph to populate with the JSON data</param>
        /// <param name="jsonInputStream">a Stream of JSON data</param>
        public static void InputGraph(Graph graph, Stream jsonInputStream)
        {
            InputGraph(graph, jsonInputStream, 1000);
        }

        /// <summary>
        /// Input the JSON stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="graph">the graph to populate with the JSON data</param>
        /// <param name="filename">name of a file of JSON data</param>
        public static void InputGraph(Graph graph, string filename)
        {
            InputGraph(graph, filename, 1000);
        }

        public static void InputGraph(Graph inputGraph, Stream jsonInputStream, int bufferSize)
        {
            InputGraph(inputGraph, jsonInputStream, bufferSize, null, null);
        }

        public static void InputGraph(Graph inputGraph, string filename, int bufferSize)
        {
            InputGraph(inputGraph, filename, bufferSize, null, null);
        }

        /// <summary>
        /// Input the JSON stream data into the graph.
        /// More control over how data is streamed is provided by this method.
        /// </summary>
        /// <param name="inputGraph">the graph to populate with the JSON data</param>
        /// <param name="filename">name of a file of JSON data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        public static void InputGraph(Graph inputGraph, string filename, int bufferSize,
                                  IEnumerable<string> edgePropertyKeys, IEnumerable<string> vertexPropertyKeys)
        {
            using (FileStream fis = File.OpenRead(filename))
            {
                GraphSONReader.InputGraph(inputGraph, fis, bufferSize, edgePropertyKeys, vertexPropertyKeys);
            }
        }

        /// <summary>
        /// Input the JSON stream data into the graph.
        /// More control over how data is streamed is provided by this method.
        /// </summary>
        /// <param name="inputGraph">the graph to populate with the JSON data</param>
        /// <param name="jsonInputStream">a Stream of JSON data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        public static void InputGraph(Graph inputGraph, Stream jsonInputStream, int bufferSize,
                                  IEnumerable<string> edgePropertyKeys, IEnumerable<string> vertexPropertyKeys)
        {

            using (StreamReader sr = new StreamReader(jsonInputStream))
            {
                using (JsonTextReader jp = new JsonTextReader(sr))
                {
                    // if this is a transactional graph then we're buffering
                    BatchGraph graph = BatchGraph.Wrap(inputGraph, bufferSize);

                    ElementFactory elementFactory = new GraphElementFactory(graph);
                    GraphSONUtility graphson = new GraphSONUtility(GraphSONMode.NORMAL, elementFactory,
                            vertexPropertyKeys, edgePropertyKeys);

                    while (jp.Read() && jp.TokenType != JsonToken.EndObject)
                    {
                        string fieldname = Convert.ToString(jp.Value);
                        if (fieldname == GraphSONTokens.MODE)
                        {
                            GraphSONMode mode = (GraphSONMode)Enum.Parse(typeof(GraphSONMode), jp.ReadAsString());
                            graphson = new GraphSONUtility(mode, elementFactory, vertexPropertyKeys, edgePropertyKeys);
                        }
                        else if (fieldname == GraphSONTokens.VERTICES)
                        {
                            jp.Read();
                            while (jp.Read() && jp.TokenType != JsonToken.EndArray)
                            {
                                JObject node = (JObject)JsonConvert.DeserializeObject(Convert.ToString(jp.Value));
                                graphson.VertexFromJson(node);
                            }
                        }
                        else if (fieldname == GraphSONTokens.EDGES)
                        {
                            jp.Read();
                            while (jp.Read() && jp.TokenType != JsonToken.EndArray)
                            {
                                JObject node = (JObject)JsonConvert.DeserializeObject(Convert.ToString(jp.Value));
                                Vertex inV = graph.GetVertex(GraphSONUtility.GetTypedValueFromJsonNode(node[GraphSONTokens._IN_V]));
                                Vertex outV = graph.GetVertex(GraphSONUtility.GetTypedValueFromJsonNode(node[GraphSONTokens._OUT_V]));
                                graphson.EdgeFromJson(node, outV, inV);
                            }
                        }
                    }

                    graph.Commit();
                }
            }
        }
    }
}
