﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Util.Wrappers.Batch;

namespace Frontenac.Blueprints.Util.IO.GML
{
    /// <summary>
    /// A reader for the Graph Modelling Language (GML).
    /// <p/>
    /// (http://www.fim.uni-passau.de/fileadmin/files/lehrstuhl/brandenburg/projekte/gml/gml-documentation.tar.gz)
    /// <p/>
    /// It's not clear that all node have to have id's or that they have to be integers - we assume that this is the case. We
    /// also assume that only one graph can be defined in a file.
    /// </summary>
    public class GMLReader
    {
        public const string DEFAULT_LABEL = "undefined";
        const int DEFAULT_BUFFER_SIZE = 1000;

        Graph _Graph;
        readonly string _DefaultEdgeLabel;
        string _VertexIdKey;
        string _EdgeIdKey;
        string _EdgeLabelKey = GMLTokens.LABEL;

        /// <summary>
        /// Create a new GML reader
        /// <p/>
        /// (Uses default edge label DEFAULT_LABEL)
        /// </summary>
        /// <param name="graph">the graph to load data into</param>
        public GMLReader(Graph graph)
            : this(graph, DEFAULT_LABEL)
        {

        }

        /// <summary>
        /// Create a new GML reader
        /// </summary>
        /// <param name="graph">the graph to load data into</param>
        /// <param name="defaultEdgeLabel">the default edge label to be used if the GML edge does not define a label</param>
        public GMLReader(Graph graph, string defaultEdgeLabel)
        {
            _Graph = graph;
            _DefaultEdgeLabel = defaultEdgeLabel;
        }

        /// <summary>
        /// gml property to use as id for vertices
        /// </summary>
        /// <param name="vertexIdKey"></param>
        public void SetVertexIdKey(string vertexIdKey)
        {
            _VertexIdKey = vertexIdKey;
        }

        /// <summary>
        /// gml property to use as id for edges
        /// </summary>
        /// <param name="edgeIdKey"></param>
        public void SetEdgeIdKey(string edgeIdKey)
        {
            _EdgeIdKey = edgeIdKey;
        }

        /// <summary>
        /// gml property to assign edge Labels to
        /// </summary>
        /// <param name="edgeLabelKey"></param>
        public void SetEdgeLabelKey(string edgeLabelKey)
        {
            _EdgeLabelKey = edgeLabelKey;
        }

        /// <summary>
        /// Read the GML from from the stream.
        /// <p/>
        /// If the file is malformed incomplete data can be loaded.
        /// </summary>
        /// <param name="inputStream"></param>
        public void InputGraph(Stream inputStream)
        {
            GMLReader.InputGraph(_Graph, inputStream, DEFAULT_BUFFER_SIZE, _DefaultEdgeLabel,
                    _VertexIdKey, _EdgeIdKey, _EdgeLabelKey);
        }

        /// <summary>
        /// Read the GML from from the stream.
        /// <p/>
        /// If the file is malformed incomplete data can be loaded.
        /// </summary>
        /// <param name="filename"></param>
        public void InputGraph(string filename)
        {
            GMLReader.InputGraph(_Graph, filename, DEFAULT_BUFFER_SIZE, _DefaultEdgeLabel,
                    _VertexIdKey, _EdgeIdKey, _EdgeLabelKey);
        }

        /// <summary>
        /// Read the GML from from the stream.
        /// <p/>
        /// If the file is malformed incomplete data can be loaded.
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="bufferSize"></param>
        public void InputGraph(Stream inputStream, int bufferSize)
        {
            GMLReader.InputGraph(_Graph, inputStream, bufferSize, _DefaultEdgeLabel,
                    _VertexIdKey, _EdgeIdKey, _EdgeLabelKey);
        }

        /// <summary>
        /// Read the GML from from the stream.
        /// <p/>
        /// If the file is malformed incomplete data can be loaded.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="bufferSize"></param>
        public void InputGraph(string filename, int bufferSize)
        {
            GMLReader.InputGraph(_Graph, filename, bufferSize, _DefaultEdgeLabel,
                    _VertexIdKey, _EdgeIdKey, _EdgeLabelKey);
        }

        /// <summary>
        /// Load the GML file into the Graph.
        /// </summary>
        /// <param name="graph">to receive the data</param>
        /// <param name="filename">GML file</param>
        public static void InputGraph(Graph graph, string filename)
        {
            InputGraph(graph, filename, DEFAULT_BUFFER_SIZE, DEFAULT_LABEL, GMLTokens.BLUEPRINTS_ID, GMLTokens.BLUEPRINTS_ID, null);
        }

        /// <summary>
        /// Load the GML file into the Graph.
        /// </summary>
        /// <param name="graph">to receive the data</param>
        /// <param name="inputStream">GML file</param>
        public static void InputGraph(Graph graph, Stream inputStream)
        {
            InputGraph(graph, inputStream, DEFAULT_BUFFER_SIZE, DEFAULT_LABEL, GMLTokens.BLUEPRINTS_ID, GMLTokens.BLUEPRINTS_ID, null);
        }

        /// <summary>
        /// Load the GML file into the Graph.
        /// </summary>
        /// <param name="inputGraph">to receive the data</param>
        /// <param name="filename">GML file</param>
        /// <param name="bufferSize"></param>
        /// <param name="defaultEdgeLabel">default edge label to be used if not defined in the data</param>
        /// <param name="vertexIdKey">if the id of a vertex is a &lt;data/&gt; property, fetch it from the data property.</param>
        /// <param name="edgeIdKey">if the id of an edge is a &lt;data/&gt; property, fetch it from the data property.</param>
        /// <param name="edgeLabelKey">if the label of an edge is a &lt;data/&gt; property, fetch it from the data property.</param>
        public static void InputGraph(Graph inputGraph, string filename, int bufferSize,
                                      string defaultEdgeLabel, string vertexIdKey, string edgeIdKey,
                                      string edgeLabelKey)
        {
            using (var fis = File.OpenRead(filename))
            {
                GMLReader.InputGraph(inputGraph, fis, bufferSize, defaultEdgeLabel,
                        vertexIdKey, edgeIdKey, edgeLabelKey);
            }
        }

        public static void InputGraph(Graph inputGraph, Stream inputStream, int bufferSize,
                                  string defaultEdgeLabel, string vertexIdKey, string edgeIdKey,
                                  string edgeLabelKey)
        {
            BatchGraph graph = BatchGraph.Wrap(inputGraph, bufferSize);

            using (var r = new StreamReader(inputStream, Encoding.GetEncoding("ISO-8859-1")))
            {
                StreamTokenizer st = new StreamTokenizer(r);

                try
                {
                    st.CommentChar(GMLTokens.COMMENT_CHAR);
                    st.OrdinaryChar('[');
                    st.OrdinaryChar(']');

                    string stringCharacters = "/\\(){}<>!£$%^&*-+=,.?:;@_`|~";
                    for (int i = 0; i < stringCharacters.Length; i++)
                        st.WordChars(stringCharacters.ElementAt(i), stringCharacters.ElementAt(i));

                    new GMLParser(graph, defaultEdgeLabel, vertexIdKey, edgeIdKey, edgeLabelKey).Parse(st);

                    graph.Commit();
                }
                catch (IOException e)
                {
                    throw new IOException(string.Concat("GML malformed line number ", st.LineNumber, ": "), e);
                }
            }
        }
    }
}