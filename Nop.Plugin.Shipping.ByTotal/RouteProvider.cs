using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Shipping.ByTotal
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Shipping.ByTotal.Configure",
                 "Plugins/ShippingByTotal/Configure",
                 new { controller = "ShippingByTotal", action = "Configure" },
                 new[] { "Nop.Plugin.Shipping.ByTotal.Controllers" }
            );
        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
