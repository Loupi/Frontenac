using System.IO;
using System.Linq;
using System.Text;
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
    public class GmlReader
    {
        public const string DefaultLabel = "undefined";
        const int DefaultBufferSize = 1000;

        readonly IGraph _graph;
        readonly string _defaultEdgeLabel;
        string _vertexIdKey;
        string _edgeIdKey;
        string _edgeLabelKey = GmlTokens.Label;

        /// <summary>
        /// Create a new GML reader
        /// <p/>
        /// (Uses default edge label DEFAULT_LABEL)
        /// </summary>
        /// <param name="graph">the graph to load data into</param>
        public GmlReader(IGraph graph)
            : this(graph, DefaultLabel)
        {

        }

        /// <summary>
        /// Create a new GML reader
        /// </summary>
        /// <param name="graph">the graph to load data into</param>
        /// <param name="defaultEdgeLabel">the default edge label to be used if the GML edge does not define a label</param>
        public GmlReader(IGraph graph, string defaultEdgeLabel)
        {
            _graph = graph;
            _defaultEdgeLabel = defaultEdgeLabel;
        }

        /// <summary>
        /// gml property to use as id for vertices
        /// </summary>
        /// <param name="vertexIdKey"></param>
        public void SetVertexIdKey(string vertexIdKey)
        {
            _vertexIdKey = vertexIdKey;
        }

        /// <summary>
        /// gml property to use as id for edges
        /// </summary>
        /// <param name="edgeIdKey"></param>
        public void SetEdgeIdKey(string edgeIdKey)
        {
            _edgeIdKey = edgeIdKey;
        }

        /// <summary>
        /// gml property to assign edge labels to
        /// </summary>
        /// <param name="edgeLabelKey"></param>
        public void SetEdgeLabelKey(string edgeLabelKey)
        {
            _edgeLabelKey = edgeLabelKey;
        }

        /// <summary>
        /// Read the GML from from the stream.
        /// <p/>
        /// If the file is malformed incomplete data can be loaded.
        /// </summary>
        /// <param name="inputStream"></param>
        public void InputGraph(Stream inputStream)
        {
            InputGraph(_graph, inputStream, DefaultBufferSize, _defaultEdgeLabel,
                    _vertexIdKey, _edgeIdKey, _edgeLabelKey);
        }

        /// <summary>
        /// Read the GML from from the stream.
        /// <p/>
        /// If the file is malformed incomplete data can be loaded.
        /// </summary>
        /// <param name="filename"></param>
        public void InputGraph(string filename)
        {
            InputGraph(_graph, filename, DefaultBufferSize, _defaultEdgeLabel,
                    _vertexIdKey, _edgeIdKey, _edgeLabelKey);
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
            InputGraph(_graph, inputStream, bufferSize, _defaultEdgeLabel,
                    _vertexIdKey, _edgeIdKey, _edgeLabelKey);
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
            InputGraph(_graph, filename, bufferSize, _defaultEdgeLabel,
                    _vertexIdKey, _edgeIdKey, _edgeLabelKey);
        }

        /// <summary>
        /// Load the GML file into the Graph.
        /// </summary>
        /// <param name="graph">to receive the data</param>
        /// <param name="filename">GML file</param>
        public static void InputGraph(IGraph graph, string filename)
        {
            InputGraph(graph, filename, DefaultBufferSize, DefaultLabel, GmlTokens.BlueprintsId, GmlTokens.BlueprintsId, null);
        }

        /// <summary>
        /// Load the GML file into the Graph.
        /// </summary>
        /// <param name="graph">to receive the data</param>
        /// <param name="inputStream">GML file</param>
        public static void InputGraph(IGraph graph, Stream inputStream)
        {
            InputGraph(graph, inputStream, DefaultBufferSize, DefaultLabel, GmlTokens.BlueprintsId, GmlTokens.BlueprintsId, null);
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
        public static void InputGraph(IGraph inputGraph, string filename, int bufferSize,
                                      string defaultEdgeLabel, string vertexIdKey, string edgeIdKey,
                                      string edgeLabelKey)
        {
            using (var fis = File.OpenRead(filename))
            {
                InputGraph(inputGraph, fis, bufferSize, defaultEdgeLabel,
                        vertexIdKey, edgeIdKey, edgeLabelKey);
            }
        }

        public static void InputGraph(IGraph inputGraph, Stream inputStream, int bufferSize,
                                  string defaultEdgeLabel, string vertexIdKey, string edgeIdKey,
                                  string edgeLabelKey)
        {
            BatchGraph graph = BatchGraph.Wrap(inputGraph, bufferSize);

            using (var r = new StreamReader(inputStream, Encoding.GetEncoding("ISO-8859-1")))
            {
                var st = new StreamTokenizer(r);

                try
                {
                    st.CommentChar(GmlTokens.CommentChar);
                    st.OrdinaryChar('[');
                    st.OrdinaryChar(']');

                    const string stringCharacters = "/\\(){}<>!£$%^&*-+=,.?:;@_`|~";
                    for (int i = 0; i < stringCharacters.Length; i++)
                        st.WordChars(stringCharacters.ElementAt(i), stringCharacters.ElementAt(i));

                    new GmlParser(graph, defaultEdgeLabel, vertexIdKey, edgeIdKey, edgeLabelKey).Parse(st);

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
