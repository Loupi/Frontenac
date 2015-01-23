using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Frontenac.Blueprints.Impls.TG;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    [TestFixture(Category = "GraphSONUtilityTest")]
    public class GraphSonUtilityTest : IDisposable
    {
        [SetUp]
        public void Setup()
        {
            _tinkerGrapĥ.Clear();
            _inputStreamVertexJson1 = new MemoryStream(Encoding.Default.GetBytes(VertexJson1));
            _inputStreamEdgeJsonLight = new MemoryStream(Encoding.Default.GetBytes(EdgeJsonLight));
        }

        [TearDown]
        public void TearDown()
        {
            Dispose();
        }

        private readonly TinkerGrapĥ _tinkerGrapĥ = new TinkerGrapĥ();

        private const string VertexJson1 = "{\"name\":\"marko\",\"age\":29,\"_id\":1,\"_type\":\"vertex\"}";
        private const string VertexJson2 = "{\"name\":\"vadas\",\"age\":27,\"_id\":2,\"_type\":\"vertex\"}";

        private const string EdgeJsonLight = "{\"weight\":0.5,\"_outV\":1,\"_inV\":2,\"_label\":\"knows\"}";

        private const string EdgeJson =
            "{\"weight\":0.5,\"_id\":7,\"_type\":\"edge\",\"_outV\":1,\"_inV\":2,\"_label\":\"knows\"}";

        private Stream _inputStreamVertexJson1;
        private Stream _inputStreamEdgeJsonLight;

        private bool _disposed;

        ~GraphSonUtilityTest()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _tinkerGrapĥ.Shutdown();
                _inputStreamVertexJson1.Dispose();
                _inputStreamEdgeJsonLight.Dispose();
            }

            _disposed = true;
        }

        [JsonConverter(typeof (CatConverter))]
        public class Cat
        {
            private readonly string _name;

            public Cat(string name)
            {
                _name = name;
            }

            public string GetName()
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

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                                            JsonSerializer serializer)
            {
                return new Cat(reader.ReadAsString());
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }
        }

        [Test]
        public void EdgeFromJsonIgnoreWeightValid()
        {
            var g = new TinkerGrapĥ();
            var factory = new GraphElementFactory(g);

            var v1 = GraphSonUtility.VertexFromJson((JObject) JsonConvert.DeserializeObject(VertexJson1), factory,
                                                    GraphSonMode.NORMAL, null);
            var v2 = GraphSonUtility.VertexFromJson((JObject) JsonConvert.DeserializeObject(VertexJson2), factory,
                                                    GraphSonMode.NORMAL, null);

            var ignoreWeight = new HashSet<string> {"weight"};
            var config = ElementPropertyConfig.ExcludeProperties(null, ignoreWeight);
            var utility = new GraphSonUtility(GraphSonMode.NORMAL, factory, config);
            var e = utility.EdgeFromJson((JObject) JsonConvert.DeserializeObject(EdgeJson), v1, v2);

            Assert.AreSame(v1, g.GetVertex(1));
            Assert.AreSame(v2, g.GetVertex(2));
            Assert.AreSame(e, g.GetEdge(7));

            // tinkergraph converts id to string
            Assert.AreEqual("7", e.Id);
            Assert.Null(e.GetProperty("weight"));
            Assert.AreEqual("knows", e.Label);
            Assert.AreEqual(v1, e.GetVertex(Direction.Out));
            Assert.AreEqual(v2, e.GetVertex(Direction.In));
        }

        [Test]
        public void EdgeFromJsonInputStreamCompactLabelOrIdOnEdge()
        {
            var g = new TinkerGrapĥ();
            var factory = new GraphElementFactory(g);

            var v1 = GraphSonUtility.VertexFromJson((JObject) JsonConvert.DeserializeObject(VertexJson1), factory,
                                                    GraphSonMode.COMPACT, null);
            var v2 = GraphSonUtility.VertexFromJson((JObject) JsonConvert.DeserializeObject(VertexJson2), factory,
                                                    GraphSonMode.COMPACT, null);
            var e = GraphSonUtility.EdgeFromJson(_inputStreamEdgeJsonLight, v1, v2, factory, GraphSonMode.COMPACT, null);

            Assert.AreSame(v1, g.GetVertex(1));
            Assert.AreSame(v2, g.GetVertex(2));
            Assert.AreSame(e, g.GetEdge(0));
        }

        [Test]
        public void EdgeFromJsonInputStreamCompactNoIdOnEdge()
        {
            IGraph g = new TinkerGrapĥ();
            IElementFactory factory = new GraphElementFactory(g);

            var vertexKeys = new HashSet<string> {GraphSonTokens.Id};
            var edgeKeys = new HashSet<string> {GraphSonTokens.InV};

            var graphson = new GraphSonUtility(GraphSonMode.COMPACT, factory, vertexKeys, edgeKeys);

            var v1 = graphson.VertexFromJson((JObject) JsonConvert.DeserializeObject(VertexJson1));
            var v2 = graphson.VertexFromJson((JObject) JsonConvert.DeserializeObject(VertexJson2));
            var e = graphson.EdgeFromJson(_inputStreamEdgeJsonLight, v1, v2);

            Assert.AreSame(v1, g.GetVertex(1));
            Assert.AreSame(v2, g.GetVertex(2));
            Assert.AreSame(e, g.GetEdge(0));
        }

        [Test]
        public void EdgeFromJsonNormalLabelOrIdOnEdge()
        {
            var g = new TinkerGrapĥ();
            var factory = new GraphElementFactory(g);

            var v1 = GraphSonUtility.VertexFromJson((JObject) JsonConvert.DeserializeObject(VertexJson1), factory,
                                                    GraphSonMode.NORMAL, null);
            var v2 = GraphSonUtility.VertexFromJson((JObject) JsonConvert.DeserializeObject(VertexJson2), factory,
                                                    GraphSonMode.NORMAL, null);
            var e = GraphSonUtility.EdgeFromJson((JObject) JsonConvert.DeserializeObject(EdgeJsonLight), v1, v2, factory,
                                                 GraphSonMode.NORMAL, null);

            Assert.AreSame(v1, g.GetVertex(1));
            Assert.AreSame(v2, g.GetVertex(2));
            Assert.AreSame(e, g.GetEdge(0));
        }

        [Test]
        public void EdgeFromJsonStringValid()
        {
            var g = new TinkerGrapĥ();
            var factory = new GraphElementFactory(g);

            var v1 = GraphSonUtility.VertexFromJson(VertexJson1, factory, GraphSonMode.NORMAL, null);
            var v2 = GraphSonUtility.VertexFromJson(VertexJson2, factory, GraphSonMode.NORMAL, null);
            var e = GraphSonUtility.EdgeFromJson(EdgeJson, v1, v2, factory, GraphSonMode.NORMAL, null);

            Assert.AreSame(v1, g.GetVertex(1));
            Assert.AreSame(v2, g.GetVertex(2));
            Assert.AreSame(e, g.GetEdge(7));

            // tinkergraph converts id to string
            Assert.AreEqual("7", e.Id);
            Assert.AreEqual(0.5d, e.GetProperty("weight"));
            Assert.AreEqual("knows", e.Label);
            Assert.AreEqual(v1, e.GetVertex(Direction.Out));
            Assert.AreEqual(v2, e.GetVertex(Direction.In));
        }

        [Test]
        public void EdgeFromJsonValid()
        {
            var g = new TinkerGrapĥ();
            var factory = new GraphElementFactory(g);

            var v1 = GraphSonUtility.VertexFromJson((JObject) JsonConvert.DeserializeObject(VertexJson1), factory,
                                                    GraphSonMode.NORMAL, null);
            var v2 = GraphSonUtility.VertexFromJson((JObject) JsonConvert.DeserializeObject(VertexJson2), factory,
                                                    GraphSonMode.NORMAL, null);
            var e = GraphSonUtility.EdgeFromJson((JObject) JsonConvert.DeserializeObject(EdgeJson), v1, v2, factory,
                                                 GraphSonMode.NORMAL, null);

            Assert.AreSame(v1, g.GetVertex(1));
            Assert.AreSame(v2, g.GetVertex(2));
            Assert.AreSame(e, g.GetEdge(7));

            // tinkergraph converts id to string
            Assert.AreEqual("7", e.Id);
            Assert.AreEqual(0.5d, e.GetProperty("weight"));
            Assert.AreEqual("knows", e.Label);
            Assert.AreEqual(v1, e.GetVertex(Direction.Out));
            Assert.AreEqual(v2, e.GetVertex(Direction.In));
        }

        [Test]
        public void JsonFromElementBooleanArrayPropertyNoKeysNoTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            var booleanArray = new[] {true, false, true};

            v.SetProperty("keyBooleanArray", booleanArray);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("keyBooleanArray"));

            var booleanArrayAsJson = (JArray) json["keyBooleanArray"];
            Assert.NotNull(booleanArrayAsJson);
            Assert.AreEqual(3, booleanArrayAsJson.Count());
        }

        [Test]
        public void JsonFromElementCrazyPropertyNoKeysNoTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            var mix = new List<object> {new Cat("smithers"), true};

            var deepCats = new List<object> {new Cat("mcallister")};
            mix.Add(deepCats);

            var map = new Dictionary<string, object>();
            map.Put("crazy", mix);

            var someInts = new[] {1, 2, 3};
            map.Put("ints", someInts);

            map.Put("regular", "stuff");

            var innerMap = new Dictionary<string, object>();
            innerMap.Put("me", "you");

            map.Put("inner", innerMap);

            v.SetProperty("crazy-map", map);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("crazy-map"));

            IDictionary<string, JToken> mapAsJson = (JObject) json["crazy-map"];
            Assert.True(mapAsJson.ContainsKey("regular"));
            Assert.AreEqual("stuff", mapAsJson["regular"].Value<string>());

            Assert.True(mapAsJson.ContainsKey("ints"));
            var intArrayAsJson = (JArray) mapAsJson["ints"];
            Assert.NotNull(intArrayAsJson);
            Assert.AreEqual(3, intArrayAsJson.Count());

            Assert.True(mapAsJson.ContainsKey("crazy"));
            var deepListAsJson = (JArray) mapAsJson["crazy"];
            Assert.NotNull(deepListAsJson);
            Assert.AreEqual(3, deepListAsJson.Count());

            Assert.True(mapAsJson.ContainsKey("inner"));
            IDictionary<string, JToken> mapInMapAsJson = (JObject) mapAsJson["inner"];
            Assert.NotNull(mapInMapAsJson);
            Assert.True(mapInMapAsJson.ContainsKey("me"));
            Assert.AreEqual("you", mapInMapAsJson["me"].Value<string>());
        }

        [Test]
        public void JsonFromElementEdgeCompactAllKeys()
        {
            var v1 = _tinkerGrapĥ.AddVertex(1);
            var v2 = _tinkerGrapĥ.AddVertex(2);

            var e = _tinkerGrapĥ.AddEdge(3, v1, v2, "test");
            e.SetProperty("weight", 0.5);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(e, null, GraphSonMode.COMPACT);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.True(json.ContainsKey(GraphSonTokens.UnderscoreType));
            Assert.True(json.ContainsKey(GraphSonTokens.Label));
            Assert.True(json.ContainsKey(GraphSonTokens.InV));
            Assert.True(json.ContainsKey(GraphSonTokens.OutV));
            Assert.AreEqual(0.5d, json["weight"].Value<double>(), 0.0d);
        }

        [Test]
        public void JsonFromElementEdgeCompactIdOnlyAsExclude()
        {
            var factory = new GraphElementFactory(_tinkerGrapĥ);
            var v1 = _tinkerGrapĥ.AddVertex(1);
            var v2 = _tinkerGrapĥ.AddVertex(2);

            var e = _tinkerGrapĥ.AddEdge(3, v1, v2, "test");
            e.SetProperty("weight", 0.5);
            e.SetProperty("x", "y");

            var propertiesToExclude = new HashSet<string>
                {
                    GraphSonTokens.UnderscoreType,
                    GraphSonTokens.Label,
                    GraphSonTokens.InV,
                    GraphSonTokens.OutV,
                    "weight"
                };

            var config = new ElementPropertyConfig(null, propertiesToExclude,
                                                   ElementPropertyConfig.ElementPropertiesRule.Include,
                                                   ElementPropertyConfig.ElementPropertiesRule.Exclude);
            var utility = new GraphSonUtility(GraphSonMode.COMPACT, factory, config);
            var json = utility.JsonFromElement(e) as IDictionary<string, JToken>;

            Assert.NotNull(json);
            Assert.False(json.ContainsKey(GraphSonTokens.UnderscoreType));
            Assert.False(json.ContainsKey(GraphSonTokens.Label));
            Assert.False(json.ContainsKey(GraphSonTokens.InV));
            Assert.False(json.ContainsKey(GraphSonTokens.OutV));
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.False(json.ContainsKey("weight"));
            Assert.True(json.ContainsKey("x"));
            Assert.AreEqual("y", json["x"].Value<string>());
        }

        [Test]
        public void JsonFromElementEdgeCompactIdOnlyAsInclude()
        {
            var v1 = _tinkerGrapĥ.AddVertex(1);
            var v2 = _tinkerGrapĥ.AddVertex(2);

            var e = _tinkerGrapĥ.AddEdge(3, v1, v2, "test");
            e.SetProperty("weight", 0.5);

            var propertiesToInclude = new HashSet<String> {GraphSonTokens.Id};

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(e, propertiesToInclude,
                                                                               GraphSonMode.COMPACT);

            Assert.NotNull(json);
            Assert.False(json.ContainsKey(GraphSonTokens.UnderscoreType));
            Assert.False(json.ContainsKey(GraphSonTokens.Label));
            Assert.False(json.ContainsKey(GraphSonTokens.InV));
            Assert.False(json.ContainsKey(GraphSonTokens.OutV));
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.False(json.ContainsKey("weight"));
        }

        [Test]
        public void JsonFromElementEdgeNoPropertiesNoKeysNoTypes()
        {
            var v1 = _tinkerGrapĥ.AddVertex(1);
            var v2 = _tinkerGrapĥ.AddVertex(2);

            var e = _tinkerGrapĥ.AddEdge(3, v1, v2, "test");
            e.SetProperty("weight", 0.5);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(e, null, GraphSonMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(3, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey(GraphSonTokens.Label));
            Assert.AreEqual("test", json[GraphSonTokens.Label].Value<string>());
            Assert.True(json.ContainsKey(GraphSonTokens.UnderscoreType));
            Assert.AreEqual("edge", json[GraphSonTokens.UnderscoreType].Value<string>());
            Assert.True(json.ContainsKey(GraphSonTokens.InV));
            Assert.AreEqual(2, json[GraphSonTokens.InV].Value<int>());
            Assert.True(json.ContainsKey(GraphSonTokens.OutV));
            Assert.AreEqual(1, json[GraphSonTokens.OutV].Value<int>());
            Assert.True(json.ContainsKey("weight"));
            Assert.AreEqual(0.5d, json["weight"].Value<double>(), 0.0d);
        }

        [Test]
        public void JsonFromElementFloatArrayPropertyNoKeysNoTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            var floatArray = new[] {1.0, 2.0, 3.0};

            v.SetProperty("keyFloatArray", floatArray);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("keyFloatArray"));

            var floatArrayAsJson = (JArray) json["keyFloatArray"];
            Assert.NotNull(floatArrayAsJson);
            Assert.AreEqual(3, floatArrayAsJson.Count());
        }

        [Test]
        public void JsonFromElementNullsNoKeysNoTypes()
        {
            var g = new TinkerGrapĥ();
            var v = g.AddVertex(1);

            var map = new Dictionary<string, object>();
            map.Put("innerkey", null);

            var innerList = new List<string> {null, "innerstring"};
            map.Put("list", innerList);

            v.SetProperty("keyMap", map);

            var list = new List<string> {null, "string"};
            v.SetProperty("keyList", list);

            var json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json["key"] == null);

            var jsonMap = (JObject) json["keyMap"];
            Assert.NotNull(jsonMap);
            Assert.True(((JValue) jsonMap["innerkey"]).Value == null);

            var jsonInnerArray = (JArray) jsonMap["list"];
            Assert.NotNull(jsonInnerArray);
            Assert.True(((JValue) jsonInnerArray[0]).Value == null);
            Assert.AreEqual("innerstring", jsonInnerArray.Get(1).Value<string>());

            var jsonArray = (JArray) json["keyList"];
            Assert.NotNull(jsonArray);
            Assert.True(((JValue) jsonArray[0]).Value == null);
            Assert.AreEqual("string", jsonArray.Get(1).Value<string>());
        }

        [Test]
        public void JsonFromElementNullsNoKeysWithTypes()
        {
            var g = new TinkerGrapĥ();
            var v = g.AddVertex(1);

            var map = new Dictionary<string, object>();
            map.Put("innerkey", null);

            var innerList = new List<string> {null, "innerstring"};
            map.Put("list", innerList);

            v.SetProperty("keyMap", map);

            var list = new List<string> {null, "string"};
            v.SetProperty("keyList", list);

            var json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.EXTENDED);

            Assert.NotNull(json);

            var jsonMap = (JObject) json["keyMap"][GraphSonTokens.Value];
            Assert.NotNull(jsonMap);
            var jsonObjectMap = (JObject) jsonMap["innerkey"];
            Assert.True(((JValue) jsonObjectMap[GraphSonTokens.Value]).Value == null);
            Assert.AreEqual(GraphSonTokens.TypeUnknown, jsonObjectMap[GraphSonTokens.Type].Value<string>());

            var jsonInnerArray = (JArray) jsonMap["list"][GraphSonTokens.Value];
            Assert.NotNull(jsonInnerArray);
            var jsonObjectInnerListFirst = (JObject) jsonInnerArray[0];
            Assert.True(((JValue) jsonObjectInnerListFirst[GraphSonTokens.Value]).Value == null);
            Assert.AreEqual(GraphSonTokens.TypeUnknown, jsonObjectInnerListFirst[GraphSonTokens.Type].Value<string>());

            var jsonArray = (JArray) json["keyList"][GraphSonTokens.Value];
            Assert.NotNull(jsonArray);
            var jsonObjectListFirst = (JObject) jsonArray[0];
            Assert.True(((JValue) jsonObjectListFirst[GraphSonTokens.Value]).Value == null);
            Assert.AreEqual(GraphSonTokens.TypeUnknown, jsonObjectListFirst[GraphSonTokens.Type].Value<string>());
        }

        [Test]
        public void JsonFromElementVertexBooleanListPropertiesNoKeysWithTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            var list = new List<bool> {true, true, true};

            v.SetProperty("keyList", list);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.EXTENDED);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("keyList"));

            IDictionary<string, JToken> listWithTypeAsJson = (JObject) json["keyList"];
            Assert.NotNull(listWithTypeAsJson);
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSonTokens.Type));
            Assert.AreEqual(GraphSonTokens.TypeList, listWithTypeAsJson[GraphSonTokens.Type].Value<string>());
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSonTokens.Value));
            var listAsJson = (JArray) listWithTypeAsJson[GraphSonTokens.Value];
            Assert.NotNull(listAsJson);
            Assert.AreEqual(3, listAsJson.Count());

            for (var ix = 0; ix < listAsJson.Count(); ix++)
            {
                IDictionary<string, JToken> valueAsJson = (JObject) listAsJson[ix];
                Assert.NotNull(valueAsJson);
                Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Type));
                Assert.AreEqual(GraphSonTokens.TypeBoolean, valueAsJson[GraphSonTokens.Type].Value<string>());
                Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Value));
                Assert.AreEqual(true, valueAsJson[GraphSonTokens.Value].Value<bool>());
            }
        }

        [Test]
        public void JsonFromElementVertexCatArrayPropertyNoKeysNoTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            var cats = new List<Cat> {new Cat("smithers"), new Cat("mcallister")};

            v.SetProperty("cats", cats);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("cats"));

            var catListAsJson = (JArray) json["cats"];
            Assert.NotNull(catListAsJson);
            Assert.AreEqual(2, catListAsJson.Count());
        }

        [Test]
        public void JsonFromElementVertexCatPropertyNoKeysNoTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            v.SetProperty("mycat", new Cat("smithers"));

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("mycat"));
            Assert.AreEqual("smithers", json["mycat"].Value<string>());
        }

        [Test]
        public void JsonFromElementVertexCatPropertyNoKeysWithTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            v.SetProperty("mycat", new Cat("smithers"));

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.EXTENDED);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("mycat"));
            IDictionary<string, JToken> jsonObjectCat = (JObject) json["mycat"];
            Assert.True(jsonObjectCat.ContainsKey("value"));
            Assert.AreEqual("smithers", jsonObjectCat["value"].Value<string>());
        }

        [Test]
        public void JsonFromElementVertexCompactAllOnly()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            v.SetProperty("name", "marko");

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.COMPACT);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.UnderscoreType));
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.True(json.ContainsKey("name"));
        }

        [Test]
        public void JsonFromElementVertexCompactIdNameOnlyAsExclude()
        {
            var factory = new GraphElementFactory(_tinkerGrapĥ);
            var v = _tinkerGrapĥ.AddVertex(1);
            v.SetProperty("name", "marko");

            var propertiesToExclude = new HashSet<String> {GraphSonTokens.UnderscoreType};

            var config = new ElementPropertyConfig(propertiesToExclude, null,
                                                   ElementPropertyConfig.ElementPropertiesRule.Exclude,
                                                   ElementPropertyConfig.ElementPropertiesRule.Exclude);

            var utility = new GraphSonUtility(GraphSonMode.COMPACT, factory, config);
            IDictionary<string, JToken> json = utility.JsonFromElement(v);

            Assert.NotNull(json);
            Assert.False(json.ContainsKey(GraphSonTokens.UnderscoreType));
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.True(json.ContainsKey("name"));
        }

        [Test]
        public void JsonFromElementVertexCompactIdOnlyAsInclude()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            v.SetProperty("name", "marko");

            var propertiesToInclude = new HashSet<String>
                {
                    GraphSonTokens.Id
                };

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, propertiesToInclude,
                                                                               GraphSonMode.COMPACT);

            Assert.NotNull(json);
            Assert.False(json.ContainsKey(GraphSonTokens.UnderscoreType));
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.False(json.ContainsKey("name"));
        }

        [Test]
        public void JsonFromElementVertexDoubleArrayPropertyNoKeysNoTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            var doubleArray = new[] {1.0, 2.0, 3.0};

            v.SetProperty("keyDoubleArray", doubleArray);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("keyDoubleArray"));

            var doubleArrayAsJson = (JArray) json["keyDoubleArray"];
            Assert.NotNull(doubleArrayAsJson);
            Assert.AreEqual(3, doubleArrayAsJson.Count());
        }

        [Test]
        public void JsonFromElementVertexIntArrayPropertyNoKeysNoTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            var intArray = new[] {1, 2, 3};

            v.SetProperty("keyIntArray", intArray);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("keyIntArray"));

            var intArrayAsJson = (JArray) json["keyIntArray"];
            Assert.NotNull(intArrayAsJson);
            Assert.AreEqual(3, intArrayAsJson.Count());
        }

        [Test]
        public void JsonFromElementVertexIntListPropertiesNoKeysWithTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            var list = new List<int> {1, 1, 1};

            v.SetProperty("keyList", list);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.EXTENDED);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("keyList"));

            IDictionary<string, JToken> listWithTypeAsJson = (JObject) json["keyList"];
            Assert.NotNull(listWithTypeAsJson);
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSonTokens.Type));
            Assert.AreEqual(GraphSonTokens.TypeList, listWithTypeAsJson[GraphSonTokens.Type].Value<string>());
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSonTokens.Value));
            var listAsJson = (JArray) listWithTypeAsJson[GraphSonTokens.Value];
            Assert.NotNull(listAsJson);
            Assert.AreEqual(3, listAsJson.Count());

            for (var ix = 0; ix < listAsJson.Count(); ix++)
            {
                IDictionary<string, JToken> valueAsJson = (JObject) listAsJson[ix];
                Assert.NotNull(valueAsJson);
                Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Type));
                //Porting Note: JSON.NET parse integers as longs
                Assert.AreEqual(GraphSonTokens.TypeLong, valueAsJson[GraphSonTokens.Type].Value<string>());
                Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Value));
                Assert.AreEqual(1, valueAsJson[GraphSonTokens.Value].Value<int>());
            }
        }

        [Test]
        public void JsonFromElementVertexListOfListPropertiesNoKeysWithTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            var list = new List<int> {1, 1, 1};

            var listList = new List<List<int>> {list};

            v.SetProperty("keyList", listList);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.EXTENDED);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("keyList"));

            IDictionary<string, JToken> listWithTypeAsJson = (JObject) json["keyList"];
            Assert.NotNull(listWithTypeAsJson);
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSonTokens.Type));
            Assert.AreEqual(GraphSonTokens.TypeList, listWithTypeAsJson[GraphSonTokens.Type].Value<string>());
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSonTokens.Value));
            var listAsJson = (JArray) listWithTypeAsJson[GraphSonTokens.Value][0][GraphSonTokens.Value];
            Assert.NotNull(listAsJson);
            Assert.AreEqual(3, listAsJson.Count());

            for (var ix = 0; ix < listAsJson.Count(); ix++)
            {
                IDictionary<string, JToken> valueAsJson = (JObject) listAsJson[ix];
                Assert.NotNull(valueAsJson);
                Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Type));
                Assert.AreEqual(GraphSonTokens.TypeLong, valueAsJson[GraphSonTokens.Type].Value<string>());
                Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Value));
                Assert.AreEqual(1, valueAsJson[GraphSonTokens.Value].Value<long>());
            }
        }

        [Test]
        public void JsonFromElementVertexListPropertiesNoKeysWithTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            var list = new List<string> {"this", "this", "this"};

            v.SetProperty("keyList", list);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.EXTENDED);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("keyList"));

            IDictionary<string, JToken> listWithTypeAsJson = (JObject) json["keyList"];
            Assert.NotNull(listWithTypeAsJson);
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSonTokens.Type));
            Assert.AreEqual(GraphSonTokens.TypeList, listWithTypeAsJson[GraphSonTokens.Type].Value<string>());
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSonTokens.Value));
            var listAsJson = (JArray) listWithTypeAsJson[GraphSonTokens.Value];
            Assert.NotNull(listAsJson);
            Assert.AreEqual(3, listAsJson.Count());

            for (var ix = 0; ix < listAsJson.Count(); ix++)
            {
                IDictionary<string, JToken> valueAsJson = (JObject) listAsJson[ix];
                Assert.NotNull(valueAsJson);
                Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Type));
                Assert.AreEqual(GraphSonTokens.TypeString, valueAsJson[GraphSonTokens.Type].Value<string>());
                Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Value));
                Assert.AreEqual("this", valueAsJson[GraphSonTokens.Value].Value<string>());
            }
        }

        [Test]
        public void JsonFromElementVertexListPropertyNoKeysNoTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            var list = new List<object> {"this", "that", "other", true};

            v.SetProperty("keyList", list);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("keyList"));

            IList<JToken> listAsJson = (JArray) json["keyList"];
            Assert.NotNull(listAsJson);
            Assert.AreEqual(4, listAsJson.Count());
        }

        [Test]
        public void JsonFromElementVertexLongArrayPropertyNoKeysNoTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            var longArray = new long[] {1, 2, 3};

            v.SetProperty("keyLongArray", longArray);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("keyLongArray"));

            var longArrayAsJson = (JArray) json["keyLongArray"];
            Assert.NotNull(longArrayAsJson);
            Assert.AreEqual(3, longArrayAsJson.Count());
        }

        [Test]
        public void JsonFromElementVertexLongListPropertiesNoKeysWithTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            var list = new List<long> {1000L, 1000L, 1000L};

            v.SetProperty("keyList", list);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.EXTENDED);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("keyList"));

            IDictionary<string, JToken> listWithTypeAsJson = (JObject) json["keyList"];
            Assert.NotNull(listWithTypeAsJson);
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSonTokens.Type));
            Assert.AreEqual(GraphSonTokens.TypeList, listWithTypeAsJson[GraphSonTokens.Type].Value<string>());
            Assert.True(listWithTypeAsJson.ContainsKey(GraphSonTokens.Value));
            var listAsJson = (JArray) listWithTypeAsJson[GraphSonTokens.Value];
            Assert.NotNull(listAsJson);
            Assert.AreEqual(3, listAsJson.Count());

            for (var ix = 0; ix < listAsJson.Count(); ix++)
            {
                IDictionary<string, JToken> valueAsJson = (JObject) listAsJson[ix];
                Assert.NotNull(valueAsJson);
                Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Type));
                Assert.AreEqual(GraphSonTokens.TypeLong, valueAsJson[GraphSonTokens.Type].Value<string>());
                Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Value));
                Assert.AreEqual(1000L, valueAsJson[GraphSonTokens.Value].Value<long>());
            }
        }

        [Test]
        public void JsonFromElementVertexMapPropertiesNoKeysWithTypes()
        {
            IVertex v = _tinkerGrapĥ.AddVertex(1);

            var map = new Dictionary<string, object>();
            map.Put("this", "some");
            map.Put("that", 1);

            v.SetProperty("keyMap", map);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.EXTENDED);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("keyMap"));

            IDictionary<string, JToken> mapWithTypeAsJson = (JObject) json["keyMap"];
            Assert.NotNull(mapWithTypeAsJson);
            Assert.True(mapWithTypeAsJson.ContainsKey(GraphSonTokens.Type));
            Assert.AreEqual(GraphSonTokens.TypeMap, mapWithTypeAsJson[GraphSonTokens.Type].Value<string>());

            Assert.True(mapWithTypeAsJson.ContainsKey(GraphSonTokens.Value));
            IDictionary<string, JToken> mapAsJson = (JObject) mapWithTypeAsJson[GraphSonTokens.Value];

            Assert.True(mapAsJson.ContainsKey("this"));
            IDictionary<string, JToken> thisAsJson = (JObject) mapAsJson["this"];
            Assert.True(thisAsJson.ContainsKey(GraphSonTokens.Type));
            Assert.AreEqual(GraphSonTokens.TypeString, thisAsJson[GraphSonTokens.Type].Value<string>());
            Assert.True(thisAsJson.ContainsKey(GraphSonTokens.Value));
            Assert.AreEqual("some", thisAsJson[GraphSonTokens.Value].Value<string>());

            Assert.True(mapAsJson.ContainsKey("that"));
            IDictionary<string, JToken> thatAsJson = (JObject) mapAsJson["that"];
            Assert.True(thatAsJson.ContainsKey(GraphSonTokens.Type));
            Assert.AreEqual(GraphSonTokens.TypeInteger, thatAsJson[GraphSonTokens.Type].Value<string>());
            Assert.True(thatAsJson.ContainsKey(GraphSonTokens.Value));
            Assert.AreEqual(1, thatAsJson[GraphSonTokens.Value].Value<int>());
        }

        [Test]
        public void JsonFromElementVertexMapPropertyNoKeysNoTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            var map = new Dictionary<string, object>();
            map.Put("this", "some");
            map.Put("that", 1);

            v.SetProperty("keyMap", map);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("keyMap"));

            IDictionary<string, JToken> mapAsJson = (JObject) json["keyMap"];
            Assert.NotNull(mapAsJson);
            Assert.True(mapAsJson.ContainsKey("this"));
            Assert.AreEqual("some", mapAsJson["this"].Value<string>());
            Assert.True(mapAsJson.ContainsKey("that"));
            Assert.AreEqual(1, mapAsJson["that"].Value<int>());
        }

        [Test]
        public void JsonFromElementVertexNoPropertiesNoKeysNoTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            v.SetProperty("name", "marko");

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey(GraphSonTokens.UnderscoreType));
            Assert.AreEqual("vertex", json[GraphSonTokens.UnderscoreType].Value<string>());
            Assert.AreEqual("marko", json["name"].Value<string>());
        }

        [Test]
        public void JsonFromElementVertexNoPropertiesWithKeysNoTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            v.SetProperty("x", "X");
            v.SetProperty("y", "Y");
            v.SetProperty("z", "Z");

            var propertiesToInclude = new HashSet<String> {"y"};
            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, propertiesToInclude,
                                                                               GraphSonMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey(GraphSonTokens.UnderscoreType));
            Assert.AreEqual("vertex", json[GraphSonTokens.UnderscoreType].Value<string>());
            Assert.False(json.ContainsKey("x"));
            Assert.False(json.ContainsKey("z"));
            Assert.True(json.ContainsKey("y"));
        }

        [Test]
        public void JsonFromElementVertexPrimitivePropertiesNoKeysNoTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            v.SetProperty("keyString", "string");
            v.SetProperty("keyLong", 1L);
            v.SetProperty("keyInt", 2);
            v.SetProperty("keyFloat", 3.3f);
            v.SetProperty("keyExponentialDouble", 1312928167.626012);
            v.SetProperty("keyDouble", 4.4);
            v.SetProperty("keyBoolean", true);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("keyString"));
            Assert.AreEqual("string", json["keyString"].Value<string>());
            Assert.True(json.ContainsKey("keyLong"));
            Assert.AreEqual(1L, json["keyLong"].Value<long>());
            Assert.True(json.ContainsKey("keyInt"));
            Assert.AreEqual(2, json["keyInt"].Value<int>());
            Assert.True(json.ContainsKey("keyFloat"));
            Assert.AreEqual(3.3f, (float) json["keyFloat"].Value<double>(), 0);
            Assert.True(json.ContainsKey("keyExponentialDouble"));
            Assert.AreEqual(1312928167.626012, json["keyExponentialDouble"].Value<double>(), 0);
            Assert.True(json.ContainsKey("keyDouble"));
            Assert.AreEqual(4.4, json["keyDouble"].Value<double>(), 0);
            Assert.True(json.ContainsKey("keyBoolean"));
            Assert.True(json["keyBoolean"].Value<bool>());
        }

        [Test]
        public void JsonFromElementVertexPrimitivePropertiesNoKeysWithTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            v.SetProperty("keyString", "string");
            v.SetProperty("keyLong", 1L);
            v.SetProperty("keyInt", 2);
            v.SetProperty("keyFloat", 3.3f);
            v.SetProperty("keyDouble", 4.4);
            v.SetProperty("keyBoolean", true);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.EXTENDED);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("keyString"));

            IDictionary<string, JToken> valueAsJson = (JObject) json["keyString"];
            Assert.NotNull(valueAsJson);
            Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Type));
            Assert.AreEqual(GraphSonTokens.TypeString, valueAsJson[GraphSonTokens.Type].Value<string>());
            Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Value));
            Assert.AreEqual("string", valueAsJson[GraphSonTokens.Value].Value<string>());

            valueAsJson = (JObject) json["keyLong"];
            Assert.NotNull(valueAsJson);
            Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Type));
            Assert.AreEqual(GraphSonTokens.TypeLong, valueAsJson[GraphSonTokens.Type].Value<string>());
            Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Value));
            Assert.AreEqual(1L, valueAsJson[GraphSonTokens.Value].Value<long>());

            valueAsJson = (JObject) json["keyInt"];
            Assert.NotNull(valueAsJson);
            Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Type));
            Assert.AreEqual(GraphSonTokens.TypeInteger, valueAsJson[GraphSonTokens.Type].Value<string>());
            Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Value));
            Assert.AreEqual(2, valueAsJson[GraphSonTokens.Value].Value<int>());

            valueAsJson = (JObject) json["keyFloat"];
            Assert.NotNull(valueAsJson);
            Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Type));
            Assert.AreEqual(GraphSonTokens.TypeFloat, valueAsJson[GraphSonTokens.Type].Value<string>());
            Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Value));
            Assert.AreEqual(3.3f, (float) valueAsJson[GraphSonTokens.Value].Value<double>(), 0);

            valueAsJson = (JObject) json["keyDouble"];
            Assert.NotNull(valueAsJson);
            Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Type));
            Assert.AreEqual(GraphSonTokens.TypeDouble, valueAsJson[GraphSonTokens.Type].Value<string>());
            Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Value));
            Assert.AreEqual(4.4, valueAsJson[GraphSonTokens.Value].Value<double>(), 0);

            valueAsJson = (JObject) json["keyBoolean"];
            Assert.NotNull(valueAsJson);
            Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Type));
            Assert.AreEqual(GraphSonTokens.TypeBoolean, valueAsJson[GraphSonTokens.Type].Value<string>());
            Assert.True(valueAsJson.ContainsKey(GraphSonTokens.Value));
            Assert.True(valueAsJson[GraphSonTokens.Value].Value<bool>());
        }

        [Test]
        public void JsonFromElementVertexStringArrayPropertyNoKeysNoTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            var stringArray = new[] {"this", "that", "other"};

            v.SetProperty("keyStringArray", stringArray);

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, null, GraphSonMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey("keyStringArray"));

            var stringArrayAsJson = (JArray) json["keyStringArray"];
            Assert.NotNull(stringArrayAsJson);
            Assert.AreEqual(3, stringArrayAsJson.Count());
        }

        [Test]
        public void JsonFromElementVertexVertexPropertiesWithKeysNoTypes()
        {
            var v = _tinkerGrapĥ.AddVertex(1);
            v.SetProperty("x", "X");
            v.SetProperty("y", "Y");
            v.SetProperty("z", "Z");

            var innerV = _tinkerGrapĥ.AddVertex(2);
            innerV.SetProperty("x", "X");
            innerV.SetProperty("y", "Y");
            innerV.SetProperty("z", "Z");

            v.SetProperty("v", innerV);

            var propertiesToInclude = new HashSet<String> {"y", "v"};

            IDictionary<string, JToken> json = GraphSonUtility.JsonFromElement(v, propertiesToInclude,
                                                                               GraphSonMode.NORMAL);

            Assert.NotNull(json);
            Assert.True(json.ContainsKey(GraphSonTokens.Id));
            Assert.AreEqual(1, json[GraphSonTokens.Id].Value<int>());
            Assert.True(json.ContainsKey(GraphSonTokens.UnderscoreType));
            Assert.AreEqual("vertex", json[GraphSonTokens.UnderscoreType].Value<string>());
            Assert.False(json.ContainsKey("x"));
            Assert.False(json.ContainsKey("z"));
            Assert.True(json.ContainsKey("y"));
            Assert.True(json.ContainsKey("v"));

            IDictionary<string, JToken> innerJson = (JObject) json["v"];
            Assert.False(innerJson.ContainsKey("x"));
            Assert.False(innerJson.ContainsKey("z"));
            Assert.True(innerJson.ContainsKey("y"));
            Assert.False(innerJson.ContainsKey("v"));
        }

        [Test]
        public void VertexFromJsonIgnoreKeyValid()
        {
            var g = new TinkerGrapĥ();
            var factory = new GraphElementFactory(g);

            var ignoreAge = new HashSet<string> {"age"};
            var config = ElementPropertyConfig.ExcludeProperties(ignoreAge, null);
            var utility = new GraphSonUtility(GraphSonMode.NORMAL, factory, config);
            var v = utility.VertexFromJson((JObject) JsonConvert.DeserializeObject(VertexJson1));

            Assert.AreSame(v, g.GetVertex(1));

            // tinkergraph converts id to string
            Assert.AreEqual("1", v.Id);
            Assert.AreEqual("marko", v.GetProperty("name"));
            Assert.Null(v.GetProperty("age"));
        }

        [Test]
        public void VertexFromJsonInputStreamValid()
        {
            var g = new TinkerGrapĥ();
            var factory = new GraphElementFactory(g);

            var v = GraphSonUtility.VertexFromJson(_inputStreamVertexJson1, factory, GraphSonMode.NORMAL, null);

            Assert.AreSame(v, g.GetVertex(1));

            // tinkergraph converts id to string
            Assert.AreEqual("1", v.Id);
            Assert.AreEqual("marko", v.GetProperty("name"));
            Assert.AreEqual(29, v.GetProperty("age"));
        }

        [Test]
        public void VertexFromJsonStringValid()
        {
            var g = new TinkerGrapĥ();
            var factory = new GraphElementFactory(g);

            var v = GraphSonUtility.VertexFromJson(VertexJson1, factory, GraphSonMode.NORMAL, null);

            Assert.AreSame(v, g.GetVertex(1));

            // tinkergraph converts id to string
            Assert.AreEqual("1", v.Id);
            Assert.AreEqual("marko", v.GetProperty("name"));
            Assert.AreEqual(29, v.GetProperty("age"));
        }

        [Test]
        public void VertexFromJsonValid()
        {
            var g = new TinkerGrapĥ();
            var factory = new GraphElementFactory(g);

            var v = GraphSonUtility.VertexFromJson((JObject) JsonConvert.DeserializeObject(VertexJson1), factory,
                                                   GraphSonMode.NORMAL, null);

            Assert.AreSame(v, g.GetVertex(1));

            // tinkergraph converts id to string
            Assert.AreEqual("1", v.Id);
            Assert.AreEqual("marko", v.GetProperty("name"));
            Assert.AreEqual(29, v.GetProperty("age"));
        }
    }
}