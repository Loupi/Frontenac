using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Impls.TG
{
    public static class TinkerGraphFactory
    {
        public static TinkerGraph createTinkerGraph()
        {
            TinkerGraph graph = new TinkerGraph();

            Vertex marko = graph.addVertex("1");
            marko.setProperty("name", "marko");
            marko.setProperty("age", 29);

            Vertex vadas = graph.addVertex("2");
            vadas.setProperty("name", "vadas");
            vadas.setProperty("age", 27);

            Vertex lop = graph.addVertex("3");
            lop.setProperty("name", "lop");
            lop.setProperty("lang", "java");

            Vertex josh = graph.addVertex("4");
            josh.setProperty("name", "josh");
            josh.setProperty("age", 32);

            Vertex ripple = graph.addVertex("5");
            ripple.setProperty("name", "ripple");
            ripple.setProperty("lang", "java");

            Vertex peter = graph.addVertex("6");
            peter.setProperty("name", "peter");
            peter.setProperty("age", 35);

            Vertex loupi = graph.addVertex("7");
            loupi.setProperty("name", "loupi");
            loupi.setProperty("age", 33);
            loupi.setProperty("lang", "c#");

            graph.addEdge("7", marko, vadas, "knows").setProperty("weight", 0.5f);
            graph.addEdge("8", marko, josh, "knows").setProperty("weight", 1.0f);
            graph.addEdge("9", marko, lop, "created").setProperty("weight", 0.4f);

            graph.addEdge("10", josh, ripple, "created").setProperty("weight", 1.0f);
            graph.addEdge("11", josh, lop, "created").setProperty("weight", 0.4f);

            graph.addEdge("12", peter, lop, "created").setProperty("weight", 0.2f);

            return graph;
        }
    }
}
