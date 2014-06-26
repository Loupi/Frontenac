using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Impls;
using NUnit.Framework;

namespace Frontenac.Gremlinq.Test
{
    public abstract class GremlinDocsTestSuite : TestSuite
    {
        protected GremlinDocsTestSuite(GraphTest graphTest)
            : base("GremlinDocsTestSuite", graphTest)
        {
        }

        [Test]
        public void TestBoth()
        {
            var g = GraphTest.GenerateGraph();
            try
            {
                var v = g.V(4);
                Assert.NotNull(v);

                var result = v.Both().ToList();
                Assert.AreEqual(3, result.Count);
                Assert.NotNull(result.SingleOrDefault(vertex => vertex.ToString() == "v[1]"));
                Assert.NotNull(result.SingleOrDefault(vertex => vertex.ToString() == "v[3]"));
                Assert.NotNull(result.SingleOrDefault(vertex => vertex.ToString() == "v[5]"));

                result = v.Both("knows").ToList();
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual("v[1]", result[0].ToString());

                result = v.Both("knows", "created").ToList();
                Assert.AreEqual(3, result.Count);
                Assert.NotNull(result.SingleOrDefault(vertex => vertex.ToString() == "v[1]"));
                Assert.NotNull(result.SingleOrDefault(vertex => vertex.ToString() == "v[3]"));
                Assert.NotNull(result.SingleOrDefault(vertex => vertex.ToString() == "v[5]"));

                result = v.Both(1, "knows", "created").ToList();
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual("v[1]", result[0].ToString());
            }
            finally
            {
                g.Shutdown();
            }
        }

        [Test]
        public void TestBothE()
        {
            var g = GraphTest.GenerateGraph();
            try
            {
                var v = g.V(4);
                Assert.NotNull(v);

                var result = v.BothE().ToList();
                Assert.AreEqual(3, result.Count);
                Assert.NotNull(result.SingleOrDefault(edge => edge.Id.ToString() == "8"));
                Assert.NotNull(result.SingleOrDefault(edge => edge.Id.ToString() == "10"));
                Assert.NotNull(result.SingleOrDefault(edge => edge.Id.ToString() == "11"));
                
                result = v.BothE("knows").ToList();
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual("8", result[0].Id.ToString());

                result = v.BothE("knows", "created").ToList();
                Assert.AreEqual(3, result.Count);
                Assert.NotNull(result.SingleOrDefault(edge => edge.Id.ToString() == "8"));
                Assert.NotNull(result.SingleOrDefault(edge => edge.Id.ToString() == "10"));
                Assert.NotNull(result.SingleOrDefault(edge => edge.Id.ToString() == "11"));
                
                result = v.BothE(1, "knows", "created").ToList();
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual("8", result[0].Id.ToString());
            }
            finally
            {
                g.Shutdown();
            }
        }

        [Test]
        public void TestBothV()
        {
            var g = GraphTest.GenerateGraph();
            try
            {
                var e = g.E(12);
                Assert.NotNull(e);

                var outV = e.OutV();
                Assert.NotNull(outV);
                Assert.AreEqual("v[6]", outV.ToString());

                var inV = e.InV();
                Assert.NotNull(inV);
                Assert.AreEqual("v[3]", inV.ToString());

                var bothV = e.BothV();
                Assert.NotNull(bothV);
                Assert.AreEqual(2, bothV.Length);
                Assert.AreEqual("v[6]", bothV[0].ToString());
                Assert.AreEqual("v[3]", bothV[1].ToString());
            }
            finally
            {
                g.Shutdown();
            }
        }

        [Test]
        public void TestE()
        {
            var g = GraphTest.GenerateGraph();
            try
            {
                var edges = g.E().ToList();
                Assert.NotNull(edges);
                Assert.AreEqual(6, edges.Count);

                var weights = g.E().Select(edge => Convert.ToDouble(edge.GetProperty("weight"))).ToList();
                Assert.NotNull(edges);
                Assert.AreEqual(6, weights.Count);
            }
            finally
            {
                g.Shutdown();
            }
        }

