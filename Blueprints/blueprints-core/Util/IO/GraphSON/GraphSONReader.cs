using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Frontenac.Blueprints.Util.Wrappers.Batch;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    /// <summary>
    /// GraphSONReader reads the data from a TinkerPop JSON stream to a graph.
    /// </summary>
    public class GraphSonReader
    {
        readonly IGraph _graph;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph">the graph to populate with the JSON data</param>
        public GraphSonReader(IGraph graph)
        {
            _graph = graph;
        }

        /// <summary>
        /// Input the JSON stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="jsonInputStream">a Stream of JSON data</param>
        public void InputGraph(Stream jsonInputStream)
        {
            InputGraph(_graph, jsonInputStream, 1000);
        }

        /// <summary>
        /// Input the JSON stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="filename">name of a file of JSON data</param>
        public void InputGraph(string filename)
        {
            InputGraph(_graph, filename, 1000);
        }

        /// <summary>
        /// Input the JSON stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="jsonInputStream">an Stream of JSON data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        public void InputGraph(Stream jsonInputStream, int bufferSize)
        {
            InputGraph(_graph, jsonInputStream, bufferSize);
        }

        /// <summary>
        /// Input the JSON stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="filename">name of a file of JSON data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        public void InputGraph(string filename, int bufferSize)
        {
            InputGraph(_graph, filename, bufferSize);
        }

        /// <summary>
        /// Input the JSON stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="graph">the graph to populate with the JSON data</param>
        /// <param name="jsonInputStream">a Stream of JSON data</param>
        public static void InputGraph(IGraph graph, Stream jsonInputStream)
        {
            InputGraph(graph, jsonInputStream, 1000);
        }

        /// <summary>
        /// Input the JSON stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="graph">the graph to populate with the JSON data</param>
        /// <param name="filename">name of a file of JSON data</param>
        public static void InputGraph(IGraph graph, string filename)
        {
            InputGraph(graph, filename, 1000);
        }

        public static void InputGraph(IGraph inputGraph, Stream jsonInputStream, int bufferSize)
        {
            InputGraph(inputGraph, jsonInputStream, bufferSize, null, null);
        }

        public static void InputGraph(IGraph inputGraph, string filename, int bufferSize)
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
        /// <param name="edgePropertyKeys"></param>
        /// <param name="vertexPropertyKeys"></param>
        public static void InputGraph(IGraph inputGraph, string filename, int bufferSize,
                                  IEnumerable<string> edgePropertyKeys, IEnumerable<string> vertexPropertyKeys)
        {
            using (FileStream fis = File.OpenRead(filename))
            {
                InputGraph(inputGraph, fis, bufferSize, edgePropertyKeys, vertexPropertyKeys);
            }
        }

        /// <summary>
        /// Input the JSON stream data into the graph.
        /// More control over how data is streamed is provided by this method.
        /// </summary>
        /// <param name="inputGraph">the graph to populate with the JSON data</param>
        /// <param name="jsonInputStream">a Stream of JSON data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        /// <param name="edgePropertyKeys"></param>
        /// <param name="vertexPropertyKeys"></param>
        public static void InputGraph(IGraph inputGraph, Stream jsonInputStream, int bufferSize,
                                  IEnumerable<string> edgePropertyKeys, IEnumerable<string> vertexPropertyKeys)
        {

            using (var sr = new StreamReader(jsonInputStream))
            {
                using (var jp = new JsonTextReader(sr))
                {
                    // if this is a transactional graph then we're buffering
                    BatchGraph graph = BatchGraph.Wrap(inputGraph, bufferSize);

                    IElementFactory elementFactory = new GraphElementFactory(graph);
                    
// ReSharper disable PossibleMultipleEnumeration
                    var graphson = new GraphSonUtility(GraphSonMode.NORMAL, elementFactory, vertexPropertyKeys, edgePropertyKeys);
// ReSharper restore PossibleMultipleEnumeration

                    var serializer = JsonSerializer.Create(null);

                    while (jp.Read() && jp.TokenType != JsonToken.EndObject)
                    {
                        string fieldname = Convert.ToString(jp.Value);
                        if (fieldname == GraphSonTokens.Mode)
                        {
                            var mode = (GraphSonMode)Enum.Parse(typeof(GraphSonMode), jp.ReadAsString());
// ReSharper disable PossibleMultipleEnumeration
                            graphson = new GraphSonUtility(mode, elementFactory, vertexPropertyKeys, edgePropertyKeys);
// ReSharper restore PossibleMultipleEnumeration
                        }
                        else if (fieldname == GraphSonTokens.Vertices)
                        {
                            jp.Read();
                            while (jp.Read() && jp.TokenType != JsonToken.EndArray)
                            {
                                var node = (JObject) serializer.Deserialize(jp);
                                graphson.VertexFromJson(node);
                            }
                        }
                        else if (fieldname == GraphSonTokens.Edges)
                        {
                            jp.Read();
                            while (jp.Read() && jp.TokenType != JsonToken.EndArray)
                            {
                                var node = (JObject)serializer.Deserialize(jp);
                                IVertex inV = graph.GetVertex(GraphSonUtility.GetTypedValueFromJsonNode(node[GraphSonTokens.InV]));
                                IVertex outV = graph.GetVertex(GraphSonUtility.GetTypedValueFromJsonNode(node[GraphSonTokens.OutV]));
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
