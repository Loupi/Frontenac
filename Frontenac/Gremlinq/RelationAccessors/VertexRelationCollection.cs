using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Castle.Components.DictionaryAdapter;
using Frontenac.Blueprints;

namespace Frontenac.Gremlinq.RelationAccessors
{
    internal class VertexRelationCollection<TModel> : ICollection<TModel>
    {
        private readonly IVertex _vertex;
        private readonly string _key;
        private readonly RelationAccessor _accessor;
        private readonly RelationAttribute _rel;
        private readonly Direction _direction;
        private readonly string _label;

        public VertexRelationCollection(IVertex vertex, string key, RelationAccessor accessor, RelationAttribute rel)
        {
            Contract.Requires(!string.IsNullOrEmpty(key));

            _vertex = vertex;
            _key = key;
            _accessor = accessor;
            _rel = rel;
            _direction = RelationAccessor.DirectionFromKey(_key, out _label);
            if (rel != null)
            {
                _label = rel.AdjustKey(key);
                _direction = rel.Direction;
            }
        }

        public IEnumerator<TModel> GetEnumerator()
        {
            return ((IEnumerable)_accessor.GetRelations(_vertex, _key, _rel)).OfType<TModel>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TModel item)
        {
            var adapter = item as IDictionaryAdapter;
            if (adapter == null)
                throw new InvalidOperationException();

            var vertex = adapter.This.Dictionary as IVertex;
            if (vertex == null)
                throw new InvalidOperationException();

            switch (_direction)
            {
                case Direction.In:
                    vertex.AddEdge(null, _label, _vertex);
                    break;
                case Direction.Out:
                    _vertex.AddEdge(null, _label, vertex);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        public void Clear()
        {
            foreach (var edge in GetEdges())
            {
                _vertex.Graph.RemoveEdge(edge);
            }
        }

        public bool Contains(TModel item)
        {
            var adapter = item as IDictionaryAdapter;
            if (adapter == null)
                throw new InvalidOperationException();

            var vertex = adapter.This.Dictionary as IVertex;
            if (vertex == null)
                throw new InvalidOperationException();

            return _vertex.GetVertices(_direction, _label, vertex.Id).Any();
        }

        public void CopyTo(TModel[] array, int arrayIndex)
        {
            var index = arrayIndex;
            foreach (var model in ((IEnumerable)_accessor.GetRelations(_vertex, _key, _rel)).OfType<TModel>())
            {
                if (index >= array.Length)
                    break;

                array[index] = model;
                index++;
            }
        }

        public bool Remove(TModel item)
        {
            var adapter = item as IDictionaryAdapter;
            if (adapter == null)
                throw new InvalidOperationException();

            var vertex = adapter.This.Dictionary as IVertex;
            if (vertex == null)
                throw new InvalidOperationException();

            var edges = GetEdges().Where(edge => edge.GetVertex(_direction).Equals(vertex));
            foreach (var edge in edges)
            {
                _vertex.Graph.RemoveEdge(edge);
            }

            return true;
        }

        public int Count
        {
            get { return (int)_vertex.GetNbEdges(_direction, _label); }
        }

        public bool IsReadOnly { get { return false; } }

        IEnumerable<IEdge> GetEdges()
        {
            switch (_direction)
            {
                case Direction.In:
                case Direction.Out:
                    return _vertex.GetEdges(_direction, _label);
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}