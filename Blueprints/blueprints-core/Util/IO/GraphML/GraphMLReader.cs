using System.Collections.Generic;
using System.IO;
using System.Xml;
using Frontenac.Blueprints.Util.Wrappers.Batch;
using System.Globalization;

namespace Frontenac.Blueprints.Util.IO.GraphML
{
    /// <summary>
    /// GraphMLReader writes the data from a GraphML stream to a graph.
    /// </summary>
    public class GraphMlReader
    {
        readonly IGraph _graph;

        string _vertexIdKey;
        string _edgeIdKey;
        string _edgeLabelKey;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph">the graph to populate with the GraphML data</param>
        public GraphMlReader(IGraph graph)
        {
            _graph = graph;
        }

        /// <summary>
        /// if the id of a vertex is a &lt;data/&gt; property, fetch it from the data property.
        /// </summary>
        /// <param name="vertexIdKey"></param>
        public void SetVertexIdKey(string vertexIdKey)
        {
            _vertexIdKey = vertexIdKey;
        }

        /// <summary>
        /// if the id of an edge is a &lt;data/&gt; property, fetch it from the data property.
        /// </summary>
        /// <param name="edgeIdKey"></param>
        public void SetEdgeIdKey(string edgeIdKey)
        {
            _edgeIdKey = edgeIdKey;
        }

        /// <summary>
        /// if the label of an edge is a &lt;data/&gt; property, fetch it from the data property.
        /// </summary>
        /// <param name="edgeLabelKey"></param>
        public void SetEdgeLabelKey(string edgeLabelKey)
        {
            _edgeLabelKey = edgeLabelKey;
        }

        /// <summary>
        /// Input the GraphML stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="graphMlInputStream">a Stream of GraphML data</param>
        public void InputGraph(Stream graphMlInputStream)
        {
            InputGraph(_graph, graphMlInputStream, 1000, _vertexIdKey, _edgeIdKey, _edgeLabelKey);
        }

        /// <summary>
        /// Input the GraphML stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="filename">name of a file containing GraphML data</param>
        public void InputGraph(string filename)
        {
            InputGraph(_graph, filename, 1000, _vertexIdKey, _edgeIdKey, _edgeLabelKey);
        }

        /// <summary>
        /// Input the GraphML stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="graphMlInputStream">a Stream of GraphML data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        public void InputGraph(Stream graphMlInputStream, int bufferSize)
        {
            InputGraph(_graph, graphMlInputStream, bufferSize, _vertexIdKey, _edgeIdKey, _edgeLabelKey);
        }

        /// <summary>
        /// Input the GraphML stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="filename">name of a file containing GraphML data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        public void InputGraph(string filename, int bufferSize)
        {
            InputGraph(_graph, filename, bufferSize, _vertexIdKey, _edgeIdKey, _edgeLabelKey);
        }

        /// <summary>
        /// Input the GraphML stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="inputGraph">the graph to populate with the GraphML data</param>
        /// <param name="graphMlInputStream">a Stream of GraphML data</param>
        public static void InputGraph(IGraph inputGraph, Stream graphMlInputStream)
        {
            InputGraph(inputGraph, graphMlInputStream, 1000, null, null, null);
        }

        /// <summary>
        /// Input the GraphML stream data into the graph.
        /// In practice, usually the provided graph is empty.
        /// </summary>
        /// <param name="inputGraph">the graph to populate with the GraphML data</param>
        /// <param name="filename">name of a file containing GraphML data</param>
        public static void InputGraph(IGraph inputGraph, string filename)
        {
            InputGraph(inputGraph, filename, 1000, null, null, null);
        }

        /// <summary>
        /// Input the GraphML stream data into the graph.
        /// More control over how data is streamed is provided by this method.
        /// </summary>
        /// <param name="inputGraph">the graph to populate with the GraphML data</param>
        /// <param name="filename">name of a file containing GraphML data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        /// <param name="vertexIdKey">if the id of a vertex is a &lt;data/&gt; property, fetch it from the data property.</param>
        /// <param name="edgeIdKey">if the id of an edge is a &lt;data/&gt; property, fetch it from the data property.</param>
        /// <param name="edgeLabelKey">if the label of an edge is a &lt;data/&gt; property, fetch it from the data property.</param>
        public static void InputGraph(IGraph inputGraph, string filename, int bufferSize, string vertexIdKey, string edgeIdKey, string edgeLabelKey)
        {
            using (var fis = File.OpenRead(filename))
            {
                InputGraph(inputGraph, fis, bufferSize, vertexIdKey, edgeIdKey, edgeLabelKey);
            }
        }