        [Test]
        public void TestIn()
        {
            var g = GraphTest.GenerateGraph();
            try
            {
                var v = g.V(4);
                Assert.NotNull(v);
                Assert.AreEqual("v[4]", v.ToString());

                var v1 = v.In().Single();
                Assert.NotNull(v1);
                Assert.AreEqual("v[1]", v1.ToString());

                v = g.V(3);
                Assert.NotNull(v);
                Assert.AreEqual("v[3]", v.ToString());

                var result = v.In("created").ToList();
                Assert.AreEqual(3, result.Count);
                Assert.NotNull(result.SingleOrDefault(vertex => vertex.ToString() == "v[1]"));
                Assert.NotNull(result.SingleOrDefault(vertex => vertex.ToString() == "v[4]"));
                Assert.NotNull(result.SingleOrDefault(vertex => vertex.ToString() == "v[6]"));

                result = v.In(2, "created").ToList();
                Assert.AreEqual(2, result.Count);
            }
            finally
            {
                g.Shutdown();
            }
        }

        [Test]
        public void TestInE()
        {
            var g = GraphTest.GenerateGraph();
            try
            {
                var v = g.V(4);
                Assert.NotNull(v);
                Assert.AreEqual("v[4]", v.ToString());

                var v1 = v.InE().OutV().Single();
                Assert.NotNull(v1);
                Assert.AreEqual("v[1]", v1.ToString());

                v = g.V(3);
                Assert.NotNull(v);
                Assert.AreEqual("v[3]", v.ToString());

                var result = v.InE("created").OutV().ToList();
                Assert.AreEqual(3, result.Count);
                Assert.NotNull(result.SingleOrDefault(vertex => vertex.ToString() == "v[1]"));
                Assert.NotNull(result.SingleOrDefault(vertex => vertex.ToString() == "v[4]"));
                Assert.NotNull(result.SingleOrDefault(vertex => vertex.ToString() == "v[6]"));

                result = v.InE(2, "created").OutV().ToList();
                Assert.AreEqual(2, result.Count);
            }
            finally
            {
                g.Shutdown();
            }
        }

        [
            Test]
        public void TestGroupBy()
        {
            var g = GraphTest.GenerateGraph();
            try
            {
                var result = g.V().GroupBy(vertex => vertex, vertex => vertex.Out()).ToList();
                Assert.AreEqual(result.Count, 7);
                var v1 = result.Single(grouping => grouping.Key.ToString() == "v[1]");
                var v2 = result.Single(grouping => grouping.Key.ToString() == "v[2]");
                var v3 = result.Single(grouping => grouping.Key.ToString() == "v[3]");
                var v4 = result.Single(grouping => grouping.Key.ToString() == "v[4]");
                var v5 = result.Single(grouping => grouping.Key.ToString() == "v[5]");
                var v6 = result.Single(grouping => grouping.Key.ToString() == "v[6]");
                var v7 = result.Single(grouping => grouping.Key.ToString() == "v[7]");

                var elems = v1.SelectMany(list => list).ToList();
                Assert.AreEqual(3, elems.Count);
                Assert.NotNull(elems.SingleOrDefault(vertex => vertex.ToString() == "v[2]"));
                Assert.NotNull(elems.SingleOrDefault(vertex => vertex.ToString() == "v[3]"));
                Assert.NotNull(elems.SingleOrDefault(vertex => vertex.ToString() == "v[4]"));

                elems = v2.SelectMany(list => list).ToList();
                Assert.AreEqual(0, elems.Count);

                elems = v3.SelectMany(list => list).ToList();
                Assert.AreEqual(0, elems.Count);

                elems = v4.SelectMany(list => list).ToList();
                Assert.AreEqual(2, elems.Count);
                Assert.NotNull(elems.SingleOrDefault(vertex => vertex.ToString() == "v[3]"));
                Assert.NotNull(elems.SingleOrDefault(vertex => vertex.ToString() == "v[5]"));

                elems = v5.SelectMany(list => list).ToList();
                Assert.AreEqual(0, elems.Count);

                elems = v6.SelectMany(list => list).ToList();
                Assert.AreEqual(1, elems.Count);
                Assert.NotNull(elems.SingleOrDefault(vertex => vertex.ToString() == "v[3]"));

                elems = v7.SelectMany(list => list).ToList();
                Assert.AreEqual(0, elems.Count);

                var result2 = g.V().Out()
                    .GroupBy(vertex => (string) vertex.GetProperty("name"), 
                             vertex => vertex.In(),  
                             (s, vertices) => new
                                {
                                    Key = s,
                                    Values = vertices.SelectMany(list => list)
                                                     .Distinct()
                                                     .Where(vertex => Convert.ToInt32(vertex.GetProperty("age")) > 30)
                                                     .Select(vertex => (string)vertex.GetProperty("name"))
                                                     .ToList()
                                })
                    .ToDictionary(pair => pair.Key, pair => pair.Values);

                Assert.AreEqual(4, result2.Count);
                var lop = result2["lop"];
                var ripple = result2["ripple"];
                var josh = result2["josh"];
                var vadas = result2["vadas"];

                Assert.AreEqual(2, lop.Count);
                Assert.NotNull(lop.SingleOrDefault(name => name == "josh"));
                Assert.NotNull(lop.SingleOrDefault(name => name == "peter"));

                Assert.AreEqual(1, ripple.Count);
                Assert.NotNull(ripple.SingleOrDefault(name => name == "josh"));

                Assert.AreEqual(0, josh.Count);
                Assert.AreEqual(0, vadas.Count);
            }
            finally
            {
                g.Shutdown();
            }
        }

        

