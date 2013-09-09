using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Frontenac.Blueprints.Util.IO
{
    /// <summary>
    /// Elements are sorted in lexicographical order of IDs.
    /// </summary>
    public class LexicographicalElementComparator : IComparer<IElement>
    {
        public int Compare(IElement a, IElement b)
        {
            Contract.Requires(a != null);
            Contract.Requires(b != null);

            return string.Compare(a.Id.ToString(), b.Id.ToString(), StringComparison.Ordinal);
        }
    }
}
