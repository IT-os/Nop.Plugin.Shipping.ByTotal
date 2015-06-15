using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Shipping.ByTotal.Models
{
    public class ShippingByTotalListModel : BaseNopModel
    {
        public ShippingByTotalListModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailableShippingMethods = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.Store")]
        public int AddStoreId { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.Warehouse")]
        public int AddWarehouseId { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.Country")]
        public int AddCountryId { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.StateProvince")]
        public int AddStateProvinceId { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.ZipPostalCode")]
        public string AddZipPostalCode { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.DisplayOrder")]
        public int AddDisplayOrder { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.ShippingMethod")]
        public int AddShippingMethodId { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.From")]
        public decimal AddFrom { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.To")]
        public decimal AddTo { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.UsePercentage")]
        public bool AddUsePercentage { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.ShippingChargePercentage")]
        public decimal AddShippingChargePercentage { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.ShippingChargeAmount")]
        public decimal AddShippingChargeAmount { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.ByTotal.Fields.LimitMethodsToCreated")]
        public bool LimitMethodsToCreated { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
        public IList<SelectListItem> AvailableShippingMethods { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableWarehouses { get; set; }
    }
}