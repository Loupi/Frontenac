using System;

namespace Grave
{
    public interface IGraveGraphFactory : IDisposable
    {
        GraveGraph Create();
        void Destroy(GraveGraph graph);
    }
}
