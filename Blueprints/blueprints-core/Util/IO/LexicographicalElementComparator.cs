using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO
{
    /// <summary>
    /// Elements are sorted in lexicographical order of IDs.
    /// </summary>
    public class LexicographicalElementComparator : IComparer<Element>
    {
        public int Compare(Element a, Element b)
        {
            return a.GetId().ToString().CompareTo(b.GetId().ToString());
        }
    }
}
