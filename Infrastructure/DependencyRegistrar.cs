using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Shipping.ByTotal.Data;
using Nop.Plugin.Shipping.ByTotal.Domain;
using Nop.Plugin.Shipping.ByTotal.Services;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.Shipping.ByTotal.Infrastructure
{
    /// <summary>
    /// Dependency Registrar
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register plugin services
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type Finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<ShippingByTotalService>().As<IShippingByTotalService>().InstancePerLifetimeScope();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}