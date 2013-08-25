using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.IO
{
    /// <summary>
    /// Elements are sorted in lexicographical order of IDs.
    /// </summary>
    public class LexicographicalElementComparator : IComparer<IElement>
    {
        public int Compare(IElement a, IElement b)
        {
            return System.String.Compare(a.Id.ToString(), b.Id.ToString(), System.StringComparison.Ordinal);
        }
    }
}
