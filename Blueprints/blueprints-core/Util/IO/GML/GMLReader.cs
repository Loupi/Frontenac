using System;
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

        readonly Graph _graph;
        readonly string _defaultEdgeLabel;
        string _vertexIdKey;
        string _edgeIdKey;
        string _edgeLabelKey = GMLTokens.LABEL;

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
            _graph = graph;
            _defaultEdgeLabel = defaultEdgeLabel;
        }

        /// <summary>
        /// gml property to use as id for vertices
        /// </summary>
        /// <param name="vertexIdKey"></param>
        public void setVertexIdKey(string vertexIdKey)
        {
            _vertexIdKey = vertexIdKey;
        }

        /// <summary>
        /// gml property to use as id for edges
        /// </summary>
        /// <param name="edgeIdKey"></param>
        public void setEdgeIdKey(string edgeIdKey)
        {
            _edgeIdKey = edgeIdKey;
        }

        /// <summary>
        /// gml property to assign edge labels to
        /// </summary>
        /// <param name="edgeLabelKey"></param>
        public void setEdgeLabelKey(string edgeLabelKey)
        {
            _edgeLabelKey = edgeLabelKey;
        }

        /// <summary>
        /// Read the GML from from the stream.
        /// <p/>
        /// If the file is malformed incomplete data can be loaded.
        /// </summary>
        /// <param name="inputStream"></param>
        public void inputGraph(Stream inputStream)
        {
            GMLReader.inputGraph(_graph, inputStream, DEFAULT_BUFFER_SIZE, _defaultEdgeLabel,
                    _vertexIdKey, _edgeIdKey, _edgeLabelKey);
        }

        /// <summary>
        /// Read the GML from from the stream.
        /// <p/>
        /// If the file is malformed incomplete data can be loaded.
        /// </summary>
        /// <param name="filename"></param>
        public void inputGraph(string filename)
        {
            GMLReader.inputGraph(_graph, filename, DEFAULT_BUFFER_SIZE, _defaultEdgeLabel,
                    _vertexIdKey, _edgeIdKey, _edgeLabelKey);
        }

        /// <summary>
        /// Read the GML from from the stream.
        /// <p/>
        /// If the file is malformed incomplete data can be loaded.
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="bufferSize"></param>
        public void inputGraph(Stream inputStream, int bufferSize)
        {
            GMLReader.inputGraph(_graph, inputStream, bufferSize, _defaultEdgeLabel,
                    _vertexIdKey, _edgeIdKey, _edgeLabelKey);
        }

        /// <summary>
        /// Read the GML from from the stream.
        /// <p/>
        /// If the file is malformed incomplete data can be loaded.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="bufferSize"></param>
        public void inputGraph(string filename, int bufferSize)
        {
            GMLReader.inputGraph(_graph, filename, bufferSize, _defaultEdgeLabel,
                    _vertexIdKey, _edgeIdKey, _edgeLabelKey);
        }

        /// <summary>
        /// Load the GML file into the Graph.
        /// </summary>
        /// <param name="graph">to receive the data</param>
        /// <param name="filename">GML file</param>
        public static void inputGraph(Graph graph, string filename)
        {
            inputGraph(graph, filename, DEFAULT_BUFFER_SIZE, DEFAULT_LABEL, GMLTokens.BLUEPRINTS_ID, GMLTokens.BLUEPRINTS_ID, null);
        }

        /// <summary>
        /// Load the GML file into the Graph.
        /// </summary>
        /// <param name="graph">to receive the data</param>
        /// <param name="inputStream">GML file</param>
        public static void inputGraph(Graph graph, Stream inputStream)
        {
            inputGraph(graph, inputStream, DEFAULT_BUFFER_SIZE, DEFAULT_LABEL, GMLTokens.BLUEPRINTS_ID, GMLTokens.BLUEPRINTS_ID, null);
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
        public static void inputGraph(Graph inputGraph, string filename, int bufferSize,
                                      string defaultEdgeLabel, string vertexIdKey, string edgeIdKey,
                                      string edgeLabelKey)
        {
            using (var fis = File.OpenRead(filename))
            {
                GMLReader.inputGraph(inputGraph, fis, bufferSize, defaultEdgeLabel,
                        vertexIdKey, edgeIdKey, edgeLabelKey);
            }
        }

        public static void inputGraph(Graph inputGraph, Stream inputStream, int bufferSize,
                                  string defaultEdgeLabel, string vertexIdKey, string edgeIdKey,
                                  string edgeLabelKey)
        {
            BatchGraph graph = BatchGraph.wrap(inputGraph, bufferSize);

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

                    new GMLParser(graph, defaultEdgeLabel, vertexIdKey, edgeIdKey, edgeLabelKey).parse(st);

                    graph.commit();
                }
                catch (IOException e)
                {
                    throw new IOException(string.Concat("GML malformed line number ", st.LineNumber, ": "), e);
                }
            }
        }
    }
}
