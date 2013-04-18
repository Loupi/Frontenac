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
        readonly Graph _graph;

        public ConsoleGraphChangedListener(Graph graph)
        {
            _graph = graph;
        }

        public void vertexAdded(Vertex vertex)
        {
            Console.WriteLine(string.Concat("Vertex [", vertex, "] added to graph [", _graph, "]"));
        }

        public void vertexPropertyChanged(Vertex vertex, string key, object oldValue, object newValue)
        {
            Console.WriteLine(string.Concat("Vertex [", vertex, "] property [", key, "] change value from [", oldValue, "] to [", newValue, "] in graph [", _graph, "]"));
        }

        public void vertexPropertyRemoved(Vertex vertex, string key, object removedValue)
        {
            Console.WriteLine(string.Concat("Vertex [", vertex, "] property [", key, "] with value of [", removedValue, "] removed in graph [", _graph, "]"));
        }

        public void vertexRemoved(Vertex vertex)
        {
            Console.WriteLine(string.Concat("Vertex [", vertex, "] removed from graph [", _graph, "]"));
        }

        public void edgeAdded(Edge edge)
        {
            Console.WriteLine(string.Concat("Edge [", edge, "] added to graph [", _graph, "]"));
        }

        public void edgePropertyChanged(Edge edge, string key, object oldValue, object newValue)
        {
            Console.WriteLine(string.Concat("Edge [", edge, "] property [", key, "] change value from [", oldValue, "] to [", newValue, "] in graph [", _graph, "]"));
        }

        public void edgePropertyRemoved(Edge edge, string key, object removedValue)
        {
            Console.WriteLine(string.Concat("Edge [", edge, "] property [", key, "] with value of [", removedValue, "] removed in graph [", _graph, "]"));
        }

        public void edgeRemoved(Edge edge)
        {
            Console.WriteLine(string.Concat("Edge [", edge, "] removed from graph [", _graph, "]"));
        }
    }
}
