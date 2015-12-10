using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Shipping.ByTotal.Data;
using Nop.Plugin.Shipping.ByTotal.Domain;
using Nop.Plugin.Shipping.ByTotal.Services;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Shipping.ByTotal
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinderr, NopConfig config)
        {
            builder.RegisterType<ShippingByTotalService>().As<IShippingByTotalService>().InstancePerRequest();

            //data context
            this.RegisterPluginDataContext<ShippingByTotalObjectContext>(builder, "nop_object_context_shipping_total");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<ShippingByTotalRecord>>()
                .As<IRepository<ShippingByTotalRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_shipping_total"))
                .InstancePerRequest();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}