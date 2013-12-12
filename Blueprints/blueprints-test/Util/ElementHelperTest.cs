using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Impls.TG;
using NUnit.Framework;

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
            v.CopyProperties(u);
            Assert.AreEqual(u.GetPropertyKeys().Count(), 2);
            Assert.AreEqual(u.GetProperty("name"), "marko");
            Assert.AreEqual(u.GetProperty("age"), 31);
        }

        [Test]
        public void TestGetProperties()
        {
            var graph = TinkerGraphFactory.CreateTinkerGraph();
            var vertex = graph.GetVertex(1);
            var map = vertex.GetProperties();
            Assert.AreEqual(map.Count, 2);
            Assert.AreEqual(map.Get("name"), "marko");
            Assert.AreEqual(map.Get("age"), 29);

            map.Put("name", "pavel");
            Assert.AreEqual(map.Get("name"), "pavel");

            Assert.AreEqual(vertex.GetProperty("name"), "marko");
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

            Assert.True(a.HaveEqualProperties(b));
            Assert.True(a.HaveEqualProperties(a));
            Assert.False(a.HaveEqualProperties(c));
            Assert.False(c.HaveEqualProperties(a));
            Assert.False(a.HaveEqualProperties(d));
            Assert.False(a.HaveEqualProperties(c));
        }

        [Test]
        public void TestRemoveProperties()
        {
            var graph = TinkerGraphFactory.CreateTinkerGraph();
            var vertex = graph.GetVertex(1);
            Assert.AreEqual(vertex.GetProperty("name"), "marko");
            Assert.AreEqual(vertex.GetProperty("age"), 29);
            Assert.AreEqual(vertex.GetPropertyKeys().Count(), 2);

            new IElement[] {vertex}.RemoveProperties();
            Assert.IsNull(vertex.GetProperty("name"));
            Assert.IsNull(vertex.GetProperty("age"));
            Assert.AreEqual(vertex.GetPropertyKeys().Count(), 0);

            new IElement[] {vertex}.RemoveProperties();
            Assert.IsNull(vertex.GetProperty("name"));
            Assert.IsNull(vertex.GetProperty("age"));
            Assert.AreEqual(vertex.GetPropertyKeys().Count(), 0);
        }

        [Test]
        public void TestRemoveProperty()
        {
            var graph = TinkerGraphFactory.CreateTinkerGraph();
            graph.GetVertices().RemoveProperty("name");
            foreach (var v in graph.GetVertices())
                Assert.IsNull(v.GetProperty("name"));
        }

        [Test]
        public void TestRenameProperty()
        {
            var graph = TinkerGraphFactory.CreateTinkerGraph();
            graph.GetVertices().RenameProperty("name", "name2");
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
        public void TestSetProperties()
        {
            var graph = new TinkerGraph();
            var vertex = graph.AddVertex(null);
            var map = new Dictionary<string, object>();
            map.Put("name", "pierre");
            vertex.SetProperties(map);
            Assert.AreEqual(vertex.GetPropertyKeys().Count(), 1);
            Assert.AreEqual(vertex.GetProperty("name"), "pierre");

            map.Put("name", "dewilde");
            map.Put("country", "belgium");
            vertex.SetProperties(map);
            Assert.AreEqual(vertex.GetPropertyKeys().Count(), 2);
            Assert.AreEqual(vertex.GetProperty("name"), "dewilde");
            Assert.AreEqual(vertex.GetProperty("country"), "belgium");
        }

        [Test]
        public void TestSetPropertiesVarArgs()
        {
            var graph = new TinkerGraph();
            var vertex = graph.AddVertex(null);
            vertex.SetProperties("name", "pierre");
            Assert.AreEqual(vertex.GetPropertyKeys().Count(), 1);
            Assert.AreEqual(vertex.GetProperty("name"), "pierre");

            vertex.SetProperties("name", "dewilde", "country", "belgium", "age", 50);
            Assert.AreEqual(vertex.GetPropertyKeys().Count(), 3);
            Assert.AreEqual(vertex.GetProperty("name"), "dewilde");
            Assert.AreEqual(vertex.GetProperty("country"), "belgium");
            Assert.AreEqual(vertex.GetProperty("age"), 50);

            try
            {
                vertex.SetProperties("a", 12, "b");
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

        [Test]
        public void TestTypecastProperty()
        {
            var graph = TinkerGraphFactory.CreateTinkerGraph();
            foreach (var e in graph.GetEdges())
                Assert.True(e.GetProperty("weight") is double);

            graph.GetEdges().TypecastProperty("weight", typeof(double));
            foreach (var e in graph.GetEdges())
                Assert.True(e.GetProperty("weight") is double);
        }
    }
}