using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Impls.TG
{
    public static class TinkerGraphFactory
    {
        public static TinkerGraph CreateTinkerGraph()
        {
            TinkerGraph graph = new TinkerGraph();

            Vertex marko = graph.AddVertex("1");
            marko.SetProperty("name", "marko");
            marko.SetProperty("age", 29);

            Vertex vadas = graph.AddVertex("2");
            vadas.SetProperty("name", "vadas");
            vadas.SetProperty("age", 27);

            Vertex lop = graph.AddVertex("3");
            lop.SetProperty("name", "lop");
            lop.SetProperty("lang", "java");

            Vertex josh = graph.AddVertex("4");
            josh.SetProperty("name", "josh");
            josh.SetProperty("age", 32);

            Vertex ripple = graph.AddVertex("5");
            ripple.SetProperty("name", "ripple");
            ripple.SetProperty("lang", "java");

            Vertex peter = graph.AddVertex("6");
            peter.SetProperty("name", "peter");
            peter.SetProperty("age", 35);

            Vertex loupi = graph.AddVertex("7");
            loupi.SetProperty("name", "loupi");
            loupi.SetProperty("age", 33);
            loupi.SetProperty("lang", "c#");

            graph.AddEdge("7", marko, vadas, "knows").SetProperty("weight", 0.5f);
            graph.AddEdge("8", marko, josh, "knows").SetProperty("weight", 1.0f);
            graph.AddEdge("9", marko, lop, "created").SetProperty("weight", 0.4f);

            graph.AddEdge("10", josh, ripple, "created").SetProperty("weight", 1.0f);
            graph.AddEdge("11", josh, lop, "created").SetProperty("weight", 0.4f);

            graph.AddEdge("12", peter, lop, "created").SetProperty("weight", 0.2f);

            return graph;
        }
    }
}
