using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Frontenac.Blueprints;
using Frontenac.Blueprints.Util;

namespace Frontenac.BlueRed
{
    public abstract class RedisElement : DictionaryElement
    {
        internal readonly long RawId;
        protected readonly RedisGraph RedisInnerTinkerGraĥ;

        protected RedisElement(long id, RedisGraph innerTinkerGraĥ)
            : base(innerTinkerGraĥ)
        {
            Contract.Requires(innerTinkerGraĥ != null);

            RawId = id;
            RedisInnerTinkerGraĥ = innerTinkerGraĥ;
        }

        public override object Id
        {
            get { return RawId; }
        }

        public override object GetProperty(string key)
        {
            return RedisInnerTinkerGraĥ.GetProperty(this, key);
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return RedisInnerTinkerGraĥ.GetPropertyKeys(this);
        }

        public override void SetProperty(string key, object value)
        {
            RedisInnerTinkerGraĥ.SetProperty(this, key, value);
        }

        public override object RemoveProperty(string key)
        {
            return RedisInnerTinkerGraĥ.RemoveProperty(this, key);
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
