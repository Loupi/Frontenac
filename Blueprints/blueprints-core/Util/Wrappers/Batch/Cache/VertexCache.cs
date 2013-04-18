using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public interface VertexCache
    {
        object getEntry(object externalId);

        void set(Vertex vertex, object externalId);

        void setId(object vertexId, object externalId);

        bool contains(object externalId);

        void newTransaction();
    }
}
