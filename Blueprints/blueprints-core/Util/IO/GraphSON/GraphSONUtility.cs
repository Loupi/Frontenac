using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    /// <summary>
    /// Helps write individual graph elements to TinkerPop JSON format known as GraphSON.
    /// </summary>
    public class GraphSonUtility
    {
        readonly GraphSonMode _mode;
        readonly IEnumerable<string> _vertexPropertyKeys;
        readonly IEnumerable<string> _edgePropertyKeys;
        readonly IElementFactory _factory;
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
        public GraphSonUtility(GraphSonMode mode, IElementFactory factory)
            : this(mode, factory, ElementPropertyConfig.AllProperties)
        {

        }

        /// <summary>
        /// A GraphSONUtility that includes the specified properties.
        /// </summary>
        public GraphSonUtility(GraphSonMode mode, IElementFactory factory,
                           IEnumerable<string> vertexPropertyKeys, IEnumerable<string> edgePropertyKeys) :
            this(mode, factory, ElementPropertyConfig.IncludeProperties(vertexPropertyKeys, edgePropertyKeys))
        {

        }

        public GraphSonUtility(GraphSonMode mode, IElementFactory factory, ElementPropertyConfig config)
        {
            _vertexPropertyKeys = config.GetVertexPropertyKeys();
            _edgePropertyKeys = config.GetEdgePropertyKeys();
            _vertexPropertiesRule = config.GetVertexPropertiesRule();
            _edgePropertiesRule = config.GetEdgePropertiesRule();

            _mode = mode;
            _factory = factory;
            _hasEmbeddedTypes = mode == GraphSonMode.EXTENDED;

// ReSharper disable PossibleMultipleEnumeration
            _includeReservedVertexId = IncludeReservedKey(mode, GraphSonTokens.Id, _vertexPropertyKeys, _vertexPropertiesRule);
            _includeReservedEdgeId = IncludeReservedKey(mode, GraphSonTokens.Id, _edgePropertyKeys, _edgePropertiesRule);
            _includeReservedVertexType = IncludeReservedKey(mode, GraphSonTokens.UnderscoreType, _vertexPropertyKeys, _vertexPropertiesRule);
            _includeReservedEdgeType = IncludeReservedKey(mode, GraphSonTokens.UnderscoreType, _edgePropertyKeys, _edgePropertiesRule);
            _includeReservedEdgeLabel = IncludeReservedKey(mode, GraphSonTokens.Label, _edgePropertyKeys, _edgePropertiesRule);
            _includeReservedEdgeOutV = IncludeReservedKey(mode, GraphSonTokens.OutV, _edgePropertyKeys, _edgePropertiesRule);
            _includeReservedEdgeInV = IncludeReservedKey(mode, GraphSonTokens.InV, _edgePropertyKeys, _edgePropertiesRule);
// ReSharper restore PossibleMultipleEnumeration
        }

        /// <summary>
        /// Creates a vertex from GraphSON using settings supplied in the constructor.
        /// </summary>
        public IVertex VertexFromJson(string json)
        {
            var node = (JObject)JsonConvert.DeserializeObject(json);
            return VertexFromJson(node);
        }

        /// <summary>
        /// Creates a vertex from GraphSON using settings supplied in the constructor.
        /// </summary>
        public IVertex VertexFromJson(Stream json)
        {
            using (var reader = new StreamReader(json))
            {
                var node = (JObject)new JsonSerializer().Deserialize(reader, typeof(object));
                return VertexFromJson(node);
            }
        }

        /// <summary>
        /// Creates a vertex from GraphSON using settings supplied in the constructor.
        /// </summary>
        public IVertex VertexFromJson(JObject json)
        {
            Dictionary<string, object> props = ReadProperties(json, true, _hasEmbeddedTypes);

            object vertexId = GetTypedValueFromJsonNode(json[GraphSonTokens.Id]);
            IVertex v = _factory.CreateVertex(vertexId);

            foreach (var entry in props)
            {
                //if (this.vertexPropertyKeys == null || vertexPropertyKeys.contains(entry.getKey())) {
                if (IncludeKey(entry.Key, _vertexPropertyKeys, _vertexPropertiesRule))
                    v.SetProperty(entry.Key, entry.Value);
            }

            return v;
        }

        /// <summary>
        /// Creates an edge from GraphSON using settings supplied in the constructor.
        /// </summary>
        public IEdge EdgeFromJson(string json, IVertex out_, IVertex in_)
        {
            var node = (JObject)JsonConvert.DeserializeObject(json);
            return EdgeFromJson(node, out_, in_);
        }

        public IEdge EdgeFromJson(Stream json, IVertex out_, IVertex in_)
        {
            using (var reader = new StreamReader(json))
            {
                var node = (JObject)new JsonSerializer().Deserialize(reader, typeof(object));
                return EdgeFromJson(node, out_, in_);
            }
        }

        /// <summary>
        /// Creates an edge from GraphSON using settings supplied in the constructor.
        /// </summary>
        public IEdge EdgeFromJson(JObject json, IVertex out_, IVertex in_)
        {
            Dictionary<string, object> props = ReadProperties(json, true, _hasEmbeddedTypes);

            object edgeId = GetTypedValueFromJsonNode(json[GraphSonTokens.Id]);
            var nodeLabel = json[GraphSonTokens.Label] ?? string.Empty;
            string label = nodeLabel == null ? null : nodeLabel.Value<string>();

            IEdge e = _factory.CreateEdge(edgeId, out_, in_, label);

            foreach (var entry in props)
            {
                // if (this.edgePropertyKeys == null || this.edgePropertyKeys.contains(entry.getKey())) {
                if (IncludeKey(entry.Key, _edgePropertyKeys, _edgePropertiesRule))
                    e.SetProperty(entry.Key, entry.Value);
            }

            return e;
        }

        /// <summary>
        /// Creates GraphSON for a single graph element.
        /// </summary>
        public JObject JsonFromElement(IElement element)
        {
            bool isEdge = element is IEdge;
            bool showTypes = _mode == GraphSonMode.EXTENDED;
            var propertyKeys = isEdge ? _edgePropertyKeys : _vertexPropertyKeys;
            var elementPropertyConfig = isEdge ? _edgePropertiesRule : _vertexPropertiesRule;

// ReSharper disable PossibleMultipleEnumeration
            JObject jsonElement = CreateJsonMap(CreatePropertyMap(element, propertyKeys, elementPropertyConfig), propertyKeys, showTypes);
// ReSharper restore PossibleMultipleEnumeration

            if ((isEdge && _includeReservedEdgeId) || (!isEdge && _includeReservedVertexId))
                PutObject(jsonElement, GraphSonTokens.Id, element.GetId());

            // it's important to keep the order of these straight. check Edge first and then Vertex because there
            // are graph implementations that have Edge extend from Vertex
            if (element is IEdge)
            {
                var edge = element as IEdge;

                if (_includeReservedEdgeId)
                    PutObject(jsonElement, GraphSonTokens.Id, element.GetId());

                if (_includeReservedEdgeType)
                    jsonElement.Add(GraphSonTokens.UnderscoreType, GraphSonTokens.Edge);

                if (_includeReservedEdgeOutV)
                    PutObject(jsonElement, GraphSonTokens.OutV, edge.GetVertex(Direction.Out).GetId());

                if (_includeReservedEdgeInV)
                    PutObject(jsonElement, GraphSonTokens.InV, edge.GetVertex(Direction.In).GetId());

                if (_includeReservedEdgeLabel)
                    jsonElement.Add(GraphSonTokens.Label, edge.GetLabel());
            }
            else if (element is IVertex)
            {
                if (_includeReservedVertexId)
                    PutObject(jsonElement, GraphSonTokens.Id, element.GetId());

                if (_includeReservedVertexType)
                    jsonElement.Add(GraphSonTokens.UnderscoreType, GraphSonTokens.Vertex);
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
        public static IVertex VertexFromJson(string json, IElementFactory factory, GraphSonMode mode,
                                        IEnumerable<string> propertyKeys)
        {
            var graphson = new GraphSonUtility(mode, factory, propertyKeys, null);
            return graphson.VertexFromJson(json);
        }

        /// <summary>
        /// Reads an individual Vertex from JSON.  The vertex must match the accepted GraphSON format.
        /// </summary>
        /// <param name="json">a single vertex in GraphSON format as a Stream.</param>
        /// <param name="factory">the factory responsible for constructing graph elements</param>
        /// <param name="mode">the mode of the GraphSON</param>
        /// <param name="propertyKeys">a list of keys to include on reading of element properties</param>
        public static IVertex VertexFromJson(Stream json, IElementFactory factory, GraphSonMode mode,
                                        IEnumerable<string> propertyKeys)
        {
            var graphson = new GraphSonUtility(mode, factory, propertyKeys, null);
            return graphson.VertexFromJson(json);
        }

        /// <summary>
        /// Reads an individual Vertex from JSON.  The vertex must match the accepted GraphSON format.
        /// </summary>
        /// <param name="json">a single vertex in GraphSON format as Jackson JsonNode</param>
        /// <param name="factory">the factory responsible for constructing graph elements</param>
        /// <param name="mode">the mode of the GraphSON</param>
        /// <param name="propertyKeys">a list of keys to include on reading of element properties</param>
        public static IVertex VertexFromJson(JObject json, IElementFactory factory, GraphSonMode mode,
                                        IEnumerable<string> propertyKeys)
        {
            var graphson = new GraphSonUtility(mode, factory, propertyKeys, null);
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
        public static IEdge EdgeFromJson(string json, IVertex out_, IVertex in_,
                                    IElementFactory factory, GraphSonMode mode,
                                    IEnumerable<string> propertyKeys)
        {
            var graphson = new GraphSonUtility(mode, factory, null, propertyKeys);
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
        public static IEdge EdgeFromJson(Stream json, IVertex out_, IVertex in_,
                                        IElementFactory factory, GraphSonMode mode,
                                        IEnumerable<string> propertyKeys)
        {
            var graphson = new GraphSonUtility(mode, factory, null, propertyKeys);
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
        public static IEdge EdgeFromJson(JObject json, IVertex out_, IVertex in_,
                                        IElementFactory factory, GraphSonMode mode,
                                        IEnumerable<string> propertyKeys)
        {
            var graphson = new GraphSonUtility(mode, factory, null, propertyKeys);
            return graphson.EdgeFromJson(json, out_, in_);
        }

        /// <summary>
        /// Creates a JSON.NET ObjectNode from a graph element.
        /// </summary>
        /// <param name="element">the graph element to convert to JSON.</param>
        /// <param name="propertyKeys">The property keys at the root of the element to serialize.  If null, then all keys are serialized.</param>
        /// <param name="mode">The type of GraphSON to generate.</param>
        public static JObject JsonFromElement(IElement element, IEnumerable<string> propertyKeys, GraphSonMode mode)
        {
            GraphSonUtility graphson = element is IEdge ? new GraphSonUtility(mode, null, null, propertyKeys)
                    : new GraphSonUtility(mode, null, propertyKeys, null);
            return graphson.JsonFromElement(element);
        }

        static Dictionary<string, object> ReadProperties(JObject node, bool ignoreReservedKeys, bool hasEmbeddedTypes)
        {
            var map = new Dictionary<string, object>();

            foreach (var entry in node)
            {
                if (!ignoreReservedKeys || !IsReservedKey(entry.Key))
                    map.Add(entry.Key, ReadProperty(entry.Value, hasEmbeddedTypes));
            }

            return map;
        }

        static bool IncludeReservedKey(GraphSonMode mode, string key, IEnumerable<string> propertyKeys,
                                       ElementPropertyConfig.ElementPropertiesRule rule)
        {
            // the key is always included in modes other than COMPACT. if it is COMPACT, then validate that the
            // key is in the property key list
            return mode != GraphSonMode.COMPACT || IncludeKey(key, propertyKeys, rule);
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
            bool keySituation = rule == ElementPropertyConfig.ElementPropertiesRule.Include;

            switch (rule)
            {
                case ElementPropertyConfig.ElementPropertiesRule.Include:
                    keySituation = propertyKeys.Contains(key);
                    break;
                case ElementPropertyConfig.ElementPropertiesRule.Exclude:
                    keySituation = !propertyKeys.Contains(key);
                    break;
            }

            return keySituation;
        }

        static bool IsReservedKey(string key)
        {
            return key == GraphSonTokens.Id || key == GraphSonTokens.UnderscoreType || key == GraphSonTokens.Label
                    || key == GraphSonTokens.OutV || key == GraphSonTokens.InV;
        }

        static object ReadProperty(JToken node, bool hasEmbeddedTypes)
        {
            object propertyValue = null;

            if (hasEmbeddedTypes)
            {
                var type = node[GraphSonTokens.Type].Value<string>();

                if (type == GraphSonTokens.TypeUnknown)
// ReSharper disable RedundantAssignment
                    propertyValue = null;
// ReSharper restore RedundantAssignment
                else if (type == GraphSonTokens.TypeBoolean)
                    propertyValue = node[GraphSonTokens.Value].Value<bool>();
                else if (type == GraphSonTokens.TypeFloat)
                    propertyValue = float.Parse(node[GraphSonTokens.Value].Value<string>(), CultureInfo.InvariantCulture);
                else if (type == GraphSonTokens.TypeDouble)
                    propertyValue = node[GraphSonTokens.Value].Value<double>();
                else if (type == GraphSonTokens.TypeInteger)
                    propertyValue = node[GraphSonTokens.Value].Value<int>();
                else if (type == GraphSonTokens.TypeLong)
                    propertyValue = node[GraphSonTokens.Value].Value<long>();
                else if (type == GraphSonTokens.TypeString)
                    propertyValue = node[GraphSonTokens.Value].Value<string>();
                else if (type == GraphSonTokens.TypeList)
                    propertyValue = ReadProperties(node[GraphSonTokens.Value], true);
                else if (type == GraphSonTokens.TypeMap)
                    propertyValue = ReadProperties(node[GraphSonTokens.Value] as JObject, false, true);
                else
                {
                    var jValue = node[GraphSonTokens.Value] as JValue;
                    if (jValue != null)
                        propertyValue = jValue.Value;
                }
            }
            else
            {
                if (node.Type == JTokenType.Null)
// ReSharper disable RedundantAssignment
                    propertyValue = null;
// ReSharper restore RedundantAssignment
                else if (node.Type == JTokenType.Boolean)
                    propertyValue = node.Value<bool>();
                else if (node.Type == JTokenType.Float)
                    propertyValue = node.Value<double>();
                else if (node.Type == JTokenType.Integer)
                    propertyValue = node.Value<long>();
                else if (node.Type == JTokenType.String)
                    propertyValue = node.Value<string>();
                else if (node.Type == JTokenType.Array)
                    propertyValue = ReadProperties(node.Values(), false);
                else if (node.Type == JTokenType.Object)
                    propertyValue = ReadProperties(node as JObject, false, false);
                else
                    propertyValue = node.Value<string>();
            }

            return propertyValue;
        }

        static IEnumerable ReadProperties(IEnumerable<JToken> listOfNodes, bool hasEmbeddedTypes)
        {
            return listOfNodes.Select(t => ReadProperty(t, hasEmbeddedTypes)).ToArray();
        }

        static JArray CreateJsonList(IEnumerable list, IEnumerable<string> propertyKeys, bool showTypes)
        {
            var jsonList = new JArray();
            foreach (object item in list)
            {
// ReSharper disable PossibleMultipleEnumeration
                if (item is IElement)
                    jsonList.Add(JsonFromElement(item as IElement, propertyKeys, showTypes ? GraphSonMode.EXTENDED : GraphSonMode.NORMAL));
                else if (item is IDictionary)
                    jsonList.Add(CreateJsonMap(item as IDictionary, propertyKeys, showTypes));
                else if (item != null && !(item is string) && item is IEnumerable)
                    jsonList.Add(CreateJsonList(item as IEnumerable, propertyKeys, showTypes));
                else
                    AddObject(jsonList, item);
// ReSharper restore PossibleMultipleEnumeration
            }
            return jsonList;
        }

        static JObject CreateJsonMap(IDictionary map, IEnumerable<string> propertyKeys, bool showTypes)
        {
            var jsonMap = new JObject();
            
            foreach (DictionaryEntry entry in map)
            {
                object value = entry.Value;
                if (value != null)
                {
// ReSharper disable PossibleMultipleEnumeration
                    if (value is IDictionary)
                        value = CreateJsonMap(value as IDictionary, propertyKeys, showTypes);
                    else if (value is IElement)
                        value = JsonFromElement((IElement)value, propertyKeys,
                                showTypes ? GraphSonMode.EXTENDED : GraphSonMode.NORMAL);
                    else if (!(value is string) && value is IEnumerable)
                        value = CreateJsonList(value as IEnumerable, propertyKeys, showTypes);
// ReSharper restore PossibleMultipleEnumeration
                }

                PutObject(jsonMap, entry.Key.ToString(), GetValue(value, showTypes));
            }
            return jsonMap;

        }

        static void AddObject(JArray jsonList, object value)
        {
            jsonList.Add(value == null || value is JToken ? value : JToken.FromObject(value));
        }

        static void PutObject(JObject jsonMap, string key, object value)
        {
            jsonMap.Put(key, (JToken)(value == null || value is JToken ? value : JToken.FromObject(value)));
        }

        static Dictionary<string, object> CreatePropertyMap(IElement element, IEnumerable<string> propertyKeys, ElementPropertyConfig.ElementPropertiesRule rule)
        {
            var map = new Dictionary<string, object>();

            if (propertyKeys == null)
            {
                foreach (string key in element.GetPropertyKeys())
                    map.Add(key, element.GetProperty(key));

            }
            else
            {
                if (rule == ElementPropertyConfig.ElementPropertiesRule.Include)
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
// ReSharper disable PossibleMultipleEnumeration
                        if (!propertyKeys.Contains(key))
// ReSharper restore PossibleMultipleEnumeration
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

                var valueAndType = new JObject {{GraphSonTokens.Type, JValue.CreateString(type)}};

                if (type == GraphSonTokens.TypeList)
                {
                    // values of lists must be accumulated as ObjectNode objects under the value key.
                    // will return as a ArrayNode. called recursively to traverse the entire
                    // object graph of each item in the array.
                    var list = (IEnumerable)value;

                    // there is a set of values that must be accumulated as an array under a key
                    var valueArray = new JArray();
                    valueAndType.Add(GraphSonTokens.Value, valueArray);
                    foreach (object o in list)
                    {
                        // the value of each item in the array is a node object from an ArrayNode...must
                        // get the value of it.
                        AddObject(valueArray, GetValue(GetTypedValueFromJsonNode((JToken)o), true));
                    }

                }
                else if (type == GraphSonTokens.TypeMap)
                {
                    // maps are converted to a ObjectNode. called recursively to traverse
                    // the entire object graph within the map.
                    var convertedMap = new JObject();
                    var jsonObject = (JObject)value;
                    foreach (var entry in jsonObject)
                    {
                        // no need to getValue() here as this is already a ObjectNode and should have type info
                        convertedMap.Add(entry.Key, entry.Value);
                    }

                    valueAndType.Add(GraphSonTokens.Value, convertedMap);
                }
                else
                {

                    // this must be a primitive value or a complex object. if a complex
                    // object it will be handled by a call to toString and stored as a
                    // string value
                    PutObject(valueAndType, GraphSonTokens.Value, value);
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
            string type = GraphSonTokens.TypeString;
            if (value == null)
                type = "unknown";
            else if (value is double)
                type = GraphSonTokens.TypeDouble;
            else if (value is float)
                type = GraphSonTokens.TypeFloat;
            else if (value is int)
                type = GraphSonTokens.TypeInteger;
            else if (value is long)
                type = GraphSonTokens.TypeLong;
            else if (value is bool)
                type = GraphSonTokens.TypeBoolean;
            else if (value is JArray)
                type = GraphSonTokens.TypeList;
            else if (value is JObject)
                type = GraphSonTokens.TypeMap;

            return type;
        }
    }
}
