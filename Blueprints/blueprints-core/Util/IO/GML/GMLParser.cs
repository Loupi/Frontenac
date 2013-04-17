using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO.GML
{
    class GMLParser
    {
        readonly Dictionary<object, object> _VertexMappedIdMap = new Dictionary<object, object>();
        readonly string _DefaultEdgeLabel;
        readonly Graph _Graph;
        readonly string _VertexIdKey;
        readonly string _EdgeIdKey;
        readonly string _EdgeLabelKey;
        bool _Directed = false;
        int _EdgeCount = 0;

        public GMLParser(Graph graph, string defaultEdgeLabel, string vertexIdKey, string edgeIdKey, string edgeLabelKey)
        {
            _Graph = graph;
            _VertexIdKey = vertexIdKey;
            _EdgeIdKey = edgeIdKey;
            _EdgeLabelKey = edgeLabelKey;
            _DefaultEdgeLabel = defaultEdgeLabel;
        }

        public void Parse(StreamTokenizer st)
        {
            while (HasNext(st))
            {
                int type = st.ttype;
                if (NotLineBreak(type))
                {
                    string value = st.StringValue;
                    if (GMLTokens.GRAPH == value)
                    {
                        ParseGraph(st);
                        if (!HasNext(st))
                            return;
                    }
                }
            }
            throw new IOException("Graph not complete");
        }

        void ParseGraph(StreamTokenizer st)
        {
            CheckValid(st, GMLTokens.GRAPH);
            while (HasNext(st))
            {
                // st.nextToken();
                int type = st.ttype;
                if (NotLineBreak(type))
                {
                    if (type == ']')
                        return;
                    else
                    {
                        string key = st.StringValue;
                        if (GMLTokens.NODE == key)
                            AddNode(ParseNode(st));
                        else if (GMLTokens.EDGE == key)
                            AddEdge(ParseEdge(st));
                        else if (GMLTokens.DIRECTED == key)
                            _Directed = ParseBoolean(st);
                        else
                        {
                            // IGNORE
                            ParseValue("ignore", st);
                        }
                    }
                }
            }
            throw new IOException("Graph not complete");
        }

        void AddNode(Dictionary<string, object> map)
        {
            object id = map.JavaRemove(GMLTokens.ID);
            if (id != null)
            {
                Vertex vertex = CreateVertex(map, id);
                AddProperties(vertex, map);
            }
            else
                throw new IOException("No id found for node");
        }

        Vertex CreateVertex(Dictionary<string, object> map, object id)
        {
            //final object vertexId = vertexIdKey == null ? (graph.getFeatures().ignoresSuppliedIds ? null : id) : map.remove(vertexIdKey);
            object vertexId = id;
            if (_VertexIdKey != null)
            {
                vertexId = map.JavaRemove(_VertexIdKey);
                if (vertexId == null) vertexId = id;
                _VertexMappedIdMap[id] = vertexId;
            }
            Vertex createdVertex = _Graph.AddVertex(vertexId);

            return createdVertex;
        }

        void AddEdge(Dictionary<string, object> map)
        {
            object source = map.JavaRemove(GMLTokens.SOURCE);
            object target = map.JavaRemove(GMLTokens.TARGET);

            if (source == null)
                throw new IOException("Edge has no source");

            if (target == null)
                throw new IOException("Edge has no target");

            if (_VertexIdKey != null)
            {
                _VertexMappedIdMap.TryGetValue(source, out source);
                _VertexMappedIdMap.TryGetValue(target, out target);
            }

            Vertex outVertex = _Graph.GetVertex(source);
            Vertex inVertex = _Graph.GetVertex(target);
            if (outVertex == null)
                throw new IOException(string.Concat("Edge source ", source, " not found"));

            if (inVertex == null)
                throw new IOException(string.Concat("Edge target ", target, " not found"));

            object label = map.JavaRemove(_EdgeLabelKey);
            if (label == null)
            {
                // try standard label key
                label = map.JavaRemove(GMLTokens.LABEL);
            }
            else
            {
                // remove label in case edge label key is not label
                // label is reserved and cannot be added as a property
                // if so this data will be lost
                map.Remove(GMLTokens.LABEL);
            }

            if (label == null)
                label = _DefaultEdgeLabel;

            object edgeId = _EdgeCount++;
            if (_EdgeIdKey != null)
            {
                object mappedKey = map.JavaRemove(_EdgeIdKey);
                if (mappedKey != null)
                    edgeId = mappedKey;
                // else use edgecount - could fail if mapped ids overlap with edge count
            }

            // remove id as reserved property - can be left is edgeIdKey in not id
            // This data will be lost
            map.Remove(GMLTokens.ID);

            Edge edge = _Graph.AddEdge(edgeId, outVertex, inVertex, label.ToString());
            if (_Directed)
                edge.SetProperty(GMLTokens.DIRECTED, _Directed);

            AddProperties(edge, map);
        }

        void AddProperties(Element element, Dictionary<string, object> map)
        {
            foreach (var entry in map)
                element.SetProperty(entry.Key, entry.Value);
        }

        object ParseValue(string key, StreamTokenizer st)
        {
            while (HasNext(st))
            {
                int type = st.ttype;
                if (NotLineBreak(type))
                {
                    if (type == StreamTokenizer.TT_NUMBER)
                    {

                        double doubleValue = st.NumberValue;
                        if (doubleValue == (Double)((int)doubleValue))
                            return (int)doubleValue;
                        else
                            return (float)doubleValue;
                    }
                    else
                    {
                        if (type == '[')
                            return ParseMap(key, st);
                        else if (type == '"')
                            return st.StringValue;
                    }
                }
            }
            throw new IOException("value not found");
        }

        bool ParseBoolean(StreamTokenizer st)
        {
            while (HasNext(st))
            {
                int type = st.ttype;
                if (NotLineBreak(type))
                {
                    if (type == StreamTokenizer.TT_NUMBER)
                        return st.NumberValue == 1.0;
                }
            }
            throw new IOException("boolean not found");
        }

        Dictionary<string, object> ParseNode(StreamTokenizer st)
        {
            return ParseElement(st, GMLTokens.NODE);
        }

        Dictionary<string, object> ParseEdge(StreamTokenizer st)
        {
            return ParseElement(st, GMLTokens.EDGE);
        }

        Dictionary<string, object> ParseElement(StreamTokenizer st, string node)
        {
            CheckValid(st, node);
            return ParseMap(node, st);
        }

        Dictionary<string, object> ParseMap(string node, StreamTokenizer st)
        {
            Dictionary<string, object> map = new Dictionary<string, object>();
            while (HasNext(st))
            {
                int type = st.ttype;
                if (NotLineBreak(type))
                {
                    if (type == ']')
                        return map;
                    else
                    {
                        string key = st.StringValue;
                        object value = ParseValue(key, st);
                        map[key] = value;
                    }
                }
            }
            throw new IOException(string.Concat(node, " incomplete"));
        }

        void CheckValid(StreamTokenizer st, string token)
        {
            if (st.NextToken() != '[')
                throw new IOException(string.Concat(token, " not followed by ["));
        }

        bool HasNext(StreamTokenizer st)
        {
            return st.NextToken() != StreamTokenizer.TT_EOF;
        }

        bool NotLineBreak(int type)
        {
            return type != StreamTokenizer.TT_EOL;
        }
    }
}
