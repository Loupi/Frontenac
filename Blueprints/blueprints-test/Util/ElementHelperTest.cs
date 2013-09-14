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
            var graph = new TinkerGraph();
            var v = graph.AddVertex(null);
            v.SetProperty("name", "marko");
            v.SetProperty("age", 31);
            var u = graph.AddVertex(null);
            Assert.AreEqual(u.GetPropertyKeys().Count(), 0);
            ElementHelper.CopyProperties(v, u);
            Assert.AreEqual(u.GetPropertyKeys().Count(), 2);
            Assert.AreEqual(u.GetProperty("name"), "marko");
            Assert.AreEqual(u.GetProperty("age"), 31);
        }

        [Test]
        public void TestRemoveProperties()
        {
            var graph = TinkerGraphFactory.CreateTinkerGraph();
            var vertex = graph.GetVertex(1);
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
            var graph = TinkerGraphFactory.CreateTinkerGraph();
            ElementHelper.RemoveProperty("name", graph.GetVertices());
            foreach (var v in graph.GetVertices())
                Assert.IsNull(v.GetProperty("name"));
        }

        [Test]
        public void TestRenameProperty()
        {
            var graph = TinkerGraphFactory.CreateTinkerGraph();
            ElementHelper.RenameProperty("name", "name2", graph.GetVertices());
            foreach (var v in graph.GetVertices())
            {
                Assert.IsNull(v.GetProperty("name"));
                Assert.IsNotNull(v.GetProperty("name2"));
                var name2 = (string) v.GetProperty("name2");
                Assert.True(name2 == "marko" || name2 == "josh" || name2 == "vadas" || name2 == "ripple" || 
                            name2 == "lop" || name2 == "peter" || name2 == "loupi");
            }
        }

        [Test]
        public void TestTypecastProperty()
        {
            var graph = TinkerGraphFactory.CreateTinkerGraph();
            foreach (var e in graph.GetEdges())
                Assert.True(e.GetProperty("weight") is double);
            
            ElementHelper.TypecastProperty("weight", typeof(double), graph.GetEdges());
            foreach (var e in graph.GetEdges())
                Assert.True(e.GetProperty("weight") is double);
        }

        [Test]
        public void TestHaveEqualProperties()
        {
            var graph = new TinkerGraph();
            var a = graph.AddVertex(null);
            var b = graph.AddVertex(null);
            var c = graph.AddVertex(null);
            var d = graph.AddVertex(null);

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
            var graph = TinkerGraphFactory.CreateTinkerGraph();
            var vertex = graph.GetVertex(1);
            var map = ElementHelper.GetProperties(vertex);
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
            var graph = new TinkerGraph();
            var vertex = graph.AddVertex(null);
            var map = new Dictionary<string, object>();
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
            var graph = new TinkerGraph();
            var vertex = graph.AddVertex(null);
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
                Assert.Fail();
            }
            catch (Exception x)
            {
                if (x.GetType().FullName != Portability.ContractExceptionName)
                {
                    throw;
                }
            }
        }
    }
}