        [Test]
        public void TestGroupCount()
        {
            var g = GraphTest.GenerateGraph();
            try
            {
                var m = new Dictionary<IVertex, double>();
                var result = g.V().Out().GroupCount(m).ToList();
                Assert.AreEqual(6, result.Count);
                Assert.AreEqual(1, result.Count(vertex => vertex.Id.ToString() == "2"));
                Assert.AreEqual(1, result.Count(vertex => vertex.Id.ToString() == "4"));
                Assert.AreEqual(3, result.Count(vertex => vertex.Id.ToString() == "3"));
                Assert.AreEqual(1, result.Count(vertex => vertex.Id.ToString() == "5"));

                Assert.AreEqual(4, m.Count);
                var v2 = m.Single(pair => pair.Key.Id.ToString() == "2");
                var v3 = m.Single(pair => pair.Key.Id.ToString() == "3");
                var v4 = m.Single(pair => pair.Key.Id.ToString() == "4");
                var v5 = m.Single(pair => pair.Key.Id.ToString() == "5");
                Assert.AreEqual(1.0, v2.Value);
                Assert.AreEqual(3.0, v3.Value);
                Assert.AreEqual(1.0, v4.Value);
                Assert.AreEqual(1.0, v5.Value);

                m = new Dictionary<IVertex, double>();
                var result2 = g.V(1)
                    .Out()
                    .GroupCount(m, vertex => vertex, (vertex, d) => d + 1.0)
                    .Out()
                    .GroupCount(m, vertex => vertex, (vertex, d) => d + 0.5)
                    .ToList();
                Assert.AreEqual(2, result2.Count);
                Assert.AreEqual(1, result2.Count(vertex => vertex.Id.ToString() == "3"));
                Assert.AreEqual(1, result2.Count(vertex => vertex.Id.ToString() == "5"));
                Assert.AreEqual(4, m.Count);
                v2 = m.Single(pair => pair.Key.Id.ToString() == "2");
                v3 = m.Single(pair => pair.Key.Id.ToString() == "3");
                v4 = m.Single(pair => pair.Key.Id.ToString() == "4");
                v5 = m.Single(pair => pair.Key.Id.ToString() == "5");
                Assert.AreEqual(1.0, v2.Value);
                Assert.AreEqual(1.5, v3.Value);
                Assert.AreEqual(1.0, v4.Value);
                Assert.AreEqual(0.5, v5.Value);
            }
            finally
            {
                g.Shutdown();
            }
        }
    }
}
