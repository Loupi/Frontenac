﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Impls.TG;

namespace Frontenac.Blueprints.Util
{
    [TestFixture(Category = "KeyIndexableGraphHelperTest")]
    public class KeyIndexableGraphHelperTest : BaseTest
    {
        [Test]
        public void TestReIndexElements()
        {
            TinkerGraph graph = TinkerGraphFactory.CreateTinkerGraph();
            Assert.True(graph.GetVertices("name", "marko") is PropertyFilteredIterable<Vertex>);
            Assert.AreEqual(Count(graph.GetVertices("name", "marko")), 1);
            Assert.AreEqual(graph.GetVertices("name", "marko").First(), graph.GetVertex(1));
            graph.CreateKeyIndex("name", typeof(Vertex));
            //KeyIndexableGraphHelper.reIndexElements(graph, graph.getVertices(), new HashSet<string>(Arrays.asList("name")));
            Assert.False(graph.GetVertices("name", "marko") is PropertyFilteredIterable<Vertex>);
            Assert.AreEqual(Count(graph.GetVertices("name", "marko")), 1);
            Assert.AreEqual(graph.GetVertices("name", "marko").First(), graph.GetVertex(1));
        }
    }
}