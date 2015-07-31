using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
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
            Contract.Requires(innerTinkerGrapĥ != null);

            GraveInnerTinkerGrapĥ = innerTinkerGrapĥ;
            RawId = id;
        }

        public override object Id
        {
            get { return RawId; }
        }

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
            GraveInnerTinkerGrapĥ.SetProperty(this, key, value);
        }

        public override object RemoveProperty(string key)
        {
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