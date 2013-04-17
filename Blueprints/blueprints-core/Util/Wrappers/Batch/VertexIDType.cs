using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frontenac.Blueprints.Util.Wrappers.Batch.Cache;

namespace Frontenac.Blueprints.Util.Wrappers.Batch
{
    /// <summary>
    /// Type of vertex ids expected by BatchGraph. The default is IdType.OBJECT.
    /// Use the IdType that best matches the used vertex id types in order to save memory.
    /// </summary>
    public enum VertexIDType
    {
        OBJECT,
        NUMBER,
        STRING,
        URL
    }

    public static class VertexIDTypes
    {
        public static VertexCache GetVertexCache(this VertexIDType vertexIDType)
        {
            switch (vertexIDType)
            {
                case VertexIDType.OBJECT:
                    return new ObjectIDVertexCache();
                case VertexIDType.NUMBER:
                    return new LongIDVertexCache();
                case VertexIDType.STRING:
                    return new StringIDVertexCache();
                case VertexIDType.URL:
                    return new StringIDVertexCache(new URLCompression());
                default:
                    throw new ArgumentException(string.Concat("Unrecognized ID type: ", vertexIDType));
            }
        }
    }
}
