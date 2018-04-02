using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.ByTotal
{
    /// <summary>
    /// Settings for the "Shipping By Total" plugin
    /// </summary>
    public class ShippingByTotalSettings : ISettings
    {
        /// <summary>
        /// Whether returned shipping methods are limited to configured ones 
        /// (if false, shipping methods that have not been configured will have a shipping cost of zero).
        /// </summary>
        public bool LimitMethodsToCreated { get; set; }
    }
}