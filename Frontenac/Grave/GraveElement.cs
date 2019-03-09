using System;
using System.Collections.Generic;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Contracts;
using Frontenac.Blueprints.Util;

namespace Frontenac.Grave
{
    public abstract class GraveElement : DictionaryElement
    {
        protected readonly GraveGraph GraveInnerTinkerGrapĥ;
        internal readonly int RawId;
        
        protected GraveElement(GraveGraph innerTinkerGrapĥ, int id)
            : base(innerTinkerGrapĥ)
        {
            if (innerTinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(innerTinkerGrapĥ));

            GraveInnerTinkerGrapĥ = innerTinkerGrapĥ;
            RawId = id;
        }

        public override object Id => RawId;

        public override object GetProperty(string key)
        {
            return GraveInnerTinkerGrapĥ.GetProperty(this, key);
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return GraveInnerTinkerGrapĥ.GetPropertyKeys(this);
        }

        public override void SetProperty(string key, object value)
        {
            ElementContract.ValidateSetProperty(key, value);
            GraveInnerTinkerGrapĥ.SetProperty(this, key, value);
        }

        public override object RemoveProperty(string key)
        {
            ElementContract.ValidateRemoveProperty(key);
            return GraveInnerTinkerGrapĥ.RemoveProperty(this, key);
        }

        public override int GetHashCode()
        {
            return RawId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.AreEqual(obj);
        }
    }
}