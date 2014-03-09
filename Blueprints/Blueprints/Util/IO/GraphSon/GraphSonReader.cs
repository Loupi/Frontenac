﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using Frontenac.Blueprints.Util.Wrappers.Batch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    /// <summary>
    ///     GraphSONReader reads the data from a TinkerPop JSON stream to a graph.
    /// </summary>
    public class GraphSonReader
    {
        private readonly IGraph _graph;

        /// <summary>
        /// </summary>
        /// <param name="graph">the graph to populate with the JSON data</param>
        public GraphSonReader(IGraph graph)
        {
            Contract.Requires(graph != null);

            _graph = graph;
        }

        /// <summary>
        ///     Input the JSON stream data into the graph.
        ///     In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="jsonInputStream">a Stream of JSON data</param>
        public void InputGraph(Stream jsonInputStream)
        {
            Contract.Requires(jsonInputStream != null);

            InputGraph(_graph, jsonInputStream, 1000);
        }

        /// <summary>
        ///     Input the JSON stream data into the graph.
        ///     In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="filename">name of a file of JSON data</param>
        public void InputGraph(string filename)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));

            InputGraph(_graph, filename, 1000);
        }

        /// <summary>
        ///     Input the JSON stream data into the graph.
        ///     In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="jsonInputStream">an Stream of JSON data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        public void InputGraph(Stream jsonInputStream, int bufferSize)
        {
            Contract.Requires(jsonInputStream != null);
            Contract.Requires(bufferSize > 0);

            InputGraph(_graph, jsonInputStream, bufferSize);
        }

        /// <summary>
        ///     Input the JSON stream data into the graph.
        ///     In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="filename">name of a file of JSON data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        public void InputGraph(string filename, int bufferSize)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));
            Contract.Requires(bufferSize > 0);

            InputGraph(_graph, filename, bufferSize);
        }

        /// <summary>
        ///     Input the JSON stream data into the graph.
        ///     In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="graph">the graph to populate with the JSON data</param>
        /// <param name="jsonInputStream">a Stream of JSON data</param>
        public static void InputGraph(IGraph graph, Stream jsonInputStream)
        {
            Contract.Requires(graph != null);
            Contract.Requires(jsonInputStream != null);

            InputGraph(graph, jsonInputStream, 1000);
        }

        /// <summary>
        ///     Input the JSON stream data into the graph.
        ///     In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="graph">the graph to populate with the JSON data</param>
        /// <param name="filename">name of a file of JSON data</param>
        public static void InputGraph(IGraph graph, string filename)
        {
            Contract.Requires(graph != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));

            InputGraph(graph, filename, 1000);
        }

        public static void InputGraph(IGraph inputGraph, Stream jsonInputStream, int bufferSize)
        {
            Contract.Requires(inputGraph != null);
            Contract.Requires(jsonInputStream != null);
            Contract.Requires(bufferSize > 0);

            InputGraph(inputGraph, jsonInputStream, bufferSize, null, null);
        }

        public static void InputGraph(IGraph inputGraph, string filename, int bufferSize)
        {
            Contract.Requires(inputGraph != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));
            Contract.Requires(bufferSize > 0);

            InputGraph(inputGraph, filename, bufferSize, null, null);
        }

        /// <summary>
        ///     Input the JSON stream data into the graph.
        ///     More control over how data is streamed is provided by this method.
        /// </summary>
        /// <param name="inputGraph">the graph to populate with the JSON data</param>
        /// <param name="filename">name of a file of JSON data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        /// <param name="edgePropertyKeys"></param>
        /// <param name="vertexPropertyKeys"></param>
        public static void InputGraph(IGraph inputGraph, string filename, int bufferSize,
                                      IEnumerable<string> edgePropertyKeys, IEnumerable<string> vertexPropertyKeys)
        {
            Contract.Requires(inputGraph != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));
            Contract.Requires(bufferSize > 0);

            using (var fis = File.OpenRead(filename))
            {
                InputGraph(inputGraph, fis, bufferSize, edgePropertyKeys, vertexPropertyKeys);
            }
        }

        /// <summary>
        ///     Input the JSON stream data into the graph.
        ///     More control over how data is streamed is provided by this method.
        /// </summary>
        /// <param name="inputGraph">the graph to populate with the JSON data</param>
        /// <param name="jsonInputStream">a Stream of JSON data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        /// <param name="edgePropertyKeys"></param>
        /// <param name="vertexPropertyKeys"></param>
        public static void InputGraph(IGraph inputGraph, Stream jsonInputStream, int bufferSize,
                                      IEnumerable<string> edgePropertyKeys, IEnumerable<string> vertexPropertyKeys)
        {
            Contract.Requires(inputGraph != null);
            Contract.Requires(jsonInputStream != null);
            Contract.Requires(bufferSize > 0);

            StreamReader sr = null;

            try
            {
                sr = new StreamReader(jsonInputStream);

                using (var jp = new JsonTextReader(sr))
                {
                    sr = null;
                    // if this is a transactional graph then we're buffering
                    var graph = BatchGraph.Wrap(inputGraph, bufferSize);
                    var elementFactory = new GraphElementFactory(graph);

                    // ReSharper disable PossibleMultipleEnumeration
                    var graphson = new GraphSonUtility(GraphSonMode.NORMAL, elementFactory, vertexPropertyKeys,
                                                       edgePropertyKeys);
                    // ReSharper restore PossibleMultipleEnumeration

                    var serializer = JsonSerializer.Create(null);

                    while (jp.Read() && jp.TokenType != JsonToken.EndObject)
                    {
                        var fieldname = Convert.ToString(jp.Value);
                        switch (fieldname)
                        {
                            case GraphSonTokens.Mode:
                                {
                                    var mode = (GraphSonMode) Enum.Parse(typeof (GraphSonMode), jp.ReadAsString());
                                    // ReSharper disable PossibleMultipleEnumeration
                                    graphson = new GraphSonUtility(mode, elementFactory, vertexPropertyKeys,
                                                                   edgePropertyKeys);
                                    // ReSharper restore PossibleMultipleEnumeration
                                }
                                break;
                            case GraphSonTokens.Vertices:
                                jp.Read();
                                while (jp.Read() && jp.TokenType != JsonToken.EndArray)
                                {
                                    var node = (JObject) serializer.Deserialize(jp);
                                    graphson.VertexFromJson(node);
                                }
                                break;
                            case GraphSonTokens.Edges:
                                jp.Read();
                                while (jp.Read() && jp.TokenType != JsonToken.EndArray)
                                {
                                    var node = (JObject) serializer.Deserialize(jp);
                                    var idIn = GraphSonUtility.GetTypedValueFromJsonNode(node[GraphSonTokens.InV]);
                                    var idOut = GraphSonUtility.GetTypedValueFromJsonNode(node[GraphSonTokens.OutV]);
                                    if (idIn == null || idOut == null) continue;
                                    var inV = graph.GetVertex(idIn);
                                    var outV = graph.GetVertex(idOut);
                                    graphson.EdgeFromJson(node, outV, inV);
                                }
                                break;
                        }
                    }

                    graph.Commit();
                }
            }
            finally
            {
                if (sr != null)
                    sr.Dispose();
            }
        }
    }
}