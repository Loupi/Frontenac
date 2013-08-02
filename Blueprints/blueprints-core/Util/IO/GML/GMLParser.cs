using System;
using System.Collections.Generic;
using System.IO;

namespace Frontenac.Blueprints.Util.IO.GML
{
    class GmlParser
    {
        readonly Dictionary<object, object> _vertexMappedIdMap = new Dictionary<object, object>();
        readonly string _defaultEdgeLabel;
        readonly IGraph _graph;
        readonly string _vertexIdKey;
        readonly string _edgeIdKey;
        readonly string _edgeLabelKey;
        bool _directed;
        int _edgeCount;

        public GmlParser(IGraph graph, string defaultEdgeLabel, string vertexIdKey, string edgeIdKey, string edgeLabelKey)
        {
            _graph = graph;
            _vertexIdKey = vertexIdKey;
            _edgeIdKey = edgeIdKey;
            _edgeLabelKey = edgeLabelKey;
            _defaultEdgeLabel = defaultEdgeLabel;
        }

        public void Parse(StreamTokenizer st)
        {
            while (HasNext(st))
            {
                int type = st.Ttype;
                if (NotLineBreak(type))
                {
                    string value = st.StringValue;
                    if (GmlTokens.Graph == value)
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
            CheckValid(st, GmlTokens.Graph);
            while (HasNext(st))
            {
                // st.nextToken();
                int type = st.Ttype;
                if (NotLineBreak(type))
                {
                    if (type == ']')
                        return;
                    string key = st.StringValue;
                    if (GmlTokens.Node == key)
                        AddNode(ParseNode(st));
                    else if (GmlTokens.Edge == key)
                        AddEdge(ParseEdge(st));
                    else if (GmlTokens.Directed == key)
                        _directed = ParseBoolean(st);
                    else
                    {
                        // IGNORE
                        ParseValue("ignore", st);
                    }
                }
            }
            throw new IOException("Graph not complete");
        }

        void AddNode(Dictionary<string, object> map)
        {
            object id = map.JavaRemove(GmlTokens.Id);
            if (id != null)
            {
                IVertex vertex = CreateVertex(map, id);
                AddProperties(vertex, map);
            }
            else
                throw new IOException("No id found for node");
        }

        IVertex CreateVertex(Dictionary<string, object> map, object id)
        {
            //final object vertexId = vertexIdKey == null ? (graph.getFeatures().ignoresSuppliedIds ? null : id) : map.remove(vertexIdKey);
            object vertexId = id;
            if (_vertexIdKey != null)
            {
                vertexId = map.JavaRemove(_vertexIdKey) ?? id;
                _vertexMappedIdMap[id] = vertexId;
            }
            IVertex createdVertex = _graph.AddVertex(vertexId);

            return createdVertex;
        }

        void AddEdge(Dictionary<string, object> map)
        {
            object source = map.JavaRemove(GmlTokens.Source);
            object target = map.JavaRemove(GmlTokens.Target);

            if (source == null)
                throw new IOException("Edge has no source");

            if (target == null)
                throw new IOException("Edge has no target");

            if (_vertexIdKey != null)
            {
                _vertexMappedIdMap.TryGetValue(source, out source);
                _vertexMappedIdMap.TryGetValue(target, out target);
            }

            IVertex outVertex = _graph.GetVertex(source);
            IVertex inVertex = _graph.GetVertex(target);
            if (outVertex == null)
                throw new IOException(string.Concat("Edge source ", source, " not found"));

            if (inVertex == null)
                throw new IOException(string.Concat("Edge target ", target, " not found"));

            object label = map.JavaRemove(_edgeLabelKey);
            if (label == null)
            {
                // try standard label key
                label = map.JavaRemove(GmlTokens.Label);
            }
            else
            {
                // remove label in case edge label key is not label
                // label is reserved and cannot be added as a property
                // if so this data will be lost
                map.Remove(GmlTokens.Label);
            }

            if (label == null)
                label = _defaultEdgeLabel;

            object edgeId = _edgeCount++;
            if (_edgeIdKey != null)
            {
                object mappedKey = map.JavaRemove(_edgeIdKey);
                if (mappedKey != null)
                    edgeId = mappedKey;
                // else use edgecount - could fail if mapped ids overlap with edge count
            }

            // remove id as reserved property - can be left is edgeIdKey in not id
            // This data will be lost
            map.Remove(GmlTokens.Id);

            IEdge edge = _graph.AddEdge(edgeId, outVertex, inVertex, label.ToString());
            if (_directed)
                edge.SetProperty(GmlTokens.Directed, _directed);

            AddProperties(edge, map);
        }

        void AddProperties(IElement element, Dictionary<string, object> map)
        {
            foreach (var entry in map)
                element.SetProperty(entry.Key, entry.Value);
        }

        object ParseValue(string key, StreamTokenizer st)
        {
            while (HasNext(st))
            {
                int type = st.Ttype;
                if (NotLineBreak(type))
                {
                    if (type == StreamTokenizer.TtNumber)
                    {
                        int intVal;
                        if (int.TryParse(st.StringValue, out intVal))
                            return intVal;
                        return st.NumberValue;
                    }
                    if (type == '[')
                        return ParseMap(key, st);
                    if (type == '"')
                        return st.StringValue;
                }
            }
            throw new IOException("value not found");
        }

        bool ParseBoolean(StreamTokenizer st)
        {
            while (HasNext(st))
            {
                int type = st.Ttype;
                if (NotLineBreak(type))
                {
                    if (type == StreamTokenizer.TtNumber)
                        return st.NumberValue.CompareTo(1.0) == 0;
                }
            }
            throw new IOException("boolean not found");
        }

        Dictionary<string, object> ParseNode(StreamTokenizer st)
        {
            return ParseElement(st, GmlTokens.Node);
        }

        Dictionary<string, object> ParseEdge(StreamTokenizer st)
        {
            return ParseElement(st, GmlTokens.Edge);
        }

        Dictionary<string, object> ParseElement(StreamTokenizer st, string node)
        {
            CheckValid(st, node);
            return ParseMap(node, st);
        }

        Dictionary<string, object> ParseMap(string node, StreamTokenizer st)
        {
            var map = new Dictionary<string, object>();
            while (HasNext(st))
            {
                int type = st.Ttype;
                if (NotLineBreak(type))
                {
                    if (type == ']')
                        return map;
                    string key = st.StringValue;
                    object value = ParseValue(key, st);
                    map[key] = value;
                }
            }
            throw new IOException(string.Concat(node, " incomplete"));
        }

        static void CheckValid(StreamTokenizer st, string token)
        {
            if (st.NextToken() != '[')
                throw new IOException(string.Concat(token, " not followed by ["));
        }

        static bool HasNext(StreamTokenizer st)
        {
            return st.NextToken() != StreamTokenizer.TtEof;
        }

        static bool NotLineBreak(int type)
        {
            return type != StreamTokenizer.TtEol;
        }
    }
}
