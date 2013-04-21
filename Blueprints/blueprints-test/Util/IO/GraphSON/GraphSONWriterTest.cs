using Frontenac.Blueprints.Impls.TG;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    [TestFixture(Category = "GraphSONWriterTest")]
    public class GraphSONWriterTest
    {
        [Test]
        public void outputGraphNoEmbeddedTypes()
        {
            Graph g = TinkerGraphFactory.createTinkerGraph();
            IDictionary<string, JToken> rootNode;

            using(var stream = new MemoryStream())
            {
                GraphSONWriter writer = new GraphSONWriter(g);
                writer.outputGraph(stream, null, null, GraphSONMode.NORMAL);
                stream.Position = 0;
                string jsonString = System.Text.Encoding.Default.GetString(stream.ToArray());
                rootNode = (IDictionary<string, JToken>)((JObject)JsonConvert.DeserializeObject(jsonString));
            }

            // ensure that the JSON conforms to basic structure and that the right
            // number of graph elements are present. other tests already cover element formatting
            Assert.NotNull(rootNode);

            Assert.True(rootNode.ContainsKey(GraphSONTokens.MODE));
            Assert.AreEqual("NORMAL", rootNode.get(GraphSONTokens.MODE).ToString());

            Assert.True(rootNode.ContainsKey(GraphSONTokens.VERTICES));

            IList<JToken> vertices = (IList<JToken>)rootNode.get(GraphSONTokens.VERTICES);
            Assert.AreEqual(7, vertices.Count());

            Assert.True(rootNode.ContainsKey(GraphSONTokens.EDGES));

            IList<JToken> edges = (IList<JToken>)rootNode.get(GraphSONTokens.EDGES);
            Assert.AreEqual(6, edges.Count());
        }

        [Test]
        public void outputGraphWithEmbeddedTypes()
        {
            Graph g = TinkerGraphFactory.createTinkerGraph();
            IDictionary<string, JToken> rootNode;

            using(var stream = new MemoryStream())
            {
                GraphSONWriter writer = new GraphSONWriter(g);
                writer.outputGraph(stream, null, null, GraphSONMode.EXTENDED);
                stream.Position = 0;
                string jsonString = System.Text.Encoding.Default.GetString(stream.ToArray());
                rootNode = (IDictionary<string, JToken>)((JObject)JsonConvert.DeserializeObject(jsonString));
            }

            // ensure that the JSON conforms to basic structure and that the right
            // number of graph elements are present. other tests already cover element formatting
            Assert.NotNull(rootNode);
            Assert.True(rootNode.ContainsKey(GraphSONTokens.MODE));
            Assert.AreEqual("EXTENDED", rootNode.get(GraphSONTokens.MODE).ToString());

            Assert.True(rootNode.ContainsKey(GraphSONTokens.VERTICES));

            JArray vertices = (JArray)rootNode.get(GraphSONTokens.VERTICES);
            Assert.AreEqual(7, vertices.Count());

            Assert.True(rootNode.ContainsKey(GraphSONTokens.EDGES));

            JArray edges = (JArray)rootNode.get(GraphSONTokens.EDGES);
            Assert.AreEqual(6, edges.Count());
        }

        [Test]
        public void outputGraphWithCompact()
        {
            Graph g = TinkerGraphFactory.createTinkerGraph();
            IDictionary<string, JToken> rootNode;

            using(var stream = new MemoryStream())
            {
                GraphSONWriter writer = new GraphSONWriter(g);
                writer.outputGraph(stream, null, null, GraphSONMode.COMPACT);
                stream.Position = 0;
                string jsonString = System.Text.Encoding.Default.GetString(stream.ToArray());
                rootNode = (IDictionary<string, JToken>)((JObject)JsonConvert.DeserializeObject(jsonString));
            }

            // ensure that the JSON conforms to basic structure and that the right
            // number of graph elements are present. other tests already cover element formatting
            Assert.NotNull(rootNode);
            Assert.True(rootNode.ContainsKey(GraphSONTokens.MODE));
            Assert.AreEqual("COMPACT", rootNode.get(GraphSONTokens.MODE).ToString());

            Assert.True(rootNode.ContainsKey(GraphSONTokens.VERTICES));

            JArray vertices = (JArray)rootNode.get(GraphSONTokens.VERTICES);
            Assert.AreEqual(7, vertices.Count());

            Assert.True(rootNode.ContainsKey(GraphSONTokens.EDGES));

            JArray edges = (JArray)rootNode.get(GraphSONTokens.EDGES);
            Assert.AreEqual(6, edges.Count());
        }
    }
}
