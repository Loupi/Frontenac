using System.Linq;
using Frontenac.Blueprints;
using Grave.Entities;

namespace Grave
{
    class Program
    {
        static void Main()
        {
            var graph = GraveFactory.CreateGraph();
            try
            {
                graph.CreateKeyIndex("test", typeof(IVertex));
                var vv = graph.AddVertex(0);
                vv.SetProperty("obj", new Test(4)
                    {
                        Name = "Test TableName",
                        Number = (int) vv.Id
                    });

                var obj = vv.GetProperty("obj") as Test;

                var vertices = graph.GetVertices().ToArray();
                var vertex = vertices.First();
                var keys1 = vertex.GetPropertyKeys().ToArray();
                var bef = vertex.GetProperty("test");

                vertex.SetProperty("test", 123);
                var val = vertex.GetProperty("test");

                vertex.RemoveProperty("test");
                vertex.SetProperty("test", null);

                var keys2 = vertex.GetPropertyKeys().ToArray();

                var v1 = graph.AddVertex(null);
                var v2 = graph.AddVertex(null);
                var e1 = v1.AddEdge("edgard", v2);
                var ee = v2.GetEdges(Direction.In).ToArray();
                graph.RemoveEdge(e1);
                ee = v2.GetEdges(Direction.In).ToArray();
            }
            finally
            {
                graph.Shutdown();
            }

            /*var father = new Person
                {
                    Name = "Bob",
                    Number = 1,
                    Weight = 89.8f,
                    Wife = new Person
                        {
                            Name = "Yolanda",
                            Number = 2,
                            Weight = 300.2f
                        },
                    Childs = new List<Person>
                        {
                            new Person
                                {
                                    Name = "Nino",
                                    Number = 3,
                                    Weight = 45
                                },
                            new Person
                                {
                                    Name = "Nina",
                                    Number = 4,
                                    Weight = 54.5f
                                }
                        }
                };*/
        }
    }
}
