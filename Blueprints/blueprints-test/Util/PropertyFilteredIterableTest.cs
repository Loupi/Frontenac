using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Impls.TG;
using NUnit.Framework;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "PropertyFilteredIterableTest")]
    public class PropertyFilteredIterableTest : BaseTest
    {
        [Test]
        public void TestBasicFunctionality()
        {
            var graph = new TinkerGraph();
            var a = graph.AddVertex("a");
            a.SetProperty("age", 29);
            var b = graph.AddVertex("b");
            b.SetProperty("age", 29);
            var c = graph.AddVertex("c");
            c.SetProperty("age", 30);
            var d = graph.AddVertex("d");
            d.SetProperty("age", 31);

            // throw a vertex without the expected key in the mix
            var e = graph.AddVertex("e");
            var list = new List<IVertex> {a, b, c, d, e};

            var iterable = new PropertyFilteredIterable<IVertex>("age", 29, list);
            Assert.AreEqual(Count(iterable), 2);
            Assert.AreEqual(Count(iterable), 2);
            foreach (var vertex in iterable)
                Assert.True(vertex.Equals(a) || vertex.Equals(b));

            iterable = new PropertyFilteredIterable<IVertex>("age", 30, list);
            Assert.AreEqual(Count(iterable), 1);
            Assert.AreEqual(iterable.First(), c);

            iterable = new PropertyFilteredIterable<IVertex>("age", 30, graph.GetVertices());
            Assert.AreEqual(Count(iterable), 1);
            Assert.AreEqual(iterable.First(), c);

            iterable = new PropertyFilteredIterable<IVertex>("age", 37, list);
            Assert.AreEqual(Count(iterable), 0);
        }
    }
}