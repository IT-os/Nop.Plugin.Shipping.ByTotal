using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Shipping.ByTotal
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Shipping.ByTotal.AddShippingRate",
                 "Plugins/ShippingByTotal/AddShippingRate",
                 new { controller = "ShippingByTotal", action = "AddShippingRate" },
                 new[] { "Nop.Plugin.Shipping.ByTotal.Controllers" }
            );
            routes.MapRoute("Plugin.Shipping.ByTotal.SaveGeneralSettings",
                 "Plugins/ShippingByTotal/SaveGeneralSettings",
                 new { controller = "ShippingByTotal", action = "SaveGeneralSettings" },
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