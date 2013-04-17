using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    /// <summary>
    /// Helps write individual graph elements to TinkerPop JSON format known as GraphSON.
    /// </summary>
    public class GraphSONUtility
    {
        readonly GraphSONMode _Mode;
        readonly IEnumerable<string> _VertexPropertyKeys;
        readonly IEnumerable<string> _EdgePropertyKeys;
        readonly ElementFactory _Factory;
        readonly bool _HasEmbeddedTypes;
        readonly ElementPropertyConfig.ElementPropertiesRule _VertexPropertiesRule;
        readonly ElementPropertyConfig.ElementPropertiesRule _EdgePropertiesRule;

        readonly bool _IncludeReservedVertexId;
        readonly bool _IncludeReservedEdgeId;
        readonly bool _IncludeReservedVertexType;
        readonly bool _IncludeReservedEdgeType;
        readonly bool _IncludeReservedEdgeLabel;
        readonly bool _IncludeReservedEdgeOutV;
        readonly bool _IncludeReservedEdgeInV;

        /// <summary>
        /// A GraphSONUtiltiy that includes all properties of vertices and edges.
        /// </summary>
        public GraphSONUtility(GraphSONMode mode, ElementFactory factory)
            : this(mode, factory, ElementPropertyConfig.AllProperties)
        {

        }

        /// <summary>
        /// A GraphSONUtility that includes the specified properties.
        /// </summary>
        public GraphSONUtility(GraphSONMode mode, ElementFactory factory,
                           IEnumerable<string> vertexPropertyKeys, IEnumerable<string> edgePropertyKeys) :
            this(mode, factory, ElementPropertyConfig.IncludeProperties(vertexPropertyKeys, edgePropertyKeys))
        {

        }

        public GraphSONUtility(GraphSONMode mode, ElementFactory factory, ElementPropertyConfig config)
        {
            _VertexPropertyKeys = config.GetVertexPropertyKeys();
            _EdgePropertyKeys = config.GetEdgePropertyKeys();
            _VertexPropertiesRule = config.GetVertexPropertiesRule();
            _EdgePropertiesRule = config.GetEdgePropertiesRule();

            _Mode = mode;
            _Factory = factory;
            _HasEmbeddedTypes = mode == GraphSONMode.EXTENDED;

            _IncludeReservedVertexId = IncludeReservedKey(mode, GraphSONTokens._ID, _VertexPropertyKeys, _VertexPropertiesRule);
            _IncludeReservedEdgeId = IncludeReservedKey(mode, GraphSONTokens._ID, _EdgePropertyKeys, _EdgePropertiesRule);
            _IncludeReservedVertexType = IncludeReservedKey(mode, GraphSONTokens._TYPE, _VertexPropertyKeys, _VertexPropertiesRule);
            _IncludeReservedEdgeType = IncludeReservedKey(mode, GraphSONTokens._TYPE, _EdgePropertyKeys, _EdgePropertiesRule);
            _IncludeReservedEdgeLabel = IncludeReservedKey(mode, GraphSONTokens._LABEL, _EdgePropertyKeys, _EdgePropertiesRule);
            _IncludeReservedEdgeOutV = IncludeReservedKey(mode, GraphSONTokens._OUT_V, _EdgePropertyKeys, _EdgePropertiesRule);
            _IncludeReservedEdgeInV = IncludeReservedKey(mode, GraphSONTokens._IN_V, _EdgePropertyKeys, _EdgePropertiesRule);
        }

        /// <summary>
        /// Creates a vertex from GraphSON using settings supplied in the constructor.
        /// </summary>
        public Vertex VertexFromJson(string json)
        {
            JObject node = (JObject)JsonConvert.DeserializeObject(json);
            return VertexFromJson(node);
        }

        /// <summary>
        /// Creates a vertex from GraphSON using settings supplied in the constructor.
        /// </summary>
        public Vertex VertexFromJson(Stream json)
        {
            using (StreamReader reader = new StreamReader(json))
            {
                JObject node = (JObject)new JsonSerializer().Deserialize(reader, typeof(object));
                return VertexFromJson(node);
            }
        }

        /// <summary>
        /// Creates a vertex from GraphSON using settings supplied in the constructor.
        /// </summary>
        public Vertex VertexFromJson(JObject json)
        {
            Dictionary<string, object> props = ReadProperties(json, true, _HasEmbeddedTypes);

            object vertexId = GetTypedValueFromJsonNode(json[GraphSONTokens._ID]);
            Vertex v = _Factory.CreateVertex(vertexId);

            foreach (var entry in props)
            {
                //if (this.vertexPropertyKeys == null || vertexPropertyKeys.contains(entry.getKey())) {
                if (IncludeKey(entry.Key, _VertexPropertyKeys, _VertexPropertiesRule))
                    v.SetProperty(entry.Key, entry.Value);
            }

            return v;
        }

        /// <summary>
        /// Creates an edge from GraphSON using settings supplied in the constructor.
        /// </summary>
        public Edge EdgeFromJson(string json, Vertex out_, Vertex in_)
        {
            JObject node = (JObject)JsonConvert.DeserializeObject(json);
            return EdgeFromJson(node, out_, in_);
        }

        public Edge EdgeFromJson(Stream json, Vertex out_, Vertex in_)
        {
            using (StreamReader reader = new StreamReader(json))
            {
                JObject node = (JObject)new JsonSerializer().Deserialize(reader, typeof(object));
                return EdgeFromJson(node, out_, in_);
            }
        }

        /// <summary>
        /// Creates an edge from GraphSON using settings supplied in the constructor.
        /// </summary>
        public Edge EdgeFromJson(JObject json, Vertex out_, Vertex in_)
        {
            Dictionary<string, object> props = GraphSONUtility.ReadProperties(json, true, _HasEmbeddedTypes);

            object edgeId = GetTypedValueFromJsonNode(json[GraphSONTokens._ID]);
            var nodeLabel = json[GraphSONTokens._LABEL];
            string label = nodeLabel == null ? null : nodeLabel.Value<string>();

            Edge e = _Factory.CreateEdge(edgeId, out_, in_, label);

            foreach (var entry in props)
            {
                // if (this.edgePropertyKeys == null || this.edgePropertyKeys.contains(entry.getKey())) {
                if (IncludeKey(entry.Key, _EdgePropertyKeys, _EdgePropertiesRule))
                    e.SetProperty(entry.Key, entry.Value);
            }

            return e;
        }

        /// <summary>
        /// Creates GraphSON for a single graph element.
        /// </summary>
        public JObject ObjectNodeFromElement(Element element)
        {
            bool isEdge = element is Edge;
            bool showTypes = _Mode == GraphSONMode.EXTENDED;
            IEnumerable<string> propertyKeys = isEdge ? _EdgePropertyKeys : _VertexPropertyKeys;
            ElementPropertyConfig.ElementPropertiesRule elementPropertyConfig = isEdge ? _EdgePropertiesRule : _VertexPropertiesRule;

            JObject jsonElement = CreateJSONMap(CreatePropertyMap(element, propertyKeys, elementPropertyConfig), propertyKeys, showTypes);

            if ((isEdge && _IncludeReservedEdgeId) || (!isEdge && _IncludeReservedVertexId))
                PutObject(jsonElement, GraphSONTokens._ID, element.GetId());

            // it's important to keep the order of these straight. check Edge first and then Vertex because there
            // are graph implementations that have Edge extend from Vertex
            if (element is Edge)
            {
                Edge edge = element as Edge;

                if (_IncludeReservedEdgeId)
                    PutObject(jsonElement, GraphSONTokens._ID, element.GetId());

                if (_IncludeReservedEdgeType)
                    jsonElement.Add(GraphSONTokens._TYPE, GraphSONTokens.EDGE);

                if (_IncludeReservedEdgeOutV)
                    PutObject(jsonElement, GraphSONTokens._OUT_V, edge.GetVertex(Direction.OUT).GetId());

                if (_IncludeReservedEdgeInV)
                    PutObject(jsonElement, GraphSONTokens._IN_V, edge.GetVertex(Direction.IN).GetId());

                if (_IncludeReservedEdgeLabel)
                    jsonElement.Add(GraphSONTokens._LABEL, edge.GetLabel());
            }
            else if (element is Vertex)
            {
                if (_IncludeReservedVertexId)
                    PutObject(jsonElement, GraphSONTokens._ID, element.GetId());

                if (_IncludeReservedVertexType)
                    jsonElement.Add(GraphSONTokens._TYPE, GraphSONTokens.VERTEX);
            }

            return jsonElement;
        }

        /// <summary>
        /// Reads an individual Vertex from JSON.  The vertex must match the accepted GraphSON format.
        /// </summary>
        /// <param name="json">a single vertex in GraphSON format as a string.</param>
        /// <param name="factory">the factory responsible for constructing graph elements</param>
        /// <param name="mode">the mode of the GraphSON</param>
        /// <param name="propertyKeys">a list of keys to include on reading of element properties</param>
        public static Vertex VertexFromJson(string json, ElementFactory factory, GraphSONMode mode,
                                        IEnumerable<string> propertyKeys)
        {
            GraphSONUtility graphson = new GraphSONUtility(mode, factory, propertyKeys, null);
            return graphson.VertexFromJson(json);
        }

        /// <summary>
        /// Reads an individual Vertex from JSON.  The vertex must match the accepted GraphSON format.
        /// </summary>
        /// <param name="json">a single vertex in GraphSON format as a Stream.</param>
        /// <param name="factory">the factory responsible for constructing graph elements</param>
        /// <param name="mode">the mode of the GraphSON</param>
        /// <param name="propertyKeys">a list of keys to include on reading of element properties</param>
        public static Vertex VertexFromJson(Stream json, ElementFactory factory, GraphSONMode mode,
                                        IEnumerable<string> propertyKeys)
        {
            GraphSONUtility graphson = new GraphSONUtility(mode, factory, propertyKeys, null);
            return graphson.VertexFromJson(json);
        }

        /// <summary>
        /// Reads an individual Vertex from JSON.  The vertex must match the accepted GraphSON format.
        /// </summary>
        /// <param name="json">a single vertex in GraphSON format as Jackson JsonNode</param>
        /// <param name="factory">the factory responsible for constructing graph elements</param>
        /// <param name="mode">the mode of the GraphSON</param>
        /// <param name="propertyKeys">a list of keys to include on reading of element properties</param>
        public static Vertex VertexFromJson(JObject json, ElementFactory factory, GraphSONMode mode,
                                        IEnumerable<string> propertyKeys)
        {
            GraphSONUtility graphson = new GraphSONUtility(mode, factory, propertyKeys, null);
            return graphson.VertexFromJson(json);
        }

        /// <summary>
        /// Reads an individual Edge from JSON.  The edge must match the accepted GraphSON format.
        /// </summary>
        /// <param name="json">a single edge in GraphSON format as a string</param>
        /// <param name="out_"></param>
        /// <param name="in_"></param>
        /// <param name="factory">the factory responsible for constructing graph elements</param>
        /// <param name="mode">the mode of the GraphSON</param>
        /// <param name="propertyKeys">a list of keys to include when reading of element properties</param>
        public static Edge EdgeFromJson(string json, Vertex out_, Vertex in_,
                                    ElementFactory factory, GraphSONMode mode,
                                    IEnumerable<string> propertyKeys)
        {
            GraphSONUtility graphson = new GraphSONUtility(mode, factory, null, propertyKeys);
            return graphson.EdgeFromJson(json, out_, in_);
        }

        /// <summary>
        /// Reads an individual Edge from JSON.  The edge must match the accepted GraphSON format.
        /// </summary>
        /// <param name="json">a single edge in GraphSON format as a Stream</param>
        /// <param name="out_"></param>
        /// <param name="in_"></param>
        /// <param name="factory">the factory responsible for constructing graph elements</param>
        /// <param name="mode">the mode of the GraphSON</param>
        /// <param name="propertyKeys">a list of keys to include when reading of element properties</param>
        public static Edge EdgeFromJson(Stream json, Vertex out_, Vertex in_,
                                        ElementFactory factory, GraphSONMode mode,
                                        IEnumerable<string> propertyKeys)
        {
            GraphSONUtility graphson = new GraphSONUtility(mode, factory, null, propertyKeys);
            return graphson.EdgeFromJson(json, out_, in_);
        }

        /// <summary>
        /// Reads an individual Edge from JSON.  The edge must match the accepted GraphSON format.
        /// </summary>
        /// <param name="json">a single edge in GraphSON format as a Stream</param>
        /// <param name="out_"></param>
        /// <param name="in_"></param>
        /// <param name="factory">the factory responsible for constructing graph elements</param>
        /// <param name="mode">the mode of the GraphSON</param>
        /// <param name="propertyKeys">a list of keys to include when reading of element properties</param>
        public static Edge EdgeFromJson(JObject json, Vertex out_, Vertex in_,
                                        ElementFactory factory, GraphSONMode mode,
                                        IEnumerable<string> propertyKeys)
        {
            GraphSONUtility graphson = new GraphSONUtility(mode, factory, null, propertyKeys);
            return graphson.EdgeFromJson(json, out_, in_);
        }

        /// <summary>
        /// Creates a Jackson ObjectNode from a graph element.
        /// </summary>
        /// <param name="element">the graph element to convert to JSON.</param>
        /// <param name="propertyKeys">The property keys at the root of the element to serialize.  If null, then all keys are serialized.</param>
        /// <param name="mode">The type of GraphSON to generate.</param>
        public static JObject ObjectNodeFromElement(Element element, IEnumerable<string> propertyKeys, GraphSONMode mode)
        {
            GraphSONUtility graphson = element is Edge ? new GraphSONUtility(mode, null, null, propertyKeys)
                    : new GraphSONUtility(mode, null, propertyKeys, null);
            return graphson.ObjectNodeFromElement(element);
        }

        static Dictionary<string, object> ReadProperties(JObject node, bool ignoreReservedKeys, bool hasEmbeddedTypes)
        {
            Dictionary<string, object> map = new Dictionary<string, object>();

            foreach (var entry in node)
            {
                if (!ignoreReservedKeys || !IsReservedKey(entry.Key))
                    map.Add(entry.Key, ReadProperty(entry.Value, hasEmbeddedTypes));
            }

            return map;
        }

        static bool IncludeReservedKey(GraphSONMode mode, string key, IEnumerable<string> propertyKeys,
                                       ElementPropertyConfig.ElementPropertiesRule rule)
        {
            // the key is always included in modes other than compact. if it is compact, then validate that the
            // key is in the property key list
            return mode != GraphSONMode.COMPACT || IncludeKey(key, propertyKeys, rule);
        }

        static bool IncludeKey(string key, IEnumerable<string> propertyKeys, ElementPropertyConfig.ElementPropertiesRule rule)
        {
            if (propertyKeys == null)
            {
                // when null always include the key and shortcut this piece
                return true;
            }

            // default the key situation. if it's included then it should be explicitly defined in the
            // property keys list to be included or the reverse otherwise
            bool keySituation = rule == ElementPropertyConfig.ElementPropertiesRule.INCLUDE;

            switch (rule)
            {
                case ElementPropertyConfig.ElementPropertiesRule.INCLUDE:
                    keySituation = propertyKeys.Contains(key);
                    break;
                case ElementPropertyConfig.ElementPropertiesRule.EXCLUDE:
                    keySituation = !propertyKeys.Contains(key);
                    break;
            }

            return keySituation;
        }

        static bool IsReservedKey(string key)
        {
            return key == GraphSONTokens._ID || key == GraphSONTokens._TYPE || key == GraphSONTokens._LABEL
                    || key == GraphSONTokens._OUT_V || key == GraphSONTokens._IN_V;
        }

        static object ReadProperty(JToken node, bool hasEmbeddedTypes)
        {
            object propertyValue;

            if (hasEmbeddedTypes)
            {
                string type = node[GraphSONTokens.TYPE].Value<string>();

                if (type == GraphSONTokens.TYPE_UNKNOWN)
                    propertyValue = null;
                else if (type == GraphSONTokens.TYPE_BOOLEAN)
                    propertyValue = node[GraphSONTokens.VALUE].Value<bool>();
                else if (type == GraphSONTokens.TYPE_FLOAT)
                    propertyValue = float.Parse(node[GraphSONTokens.VALUE].Value<string>(), CultureInfo.InvariantCulture);
                else if (type == GraphSONTokens.TYPE_DOUBLE)
                    propertyValue = node[GraphSONTokens.VALUE].Value<double>();
                else if (type == GraphSONTokens.TYPE_INTEGER)
                    propertyValue = node[GraphSONTokens.VALUE].Value<int>();
                else if (type == GraphSONTokens.TYPE_LONG)
                    propertyValue = node[GraphSONTokens.VALUE].Value<long>();
                else if (type == GraphSONTokens.TYPE_STRING)
                    propertyValue = node[GraphSONTokens.VALUE].Value<string>();
                else if (type == GraphSONTokens.TYPE_LIST)
                    propertyValue = ReadProperties(node[GraphSONTokens.VALUE].Values(), hasEmbeddedTypes);
                else if (type == GraphSONTokens.TYPE_MAP)
                    propertyValue = ReadProperties(node[GraphSONTokens.VALUE] as JObject, false, hasEmbeddedTypes);
                else
                    propertyValue = node.Value<string>();

            }
            else
            {
                if (node.Type == JTokenType.Null)
                    propertyValue = null;
                else if (node.Type == JTokenType.Boolean)
                    propertyValue = node.Value<bool>();
                else if (node.Type == JTokenType.Float)
                    propertyValue = node.Value<double>();
                else if (node.Type == JTokenType.Integer)
                    propertyValue = node.Value<long>();
                else if (node.Type == JTokenType.String)
                    propertyValue = node.Value<string>();
                else if (node.Type == JTokenType.Array)
                    propertyValue = ReadProperties(node.Values(), hasEmbeddedTypes);
                else if (node.Type == JTokenType.Object)
                    propertyValue = ReadProperties(node as JObject, false, hasEmbeddedTypes);
                else
                    propertyValue = node.Value<string>();
            }

            return propertyValue;
        }

        static IEnumerable ReadProperties(IJEnumerable<JToken> listOfNodes, bool hasEmbeddedTypes)
        {
            return listOfNodes.Select(t => ReadProperty(t, hasEmbeddedTypes)).ToArray();
        }

        static JArray CreateJSONList(IEnumerable list, IEnumerable<string> propertyKeys, bool showTypes)
        {
            JArray jsonList = new JArray();
            foreach (object item in list)
            {
                if (item is Element)
                    jsonList.Add(ObjectNodeFromElement(item as Element, propertyKeys, showTypes ? GraphSONMode.EXTENDED : GraphSONMode.NORMAL));
                else if (item is IDictionary)
                    jsonList.Add(CreateJSONMap(item as IDictionary, propertyKeys, showTypes));
                else if (item != null && item is IEnumerable)
                    jsonList.Add(CreateJSONList(item as IEnumerable, propertyKeys, showTypes));
                else
                    AddObject(jsonList, item);
            }
            return jsonList;
        }

        static JObject CreateJSONMap(IDictionary map, IEnumerable<string> propertyKeys, bool showTypes)
        {
            JObject jsonMap = new JObject();
            foreach (DictionaryEntry entry in map)
            {
                object value = entry.Value;
                if (value != null)
                {
                    if (value is IDictionary)
                        value = CreateJSONMap(value as IDictionary, propertyKeys, showTypes);
                    else if (value is Element)
                        value = ObjectNodeFromElement((Element)value, propertyKeys,
                                showTypes ? GraphSONMode.EXTENDED : GraphSONMode.NORMAL);
                    else if (value is IEnumerable)
                        value = CreateJSONList(value as IEnumerable, propertyKeys, showTypes);
                }

                PutObject(jsonMap, entry.Key.ToString(), GetValue(value, showTypes));
            }
            return jsonMap;

        }

        static void AddObject(JArray jsonList, object value)
        {
            jsonList.Add(value);
        }

        static void PutObject(JObject jsonMap, string key, object value)
        {
            jsonMap.Add(key, JValue.FromObject(value));
        }

        static Dictionary<string, object> CreatePropertyMap(Element element, IEnumerable<string> propertyKeys, ElementPropertyConfig.ElementPropertiesRule rule)
        {
            Dictionary<string, object> map = new Dictionary<string, object>();

            if (propertyKeys == null)
            {
                foreach (string key in element.GetPropertyKeys())
                    map.Add(key, element.GetProperty(key));

            }
            else
            {
                if (rule == ElementPropertyConfig.ElementPropertiesRule.INCLUDE)
                {
                    foreach (string key in propertyKeys)
                    {
                        object valToPutInMap = element.GetProperty(key);
                        if (valToPutInMap != null)
                            map.Add(key, valToPutInMap);
                    }
                }
                else
                {
                    foreach (string key in element.GetPropertyKeys())
                    {
                        if (!propertyKeys.Contains(key))
                            map.Add(key, element.GetProperty(key));
                    }
                }
            }

            return map;
        }

        static object GetValue(object value, bool includeType)
        {
            object returnValue = value;

            // if the includeType is set to true then show the data types of the properties
            if (includeType)
            {
                // type will be one of: map, list, string, long, int, double, float.
                // in the event of a complex object it will call a toString and store as a
                // string
                string type = DetermineType(value);

                JObject valueAndType = new JObject();
                valueAndType.Add(GraphSONTokens.TYPE, JValue.CreateString(type));

                if (type == GraphSONTokens.TYPE_LIST)
                {
                    // values of lists must be accumulated as ObjectNode objects under the value key.
                    // will return as a ArrayNode. called recursively to traverse the entire
                    // object graph of each item in the array.
                    IEnumerable list = (IEnumerable)value;

                    // there is a set of values that must be accumulated as an array under a key
                    JArray valueArray = new JArray();
                    valueAndType.Add(GraphSONTokens.VALUE, valueArray);
                    foreach (object o in list)
                    {
                        // the value of each item in the array is a node object from an ArrayNode...must
                        // get the value of it.
                        AddObject(valueArray, GetValue(GetTypedValueFromJsonNode((JToken)o), includeType));
                    }

                }
                else if (type == GraphSONTokens.TYPE_MAP)
                {
                    // maps are converted to a ObjectNode. called recursively to traverse
                    // the entire object graph within the map.
                    JObject convertedMap = new JObject();
                    JObject jsonObject = (JObject)value;
                    foreach (var entry in jsonObject)
                    {
                        // no need to getValue() here as this is already a ObjectNode and should have type info
                        convertedMap.Add(entry.Key, entry.Value);
                    }

                    valueAndType.Add(GraphSONTokens.VALUE, convertedMap);
                }
                else
                {

                    // this must be a primitive value or a complex object. if a complex
                    // object it will be handled by a call to toString and stored as a
                    // string value
                    PutObject(valueAndType, GraphSONTokens.VALUE, value);
                }

                // this goes back as a JSONObject with data type and value
                returnValue = valueAndType;
            }

            return returnValue;
        }

        public static object GetTypedValueFromJsonNode(JToken node)
        {
            object theValue = null;

            if (node != null)
            {
                if (node.Type == JTokenType.Boolean)
                    theValue = node.Value<bool>();
                else if (node.Type == JTokenType.Float)
                    theValue = node.Value<double>();
                else if (node.Type == JTokenType.Integer)
                    theValue = node.Value<long>();
                else if (node.Type == JTokenType.String)
                    theValue = node.Value<string>();
                else if (node.Type == JTokenType.Array)
                {
                    // this is an array so just send it back so that it can be
                    // reprocessed to its primitive components
                    theValue = node;
                }
                else
                    theValue = node.Value<string>();
            }

            return theValue;
        }

        static string DetermineType(object value)
        {
            string type = GraphSONTokens.TYPE_STRING;
            if (value == null)
                type = "unknown";
            else if (value is double)
                type = GraphSONTokens.TYPE_DOUBLE;
            else if (value is float)
                type = GraphSONTokens.TYPE_FLOAT;
            else if (value is int)
                type = GraphSONTokens.TYPE_INTEGER;
            else if (value is long)
                type = GraphSONTokens.TYPE_LONG;
            else if (value is bool)
                type = GraphSONTokens.TYPE_BOOLEAN;
            else if (value is JArray)
                type = GraphSONTokens.TYPE_LIST;
            else if (value is JObject)
                type = GraphSONTokens.TYPE_MAP;

            return type;
        }
    }
}
