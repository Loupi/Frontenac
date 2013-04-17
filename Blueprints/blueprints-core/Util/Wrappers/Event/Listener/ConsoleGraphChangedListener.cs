using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    /// An example listener that writes a message to the console for each event that fires from the graph.
    /// </summary>
    public class ConsoleGraphChangedListener : GraphChangedListener
    {
        readonly Graph _Graph;

        public ConsoleGraphChangedListener(Graph graph)
        {
            _Graph = graph;
        }

        public void VertexAdded(Vertex vertex)
        {
            Console.WriteLine(string.Concat("Vertex [", vertex, "] added to graph [", _Graph, "]"));
        }

        public void VertexPropertyChanged(Vertex vertex, string key, object oldValue, object newValue)
        {
            Console.WriteLine(string.Concat("Vertex [", vertex, "] property [", key, "] change value from [", oldValue, "] to [", newValue, "] in graph [", _Graph, "]"));
        }

        public void VertexPropertyRemoved(Vertex vertex, string key, object removedValue)
        {
            Console.WriteLine(string.Concat("Vertex [", vertex, "] property [", key, "] with value of [", removedValue, "] removed in graph [", _Graph, "]"));
        }

        public void VertexRemoved(Vertex vertex)
        {
            Console.WriteLine(string.Concat("Vertex [", vertex, "] removed from graph [", _Graph, "]"));
        }

        public void EdgeAdded(Edge edge)
        {
            Console.WriteLine(string.Concat("Edge [", edge, "] added to graph [", _Graph, "]"));
        }

        public void EdgePropertyChanged(Edge edge, string key, object oldValue, object newValue)
        {
            Console.WriteLine(string.Concat("Edge [", edge, "] property [", key, "] change value from [", oldValue, "] to [", newValue, "] in graph [", _Graph, "]"));
        }

        public void EdgePropertyRemoved(Edge edge, string key, object removedValue)
        {
            Console.WriteLine(string.Concat("Edge [", edge, "] property [", key, "] with value of [", removedValue, "] removed in graph [", _Graph, "]"));
        }

        public void EdgeRemoved(Edge edge)
        {
            Console.WriteLine(string.Concat("Edge [", edge, "] removed from graph [", _Graph, "]"));
        }
    }
}
