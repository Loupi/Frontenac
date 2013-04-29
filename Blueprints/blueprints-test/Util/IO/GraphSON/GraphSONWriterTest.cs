using Frontenac.Blueprints.Impls.TG;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    [TestFixture(Category = "GraphSONWriterTest")]
    public class GraphSonWriterTest
    {
        [Test]
        public void OutputGraphNoEmbeddedTypes()
        {
            IGraph g = TinkerGraphFactory.CreateTinkerGraph();
            IDictionary<string, JToken> rootNode;

            using (var stream = new MemoryStream())
            {
                var writer = new GraphSonWriter(g);
                writer.OutputGraph(stream, null, null, GraphSonMode.NORMAL);
                stream.Position = 0;
                string jsonString = Encoding.Default.GetString(stream.ToArray());
                rootNode = (JObject)JsonConvert.DeserializeObject(jsonString);
            }

            // ensure that the JSON conforms to basic structure and that the right
            // number of graph elements are present. other tests already cover element formatting
            Assert.NotNull(rootNode);

            Assert.True(rootNode.ContainsKey(GraphSonTokens.Mode));
            Assert.AreEqual("NORMAL", rootNode.Get(GraphSonTokens.Mode).ToString());

            Assert.True(rootNode.ContainsKey(GraphSonTokens.Vertices));

            var vertices = (IList<JToken>)rootNode.Get(GraphSonTokens.Vertices);
            Assert.AreEqual(7, vertices.Count());

            Assert.True(rootNode.ContainsKey(GraphSonTokens.Edges));

            var edges = (IList<JToken>)rootNode.Get(GraphSonTokens.Edges);
            Assert.AreEqual(6, edges.Count());
        }

        [Test]
        public void OutputGraphWithEmbeddedTypes()
        {
            IGraph g = TinkerGraphFactory.CreateTinkerGraph();
            IDictionary<string, JToken> rootNode;

            using (var stream = new MemoryStream())
            {
                var writer = new GraphSonWriter(g);
                writer.OutputGraph(stream, null, null, GraphSonMode.EXTENDED);
                stream.Position = 0;
                string jsonString = Encoding.Default.GetString(stream.ToArray());
                rootNode = (JObject)JsonConvert.DeserializeObject(jsonString);
            }

            // ensure that the JSON conforms to basic structure and that the right
            // number of graph elements are present. other tests already cover element formatting
            Assert.NotNull(rootNode);
            Assert.True(rootNode.ContainsKey(GraphSonTokens.Mode));
            Assert.AreEqual("EXTENDED", rootNode.Get(GraphSonTokens.Mode).ToString());

            Assert.True(rootNode.ContainsKey(GraphSonTokens.Vertices));

            var vertices = (JArray)rootNode.Get(GraphSonTokens.Vertices);
            Assert.AreEqual(7, vertices.Count());

            Assert.True(rootNode.ContainsKey(GraphSonTokens.Edges));

            var edges = (JArray)rootNode.Get(GraphSonTokens.Edges);
            Assert.AreEqual(6, edges.Count());
        }

        [Test]
        public void OutputGraphWithCompact()
        {
            IGraph g = TinkerGraphFactory.CreateTinkerGraph();
            IDictionary<string, JToken> rootNode;

            using (var stream = new MemoryStream())
            {
                var writer = new GraphSonWriter(g);
                writer.OutputGraph(stream, null, null, GraphSonMode.COMPACT);
                stream.Position = 0;
                string jsonString = Encoding.Default.GetString(stream.ToArray());
                rootNode = (JObject)JsonConvert.DeserializeObject(jsonString);
            }

            // ensure that the JSON conforms to basic structure and that the right
            // number of graph elements are present. other tests already cover element formatting
            Assert.NotNull(rootNode);
            Assert.True(rootNode.ContainsKey(GraphSonTokens.Mode));
            Assert.AreEqual("COMPACT", rootNode.Get(GraphSonTokens.Mode).ToString());

            Assert.True(rootNode.ContainsKey(GraphSonTokens.Vertices));

            var vertices = (JArray)rootNode.Get(GraphSonTokens.Vertices);
            Assert.AreEqual(7, vertices.Count());

            Assert.True(rootNode.ContainsKey(GraphSonTokens.Edges));

            var edges = (JArray)rootNode.Get(GraphSonTokens.Edges);
            Assert.AreEqual(6, edges.Count());
        }
    }
}
