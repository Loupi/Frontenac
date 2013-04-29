using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "ElementHelperTest")]
    public class ElementHelperTest : BaseTest
    {
        [Test]
        public void TestCopyElementProperties()
        {
            IGraph graph = new TinkerGraph();
            IVertex v = graph.AddVertex(null);
            v.SetProperty("name", "marko");
            v.SetProperty("age", 31);
            IVertex u = graph.AddVertex(null);
            Assert.AreEqual(u.GetPropertyKeys().Count(), 0);
            ElementHelper.CopyProperties(v, u);
            Assert.AreEqual(u.GetPropertyKeys().Count(), 2);
            Assert.AreEqual(u.GetProperty("name"), "marko");
            Assert.AreEqual(u.GetProperty("age"), 31);
        }

        [Test]
        public void TestRemoveProperties()
        {
            IGraph graph = TinkerGraphFactory.CreateTinkerGraph();
            IVertex vertex = graph.GetVertex(1);
            Assert.AreEqual(vertex.GetProperty("name"), "marko");
            Assert.AreEqual(vertex.GetProperty("age"), 29);
            Assert.AreEqual(vertex.GetPropertyKeys().Count(), 2);

            ElementHelper.RemoveProperties(new IElement[]{vertex});
            Assert.IsNull(vertex.GetProperty("name"));
            Assert.IsNull(vertex.GetProperty("age"));
            Assert.AreEqual(vertex.GetPropertyKeys().Count(), 0);

            ElementHelper.RemoveProperties(new IElement[]{vertex});
            Assert.IsNull(vertex.GetProperty("name"));
            Assert.IsNull(vertex.GetProperty("age"));
            Assert.AreEqual(vertex.GetPropertyKeys().Count(), 0);
        }

        [Test]
        public void TestRemoveProperty()
        {
            IGraph graph = TinkerGraphFactory.CreateTinkerGraph();
            ElementHelper.RemoveProperty("name", graph.GetVertices());
            foreach (IVertex v in graph.GetVertices())
                Assert.IsNull(v.GetProperty("name"));
        }

        [Test]
        public void TestRenameProperty()
        {
            IGraph graph = TinkerGraphFactory.CreateTinkerGraph();
            ElementHelper.RenameProperty("name", "name2", graph.GetVertices());
            foreach (IVertex v in graph.GetVertices())
            {
                Assert.IsNull(v.GetProperty("name"));
                Assert.IsNotNull(v.GetProperty("name2"));
                var name2 = (string) v.GetProperty("name2");
                Assert.True(name2 == "marko" || name2 == "josh" || name2 == "vadas" || name2 == "ripple" || name2 == "lop" || name2 == "peter" || name2 == "loupi");
            }
        }

        [Test]
        public void TestTypecastProperty()
        {
            IGraph graph = TinkerGraphFactory.CreateTinkerGraph();
            foreach (IEdge e in graph.GetEdges())
                Assert.True(e.GetProperty("weight") is float);
            
            ElementHelper.TypecastProperty("weight", typeof(double), graph.GetEdges());
            foreach (IEdge e in graph.GetEdges())
                Assert.True(e.GetProperty("weight") is double);
        }

        [Test]
        public void TestHaveEqualProperties()
        {
            IGraph graph = new TinkerGraph();
            IVertex a = graph.AddVertex(null);
            IVertex b = graph.AddVertex(null);
            IVertex c = graph.AddVertex(null);
            IVertex d = graph.AddVertex(null);

            a.SetProperty("name", "marko");
            a.SetProperty("age", 31);
            b.SetProperty("name", "marko");
            b.SetProperty("age", 31);
            c.SetProperty("name", "marko");
            d.SetProperty("name", "pavel");
            d.SetProperty("age", 31);

            Assert.True(ElementHelper.HaveEqualProperties(a, b));
            Assert.True(ElementHelper.HaveEqualProperties(a, a));
            Assert.False(ElementHelper.HaveEqualProperties(a, c));
            Assert.False(ElementHelper.HaveEqualProperties(c, a));
            Assert.False(ElementHelper.HaveEqualProperties(a, d));
            Assert.False(ElementHelper.HaveEqualProperties(a, c));
        }

        [Test]
        public void TestGetProperties()
        {
            IGraph graph = TinkerGraphFactory.CreateTinkerGraph();
            IVertex vertex = graph.GetVertex(1);
            IDictionary<string, object> map = ElementHelper.GetProperties(vertex);
            Assert.AreEqual(map.Count, 2);
            Assert.AreEqual(map.Get("name"), "marko");
            Assert.AreEqual(map.Get("age"), 29);

            map.Put("name", "pavel");
            Assert.AreEqual(map.Get("name"), "pavel");

            Assert.AreEqual(vertex.GetProperty("name"), "marko");
        }

        [Test]
        public void TestSetProperties()
        {
            IGraph graph = new TinkerGraph();
            IVertex vertex = graph.AddVertex(null);
            IDictionary<string, object> map = new Dictionary<string, object>();
            map.Put("name", "pierre");
            ElementHelper.SetProperties(vertex, map);
            Assert.AreEqual(vertex.GetPropertyKeys().Count(), 1);
            Assert.AreEqual(vertex.GetProperty("name"), "pierre");

            map.Put("name", "dewilde");
            map.Put("country", "belgium");
            ElementHelper.SetProperties(vertex, map);
            Assert.AreEqual(vertex.GetPropertyKeys().Count(), 2);
            Assert.AreEqual(vertex.GetProperty("name"), "dewilde");
            Assert.AreEqual(vertex.GetProperty("country"), "belgium");
        }

        [Test]
        public void TestSetPropertiesVarArgs()
        {
            IGraph graph = new TinkerGraph();
            IVertex vertex = graph.AddVertex(null);
            ElementHelper.SetProperties(vertex, "name", "pierre");
            Assert.AreEqual(vertex.GetPropertyKeys().Count(), 1);
            Assert.AreEqual(vertex.GetProperty("name"), "pierre");

            ElementHelper.SetProperties(vertex, "name", "dewilde", "country", "belgium", "age", 50);
            Assert.AreEqual(vertex.GetPropertyKeys().Count(), 3);
            Assert.AreEqual(vertex.GetProperty("name"), "dewilde");
            Assert.AreEqual(vertex.GetProperty("country"), "belgium");
            Assert.AreEqual(vertex.GetProperty("age"), 50);

            try
            {
                ElementHelper.SetProperties(vertex, "a", 12, "b");
                Assert.True(false);
            }
            catch (ArgumentException)
            {
                Assert.False(false);
            }
        }
    }
}