        /// <summary>
        /// Input the GraphML stream data into the graph.
        /// More control over how data is streamed is provided by this method.
        /// </summary>
        /// <param name="inputGraph">the graph to populate with the GraphML data</param>
        /// <param name="graphMlInputStream">a Stream of GraphML data</param>
        /// <param name="bufferSize">the amount of elements to hold in memory before committing a transactions (only valid for TransactionalGraphs)</param>
        /// <param name="vertexIdKey">if the id of a vertex is a &lt;data/&gt; property, fetch it from the data property.</param>
        /// <param name="edgeIdKey">if the id of an edge is a &lt;data/&gt; property, fetch it from the data property.</param>
        /// <param name="edgeLabelKey">if the label of an edge is a &lt;data/&gt; property, fetch it from the data property.</param>
        public static void InputGraph(IGraph inputGraph, Stream graphMlInputStream, int bufferSize, string vertexIdKey, string edgeIdKey, string edgeLabelKey)
        {
            using (XmlReader reader = XmlReader.Create(graphMlInputStream))
            {
                BatchGraph graph = BatchGraph.Wrap(inputGraph, bufferSize);

                var keyIdMap = new Dictionary<string, string>();
                var keyTypesMaps = new Dictionary<string, string>();
                // <Mapped ID string, ID object>

                // <Default ID string, Mapped ID string>
                var vertexMappedIdMap = new Dictionary<string, string>();

                // Buffered Vertex Data
                string vertexId = null;
                Dictionary<string, object> vertexProps = null;
                bool inVertex = false;

                // Buffered Edge Data
                string edgeId = null;
                string edgeLabel = null;
                IVertex[] edgeEndVertices = null; //[0] = outVertex , [1] = inVertex
                Dictionary<string, object> edgeProps = null;
                bool inEdge = false;

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        string elementName = reader.Name;

                        if (elementName == GraphMlTokens.Key)
                        {
                            string id = reader.GetAttribute(GraphMlTokens.Id);
                            string attributeName = reader.GetAttribute(GraphMlTokens.AttrName);
                            string attributeType = reader.GetAttribute(GraphMlTokens.AttrType);
                            keyIdMap[id] = attributeName;
                            keyTypesMaps[id] = attributeType;
                        }
                        else if (elementName == GraphMlTokens.Node)
                        {
                            vertexId = reader.GetAttribute(GraphMlTokens.Id);
                            if (vertexIdKey != null)
                                vertexMappedIdMap[vertexId] = vertexId;
                            inVertex = true;
                            vertexProps = new Dictionary<string, object>();
                        }
                        else if (elementName == GraphMlTokens.Edge)
                        {
                            edgeId = reader.GetAttribute(GraphMlTokens.Id);
                            edgeLabel = reader.GetAttribute(GraphMlTokens.Label);
                            edgeLabel = edgeLabel ?? GraphMlTokens.Default;

                            var vertexIds = new string[2];
                            vertexIds[0] = reader.GetAttribute(GraphMlTokens.Source);
                            vertexIds[1] = reader.GetAttribute(GraphMlTokens.Target);
                            edgeEndVertices = new IVertex[2];

                            for (int i = 0; i < 2; i++)
                            { //i=0 => outVertex, i=1 => inVertex
                                if (vertexIdKey == null)
                                {
                                    edgeEndVertices[i] = graph.GetVertex(vertexIds[i]);
                                }
                                else
                                {
                                    edgeEndVertices[i] = graph.GetVertex(vertexMappedIdMap.Get(vertexIds[i]));
                                }

                                if (null == edgeEndVertices[i])
                                {
                                    edgeEndVertices[i] = graph.AddVertex(vertexIds[i]);
                                    if (vertexIdKey != null)
                                        // Default to standard ID system (in case no mapped
                                        // ID is found later)
                                        vertexMappedIdMap[vertexIds[i]] = vertexIds[i];
                                }
                            }

                            inEdge = true;
                            edgeProps = new Dictionary<string, object>();
                        }
                        else if (elementName == GraphMlTokens.Data)
                        {
                            string key = reader.GetAttribute(GraphMlTokens.Key);
                            string attributeName = keyIdMap.Get(key);

                            if (attributeName != null)
                            {
                                reader.Read();
                                string value = reader.Value;

                                if (inVertex)
                                {
                                    if ((vertexIdKey != null) && (key == vertexIdKey))
                                    {
                                        // Should occur at most once per Vertex
                                        // Assumes single ID prop per Vertex
                                        vertexMappedIdMap[vertexId] = value;
                                        vertexId = value;
                                    }
                                    else
                                        vertexProps[attributeName] = TypeCastValue(key, value, keyTypesMaps);
                                }
                                else if (inEdge)
                                {
                                    if ((edgeLabelKey != null) && (key == edgeLabelKey))
                                        edgeLabel = value;
                                    else if ((edgeIdKey != null) && (key == edgeIdKey))
                                        edgeId = value;
                                    else
                                        edgeProps[attributeName] = TypeCastValue(key, value, keyTypesMaps);
                                }
                            }
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        string elementName = reader.Name;

                        if (elementName == GraphMlTokens.Node)
                        {
                            IVertex currentVertex = graph.GetVertex(vertexId) ?? graph.AddVertex(vertexId);

                            foreach (var prop in vertexProps)
                                currentVertex.SetProperty(prop.Key, prop.Value);

                            vertexId = null;
                            vertexProps = null;
                            inVertex = false;
                        }
                        else if (elementName == GraphMlTokens.Edge)
                        {
                            IEdge currentEdge = graph.AddEdge(edgeId, edgeEndVertices[0], edgeEndVertices[1], edgeLabel);

                            foreach (var prop in edgeProps)
                                currentEdge.SetProperty(prop.Key, prop.Value);

                            edgeId = null;
                            edgeLabel = null;
                            edgeEndVertices = null;
                            edgeProps = null;
                            inEdge = false;
                        }
                    }
                }

                graph.Commit();
            }
        }

        static object TypeCastValue(string key, string value, Dictionary<string, string> keyTypes)
        {
            string type = keyTypes.Get(key);
            if (null == type || type == GraphMlTokens.String)
                return value;
            if (type == GraphMlTokens.Float)
                return float.Parse(value, CultureInfo.InvariantCulture);
            if (type == GraphMlTokens.Int)
                return int.Parse(value, CultureInfo.InvariantCulture);
            if (type == GraphMlTokens.Double)
                return double.Parse(value, CultureInfo.InvariantCulture);
            if (type == GraphMlTokens.Boolean)
                return bool.Parse(value);
            if (type == GraphMlTokens.Long)
                return long.Parse(value, CultureInfo.InvariantCulture);
            return value;
        }
    }
}
