﻿using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public class EdgePropertyChangedEvent : EdgePropertyEvent
    {
        public EdgePropertyChangedEvent(IEdge edge, string key, object oldValue, object newValue) :
            base(edge, key, oldValue, newValue)
        {
            Contract.Requires(edge != null);
        }

        protected override void Fire(IGraphChangedListener listener, IEdge edge, string key, object oldValue,
                                     object newValue)
        {
            listener.EdgePropertyChanged(edge, key, oldValue, newValue);
        }
    }
}