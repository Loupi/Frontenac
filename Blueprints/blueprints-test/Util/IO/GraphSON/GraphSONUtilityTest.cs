using System.Collections;
using System.IO;
using Frontenac.Blueprints.Impls.TG;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    [TestFixture(Category = "GraphSONUtilityTest")]
    public class GraphSONUtilityTest
    {
        TinkerGraph graph = new TinkerGraph();

        string vertexJson1 = "{\"name\":\"marko\",\"age\":29,\"_id\":1,\"_type\":\"vertex\"}";
        string vertexJson2 = "{\"name\":\"vadas\",\"age\":27,\"_id\":2,\"_type\":\"vertex\"}";

        string edgeJsonLight = "{\"weight\":0.5,\"_outV\":1,\"_inV\":2}";
        string edgeJson = "{\"weight\":0.5,\"_id\":7,\"_type\":\"edge\",\"_outV\":1,\"_inV\":2,\"_label\":\"knows\"}";

        Stream inputStreamVertexJson1;
        Stream inputStreamEdgeJsonLight;

        [SetUp]
        public void Setup()
        {
            this.graph.clear();

            this.inputStreamVertexJson1 = new MemoryStream(System.Text.Encoding.Default.GetBytes(vertexJson1));
            this.inputStreamEdgeJsonLight = new MemoryStream(System.Text.Encoding.Default.GetBytes(edgeJsonLight));
        }

        [Test]
        public void jsonFromElementEdgeNoPropertiesNoKeysNoTypes()
        {
            Vertex v1 = this.graph.addVertex(1);
            Vertex v2 = this.graph.addVertex(2);

            Edge e = this.graph.addEdge(3, v1, v2, "test");
            e.setProperty("weight", 0.5f);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(e, null, GraphSONMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(3, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey(GraphSONTokens._LABEL));
            Assert.AreEqual("test", json[GraphSONTokens._LABEL].Value<string>());
            Assert.True(json.ContainsKey(GraphSONTokens._TYPE));
            Assert.AreEqual("edge", json[GraphSONTokens._TYPE].Value<string>());
            Assert.True(json.ContainsKey(GraphSONTokens._IN_V));
            Assert.AreEqual(2, json[GraphSONTokens._IN_V].Value<int>());
            Assert.True(json.ContainsKey(GraphSONTokens._OUT_V));
            Assert.AreEqual(1, json[GraphSONTokens._OUT_V].Value<int>());
            Assert.True(json.ContainsKey("weight"));
            Assert.AreEqual(0.5d, json["weight"].Value<double>(), 0.0d);
        }

        [Test]
        public void jsonFromElementEdgeCompactIdOnlyAsInclude()
        {
            Vertex v1 = this.graph.addVertex(1);
            Vertex v2 = this.graph.addVertex(2);

            Edge e = this.graph.addEdge(3, v1, v2, "test");
            e.setProperty("weight", 0.5f);

            HashSet<String> propertiesToInclude = new HashSet<String>();
            propertiesToInclude.Add(GraphSONTokens._ID);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(e, propertiesToInclude, GraphSONMode.COMPACT);

            Assert.NotNull(json);
            Assert.False(json.ContainsKey(GraphSONTokens._TYPE));
            Assert.False(json.ContainsKey(GraphSONTokens._LABEL));
            Assert.False(json.ContainsKey(GraphSONTokens._IN_V));
            Assert.False(json.ContainsKey(GraphSONTokens._OUT_V));
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.False(json.ContainsKey("weight"));
        }

        [Test]
        public void jsonFromElementEdgeCompactIdOnlyAsExclude()
        {
            ElementFactory factory = new GraphElementFactory(this.graph);
            Vertex v1 = this.graph.addVertex(1);
            Vertex v2 = this.graph.addVertex(2);

            Edge e = this.graph.addEdge(3, v1, v2, "test");
            e.setProperty("weight", 0.5f);
            e.setProperty("x", "y");

            HashSet<string> propertiesToExclude = new HashSet<string>() {
                GraphSONTokens._TYPE,
                GraphSONTokens._LABEL,
                GraphSONTokens._IN_V,
                GraphSONTokens._OUT_V,
                "weight"
            };

            ElementPropertyConfig config = new ElementPropertyConfig(null, propertiesToExclude,
                    ElementPropertyConfig.ElementPropertiesRule.INCLUDE,
                    ElementPropertyConfig.ElementPropertiesRule.EXCLUDE);
            GraphSONUtility utility = new GraphSONUtility(GraphSONMode.COMPACT, factory, config);
            IDictionary<string, JToken> json = utility.jsonFromElement(e);

            Assert.NotNull(json);
            Assert.False(json.ContainsKey(GraphSONTokens._TYPE));
            Assert.False(json.ContainsKey(GraphSONTokens._LABEL));
            Assert.False(json.ContainsKey(GraphSONTokens._IN_V));
            Assert.False(json.ContainsKey(GraphSONTokens._OUT_V));
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.False(json.ContainsKey("weight"));
            Assert.True(json.ContainsKey("x"));
            Assert.AreEqual("y", json["x"].Value<string>());
        }

        [Test]
        public void jsonFromElementEdgeCompactAllKeys()
        {
            Vertex v1 = this.graph.addVertex(1);
            Vertex v2 = this.graph.addVertex(2);

            Edge e = this.graph.addEdge(3, v1, v2, "test");
            e.setProperty("weight", 0.5f);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(e, null, GraphSONMode.COMPACT);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.True(json.ContainsKey(GraphSONTokens._TYPE));
            Assert.True(json.ContainsKey(GraphSONTokens._LABEL));
            Assert.True(json.ContainsKey(GraphSONTokens._IN_V));
            Assert.True(json.ContainsKey(GraphSONTokens._OUT_V));
            Assert.AreEqual(0.5d, json["weight"].Value<double>(), 0.0d);
        }

        [Test]
        public void jsonFromElementVertexNoPropertiesNoKeysNoTypes()
        {
            Vertex v = this.graph.addVertex(1);
            v.setProperty("name", "marko");

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey(GraphSONTokens._TYPE));
            Assert.AreEqual("vertex", json[GraphSONTokens._TYPE].Value<string>());
            Assert.AreEqual("marko", json["name"].Value<string>());
        }

        [Test]
        public void jsonFromElementVertexCompactIdOnlyAsInclude()
        {
            Vertex v = this.graph.addVertex(1);
            v.setProperty("name", "marko");

            HashSet<String> propertiesToInclude = new HashSet<String>() {
               GraphSONTokens._ID
            };

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, propertiesToInclude, GraphSONMode.COMPACT);

            Assert.NotNull(json);
            Assert.False(json.ContainsKey(GraphSONTokens._TYPE));
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.False(json.ContainsKey("name"));
        }

        [Test]
        public void jsonFromElementVertexCompactIdNameOnlyAsExclude()
        {
            GraphElementFactory factory = new GraphElementFactory(this.graph);
            Vertex v = this.graph.addVertex(1);
            v.setProperty("name", "marko");

            HashSet<String> propertiesToExclude = new HashSet<String>() { GraphSONTokens._TYPE };

            ElementPropertyConfig config = new ElementPropertyConfig(propertiesToExclude, null,
                    ElementPropertyConfig.ElementPropertiesRule.EXCLUDE,
                    ElementPropertyConfig.ElementPropertiesRule.EXCLUDE);

            GraphSONUtility utility = new GraphSONUtility(GraphSONMode.COMPACT, factory, config);
            IDictionary<string, JToken> json = utility.jsonFromElement(v);

            Assert.NotNull(json);
            Assert.False(json.ContainsKey(GraphSONTokens._TYPE));
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.True(json.ContainsKey("name"));
        }

        [Test]
        public void jsonFromElementVertexCompactAllOnly()
        {
            Vertex v = this.graph.addVertex(1);
            v.setProperty("name", "marko");

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.COMPACT);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._TYPE));
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.True(json.ContainsKey("name"));
        }

        [Test]
        public void jsonFromElementVertexPrimitivePropertiesNoKeysNoTypes()
        {
            Vertex v = this.graph.addVertex(1);
            v.setProperty("keyString", "string");
            v.setProperty("keyLong", 1L);
            v.setProperty("keyInt", 2);
            v.setProperty("keyFloat", 3.3f);
            v.setProperty("keyExponentialDouble", 1312928167.626012);
            v.setProperty("keyDouble", 4.4);
            v.setProperty("keyBoolean", true);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("keyString"));
            Assert.AreEqual("string", json["keyString"].Value<string>());
            Assert.True(json.ContainsKey("keyLong"));
            Assert.AreEqual(1L, json["keyLong"].Value<long>());
            Assert.True(json.ContainsKey("keyInt"));
            Assert.AreEqual(2, json["keyInt"].Value<int>());
            Assert.True(json.ContainsKey("keyFloat"));
            Assert.AreEqual(3.3f, (float)json["keyFloat"].Value<double>(), 0);
            Assert.True(json.ContainsKey("keyExponentialDouble"));
            Assert.AreEqual(1312928167.626012, json["keyExponentialDouble"].Value<double>(), 0);
            Assert.True(json.ContainsKey("keyDouble"));
            Assert.AreEqual(4.4, json["keyDouble"].Value<double>(), 0);
            Assert.True(json.ContainsKey("keyBoolean"));
            Assert.True(json["keyBoolean"].Value<bool>());
        }

        [Test]
        public void jsonFromElementVertexMapPropertyNoKeysNoTypes()
        {
            Vertex v = this.graph.addVertex(1);
            var map = new Dictionary<string, object>();
            map.put("this", "some");
            map.put("that", 1);

            v.setProperty("keyMap", map);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("keyMap"));

            IDictionary<string, JToken> mapAsJSON = (JObject)json["keyMap"];
            Assert.NotNull(mapAsJSON);
            Assert.True(mapAsJSON.ContainsKey("this"));
            Assert.AreEqual("some", mapAsJSON["this"].Value<string>());
            Assert.True(mapAsJSON.ContainsKey("that"));
            Assert.AreEqual(1, mapAsJSON["that"].Value<int>());
        }

        [Test]
        public void jsonFromElementVertexListPropertyNoKeysNoTypes()
        {
            Vertex v = this.graph.addVertex(1);
            List<object> list = new List<object>();
            list.Add("this");
            list.Add("that");
            list.Add("other");
            list.Add(true);

            v.setProperty("keyList", list);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("keyList"));

            IList<JToken> listAsJSON = (JArray)json["keyList"];
            Assert.NotNull(listAsJSON);
            Assert.AreEqual(4, listAsJSON.Count());
        }

        [Test]
        public void jsonFromElementVertexStringArrayPropertyNoKeysNoTypes()
        {
            Vertex v = this.graph.addVertex(1);
            string[] stringArray = new string[] { "this", "that", "other" };

            v.setProperty("keyStringArray", stringArray);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("keyStringArray"));

            JArray stringArrayAsJSON = (JArray)json["keyStringArray"];
            Assert.NotNull(stringArrayAsJSON);
            Assert.AreEqual(3, stringArrayAsJSON.Count());
        }

        [Test]
        public void jsonFromElementVertexDoubleArrayPropertyNoKeysNoTypes()
        {
            Vertex v = this.graph.addVertex(1);
            double[] doubleArray = new double[] { 1.0, 2.0, 3.0 };

            v.setProperty("keyDoubleArray", doubleArray);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("keyDoubleArray"));

            JArray doubleArrayAsJSON = (JArray)json["keyDoubleArray"];
            Assert.NotNull(doubleArrayAsJSON);
            Assert.AreEqual(3, doubleArrayAsJSON.Count());
        }

        [Test]
        public void jsonFromElementVertexIntArrayPropertyNoKeysNoTypes()
        {
            Vertex v = this.graph.addVertex(1);
            int[] intArray = new int[] { 1, 2, 3 };

            v.setProperty("keyIntArray", intArray);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("keyIntArray"));

            JArray intArrayAsJSON = (JArray)json["keyIntArray"];
            Assert.NotNull(intArrayAsJSON);
            Assert.AreEqual(3, intArrayAsJSON.Count());
        }

        [Test]
        public void jsonFromElementVertexLongArrayPropertyNoKeysNoTypes()
        {
            Vertex v = this.graph.addVertex(1);
            long[] longArray = new long[] { 1, 2, 3 };

            v.setProperty("keyLongArray", longArray);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("keyLongArray"));

            JArray longArrayAsJSON = (JArray)json["keyLongArray"];
            Assert.NotNull(longArrayAsJSON);
            Assert.AreEqual(3, longArrayAsJSON.Count());
        }

        [Test]
        public void jsonFromElementFloatArrayPropertyNoKeysNoTypes()
        {
            Vertex v = this.graph.addVertex(1);
            float[] floatArray = new float[] { 1.0f, 2.0f, 3.0f };

            v.setProperty("keyFloatArray", floatArray);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("keyFloatArray"));

            JArray floatArrayAsJSON = (JArray)json["keyFloatArray"];
            Assert.NotNull(floatArrayAsJSON);
            Assert.AreEqual(3, floatArrayAsJSON.Count());
        }

        [Test]
        public void jsonFromElementBooleanArrayPropertyNoKeysNoTypes()
        {
            Vertex v = this.graph.addVertex(1);
            bool[] booleanArray = new bool[] { true, false, true };

            v.setProperty("keyBooleanArray", booleanArray);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("keyBooleanArray"));

            JArray booleanArrayAsJSON = (JArray)json["keyBooleanArray"];
            Assert.NotNull(booleanArrayAsJSON);
            Assert.AreEqual(3, booleanArrayAsJSON.Count());
        }

        [Test]
        public void jsonFromElementVertexCatPropertyNoKeysNoTypes()
        {
            Vertex v = this.graph.addVertex(1);
            v.setProperty("mycat", new Cat("smithers"));

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("mycat"));
            Assert.AreEqual("smithers", json["mycat"].Value<string>());
        }

        [Test]
        public void jsonFromElementVertexCatPropertyNoKeysWithTypes()
        {
            Vertex v = this.graph.addVertex(1);
            v.setProperty("mycat", new Cat("smithers"));

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.EXTENDED);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("mycat"));
            IDictionary<string, JToken> jsonObjectCat = (JObject)json["mycat"];
            Assert.True(jsonObjectCat.ContainsKey("value"));
            Assert.AreEqual("smithers", jsonObjectCat["value"].Value<string>());
        }

        [Test]
        public void jsonFromElementVertexCatArrayPropertyNoKeysNoTypes()
        {
            Vertex v = this.graph.addVertex(1);
            List<Cat> cats = new List<Cat>();
            cats.Add(new Cat("smithers"));
            cats.Add(new Cat("mcallister"));

            v.setProperty("cats", cats);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("cats"));

            JArray catListAsJson = (JArray)json["cats"];
            Assert.NotNull(catListAsJson);
            Assert.AreEqual(2, catListAsJson.Count());
        }

        [Test]
        public void jsonFromElementCrazyPropertyNoKeysNoTypes()
        {
            Vertex v = this.graph.addVertex(1);
            var mix = new List<object>();
            mix.Add(new Cat("smithers"));
            mix.Add(true);

            var deepCats = new List<object>();
            deepCats.Add(new Cat("mcallister"));
            mix.Add(deepCats);

            var map = new Dictionary<string, object>();
            map.put("crazy", mix);

            int[] someInts = new int[] { 1, 2, 3 };
            map.put("ints", someInts);

            map.put("regular", "stuff");

            var innerMap = new Dictionary<string, object>();
            innerMap.put("me", "you");

            map.put("inner", innerMap);

            v.setProperty("crazy-map", map);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("crazy-map"));

            IDictionary<string, JToken> mapAsJson = (JObject)json["crazy-map"];
            Assert.True(mapAsJson.ContainsKey("regular"));
            Assert.AreEqual("stuff", mapAsJson["regular"].Value<string>());

            Assert.True(mapAsJson.ContainsKey("ints"));
            JArray intArrayAsJson = (JArray)mapAsJson["ints"];
            Assert.NotNull(intArrayAsJson);
            Assert.AreEqual(3, intArrayAsJson.Count());

            Assert.True(mapAsJson.ContainsKey("crazy"));
            JArray deepListAsJSON = (JArray)mapAsJson["crazy"];
            Assert.NotNull(deepListAsJSON);
            Assert.AreEqual(3, deepListAsJSON.Count());

            Assert.True(mapAsJson.ContainsKey("inner"));
            IDictionary<string, JToken> mapInMapAsJSON = (JObject)mapAsJson["inner"];
            Assert.NotNull(mapInMapAsJSON);
            Assert.True(mapInMapAsJSON.ContainsKey("me"));
            Assert.AreEqual("you", mapInMapAsJSON["me"].Value<string>());

        }

        [Test]
        public void jsonFromElementVertexNoPropertiesWithKeysNoTypes()
        {
            Vertex v = this.graph.addVertex(1);
            v.setProperty("x", "X");
            v.setProperty("y", "Y");
            v.setProperty("z", "Z");

            HashSet<String> propertiesToInclude = new HashSet<String>();
            propertiesToInclude.Add("y");
            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, propertiesToInclude, GraphSONMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey(GraphSONTokens._TYPE));
            Assert.AreEqual("vertex", json[GraphSONTokens._TYPE].Value<string>());
            Assert.False(json.ContainsKey("x"));
            Assert.False(json.ContainsKey("z"));
            Assert.True(json.ContainsKey("y"));
        }

        [Test]
        public void jsonFromElementVertexVertexPropertiesWithKeysNoTypes()
        {
            Vertex v = this.graph.addVertex(1);
            v.setProperty("x", "X");
            v.setProperty("y", "Y");
            v.setProperty("z", "Z");

            Vertex innerV = this.graph.addVertex(2);
            innerV.setProperty("x", "X");
            innerV.setProperty("y", "Y");
            innerV.setProperty("z", "Z");

            v.setProperty("v", innerV);

            HashSet<String> propertiesToInclude = new HashSet<String>();
            propertiesToInclude.Add("y");
            propertiesToInclude.Add("v");

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, propertiesToInclude, GraphSONMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey(GraphSONTokens._TYPE));
            Assert.AreEqual("vertex", json[GraphSONTokens._TYPE].Value<string>());
            Assert.False(json.ContainsKey("x"));
            Assert.False(json.ContainsKey("z"));
            Assert.True(json.ContainsKey("y"));
            Assert.True(json.ContainsKey("v"));

            IDictionary<string, JToken> innerJson = (JObject)json["v"];
            Assert.False(innerJson.ContainsKey("x"));
            Assert.False(innerJson.ContainsKey("z"));
            Assert.True(innerJson.ContainsKey("y"));
            Assert.False(innerJson.ContainsKey("v"));
        }

        [Test]
        public void jsonFromElementVertexPrimitivePropertiesNoKeysWithTypes()
        {
            Vertex v = this.graph.addVertex(1);
            v.setProperty("keyString", "string");
            v.setProperty("keyLong", 1L);
            v.setProperty("keyInt", 2);
            v.setProperty("keyFloat", 3.3f);
            v.setProperty("keyDouble", 4.4);
            v.setProperty("keyBoolean", true);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.EXTENDED);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("keyString"));

            IDictionary<string, JToken> valueAsJson = (JObject)json["keyString"];
            Assert.NotNull(valueAsJson);
            Assert.True(valueAsJson.ContainsKey(GraphSONTokens.TYPE));
            Assert.AreEqual(GraphSONTokens.TYPE_STRING, valueAsJson[GraphSONTokens.TYPE].Value<string>());
            Assert.True(valueAsJson.ContainsKey(GraphSONTokens.VALUE));
            Assert.AreEqual("string", valueAsJson[GraphSONTokens.VALUE].Value<string>());

            valueAsJson = (JObject)json["keyLong"];
            Assert.NotNull(valueAsJson);
            Assert.True(valueAsJson.ContainsKey(GraphSONTokens.TYPE));
            Assert.AreEqual(GraphSONTokens.TYPE_LONG, valueAsJson[GraphSONTokens.TYPE].Value<string>());
            Assert.True(valueAsJson.ContainsKey(GraphSONTokens.VALUE));
            Assert.AreEqual(1L, valueAsJson[GraphSONTokens.VALUE].Value<long>());

            valueAsJson = (JObject)json["keyInt"];
            Assert.NotNull(valueAsJson);
            Assert.True(valueAsJson.ContainsKey(GraphSONTokens.TYPE));
            Assert.AreEqual(GraphSONTokens.TYPE_INTEGER, valueAsJson[GraphSONTokens.TYPE].Value<string>());
            Assert.True(valueAsJson.ContainsKey(GraphSONTokens.VALUE));
            Assert.AreEqual(2, valueAsJson[GraphSONTokens.VALUE].Value<int>());

            valueAsJson = (JObject)json["keyFloat"];
            Assert.NotNull(valueAsJson);
            Assert.True(valueAsJson.ContainsKey(GraphSONTokens.TYPE));
            Assert.AreEqual(GraphSONTokens.TYPE_FLOAT, valueAsJson[GraphSONTokens.TYPE].Value<string>());
            Assert.True(valueAsJson.ContainsKey(GraphSONTokens.VALUE));
            Assert.AreEqual(3.3f, (float)valueAsJson[GraphSONTokens.VALUE].Value<double>(), 0);

            valueAsJson = (JObject)json["keyDouble"];
            Assert.NotNull(valueAsJson);
            Assert.True(valueAsJson.ContainsKey(GraphSONTokens.TYPE));
            Assert.AreEqual(GraphSONTokens.TYPE_DOUBLE, valueAsJson[GraphSONTokens.TYPE].Value<string>());
            Assert.True(valueAsJson.ContainsKey(GraphSONTokens.VALUE));
            Assert.AreEqual(4.4, valueAsJson[GraphSONTokens.VALUE].Value<double>(), 0);

            valueAsJson = (JObject)json["keyBoolean"];
            Assert.NotNull(valueAsJson);
            Assert.True(valueAsJson.ContainsKey(GraphSONTokens.TYPE));
            Assert.AreEqual(GraphSONTokens.TYPE_BOOLEAN, valueAsJson[GraphSONTokens.TYPE].Value<string>());
            Assert.True(valueAsJson.ContainsKey(GraphSONTokens.VALUE));
            Assert.True(valueAsJson[GraphSONTokens.VALUE].Value<bool>());
        }

        [Test]
        public void jsonFromElementVertexListPropertiesNoKeysWithTypes()
        {
            Vertex v = this.graph.addVertex(1);
            List<string> list = new List<string>();
            list.Add("this");
            list.Add("this");
            list.Add("this");

            v.setProperty("keyList", list);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.EXTENDED);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("keyList"));

            IDictionary<string, JToken> listWithTypeAsJson = (JObject)json["keyList"];
            Assert.NotNull(listWithTypeAsJson);
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSONTokens.TYPE));
            Assert.AreEqual(GraphSONTokens.TYPE_LIST, listWithTypeAsJson[GraphSONTokens.TYPE].Value<string>());
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSONTokens.VALUE));
            JArray listAsJSON = (JArray)listWithTypeAsJson[GraphSONTokens.VALUE];
            Assert.NotNull(listAsJSON);
            Assert.AreEqual(3, listAsJSON.Count());

            for (int ix = 0; ix < listAsJSON.Count(); ix++)
            {
                IDictionary<string, JToken> valueAsJson = (JObject)listAsJSON[ix];
                Assert.NotNull(valueAsJson);
                Assert.True(valueAsJson.ContainsKey(GraphSONTokens.TYPE));
                Assert.AreEqual(GraphSONTokens.TYPE_STRING, valueAsJson[GraphSONTokens.TYPE].Value<string>());
                Assert.True(valueAsJson.ContainsKey(GraphSONTokens.VALUE));
                Assert.AreEqual("this", valueAsJson[GraphSONTokens.VALUE].Value<string>());
            }
        }

        [Test]
        public void jsonFromElementVertexBooleanListPropertiesNoKeysWithTypes()
        {
            Vertex v = this.graph.addVertex(1);
            List<bool> list = new List<bool>();
            list.Add(true);
            list.Add(true);
            list.Add(true);

            v.setProperty("keyList", list);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.EXTENDED);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("keyList"));

            IDictionary<string, JToken> listWithTypeAsJson = (JObject)json["keyList"];
            Assert.NotNull(listWithTypeAsJson);
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSONTokens.TYPE));
            Assert.AreEqual(GraphSONTokens.TYPE_LIST, listWithTypeAsJson[GraphSONTokens.TYPE].Value<string>());
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSONTokens.VALUE));
            JArray listAsJSON = (JArray)listWithTypeAsJson[GraphSONTokens.VALUE];
            Assert.NotNull(listAsJSON);
            Assert.AreEqual(3, listAsJSON.Count());

            for (int ix = 0; ix < listAsJSON.Count(); ix++)
            {
                IDictionary<string, JToken> valueAsJson = (JObject)listAsJSON[ix];
                Assert.NotNull(valueAsJson);
                Assert.True(valueAsJson.ContainsKey(GraphSONTokens.TYPE));
                Assert.AreEqual(GraphSONTokens.TYPE_BOOLEAN, valueAsJson[GraphSONTokens.TYPE].Value<string>());
                Assert.True(valueAsJson.ContainsKey(GraphSONTokens.VALUE));
                Assert.AreEqual(true, valueAsJson[GraphSONTokens.VALUE].Value<bool>());
            }
        }

        [Test]
        public void jsonFromElementVertexLongListPropertiesNoKeysWithTypes()
        {
            Vertex v = this.graph.addVertex(1);
            List<long> list = new List<long>();
            list.Add(1000L);
            list.Add(1000L);
            list.Add(1000L);

            v.setProperty("keyList", list);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.EXTENDED);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("keyList"));

            IDictionary<string, JToken> listWithTypeAsJson = (JObject)json["keyList"];
            Assert.NotNull(listWithTypeAsJson);
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSONTokens.TYPE));
            Assert.AreEqual(GraphSONTokens.TYPE_LIST, listWithTypeAsJson[GraphSONTokens.TYPE].Value<string>());
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSONTokens.VALUE));
            JArray listAsJSON = (JArray)listWithTypeAsJson[GraphSONTokens.VALUE];
            Assert.NotNull(listAsJSON);
            Assert.AreEqual(3, listAsJSON.Count());

            for (int ix = 0; ix < listAsJSON.Count(); ix++)
            {
                IDictionary<string, JToken> valueAsJson = (JObject)listAsJSON[ix];
                Assert.NotNull(valueAsJson);
                Assert.True(valueAsJson.ContainsKey(GraphSONTokens.TYPE));
                Assert.AreEqual(GraphSONTokens.TYPE_LONG, valueAsJson[GraphSONTokens.TYPE].Value<string>());
                Assert.True(valueAsJson.ContainsKey(GraphSONTokens.VALUE));
                Assert.AreEqual(1000L, valueAsJson[GraphSONTokens.VALUE].Value<long>());
            }
        }

        [Test]
        public void jsonFromElementVertexIntListPropertiesNoKeysWithTypes()
        {
            Vertex v = this.graph.addVertex(1);
            List<int> list = new List<int>();
            list.Add(1);
            list.Add(1);
            list.Add(1);

            v.setProperty("keyList", list);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.EXTENDED);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("keyList"));

            IDictionary<string, JToken> listWithTypeAsJson = (JObject)json["keyList"];
            Assert.NotNull(listWithTypeAsJson);
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSONTokens.TYPE));
            Assert.AreEqual(GraphSONTokens.TYPE_LIST, listWithTypeAsJson[GraphSONTokens.TYPE].Value<string>());
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSONTokens.VALUE));
            JArray listAsJSON = (JArray)listWithTypeAsJson[GraphSONTokens.VALUE];
            Assert.NotNull(listAsJSON);
            Assert.AreEqual(3, listAsJSON.Count());

            for (int ix = 0; ix < listAsJSON.Count(); ix++)
            {
                IDictionary<string, JToken> valueAsJson = (JObject)listAsJSON[ix];
                Assert.NotNull(valueAsJson);
                Assert.True(valueAsJson.ContainsKey(GraphSONTokens.TYPE));
                //Porting Note: JSON.NET parse integers as longs
                Assert.AreEqual(GraphSONTokens.TYPE_LONG, valueAsJson[GraphSONTokens.TYPE].Value<string>());
                Assert.True(valueAsJson.ContainsKey(GraphSONTokens.VALUE));
                Assert.AreEqual(1, valueAsJson[GraphSONTokens.VALUE].Value<int>());
            }
        }

        [Test]
        public void jsonFromElementVertexListOfListPropertiesNoKeysWithTypes()
        {
            Vertex v = this.graph.addVertex(1);
            List<int> list = new List<int>();
            list.Add(1);
            list.Add(1);
            list.Add(1);

            List<List<int>> listList = new List<List<int>>();
            listList.Add(list);

            v.setProperty("keyList", listList);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.EXTENDED);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("keyList"));

            IDictionary<string, JToken> listWithTypeAsJson = (JObject)json["keyList"];
            Assert.NotNull(listWithTypeAsJson);
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSONTokens.TYPE));
            Assert.AreEqual(GraphSONTokens.TYPE_LIST, listWithTypeAsJson[GraphSONTokens.TYPE].Value<string>());
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSONTokens.VALUE));
            JArray listAsJSON = (JArray)listWithTypeAsJson[GraphSONTokens.VALUE][0][GraphSONTokens.VALUE];
            Assert.NotNull(listAsJSON);
            Assert.AreEqual(3, listAsJSON.Count());

            for (int ix = 0; ix < listAsJSON.Count(); ix++)
            {
                IDictionary<string, JToken> valueAsJson = (JObject)listAsJSON[ix];
                Assert.NotNull(valueAsJson);
                Assert.True(valueAsJson.ContainsKey(GraphSONTokens.TYPE));
                //Porting Note: JSON.NET parse integers as longs
                Assert.AreEqual(GraphSONTokens.TYPE_LONG, valueAsJson[GraphSONTokens.TYPE].Value<string>());
                Assert.True(valueAsJson.ContainsKey(GraphSONTokens.VALUE));
                Assert.AreEqual(1, valueAsJson[GraphSONTokens.VALUE].Value<long>());
            }
        }

        [Test]
        public void jsonFromElementVertexMapPropertiesNoKeysWithTypes()
        {
            Vertex v = this.graph.addVertex(1);

            var map = new Dictionary<string, object>();
            map.put("this", "some");
            map.put("that", 1);

            v.setProperty("keyMap", map);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.EXTENDED);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSONTokens._ID));
            Assert.AreEqual(1, json[GraphSONTokens._ID].Value<int>());
            Assert.True(json.ContainsKey("keyMap"));

            IDictionary<string, JToken> mapWithTypeAsJSON = (JObject)json["keyMap"];
            Assert.NotNull(mapWithTypeAsJSON);
            Assert.True(mapWithTypeAsJSON.ContainsKey(GraphSONTokens.TYPE));
            Assert.AreEqual(GraphSONTokens.TYPE_MAP, mapWithTypeAsJSON[GraphSONTokens.TYPE].Value<string>());

            Assert.True(mapWithTypeAsJSON.ContainsKey(GraphSONTokens.VALUE));
            IDictionary<string, JToken> mapAsJSON = (JObject)mapWithTypeAsJSON[GraphSONTokens.VALUE];

            Assert.True(mapAsJSON.ContainsKey("this"));
            IDictionary<string, JToken> thisAsJson = (JObject)mapAsJSON["this"];
            Assert.True(thisAsJson.ContainsKey(GraphSONTokens.TYPE));
            Assert.AreEqual(GraphSONTokens.TYPE_STRING, thisAsJson[GraphSONTokens.TYPE].Value<string>());
            Assert.True(thisAsJson.ContainsKey(GraphSONTokens.VALUE));
            Assert.AreEqual("some", thisAsJson[GraphSONTokens.VALUE].Value<string>());

            Assert.True(mapAsJSON.ContainsKey("that"));
            IDictionary<string, JToken> thatAsJson = (JObject)mapAsJSON["that"];
            Assert.True(thatAsJson.ContainsKey(GraphSONTokens.TYPE));
            Assert.AreEqual(GraphSONTokens.TYPE_INTEGER, thatAsJson[GraphSONTokens.TYPE].Value<string>());
            Assert.True(thatAsJson.ContainsKey(GraphSONTokens.VALUE));
            Assert.AreEqual(1, thatAsJson[GraphSONTokens.VALUE].Value<int>());
        }

        [Test]
        public void jsonFromElementNullsNoKeysNoTypes()
        {
            Graph g = new TinkerGraph();
            Vertex v = g.addVertex(1);
            //v.setProperty("key", null);

            var map = new Dictionary<string, object>();
            map.put("innerkey", null);

            List<String> innerList = new List<string>();
            innerList.Add(null);
            innerList.Add("innerstring");
            map.put("list", innerList);

            v.setProperty("keyMap", map);

            List<string> list = new List<string>();
            list.Add(null);
            list.Add("string");
            v.setProperty("keyList", list);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json["key"] == null);

            IDictionary<string, JToken> jsonMap = (JObject)json["keyMap"];
            Assert.NotNull(jsonMap);
            Assert.True(((JValue)jsonMap["innerkey"]).Value == null);

            JArray jsonInnerArray = (JArray)jsonMap["list"];
            Assert.NotNull(jsonInnerArray);
            Assert.True(((JValue)jsonInnerArray[0]).Value == null);
            Assert.AreEqual("innerstring", jsonInnerArray.get(1).Value<string>());

            JArray jsonArray = (JArray)json["keyList"];
            Assert.NotNull(jsonArray);
            Assert.True(((JValue)jsonArray[0]).Value == null);
            Assert.AreEqual("string", jsonArray.get(1).Value<string>());
        }

        [Test]
        public void jsonFromElementNullsNoKeysWithTypes()
        {
            Graph g = new TinkerGraph();
            Vertex v = g.addVertex(1);
            // v.setProperty("key", null);

            var map = new Dictionary<string, object>();
            map.put("innerkey", null);

            List<string> innerList = new List<string>();
            innerList.Add(null);
            innerList.Add("innerstring");
            map.put("list", innerList);

            v.setProperty("keyMap", map);

            List<string> list = new List<string>();
            list.Add(null);
            list.Add("string");
            v.setProperty("keyList", list);

            IDictionary<string, JToken> json = GraphSONUtility.jsonFromElement(v, null, GraphSONMode.EXTENDED);

            Assert.NotNull(json);
            IDictionary<string, JToken> jsonObjectKey = (JObject)json["key"];
            // Assert.assertTrue(jsonObjectKey.isNull(GraphSONTokens.VALUE));
            // Assert.assertEquals(GraphSONTokens.TYPE_UNKNOWN, jsonObjectKey.optString(GraphSONTokens.TYPE));

            IDictionary<string, JToken> jsonMap = (JObject)json["keyMap"][GraphSONTokens.VALUE];
            Assert.NotNull(jsonMap);
            IDictionary<string, JToken> jsonObjectMap = (JObject)jsonMap["innerkey"];
            Assert.True(((JValue)jsonObjectMap[GraphSONTokens.VALUE]).Value == null);
            Assert.AreEqual(GraphSONTokens.TYPE_UNKNOWN, jsonObjectMap[GraphSONTokens.TYPE].Value<string>());

            JArray jsonInnerArray = (JArray)jsonMap["list"][GraphSONTokens.VALUE];
            Assert.NotNull(jsonInnerArray);
            IDictionary<string, JToken> jsonObjectInnerListFirst = (JObject)jsonInnerArray[0];
            Assert.True(((JValue)jsonObjectInnerListFirst[GraphSONTokens.VALUE]).Value == null);
            Assert.AreEqual(GraphSONTokens.TYPE_UNKNOWN, jsonObjectInnerListFirst[GraphSONTokens.TYPE].Value<string>());

            JArray jsonArray = (JArray)json["keyList"][GraphSONTokens.VALUE];
            Assert.NotNull(jsonArray);
            IDictionary<string, JToken> jsonObjectListFirst = (JObject)jsonArray[0];
            Assert.True(((JValue)jsonObjectListFirst[GraphSONTokens.VALUE]).Value == null);
            Assert.AreEqual(GraphSONTokens.TYPE_UNKNOWN, jsonObjectListFirst[GraphSONTokens.TYPE].Value<string>());
        }

        [Test]
        public void vertexFromJsonValid()
        {
            Graph g = new TinkerGraph();
            ElementFactory factory = new GraphElementFactory(g);

            Vertex v = GraphSONUtility.vertexFromJson((JObject)JsonConvert.DeserializeObject(vertexJson1), factory, GraphSONMode.NORMAL, null);

            Assert.AreSame(v, g.getVertex(1));

            // tinkergraph converts id to string
            Assert.AreEqual("1", v.getId());
            Assert.AreEqual("marko", v.getProperty("name"));
            Assert.AreEqual(29, v.getProperty("age"));
        }

        [Test]
        public void vertexFromJsonStringValid()
        {
            Graph g = new TinkerGraph();
            ElementFactory factory = new GraphElementFactory(g);

            Vertex v = GraphSONUtility.vertexFromJson(vertexJson1, factory, GraphSONMode.NORMAL, null);

            Assert.AreSame(v, g.getVertex(1));

            // tinkergraph converts id to string
            Assert.AreEqual("1", v.getId());
            Assert.AreEqual("marko", v.getProperty("name"));
            Assert.AreEqual(29, v.getProperty("age"));
        }

        [Test]
        public void vertexFromJsonInputStreamValid()
        {
            Graph g = new TinkerGraph();
            ElementFactory factory = new GraphElementFactory(g);

            Vertex v = GraphSONUtility.vertexFromJson(inputStreamVertexJson1, factory, GraphSONMode.NORMAL, null);

            Assert.AreSame(v, g.getVertex(1));

            // tinkergraph converts id to string
            Assert.AreEqual("1", v.getId());
            Assert.AreEqual("marko", v.getProperty("name"));
            Assert.AreEqual(29, v.getProperty("age"));
        }

        [Test]
        public void vertexFromJsonIgnoreKeyValid()
        {
            Graph g = new TinkerGraph();
            ElementFactory factory = new GraphElementFactory(g);

            HashSet<string> ignoreAge = new HashSet<string>();
            ignoreAge.Add("age");
            ElementPropertyConfig config = ElementPropertyConfig.ExcludeProperties(ignoreAge, null);
            GraphSONUtility utility = new GraphSONUtility(GraphSONMode.NORMAL, factory, config);
            Vertex v = utility.vertexFromJson((JObject)JsonConvert.DeserializeObject(vertexJson1));

            Assert.AreSame(v, g.getVertex(1));

            // tinkergraph converts id to string
            Assert.AreEqual("1", v.getId());
            Assert.AreEqual("marko", v.getProperty("name"));
            Assert.Null(v.getProperty("age"));
        }

        [Test]
        public void edgeFromJsonValid()
        {
            Graph g = new TinkerGraph();
            ElementFactory factory = new GraphElementFactory(g);

            Vertex v1 = GraphSONUtility.vertexFromJson((JObject)JsonConvert.DeserializeObject(vertexJson1), factory, GraphSONMode.NORMAL, null);
            Vertex v2 = GraphSONUtility.vertexFromJson((JObject)JsonConvert.DeserializeObject(vertexJson2), factory, GraphSONMode.NORMAL, null);
            Edge e = GraphSONUtility.edgeFromJson((JObject)JsonConvert.DeserializeObject(edgeJson), v1, v2, factory, GraphSONMode.NORMAL, null);

            Assert.AreSame(v1, g.getVertex(1));
            Assert.AreSame(v2, g.getVertex(2));
            Assert.AreSame(e, g.getEdge(7));

            // tinkergraph converts id to string
            Assert.AreEqual("7", e.getId());
            Assert.AreEqual(0.5d, e.getProperty("weight"));
            Assert.AreEqual("knows", e.getLabel());
            Assert.AreEqual(v1, e.getVertex(Direction.OUT));
            Assert.AreEqual(v2, e.getVertex(Direction.IN));
        }

        [Test]
        public void edgeFromJsonStringValid()
        {
            Graph g = new TinkerGraph();
            ElementFactory factory = new GraphElementFactory(g);

            Vertex v1 = GraphSONUtility.vertexFromJson(vertexJson1, factory, GraphSONMode.NORMAL, null);
            Vertex v2 = GraphSONUtility.vertexFromJson(vertexJson2, factory, GraphSONMode.NORMAL, null);
            Edge e = GraphSONUtility.edgeFromJson(edgeJson, v1, v2, factory, GraphSONMode.NORMAL, null);

            Assert.AreSame(v1, g.getVertex(1));
            Assert.AreSame(v2, g.getVertex(2));
            Assert.AreSame(e, g.getEdge(7));

            // tinkergraph converts id to string
            Assert.AreEqual("7", e.getId());
            Assert.AreEqual(0.5d, e.getProperty("weight"));
            Assert.AreEqual("knows", e.getLabel());
            Assert.AreEqual(v1, e.getVertex(Direction.OUT));
            Assert.AreEqual(v2, e.getVertex(Direction.IN));
        }

        [Test]
        public void edgeFromJsonIgnoreWeightValid()
        {
            Graph g = new TinkerGraph();
            ElementFactory factory = new GraphElementFactory(g);

            Vertex v1 = GraphSONUtility.vertexFromJson((JObject)JsonConvert.DeserializeObject(vertexJson1), factory, GraphSONMode.NORMAL, null);
            Vertex v2 = GraphSONUtility.vertexFromJson((JObject)JsonConvert.DeserializeObject(vertexJson2), factory, GraphSONMode.NORMAL, null);

            HashSet<string> ignoreWeight = new HashSet<string>();
            ignoreWeight.Add("weight");
            ElementPropertyConfig config = ElementPropertyConfig.ExcludeProperties(null, ignoreWeight);
            GraphSONUtility utility = new GraphSONUtility(GraphSONMode.NORMAL, factory, config);
            Edge e = utility.edgeFromJson((JObject)JsonConvert.DeserializeObject(edgeJson), v1, v2);

            Assert.AreSame(v1, g.getVertex(1));
            Assert.AreSame(v2, g.getVertex(2));
            Assert.AreSame(e, g.getEdge(7));

            // tinkergraph converts id to string
            Assert.AreEqual("7", e.getId());
            Assert.Null(e.getProperty("weight"));
            Assert.AreEqual("knows", e.getLabel());
            Assert.AreEqual(v1, e.getVertex(Direction.OUT));
            Assert.AreEqual(v2, e.getVertex(Direction.IN));
        }

        [Test]
        public void edgeFromJsonNormalLabelOrIdOnEdge()
        {
            Graph g = new TinkerGraph();
            ElementFactory factory = new GraphElementFactory(g);

            Vertex v1 = GraphSONUtility.vertexFromJson((JObject)JsonConvert.DeserializeObject(vertexJson1), factory, GraphSONMode.NORMAL, null);
            Vertex v2 = GraphSONUtility.vertexFromJson((JObject)JsonConvert.DeserializeObject(vertexJson2), factory, GraphSONMode.NORMAL, null);
            Edge e = GraphSONUtility.edgeFromJson((JObject)JsonConvert.DeserializeObject(edgeJsonLight), v1, v2, factory, GraphSONMode.NORMAL, null);

            Assert.AreSame(v1, g.getVertex(1));
            Assert.AreSame(v2, g.getVertex(2));
            Assert.AreSame(e, g.getEdge(0));
        }

        [Test]
        public void edgeFromJsonInputStreamCompactLabelOrIdOnEdge()
        {
            Graph g = new TinkerGraph();
            ElementFactory factory = new GraphElementFactory(g);

            Vertex v1 = GraphSONUtility.vertexFromJson((JObject)JsonConvert.DeserializeObject(vertexJson1), factory, GraphSONMode.COMPACT, null);
            Vertex v2 = GraphSONUtility.vertexFromJson((JObject)JsonConvert.DeserializeObject(vertexJson2), factory, GraphSONMode.COMPACT, null);
            Edge e = GraphSONUtility.edgeFromJson(inputStreamEdgeJsonLight, v1, v2, factory, GraphSONMode.COMPACT, null);

            Assert.AreSame(v1, g.getVertex(1));
            Assert.AreSame(v2, g.getVertex(2));
            Assert.AreSame(e, g.getEdge(0));
        }

        [Test]
        public void edgeFromJsonInputStreamCompactNoIdOnEdge()
        {
            Graph g = new TinkerGraph();
            ElementFactory factory = new GraphElementFactory(g);

            HashSet<string> vertexKeys = new HashSet<string>() {
                GraphSONTokens._ID
            };

            HashSet<string> edgeKeys = new HashSet<string>() {
                GraphSONTokens._IN_V
            };

            GraphSONUtility graphson = new GraphSONUtility(GraphSONMode.COMPACT, factory, vertexKeys, edgeKeys);

            Vertex v1 = graphson.vertexFromJson((JObject)JsonConvert.DeserializeObject(vertexJson1));
            Vertex v2 = graphson.vertexFromJson((JObject)JsonConvert.DeserializeObject(vertexJson2));
            Edge e = graphson.edgeFromJson(inputStreamEdgeJsonLight, v1, v2);

            Assert.AreSame(v1, g.getVertex(1));
            Assert.AreSame(v2, g.getVertex(2));
            Assert.AreSame(e, g.getEdge(0));
        }

        [JsonConverter(typeof(CatConverter))]
        public class Cat
        {
            readonly string _name;

            public Cat(string name)
            {
                _name = name;
            }

            public string getName()
            {
                return _name;
            }

            public override string ToString()
            {
                return _name;
            }
        }

        public class CatConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return true;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return new Cat(reader.ReadAsString());
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }
        }
    }
}
