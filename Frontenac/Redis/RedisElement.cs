using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;

namespace Frontenac.Redis
{
    public abstract class RedisElement : DictionaryElement
    {
        internal readonly long RawId;
        protected readonly RedisGraph RedisInnerTinkerGrapĥ;

        protected RedisElement(long id, RedisGraph innerTinkerGrapĥ)
            : base(innerTinkerGrapĥ)
        {
            Contract.Requires(innerTinkerGrapĥ != null);

            RawId = id;
            RedisInnerTinkerGrapĥ = innerTinkerGrapĥ;
        }

        public override object Id => RawId;

        public override object GetProperty(string key)
        {
            return RedisInnerTinkerGrapĥ.GetProperty(this, key);
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return RedisInnerTinkerGrapĥ.GetPropertyKeys(this);
        }

        public override void SetProperty(string key, object value)
        {
            RedisInnerTinkerGrapĥ.SetProperty(this, key, value);
        }

        public override object RemoveProperty(string key)
        {
            return RedisInnerTinkerGrapĥ.RemoveProperty(this, key);
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
