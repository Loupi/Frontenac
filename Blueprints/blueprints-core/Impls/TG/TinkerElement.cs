﻿using System;
using System.Collections.Generic;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
{
    [Serializable]
    abstract class TinkerElement : IElement
    {
        protected Dictionary<string, object> Properties = new Dictionary<string, object>();
        protected readonly string Id;
        protected readonly TinkerGraph Graph;

        protected TinkerElement(string id, TinkerGraph graph)
        {
            Graph = graph;
            Id = id;
        }

        public IEnumerable<string> GetPropertyKeys()
        {
            return new HashSet<string>(Properties.Keys);
        }

        public object GetProperty(string key)
        {
            return Properties.Get(key);
        }

        public void SetProperty(string key, object value)
        {
            ElementHelper.ValidateProperty(this, key, value);
            object oldValue = Properties.Put(key, value);
            if (this is TinkerVertex)
                Graph.VertexKeyIndex.AutoUpdate(key, value, oldValue, this);
            else
                Graph.EdgeKeyIndex.AutoUpdate(key, value, oldValue, this);
        }

        public object RemoveProperty(string key)
        {
            object oldValue = Properties.JavaRemove(key);
            if (this is TinkerVertex)
                Graph.VertexKeyIndex.AutoRemove(key, oldValue, this);
            else
                Graph.EdgeKeyIndex.AutoRemove(key, oldValue, this);

            return oldValue;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public object GetId()
        {
            return Id;
        }

        public override bool Equals(object obj)
        {
            return ElementHelper.AreEqual(this, obj);
        }

        public void Remove()
        {
            var vertex = this as IVertex;
            if (vertex != null)
                Graph.RemoveVertex(vertex);
            else
                Graph.RemoveEdge((IEdge)this);
        }
    }
}
