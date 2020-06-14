using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Shipping.ByTotal.Domain;
using Nop.Plugin.Shipping.ByTotal.Models;
using Nop.Plugin.Shipping.ByTotal.Services;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Shipping.ByTotal.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class ShippingByTotalController : BasePluginController
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IShippingByTotalService _shippingByTotalService;
        private readonly IShippingService _shippingService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreService _storeService;
        private readonly ShippingByTotalSettings _shippingByTotalSettings;

        #endregion Fields

        #region Ctor

        public ShippingByTotalController(
            CurrencySettings currencySettings,
            ICountryService countryService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IShippingByTotalService shippingByTotalService,
            IShippingService shippingService,
            IStateProvinceService stateProvinceService,
            IStoreService storeService,
            ShippingByTotalSettings shippingByTotalSettings)
        {
            _countryService = countryService;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _shippingByTotalService = shippingByTotalService;
            _shippingByTotalSettings = shippingByTotalSettings;
            _shippingService = shippingService;
            _stateProvinceService = stateProvinceService;
            _storeService = storeService;
        }

        #endregion Ctor

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
            {
                return AccessDeniedView();
            }

            var shippingMethods = _shippingService.GetAllShippingMethods();
            if (shippingMethods.Count == 0)
            {
                return Content("No shipping methods can be loaded");
            }

            var model = new ConfigurationModel();

            // stores
            model.AvailableStores.Add(new SelectListItem() { Text = "*", Value = "0" });
            foreach (var store in _storeService.GetAllStores())
            {
                model.AvailableStores.Add(new SelectListItem() { Text = store.Name, Value = store.Id.ToString() });
            }

            // warehouses
            model.AvailableWarehouses.Add(new SelectListItem() { Text = "*", Value = "0" });
            foreach (var warehouse in _shippingService.GetAllWarehouses())
            {
                model.AvailableWarehouses.Add(new SelectListItem() { Text = warehouse.Name, Value = warehouse.Id.ToString() });
            }

            // shipping methods
            foreach (var sm in shippingMethods)
            {
                model.AvailableShippingMethods.Add(new SelectListItem() { Text = sm.Name, Value = sm.Id.ToString() });
            }

            // countries
            model.AvailableCountries.Add(new SelectListItem() { Text = "*", Value = "0" });
            var countries = _countryService.GetAllCountries(showHidden: true);
            foreach (var c in countries)
            {
                model.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString() });
            }

            model.AvailableStates.Add(new SelectListItem() { Text = "*", Value = "0" });
            model.LimitMethodsToCreated = _shippingByTotalSettings.LimitMethodsToCreated;
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;

            model.SetGridPageSize();

            return View("~/Plugins/Shipping.ByTotal/Views/Configure.cshtml", model);
        }

        //[HttpPost]  //TODO: Determine how to add back AdminAntiForgery
        [HttpPost]
        public IActionResult RatesList(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
            {
                return AccessDeniedDataTablesJson();
            }

            var records = _shippingByTotalService.GetAllShippingByTotalRecords(model.Page - 1, model.PageSize);

            var gridModel = new ShippingByTotalListModel().PrepareToGrid(model, records, () =>
            {
                return records.Select(record =>
                {
                    var sbtModel = new ShippingByTotalModel
                    {
                        Id = record.Id,
                        StoreId = record.StoreId,
                        WarehouseId = record.WarehouseId,
                        ShippingMethodId = record.ShippingMethodId,
                        CountryId = record.CountryId,
                        DisplayOrder = record.DisplayOrder,
                        From = record.From,
                        To = record.To,
                        UsePercentage = record.UsePercentage,
                        ShippingChargePercentage = record.ShippingChargePercentage,
                        ShippingChargeAmount = record.ShippingChargeAmount,
                    };

                    // shipping method
                    var shippingMethod = _shippingService.GetShippingMethodById(record.ShippingMethodId);
                    sbtModel.ShippingMethodName = (shippingMethod != null) ? shippingMethod.Name : "Unavailable";

                    // store
                    var store = _storeService.GetStoreById(record.StoreId);
                    sbtModel.StoreName = (store != null) ? store.Name : "*";

                    // warehouse
                    var warehouse = _shippingService.GetWarehouseById(record.WarehouseId);
                    sbtModel.WarehouseName = (warehouse != null) ? warehouse.Name : "*";

                    // country
                    var c = _countryService.GetCountryById(record.CountryId);
                    sbtModel.CountryName = (c != null) ? c.Name : "*";
                    sbtModel.CountryId = record.CountryId;

                    // state/province
                    var s = _stateProvinceService.GetStateProvinceById(record.StateProvinceId);
                    sbtModel.StateProvinceName = (s != null) ? s.Name : "*";
                    sbtModel.StateProvinceId = record.StateProvinceId;

                    // ZIP / postal code
                    sbtModel.ZipPostalCode = (!string.IsNullOrEmpty(record.ZipPostalCode)) ? record.ZipPostalCode : "*";

                    return sbtModel;
                });
            });

            return Json(gridModel);
        }

        [HttpPost]  //TODO: Determine how to add back AdminAntiForgery
        public IActionResult GetRate(int id)
        {
            var shippingByTotalRecord = _shippingByTotalService.GetShippingByTotalRecordById(id);

            if (shippingByTotalRecord != null)
            {
                var model = new ShippingByTotalModel
                {
                    Id = shippingByTotalRecord.Id,
                    ZipPostalCode = shippingByTotalRecord.ZipPostalCode,
                    DisplayOrder = shippingByTotalRecord.DisplayOrder,
                    From = shippingByTotalRecord.From,
                    To = shippingByTotalRecord.To,
                    UsePercentage = shippingByTotalRecord.UsePercentage,
                    ShippingChargePercentage = shippingByTotalRecord.ShippingChargePercentage,
                    ShippingChargeAmount = shippingByTotalRecord.ShippingChargeAmount,
                    ShippingMethodId = shippingByTotalRecord.ShippingMethodId,
                    StoreId = shippingByTotalRecord.StoreId,
                    WarehouseId = shippingByTotalRecord.WarehouseId,
                    StateProvinceId = shippingByTotalRecord.StateProvinceId,
                    CountryId = shippingByTotalRecord.CountryId
                };

                return Json(model);
            }
            return new NullJsonResult();
        }

        [HttpPost]  //TODO: Determine how to add back AdminAntiForgery
        public IActionResult RateUpdate(ShippingByTotalModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
            {
                return AccessDeniedDataTablesJson();
            }

            var shippingByTotalRecord = _shippingByTotalService.GetShippingByTotalRecordById(model.Id);
            shippingByTotalRecord.ZipPostalCode = model.ZipPostalCode == "*" ? null : model.ZipPostalCode;
            shippingByTotalRecord.DisplayOrder = model.DisplayOrder;
            shippingByTotalRecord.From = model.From;
            shippingByTotalRecord.To = model.To;
            shippingByTotalRecord.UsePercentage = model.UsePercentage;
            shippingByTotalRecord.ShippingChargePercentage = model.UsePercentage ? model.ShippingChargePercentage : 0;
            shippingByTotalRecord.ShippingChargeAmount = !model.UsePercentage ? model.ShippingChargeAmount : 0;
            shippingByTotalRecord.ShippingMethodId = model.ShippingMethodId;
            shippingByTotalRecord.StoreId = model.StoreId;
            shippingByTotalRecord.WarehouseId = model.WarehouseId;
            shippingByTotalRecord.StateProvinceId = model.StateProvinceId;
            shippingByTotalRecord.CountryId = model.CountryId;
            _shippingByTotalService.UpdateShippingByTotalRecord(shippingByTotalRecord);

            return new NullJsonResult();
        }

        [HttpPost]  //TODO: Determine how to add back AdminAntiForgery
        public IActionResult RateDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
            {
                return AccessDeniedDataTablesJson();
            }

            var shippingByTotalRecord = _shippingByTotalService.GetShippingByTotalRecordById(id);
            if (shippingByTotalRecord != null)
            {
                _shippingByTotalService.DeleteShippingByTotalRecord(shippingByTotalRecord);
            }
            return new NullJsonResult();
        }

        [HttpPost]  //TODO: Determine how to add back AdminAntiForgery
        public IActionResult AddShippingRate(ShippingByTotalModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
            {
                return Json(new { Result = false, Message = _localizationService.GetResource("Plugins.Shipping.ByTotal.ManageShippingSettings.AccessDenied") });
            }

            var zipPostalCode = model.ZipPostalCode;

            if (zipPostalCode != null)
            {
                int zipMaxLength = ByTotalShippingComputationMethod.ZipPostalCodeMaxLength;
                zipPostalCode = zipPostalCode.Trim();
                if (zipPostalCode.Length > zipMaxLength)
                {
                    zipPostalCode = zipPostalCode.Substring(0, zipMaxLength);
                }
            }

            var shippingByTotalRecord = new ShippingByTotalRecord
            {
                ShippingMethodId = model.ShippingMethodId,
                StoreId = model.StoreId,
                WarehouseId = model.WarehouseId,
                CountryId = model.CountryId,
                StateProvinceId = model.StateProvinceId,
                ZipPostalCode = zipPostalCode,
                DisplayOrder = model.DisplayOrder,
                From = model.From,
                To = model.To,
                UsePercentage = model.UsePercentage,
                ShippingChargePercentage = (model.UsePercentage) ? model.ShippingChargePercentage : 0,
                ShippingChargeAmount = (model.UsePercentage) ? 0 : model.ShippingChargeAmount
            };
            _shippingByTotalService.InsertShippingByTotalRecord(shippingByTotalRecord);

            return Json(new { Result = true });
        }

        [HttpPost]  //TODO: Determine how to add back AdminAntiForgery
        public IActionResult SaveGeneralSettings(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
            {
                return Json(new { Result = false, Message = _localizationService.GetResource("Plugins.Shipping.ByTotal.ManageShippingSettings.AccessDenied") });
            }

            //save settings
            _shippingByTotalSettings.LimitMethodsToCreated = model.LimitMethodsToCreated;
            _settingService.SaveSetting(_shippingByTotalSettings);

            return Json(new { Result = true, Message = _localizationService.GetResource("Plugins.Shipping.ByTotal.ManageShippingSettings.Saved") });
        }
    }
}