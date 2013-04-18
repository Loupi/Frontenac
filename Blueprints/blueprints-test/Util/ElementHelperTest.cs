using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "ElementHelperTest")]
    public class ElementHelperTest : BaseTest
    {
        [Test]
        public void testCopyElementProperties()
        {
            Graph graph = new TinkerGraph();
            Vertex v = graph.addVertex(null);
            v.setProperty("name", "marko");
            v.setProperty("age", 31);
            Vertex u = graph.addVertex(null);
            Assert.AreEqual(u.getPropertyKeys().Count(), 0);
            ElementHelper.copyProperties(v, u);
            Assert.AreEqual(u.getPropertyKeys().Count(), 2);
            Assert.AreEqual(u.getProperty("name"), "marko");
            Assert.AreEqual(u.getProperty("age"), 31);
        }

        [Test]
        public void testRemoveProperties()
        {
            Graph graph = TinkerGraphFactory.createTinkerGraph();
            Vertex vertex = graph.getVertex(1);
            Assert.AreEqual(vertex.getProperty("name"), "marko");
            Assert.AreEqual(vertex.getProperty("age"), 29);
            Assert.AreEqual(vertex.getPropertyKeys().Count(), 2);

            ElementHelper.removeProperties(new Element[]{vertex});
            Assert.IsNull(vertex.getProperty("name"));
            Assert.IsNull(vertex.getProperty("age"));
            Assert.AreEqual(vertex.getPropertyKeys().Count(), 0);

            ElementHelper.removeProperties(new Element[]{vertex});
            Assert.IsNull(vertex.getProperty("name"));
            Assert.IsNull(vertex.getProperty("age"));
            Assert.AreEqual(vertex.getPropertyKeys().Count(), 0);
        }

        [Test]
        public void testRemoveProperty()
        {
            Graph graph = TinkerGraphFactory.createTinkerGraph();
            ElementHelper.removeProperty("name", graph.getVertices());
            foreach (Vertex v in graph.getVertices())
                Assert.IsNull(v.getProperty("name"));
        }

        [Test]
        public void testRenameProperty()
        {
            Graph graph = TinkerGraphFactory.createTinkerGraph();
            ElementHelper.renameProperty("name", "name2", graph.getVertices());
            foreach (Vertex v in graph.getVertices())
            {
                Assert.IsNull(v.getProperty("name"));
                Assert.IsNotNull(v.getProperty("name2"));
                string name2 = (string) v.getProperty("name2");
                Assert.True(name2 == "marko" || name2 == "josh" || name2 == "vadas" || name2 == "ripple" || name2 == "lop" || name2 == "peter" || name2 == "loupi");
            }
        }

        [Test]
        public void testTypecastProperty()
        {
            Graph graph = TinkerGraphFactory.createTinkerGraph();
            foreach (Edge e in graph.getEdges())
                Assert.True(e.getProperty("weight") is float);
            
            ElementHelper.typecastProperty("weight", typeof(double), graph.getEdges());
            foreach (Edge e in graph.getEdges())
                Assert.True(e.getProperty("weight") is double);
        }

        [Test]
        public void testHaveEqualProperties()
        {
            Graph graph = new TinkerGraph();
            Vertex a = graph.addVertex(null);
            Vertex b = graph.addVertex(null);
            Vertex c = graph.addVertex(null);
            Vertex d = graph.addVertex(null);

            a.setProperty("name", "marko");
            a.setProperty("age", 31);
            b.setProperty("name", "marko");
            b.setProperty("age", 31);
            c.setProperty("name", "marko");
            d.setProperty("name", "pavel");
            d.setProperty("age", 31);

            Assert.True(ElementHelper.haveEqualProperties(a, b));
            Assert.True(ElementHelper.haveEqualProperties(a, a));
            Assert.False(ElementHelper.haveEqualProperties(a, c));
            Assert.False(ElementHelper.haveEqualProperties(c, a));
            Assert.False(ElementHelper.haveEqualProperties(a, d));
            Assert.False(ElementHelper.haveEqualProperties(a, c));
        }

        [Test]
        public void testGetProperties()
        {
            Graph graph = TinkerGraphFactory.createTinkerGraph();
            Vertex vertex = graph.getVertex(1);
            IDictionary<string, object> map = ElementHelper.getProperties(vertex);
            Assert.AreEqual(map.Count, 2);
            Assert.AreEqual(map.get("name"), "marko");
            Assert.AreEqual(map.get("age"), 29);

            map.put("name", "pavel");
            Assert.AreEqual(map.get("name"), "pavel");

            Assert.AreEqual(vertex.getProperty("name"), "marko");
        }

        [Test]
        public void testSetProperties()
        {
            Graph graph = new TinkerGraph();
            Vertex vertex = graph.addVertex(null);
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.put("name", "pierre");
            ElementHelper.setProperties(vertex, map);
            Assert.AreEqual(vertex.getPropertyKeys().Count(), 1);
            Assert.AreEqual(vertex.getProperty("name"), "pierre");

            map.put("name", "dewilde");
            map.put("country", "belgium");
            ElementHelper.setProperties(vertex, map);
            Assert.AreEqual(vertex.getPropertyKeys().Count(), 2);
            Assert.AreEqual(vertex.getProperty("name"), "dewilde");
            Assert.AreEqual(vertex.getProperty("country"), "belgium");
        }

        [Test]
        public void testSetPropertiesVarArgs()
        {
            Graph graph = new TinkerGraph();
            Vertex vertex = graph.addVertex(null);
            ElementHelper.setProperties(vertex, "name", "pierre");
            Assert.AreEqual(vertex.getPropertyKeys().Count(), 1);
            Assert.AreEqual(vertex.getProperty("name"), "pierre");

            ElementHelper.setProperties(vertex, "name", "dewilde", "country", "belgium", "age", 50);
            Assert.AreEqual(vertex.getPropertyKeys().Count(), 3);
            Assert.AreEqual(vertex.getProperty("name"), "dewilde");
            Assert.AreEqual(vertex.getProperty("country"), "belgium");
            Assert.AreEqual(vertex.getProperty("age"), 50);

            try
            {
                ElementHelper.setProperties(vertex, "a", 12, "b");
                Assert.True(false);
            }
            catch (ArgumentException)
            {
                Assert.False(false);
            }
        }
    }
}
