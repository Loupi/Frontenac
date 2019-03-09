using System;
using Frontenac.Infrastructure.Serializers;
using Microsoft.Isam.Esent.Interop;

namespace Frontenac.Grave.Esent
{
    public class EsentConfigTable : EsentVertexTable
    {
        public EsentConfigTable(Session session, IContentSerializer contentSerializer)
            : base(session, contentSerializer)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (contentSerializer == null)
                throw new ArgumentNullException(nameof(contentSerializer));

            TableName = "Config";
        }
    }
}