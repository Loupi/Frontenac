using Frontenac.Grave.Esent.Serializers;
using Microsoft.Isam.Esent.Interop;

namespace Frontenac.Grave.Esent
{
    public class EsentContext : EsentContextBase
    {
        public EsentContext(Session session, string databaseName, IContentSerializer contentSerializer) :
            base(session, databaseName, contentSerializer)
        {
            OpenDatabase();
        }
    }
}