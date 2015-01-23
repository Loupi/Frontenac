using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using Frontenac.Blueprints.Util.Wrappers.Batch;

namespace Frontenac.Blueprints.Util.IO.GML
{
    /// <summary>
    ///     A reader for the Graph Modelling Language (GML).
    ///     <p />
    ///     (http://www.fim.uni-passau.de/fileadmin/files/lehrstuhl/brandenburg/projekte/gml/gml-documentation.tar.gz)
    ///     <p />
    ///     It's not clear that all node have to have id's or that they have to be integers - we assume that this is the case. We
    ///     also assume that only one graph can be defined in a file.
    /// </summary>
    public class GmlReader
    {
        public const string DefaultLabel = "undefined";
        private const int DefaultBufferSize = 1000;

        private readonly string _defaultEdgeLabel;
        private readonly IGraph _graph;
        private string _edgeLabelKey = GmlTokens.Label;

        /// <summary>
        ///     Create a new GML reader
        ///     <p />
        ///     (Uses default edge label DEFAULT_LABEL)
        /// </summary>
        /// <param name="graph">the graph to load data into</param>
        public GmlReader(IGraph graph)
            : this(graph, DefaultLabel)
        {
            Contract.Requires(graph != null);
        }

        /// <summary>
        ///     Create a new GML reader
        /// </summary>
        /// <param name="graph">the graph to load data into</param>
        /// <param name="defaultEdgeLabel">the default edge label to be used if the GML edge does not define a label</param>
        public GmlReader(IGraph graph, string defaultEdgeLabel)
        {
            Contract.Requires(graph != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(defaultEdgeLabel));

            _graph = graph;
            _defaultEdgeLabel = defaultEdgeLabel;
        }

        /// <summary>
        ///     gml property to use as id for vertices
        /// </summary>
        /// <value></value>
        public string VertexIdKey { get; set; }

        /// <summary>
        ///     gml property to use as id for edges
        /// </summary>
        /// <value></value>
        public string EdgeIdKey { get; set; }

        /// <summary>
        ///     gml property to assign edge labels to
        /// </summary>
        /// <value></value>
        public string EdgeLabelKey
        {
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));
                return _edgeLabelKey;
            }
            set
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(value));
                _edgeLabelKey = value;
            }
        }

        /// <summary>
        ///     Read the GML from from the stream.
        ///     <p />
        ///     If the file is malformed incomplete data can be loaded.
        /// </summary>
        /// <param name="inputStream"></param>
        public void InputGraph(Stream inputStream)
        {
            Contract.Requires(inputStream != null);

            InputGraph(_graph, inputStream, DefaultBufferSize, _defaultEdgeLabel,
                       VertexIdKey, EdgeIdKey, EdgeLabelKey);
        }

        /// <summary>
        ///     Read the GML from from the stream.
        ///     <p />
        ///     If the file is malformed incomplete data can be loaded.
        /// </summary>
        /// <param name="filename"></param>
        public void InputGraph(string filename)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));

            InputGraph(_graph, filename, DefaultBufferSize, _defaultEdgeLabel,
                       VertexIdKey, EdgeIdKey, EdgeLabelKey);
        }

        /// <summary>
        ///     Read the GML from from the stream.
        ///     <p />
        ///     If the file is malformed incomplete data can be loaded.
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="bufferSize"></param>
        public void InputGraph(Stream inputStream, int bufferSize)
        {
            Contract.Requires(inputStream != null);
            Contract.Requires(bufferSize > 0);

            InputGraph(_graph, inputStream, bufferSize, _defaultEdgeLabel,
                       VertexIdKey, EdgeIdKey, EdgeLabelKey);
        }

        /// <summary>
        ///     Read the GML from from the stream.
        ///     <p />
        ///     If the file is malformed incomplete data can be loaded.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="bufferSize"></param>
        public void InputGraph(string filename, int bufferSize)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));
            Contract.Requires(bufferSize > 0);

            InputGraph(_graph, filename, bufferSize, _defaultEdgeLabel,
                       VertexIdKey, EdgeIdKey, EdgeLabelKey);
        }

        /// <summary>
        ///     Load the GML file into the Graph.
        /// </summary>
        /// <param name="graph">to receive the data</param>
        /// <param name="filename">GML file</param>
        public static void InputGraph(IGraph graph, string filename)
        {
            Contract.Requires(graph != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));

            InputGraph(graph, filename, DefaultBufferSize, DefaultLabel, GmlTokens.BlueprintsId, GmlTokens.BlueprintsId,
                       null);
        }

        /// <summary>
        ///     Load the GML file into the Graph.
        /// </summary>
        /// <param name="graph">to receive the data</param>
        /// <param name="inputStream">GML file</param>
        public static void InputGraph(IGraph graph, Stream inputStream)
        {
            Contract.Requires(graph != null);
            Contract.Requires(inputStream != null);

            InputGraph(graph, inputStream, DefaultBufferSize, DefaultLabel, GmlTokens.BlueprintsId,
                       GmlTokens.BlueprintsId, null);
        }

        /// <summary>
        ///     Load the GML file into the Graph.
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
            Contract.Requires(inputGraph != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(filename));
            Contract.Requires(bufferSize > 0);
            Contract.Requires(!string.IsNullOrWhiteSpace(defaultEdgeLabel));

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
            Contract.Requires(inputGraph != null);
            Contract.Requires(inputStream != null);
            Contract.Requires(bufferSize > 0);
            Contract.Requires(!string.IsNullOrWhiteSpace(defaultEdgeLabel));

            var graph = BatchGraph.Wrap(inputGraph, bufferSize);

            using (var r = new StreamReader(inputStream, Encoding.GetEncoding("ISO-8859-1")))
            {
                var st = new StreamTokenizer(r);

                try
                {
                    st.CommentChar(GmlTokens.CommentChar);
                    st.OrdinaryChar('[');
                    st.OrdinaryChar(']');

                    const string stringCharacters = "/\\(){}<>!£$%^&*-+=,.?:;@_`|~";
                    for (var i = 0; i < stringCharacters.Length; i++)
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