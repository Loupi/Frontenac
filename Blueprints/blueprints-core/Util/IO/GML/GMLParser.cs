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
        readonly Dictionary<object, object> _vertexMappedIdMap = new Dictionary<object, object>();
        readonly string _defaultEdgeLabel;
        readonly Graph _graph;
        readonly string _vertexIdKey;
        readonly string _edgeIdKey;
        readonly string _edgeLabelKey;
        bool _directed = false;
        int _edgeCount = 0;

        public GMLParser(Graph graph, string defaultEdgeLabel, string vertexIdKey, string edgeIdKey, string edgeLabelKey)
        {
            _graph = graph;
            _vertexIdKey = vertexIdKey;
            _edgeIdKey = edgeIdKey;
            _edgeLabelKey = edgeLabelKey;
            _defaultEdgeLabel = defaultEdgeLabel;
        }

        public void parse(StreamTokenizer st)
        {
            while (hasNext(st))
            {
                int type = st.ttype;
                if (notLineBreak(type))
                {
                    string value = st.StringValue;
                    if (GMLTokens.GRAPH == value)
                    {
                        parseGraph(st);
                        if (!hasNext(st))
                            return;
                    }
                }
            }
            throw new IOException("Graph not complete");
        }

        void parseGraph(StreamTokenizer st)
        {
            checkValid(st, GMLTokens.GRAPH);
            while (hasNext(st))
            {
                // st.nextToken();
                int type = st.ttype;
                if (notLineBreak(type))
                {
                    if (type == ']')
                        return;
                    else
                    {
                        string key = st.StringValue;
                        if (GMLTokens.NODE == key)
                            addNode(parseNode(st));
                        else if (GMLTokens.EDGE == key)
                            addEdge(parseEdge(st));
                        else if (GMLTokens.DIRECTED == key)
                            _directed = parseBoolean(st);
                        else
                        {
                            // IGNORE
                            parseValue("ignore", st);
                        }
                    }
                }
            }
            throw new IOException("Graph not complete");
        }

        void addNode(Dictionary<string, object> map)
        {
            object id = map.javaRemove(GMLTokens.ID);
            if (id != null)
            {
                Vertex vertex = createVertex(map, id);
                addProperties(vertex, map);
            }
            else
                throw new IOException("No id found for node");
        }

        Vertex createVertex(Dictionary<string, object> map, object id)
        {
            //final object vertexId = vertexIdKey == null ? (graph.getFeatures().ignoresSuppliedIds ? null : id) : map.remove(vertexIdKey);
            object vertexId = id;
            if (_vertexIdKey != null)
            {
                vertexId = map.javaRemove(_vertexIdKey);
                if (vertexId == null) vertexId = id;
                _vertexMappedIdMap[id] = vertexId;
            }
            Vertex createdVertex = _graph.addVertex(vertexId);

            return createdVertex;
        }

        void addEdge(Dictionary<string, object> map)
        {
            object source = map.javaRemove(GMLTokens.SOURCE);
            object target = map.javaRemove(GMLTokens.TARGET);

            if (source == null)
                throw new IOException("Edge has no source");

            if (target == null)
                throw new IOException("Edge has no target");

            if (_vertexIdKey != null)
            {
                _vertexMappedIdMap.TryGetValue(source, out source);
                _vertexMappedIdMap.TryGetValue(target, out target);
            }

            Vertex outVertex = _graph.getVertex(source);
            Vertex inVertex = _graph.getVertex(target);
            if (outVertex == null)
                throw new IOException(string.Concat("Edge source ", source, " not found"));

            if (inVertex == null)
                throw new IOException(string.Concat("Edge target ", target, " not found"));

            object label = map.javaRemove(_edgeLabelKey);
            if (label == null)
            {
                // try standard label key
                label = map.javaRemove(GMLTokens.LABEL);
            }
            else
            {
                // remove label in case edge label key is not label
                // label is reserved and cannot be added as a property
                // if so this data will be lost
                map.Remove(GMLTokens.LABEL);
            }

            if (label == null)
                label = _defaultEdgeLabel;

            object edgeId = _edgeCount++;
            if (_edgeIdKey != null)
            {
                object mappedKey = map.javaRemove(_edgeIdKey);
                if (mappedKey != null)
                    edgeId = mappedKey;
                // else use edgecount - could fail if mapped ids overlap with edge count
            }

            // remove id as reserved property - can be left is edgeIdKey in not id
            // This data will be lost
            map.Remove(GMLTokens.ID);

            Edge edge = _graph.addEdge(edgeId, outVertex, inVertex, label.ToString());
            if (_directed)
                edge.setProperty(GMLTokens.DIRECTED, _directed);

            addProperties(edge, map);
        }

        void addProperties(Element element, Dictionary<string, object> map)
        {
            foreach (var entry in map)
                element.setProperty(entry.Key, entry.Value);
        }

        object parseValue(string key, StreamTokenizer st)
        {
            while (hasNext(st))
            {
                int type = st.ttype;
                if (notLineBreak(type))
                {
                    if (type == StreamTokenizer.TT_NUMBER)
                    {

                        double doubleValue = st.NumberValue;
                        if (doubleValue == (Double)((int)doubleValue))
                            return (int)doubleValue;
                        else
                            return doubleValue;
                    }
                    else
                    {
                        if (type == '[')
                            return parseMap(key, st);
                        else if (type == '"')
                            return st.StringValue;
                    }
                }
            }
            throw new IOException("value not found");
        }

        bool parseBoolean(StreamTokenizer st)
        {
            while (hasNext(st))
            {
                int type = st.ttype;
                if (notLineBreak(type))
                {
                    if (type == StreamTokenizer.TT_NUMBER)
                        return st.NumberValue == 1.0;
                }
            }
            throw new IOException("boolean not found");
        }

        Dictionary<string, object> parseNode(StreamTokenizer st)
        {
            return parseElement(st, GMLTokens.NODE);
        }

        Dictionary<string, object> parseEdge(StreamTokenizer st)
        {
            return parseElement(st, GMLTokens.EDGE);
        }

        Dictionary<string, object> parseElement(StreamTokenizer st, string node)
        {
            checkValid(st, node);
            return parseMap(node, st);
        }

        Dictionary<string, object> parseMap(string node, StreamTokenizer st)
        {
            Dictionary<string, object> map = new Dictionary<string, object>();
            while (hasNext(st))
            {
                int type = st.ttype;
                if (notLineBreak(type))
                {
                    if (type == ']')
                        return map;
                    else
                    {
                        string key = st.StringValue;
                        object value = parseValue(key, st);
                        map[key] = value;
                    }
                }
            }
            throw new IOException(string.Concat(node, " incomplete"));
        }

        void checkValid(StreamTokenizer st, string token)
        {
            if (st.NextToken() != '[')
                throw new IOException(string.Concat(token, " not followed by ["));
        }

        bool hasNext(StreamTokenizer st)
        {
            return st.NextToken() != StreamTokenizer.TT_EOF;
        }

        bool notLineBreak(int type)
        {
            return type != StreamTokenizer.TT_EOL;
        }
    }
}
