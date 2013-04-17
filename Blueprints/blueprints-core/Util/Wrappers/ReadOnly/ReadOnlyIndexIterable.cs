﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    class ReadOnlyIndexIterable : IEnumerable<Index>
    {
        readonly IEnumerable<Index> _Iterable;

        public ReadOnlyIndexIterable(IEnumerable<Index> iterable)
        {
            _Iterable = iterable;
        }

        public IEnumerator<Index> GetEnumerator()
        {
            foreach (Index index in _Iterable)
                yield return new ReadOnlyIndex(index);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Index>).GetEnumerator();
        }
    }
}