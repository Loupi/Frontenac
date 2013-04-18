using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers
{
    /// <summary>
    /// A WrapperGraph has an underlying graph object to which it delegates its operations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface WrapperGraph
    {
        /// <summary>
        /// Get the graph this wrapper delegates to.
        /// </summary>
        /// <returns>the underlying graph that this WrapperGraph delegates its operations to.</returns>
        Graph getBaseGraph();
    }
}
