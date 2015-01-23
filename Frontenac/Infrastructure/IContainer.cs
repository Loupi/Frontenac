using System;

namespace Frontenac.Infrastructure
{
    public interface IContainer : IDisposable
    {
        void Register(LifeStyle lifeStyle, Type implementation, params Type[] services);
        void Register(object instance, params Type[] services);
        TService Resolve<TService>();
        void Release(object instance);
    }
}