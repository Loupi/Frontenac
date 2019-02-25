using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Castle.Components.DictionaryAdapter;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq.RelationAccessors
{
    internal class EdgeVertexRelationCollection<TEdgeModel, TVertexModel> : ICollection<KeyValuePair<TEdgeModel, TVertexModel>>
        where TEdgeModel : class 
        where TVertexModel : class
    {
        private readonly IVertex _vertex;
        private readonly string _key;
        private readonly RelationAccessor _accessor;
        private readonly Direction _direction;
        private readonly string _label;

        public EdgeVertexRelationCollection(IVertex vertex, string key, RelationAccessor accessor)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            _vertex = vertex;
            _key = key;
            _accessor = accessor;
            _direction = RelationAccessor.DirectionFromKey(_key, out _label);
        }

        public IEnumerator<KeyValuePair<TEdgeModel, TVertexModel>> GetEnumerator()
        {
            return ((IEnumerable)_accessor.GetRelations(_vertex, _key)).OfType<KeyValuePair<TEdgeModel, TVertexModel>>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TEdgeModel, TVertexModel> item)
        {
            var adapter = item.Value as IDictionaryAdapter;
            if (adapter == null)
                throw new InvalidOperationException();

            var vertex = adapter.This.Dictionary as IVertex;
            if (vertex == null)
                throw new InvalidOperationException();

            IVertex inVertex;
            IVertex outVertex;

            switch (_direction)
            {

                case Direction.In:
                    outVertex = vertex;
                    inVertex = _vertex;
                    break;
                case Direction.Out:
                    outVertex = _vertex;
                    inVertex = vertex;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if(inVertex == null || outVertex == null)
                throw new InvalidOperationException();

            var edge = outVertex.AddEdge(null, _label, inVertex);

            var keyAdapter = item.Key as IDictionaryAdapter;
            if (keyAdapter != null)
            {
                GremlinqContext.Current.TypeProvider.SetType(edge, keyAdapter.Meta.Type);
                var edgeProps = keyAdapter.This.Dictionary as IDictionary<string, object>;
                if (edgeProps != null)
                {
                    foreach (var property in edgeProps)
                    {
                        edge.SetProperty(property.Key, property.Value);
                    }
                }
            }
            else
                GremlinqContext.Current.TypeProvider.SetType(edge, typeof(TEdgeModel));
        }

        public void Clear()
        {
            foreach (var edge in GetEdges())
            {
                _vertex.Graph.RemoveEdge(edge.Key);
            }
        }

        public bool Contains(KeyValuePair<TEdgeModel, TVertexModel> item)
        {
            var adapter = item.Value as IDictionaryAdapter;
            if (adapter == null)
                throw new InvalidOperationException();

            var vertex = adapter.This.Dictionary as IVertex;
            if (vertex == null)
                throw new InvalidOperationException();

            return GetEdges().Any(edge => edge.Key.GetVertex(_direction).Equals(vertex));
        }

        public void CopyTo(KeyValuePair<TEdgeModel, TVertexModel>[] array, int arrayIndex)
        {
            var index = arrayIndex;
            foreach (var model in ((IEnumerable)_accessor.GetRelations(_vertex, _key)).OfType<KeyValuePair<TEdgeModel, TVertexModel>>())
            {
                if (index >= array.Length)
                    break;

                array[index] = model;
                index++;
            }
        }

        public bool Remove(KeyValuePair<TEdgeModel, TVertexModel> item)
        {
            var adapter = item.Key as IDictionaryAdapter;
            if (adapter == null)
                throw new InvalidOperationException();

            var edge = adapter.This.Dictionary as IEdge;
            _vertex.Graph.RemoveEdge(edge);
            return true;
        }

        public int Count
        {
            get { return GetEdges().Count(); }
        }

        public bool IsReadOnly { get { return false; } }

        IEnumerable<KeyValuePair<IEdge,IVertex>> GetEdges()
        {
            switch (_direction)
            {
                case Direction.In:
                    return _vertex.InE(_label).Select(edge => new KeyValuePair<IEdge, IVertex>(edge, edge.GetVertex(Direction.Out)));
                case Direction.Out:
                    return _vertex.OutE(_label).Select(edge => new KeyValuePair<IEdge, IVertex>(edge, edge.GetVertex(Direction.In)));
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}