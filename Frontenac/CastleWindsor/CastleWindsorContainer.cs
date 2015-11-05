using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Frontenac.Infrastructure;

namespace Frontenac.CastleWindsor
{
    public class CastleWindsorContainer : IContainer
    {
        public readonly IWindsorContainer Container = new WindsorContainer();

        public CastleWindsorContainer()
        {
            Register(this, typeof(IContainer));
        }

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~CastleWindsorContainer()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Container.Release(this);
                Container.Dispose();
            }
            
            _disposed = true;
        }

        #endregion

        public void Register(LifeStyle lifeStyle, Type implementation, params Type[] services)
        {
            if (services.Length == 0)
                Container.Register(lifeStyle == LifeStyle.Transient 
                    ? Component.For(implementation).LifestyleTransient()
                    : Component.For(implementation));
            else
                Container.Register(lifeStyle == LifeStyle.Transient
                    ? Component.For(services).ImplementedBy(implementation).LifestyleTransient()
                    : Component.For(services).ImplementedBy(implementation));
        }

        public void Register(object instance, params Type[] services)
        {
            Container.Register(services.Length == 0
                ? Component.For().Instance(instance)
                : Component.For(services).Instance(instance));
        }

        public TService Resolve<TService>()
        {
            return Container.Resolve<TService>();
        }

        public void Release(object instance)
        {
            Container.Release(instance);
        }
    }
}