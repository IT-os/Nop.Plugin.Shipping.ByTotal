using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Shipping.ByTotal.Models
{
    public class ShippingByTotalModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.Country")]
        public int CountryId { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.Country")]
        public string CountryName { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.StateProvince")]
        public int StateProvinceId { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.StateProvince")]
        public string StateProvinceName { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.Zip")]
        public string Zip { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.ShippingMethod")]
        public int ShippingMethodId { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.ShippingMethod")]
        public string ShippingMethodName { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.From")]
        public decimal From { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.To")]
        public decimal To { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.UsePercentage")]
        public bool UsePercentage { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.ShippingChargePercentage")]
        public decimal ShippingChargePercentage { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.ShippingChargeAmount")]
        public decimal ShippingChargeAmount { get; set; }
    }
}
