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
        readonly GraphSONMode _mode;
        readonly IEnumerable<string> _vertexPropertyKeys;
        readonly IEnumerable<string> _edgePropertyKeys;
        readonly ElementFactory _factory;
        readonly bool _hasEmbeddedTypes;
        readonly ElementPropertyConfig.ElementPropertiesRule _vertexPropertiesRule;
        readonly ElementPropertyConfig.ElementPropertiesRule _edgePropertiesRule;

        readonly bool _includeReservedVertexId;
        readonly bool _includeReservedEdgeId;
        readonly bool _includeReservedVertexType;
        readonly bool _includeReservedEdgeType;
        readonly bool _includeReservedEdgeLabel;
        readonly bool _includeReservedEdgeOutV;
        readonly bool _includeReservedEdgeInV;

        /// <summary>
        /// A GraphSONUtiltiy that includes all properties of vertices and edges.
        /// </summary>
        public GraphSONUtility(GraphSONMode mode, ElementFactory factory)
            : this(mode, factory, ElementPropertyConfig.allProperties)
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
            _vertexPropertyKeys = config.getVertexPropertyKeys();
            _edgePropertyKeys = config.getEdgePropertyKeys();
            _vertexPropertiesRule = config.getVertexPropertiesRule();
            _edgePropertiesRule = config.getEdgePropertiesRule();

            _mode = mode;
            _factory = factory;
            _hasEmbeddedTypes = mode == GraphSONMode.EXTENDED;

            _includeReservedVertexId = includeReservedKey(mode, GraphSONTokens._ID, _vertexPropertyKeys, _vertexPropertiesRule);
            _includeReservedEdgeId = includeReservedKey(mode, GraphSONTokens._ID, _edgePropertyKeys, _edgePropertiesRule);
            _includeReservedVertexType = includeReservedKey(mode, GraphSONTokens._TYPE, _vertexPropertyKeys, _vertexPropertiesRule);
            _includeReservedEdgeType = includeReservedKey(mode, GraphSONTokens._TYPE, _edgePropertyKeys, _edgePropertiesRule);
            _includeReservedEdgeLabel = includeReservedKey(mode, GraphSONTokens._LABEL, _edgePropertyKeys, _edgePropertiesRule);
            _includeReservedEdgeOutV = includeReservedKey(mode, GraphSONTokens._OUT_V, _edgePropertyKeys, _edgePropertiesRule);
            _includeReservedEdgeInV = includeReservedKey(mode, GraphSONTokens._IN_V, _edgePropertyKeys, _edgePropertiesRule);
        }

        /// <summary>
        /// Creates a vertex from GraphSON using settings supplied in the constructor.
        /// </summary>
        public Vertex vertexFromJson(string json)
        {
            JObject node = (JObject)JsonConvert.DeserializeObject(json);
            return vertexFromJson(node);
        }

        /// <summary>
        /// Creates a vertex from GraphSON using settings supplied in the constructor.
        /// </summary>
        public Vertex vertexFromJson(Stream json)
        {
            using (StreamReader reader = new StreamReader(json))
            {
                JObject node = (JObject)new JsonSerializer().Deserialize(reader, typeof(object));
                return vertexFromJson(node);
            }
        }

        /// <summary>
        /// Creates a vertex from GraphSON using settings supplied in the constructor.
        /// </summary>
        public Vertex vertexFromJson(JObject json)
        {
            Dictionary<string, object> props = readProperties(json, true, _hasEmbeddedTypes);

            object vertexId = getTypedValueFromJsonNode(json[GraphSONTokens._ID]);
            Vertex v = _factory.createVertex(vertexId);

            foreach (var entry in props)
            {
                //if (this.vertexPropertyKeys == null || vertexPropertyKeys.contains(entry.getKey())) {
                if (includeKey(entry.Key, _vertexPropertyKeys, _vertexPropertiesRule))
                    v.setProperty(entry.Key, entry.Value);
            }

            return v;
        }

        /// <summary>
        /// Creates an edge from GraphSON using settings supplied in the constructor.
        /// </summary>
        public Edge edgeFromJson(string json, Vertex out_, Vertex in_)
        {
            JObject node = (JObject)JsonConvert.DeserializeObject(json);
            return edgeFromJson(node, out_, in_);
        }

        public Edge edgeFromJson(Stream json, Vertex out_, Vertex in_)
        {
            using (StreamReader reader = new StreamReader(json))
            {
                JObject node = (JObject)new JsonSerializer().Deserialize(reader, typeof(object));
                return edgeFromJson(node, out_, in_);
            }
        }

        /// <summary>
        /// Creates an edge from GraphSON using settings supplied in the constructor.
        /// </summary>
        public Edge edgeFromJson(JObject json, Vertex out_, Vertex in_)
        {
            Dictionary<string, object> props = GraphSONUtility.readProperties(json, true, _hasEmbeddedTypes);

            object edgeId = getTypedValueFromJsonNode(json[GraphSONTokens._ID]);
            var nodeLabel = json[GraphSONTokens._LABEL] ?? string.Empty;
            string label = nodeLabel == null ? null : nodeLabel.Value<string>();

            Edge e = _factory.createEdge(edgeId, out_, in_, label);

            foreach (var entry in props)
            {
                // if (this.edgePropertyKeys == null || this.edgePropertyKeys.contains(entry.getKey())) {
                if (includeKey(entry.Key, _edgePropertyKeys, _edgePropertiesRule))
                    e.setProperty(entry.Key, entry.Value);
            }

            return e;
        }

        /// <summary>
        /// Creates GraphSON for a single graph element.
        /// </summary>
        public JObject jsonFromElement(Element element)
        {
            bool isEdge = element is Edge;
            bool showTypes = _mode == GraphSONMode.EXTENDED;
            IEnumerable<string> propertyKeys = isEdge ? _edgePropertyKeys : _vertexPropertyKeys;
            ElementPropertyConfig.ElementPropertiesRule elementPropertyConfig = isEdge ? _edgePropertiesRule : _vertexPropertiesRule;

            JObject jsonElement = createJsonMap(createPropertyMap(element, propertyKeys, elementPropertyConfig), propertyKeys, showTypes);

            if ((isEdge && _includeReservedEdgeId) || (!isEdge && _includeReservedVertexId))
                putObject(jsonElement, GraphSONTokens._ID, element.getId());

            // it's important to keep the order of these straight. check Edge first and then Vertex because there
            // are graph implementations that have Edge extend from Vertex
            if (element is Edge)
            {
                Edge edge = element as Edge;

                if (_includeReservedEdgeId)
                    putObject(jsonElement, GraphSONTokens._ID, element.getId());

                if (_includeReservedEdgeType)
                    jsonElement.Add(GraphSONTokens._TYPE, GraphSONTokens.EDGE);

                if (_includeReservedEdgeOutV)
                    putObject(jsonElement, GraphSONTokens._OUT_V, edge.getVertex(Direction.OUT).getId());

                if (_includeReservedEdgeInV)
                    putObject(jsonElement, GraphSONTokens._IN_V, edge.getVertex(Direction.IN).getId());

                if (_includeReservedEdgeLabel)
                    jsonElement.Add(GraphSONTokens._LABEL, edge.getLabel());
            }
            else if (element is Vertex)
            {
                if (_includeReservedVertexId)
                    putObject(jsonElement, GraphSONTokens._ID, element.getId());

                if (_includeReservedVertexType)
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
        public static Vertex vertexFromJson(string json, ElementFactory factory, GraphSONMode mode,
                                        IEnumerable<string> propertyKeys)
        {
            GraphSONUtility graphson = new GraphSONUtility(mode, factory, propertyKeys, null);
            return graphson.vertexFromJson(json);
        }

        /// <summary>
        /// Reads an individual Vertex from JSON.  The vertex must match the accepted GraphSON format.
        /// </summary>
        /// <param name="json">a single vertex in GraphSON format as a Stream.</param>
        /// <param name="factory">the factory responsible for constructing graph elements</param>
        /// <param name="mode">the mode of the GraphSON</param>
        /// <param name="propertyKeys">a list of keys to include on reading of element properties</param>
        public static Vertex vertexFromJson(Stream json, ElementFactory factory, GraphSONMode mode,
                                        IEnumerable<string> propertyKeys)
        {
            GraphSONUtility graphson = new GraphSONUtility(mode, factory, propertyKeys, null);
            return graphson.vertexFromJson(json);
        }

        /// <summary>
        /// Reads an individual Vertex from JSON.  The vertex must match the accepted GraphSON format.
        /// </summary>
        /// <param name="json">a single vertex in GraphSON format as Jackson JsonNode</param>
        /// <param name="factory">the factory responsible for constructing graph elements</param>
        /// <param name="mode">the mode of the GraphSON</param>
        /// <param name="propertyKeys">a list of keys to include on reading of element properties</param>
        public static Vertex vertexFromJson(JObject json, ElementFactory factory, GraphSONMode mode,
                                        IEnumerable<string> propertyKeys)
        {
            GraphSONUtility graphson = new GraphSONUtility(mode, factory, propertyKeys, null);
            return graphson.vertexFromJson(json);
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
        public static Edge edgeFromJson(string json, Vertex out_, Vertex in_,
                                    ElementFactory factory, GraphSONMode mode,
                                    IEnumerable<string> propertyKeys)
        {
            GraphSONUtility graphson = new GraphSONUtility(mode, factory, null, propertyKeys);
            return graphson.edgeFromJson(json, out_, in_);
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
        public static Edge edgeFromJson(Stream json, Vertex out_, Vertex in_,
                                        ElementFactory factory, GraphSONMode mode,
                                        IEnumerable<string> propertyKeys)
        {
            GraphSONUtility graphson = new GraphSONUtility(mode, factory, null, propertyKeys);
            return graphson.edgeFromJson(json, out_, in_);
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
        public static Edge edgeFromJson(JObject json, Vertex out_, Vertex in_,
                                        ElementFactory factory, GraphSONMode mode,
                                        IEnumerable<string> propertyKeys)
        {
            GraphSONUtility graphson = new GraphSONUtility(mode, factory, null, propertyKeys);
            return graphson.edgeFromJson(json, out_, in_);
        }

        /// <summary>
        /// Creates a JSON.NET ObjectNode from a graph element.
        /// </summary>
        /// <param name="element">the graph element to convert to JSON.</param>
        /// <param name="propertyKeys">The property keys at the root of the element to serialize.  If null, then all keys are serialized.</param>
        /// <param name="mode">The type of GraphSON to generate.</param>
        public static JObject jsonFromElement(Element element, IEnumerable<string> propertyKeys, GraphSONMode mode)
        {
            GraphSONUtility graphson = element is Edge ? new GraphSONUtility(mode, null, null, propertyKeys)
                    : new GraphSONUtility(mode, null, propertyKeys, null);
            return graphson.jsonFromElement(element);
        }

        static Dictionary<string, object> readProperties(JObject node, bool ignoreReservedKeys, bool hasEmbeddedTypes)
        {
            Dictionary<string, object> map = new Dictionary<string, object>();

            foreach (var entry in node)
            {
                if (!ignoreReservedKeys || !isReservedKey(entry.Key))
                    map.Add(entry.Key, readProperty(entry.Value, hasEmbeddedTypes));
            }

            return map;
        }

        static bool includeReservedKey(GraphSONMode mode, string key, IEnumerable<string> propertyKeys,
                                       ElementPropertyConfig.ElementPropertiesRule rule)
        {
            // the key is always included in modes other than compact. if it is compact, then validate that the
            // key is in the property key list
            return mode != GraphSONMode.COMPACT || includeKey(key, propertyKeys, rule);
        }

        static bool includeKey(string key, IEnumerable<string> propertyKeys, ElementPropertyConfig.ElementPropertiesRule rule)
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

        static bool isReservedKey(string key)
        {
            return key == GraphSONTokens._ID || key == GraphSONTokens._TYPE || key == GraphSONTokens._LABEL
                    || key == GraphSONTokens._OUT_V || key == GraphSONTokens._IN_V;
        }

        static object readProperty(JToken node, bool hasEmbeddedTypes)
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
                    propertyValue = readProperties(node[GraphSONTokens.VALUE], hasEmbeddedTypes);
                else if (type == GraphSONTokens.TYPE_MAP)
                    propertyValue = readProperties(node[GraphSONTokens.VALUE] as JObject, false, hasEmbeddedTypes);
                else
                    propertyValue = (node[GraphSONTokens.VALUE] as JValue).Value;

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
                    propertyValue = readProperties(node.Values(), hasEmbeddedTypes);
                else if (node.Type == JTokenType.Object)
                    propertyValue = readProperties(node as JObject, false, hasEmbeddedTypes);
                else
                    propertyValue = node.Value<string>();
            }

            return propertyValue;
        }

        static IEnumerable readProperties(IJEnumerable<JToken> listOfNodes, bool hasEmbeddedTypes)
        {
            return listOfNodes.Select(t => readProperty(t, hasEmbeddedTypes)).ToArray();
        }

        static JArray createJsonList(IEnumerable list, IEnumerable<string> propertyKeys, bool showTypes)
        {
            JArray jsonList = new JArray();
            foreach (object item in list)
            {
                if (item is Element)
                    jsonList.Add(jsonFromElement(item as Element, propertyKeys, showTypes ? GraphSONMode.EXTENDED : GraphSONMode.NORMAL));
                else if (item is IDictionary)
                    jsonList.Add(createJsonMap(item as IDictionary, propertyKeys, showTypes));
                else if (item != null && !(item is string) && item is IEnumerable)
                    jsonList.Add(createJsonList(item as IEnumerable, propertyKeys, showTypes));
                else
                    addObject(jsonList, item);
            }
            return jsonList;
        }

        static JObject createJsonMap(IDictionary map, IEnumerable<string> propertyKeys, bool showTypes)
        {
            JObject jsonMap = new JObject();
            foreach (DictionaryEntry entry in map)
            {
                object value = entry.Value;
                if (value != null)
                {
                    if (value is IDictionary)
                        value = createJsonMap(value as IDictionary, propertyKeys, showTypes);
                    else if (value is Element)
                        value = jsonFromElement((Element)value, propertyKeys,
                                showTypes ? GraphSONMode.EXTENDED : GraphSONMode.NORMAL);
                    else if (!(value is string) && value is IEnumerable)
                        value = createJsonList(value as IEnumerable, propertyKeys, showTypes);
                }

                putObject(jsonMap, entry.Key.ToString(), getValue(value, showTypes));
            }
            return jsonMap;

        }

        static void addObject(JArray jsonList, object value)
        {
            jsonList.Add(value == null || value is JToken ? value : JToken.FromObject(value));
        }

        static void putObject(JObject jsonMap, string key, object value)
        {
            jsonMap.put(key, (JToken)(value == null || value is JToken ? value : JToken.FromObject(value)));
        }

        static Dictionary<string, object> createPropertyMap(Element element, IEnumerable<string> propertyKeys, ElementPropertyConfig.ElementPropertiesRule rule)
        {
            Dictionary<string, object> map = new Dictionary<string, object>();

            if (propertyKeys == null)
            {
                foreach (string key in element.getPropertyKeys())
                    map.Add(key, element.getProperty(key));

            }
            else
            {
                if (rule == ElementPropertyConfig.ElementPropertiesRule.INCLUDE)
                {
                    foreach (string key in propertyKeys)
                    {
                        object valToPutInMap = element.getProperty(key);
                        if (valToPutInMap != null)
                            map.Add(key, valToPutInMap);
                    }
                }
                else
                {
                    foreach (string key in element.getPropertyKeys())
                    {
                        if (!propertyKeys.Contains(key))
                            map.Add(key, element.getProperty(key));
                    }
                }
            }

            return map;
        }

        static object getValue(object value, bool includeType)
        {
            object returnValue = value;

            // if the includeType is set to true then show the data types of the properties
            if (includeType)
            {
                // type will be one of: map, list, string, long, int, double, float.
                // in the event of a complex object it will call a toString and store as a
                // string
                string type = determineType(value);

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
                        addObject(valueArray, getValue(getTypedValueFromJsonNode((JToken)o), includeType));
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
                    putObject(valueAndType, GraphSONTokens.VALUE, value);
                }

                // this goes back as a JSONObject with data type and value
                returnValue = valueAndType;
            }

            return returnValue;
        }

        public static object getTypedValueFromJsonNode(JToken node)
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

        static string determineType(object value)
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
