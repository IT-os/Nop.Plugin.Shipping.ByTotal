using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Shipping.ByTotal.Data;
using Nop.Plugin.Shipping.ByTotal.Domain;
using Nop.Plugin.Shipping.ByTotal.Services;

namespace Nop.Plugin.Shipping.ByTotal
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<ShippingByTotalService>().As<IShippingByTotalService>().InstancePerHttpRequest();

            //data layer
            var dataSettingsManager = new DataSettingsManager();
            var dataProviderSettings = dataSettingsManager.LoadSettings();

            if (dataProviderSettings != null && dataProviderSettings.IsValid())
            {
                //register named context
                builder.Register<IDbContext>(c => new ShippingByTotalObjectContext(dataProviderSettings.DataConnectionString))
                    .Named<IDbContext>("nop_object_context_shipping_total")
                    .InstancePerHttpRequest();

                builder.Register<ShippingByTotalObjectContext>(c => new ShippingByTotalObjectContext(dataProviderSettings.DataConnectionString))
                    .InstancePerHttpRequest();
            }
            else
            {
                //register named context
                builder.Register<IDbContext>(c => new ShippingByTotalObjectContext(c.Resolve<DataSettings>().DataConnectionString))
                    .Named<IDbContext>("nop_object_context_shipping_total")
                    .InstancePerHttpRequest();

                builder.Register<ShippingByTotalObjectContext>(c => new ShippingByTotalObjectContext(c.Resolve<DataSettings>().DataConnectionString))
                    .InstancePerHttpRequest();
            }

            //override required repository with our custom context
            builder.RegisterType<EfRepository<ShippingByTotalRecord>>()
                .As<IRepository<ShippingByTotalRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_shipping_total"))
                .InstancePerHttpRequest();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
