using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.ByTotal
{
    public class ShippingByTotalSettings : ISettings
    {
        public bool LimitMethodsToCreated { get; set; }
    }
}
