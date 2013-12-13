using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
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
using Nop.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace Nop.Plugin.Shipping.ByTotal.Controllers
{
    [AdminAuthorize]
    public class ShippingByTotalController : Controller
    {
        private readonly IShippingService _shippingService;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IShippingByTotalService _shippingByTotalService;
        private readonly ShippingByTotalSettings _shippingByTotalSettings;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;

        public ShippingByTotalController(IShippingService shippingService,
            IStoreService storeService,
            ISettingService settingService,
            IShippingByTotalService shippingByTotalService,
            ShippingByTotalSettings shippingByTotalSettings,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IPermissionService permissionService,
            ILocalizationService localizationService)
        {
            this._shippingService = shippingService;
            this._storeService = storeService;
            this._settingService = settingService;
            this._shippingByTotalService = shippingByTotalService;
            this._shippingByTotalSettings = shippingByTotalSettings;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._permissionService = permissionService;
            this._localizationService = localizationService;
        }

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {
            //always set culture to 'en-US' (Telerik Grid has a bug related to editing decimal values in other cultures). Like currently it's done for admin area in Global.asax.cs
            var culture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            base.Initialize(requestContext);
        }

        [ChildActionOnly]
        public ActionResult Configure()
        {
            var shippingMethods = _shippingService.GetAllShippingMethods();
            if (shippingMethods.Count == 0)
            {
                return Content("No shipping methods can be loaded");
            }

            var model = new ShippingByTotalListModel();

            // stores
            model.AvailableStores.Add(new SelectListItem() { Text = "*", Value = "0" });
            foreach (var store in _storeService.GetAllStores())
            {
                model.AvailableStores.Add(new SelectListItem() { Text = store.Name, Value = store.Id.ToString() });
            }

            // shipping methods
            foreach (var sm in shippingMethods)
            {
                model.AvailableShippingMethods.Add(new SelectListItem() { Text = sm.Name, Value = sm.Id.ToString() });
            }

            // countries
            model.AvailableCountries.Add(new SelectListItem() { Text = "*", Value = "0" });
            var countries = _countryService.GetAllCountries(true);
            foreach (var c in countries)
            {
                model.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString() });
            }

            model.AvailableStates.Add(new SelectListItem() { Text = "*", Value = "0" });
            model.LimitMethodsToCreated = _shippingByTotalSettings.LimitMethodsToCreated;
            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;

            return View("Nop.Plugin.Shipping.ByTotal.Views.ShippingByTotal.Configure", model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult RatesList(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
            {
                return Content(_localizationService.GetResource("Plugins.Shipping.ByTotal.ManageShippingSettings.AccessDenied"));
            }

            var records = _shippingByTotalService.GetAllShippingByTotalRecords(command.Page - 1, command.PageSize);
            var sbtModel = records.Select(x =>
                {
                    var m = new ShippingByTotalModel
                    {
                        Id = x.Id,
                        StoreId = x.StoreId,
                        ShippingMethodId = x.ShippingMethodId,
                        CountryId = x.CountryId,
                        DisplayOrder = x.DisplayOrder,
                        From = x.From,
                        To = x.To,
                        UsePercentage = x.UsePercentage,
                        ShippingChargePercentage = x.ShippingChargePercentage,
                        ShippingChargeAmount = x.ShippingChargeAmount,
                    };

                    // shipping method
                    var shippingMethod = _shippingService.GetShippingMethodById(x.ShippingMethodId);
                    m.ShippingMethodName = (shippingMethod != null) ? shippingMethod.Name : "Unavailable";

                    // store
                    var store = _storeService.GetStoreById(x.StoreId);
                    m.StoreName = (store != null) ? store.Name : "*";

                    // country
                    var c = _countryService.GetCountryById(x.CountryId);
                    m.CountryName = (c != null) ? c.Name : "*";

                    // state/province
                    var s = _stateProvinceService.GetStateProvinceById(x.StateProvinceId);
                    m.StateProvinceName = (s != null) ? s.Name : "*";

                    // ZIP / postal code
                    m.ZipPostalCode = (!String.IsNullOrEmpty(x.ZipPostalCode)) ? x.ZipPostalCode : "*";

                    return m;
                })
                .ToList();
            var model = new GridModel<ShippingByTotalModel>
            {
                Data = sbtModel,
                Total = records.TotalCount
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult RateUpdate(ShippingByTotalModel model, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
            {
                return Content(_localizationService.GetResource("Plugins.Shipping.ByTotal.ManageShippingSettings.AccessDenied"));
            }

            if (!ModelState.IsValid)
            {
                return new JsonResult { Data = "error" };
            }

            var shippingByTotalRecord = _shippingByTotalService.GetShippingByTotalRecordById(model.Id);
            shippingByTotalRecord.ZipPostalCode = model.ZipPostalCode == "*" ? null : model.ZipPostalCode;
            shippingByTotalRecord.DisplayOrder = model.DisplayOrder;
            shippingByTotalRecord.From = model.From;
            shippingByTotalRecord.To = model.To;
            shippingByTotalRecord.UsePercentage = model.UsePercentage;
            shippingByTotalRecord.ShippingChargeAmount = model.ShippingChargeAmount;
            shippingByTotalRecord.ShippingChargePercentage = model.ShippingChargePercentage;
            _shippingByTotalService.UpdateShippingByTotalRecord(shippingByTotalRecord);

            return RatesList(command);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult RateDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
            {
                return Content(_localizationService.GetResource("Plugins.Shipping.ByTotal.ManageShippingSettings.AccessDenied"));
            }

            var shippingByTotalRecord = _shippingByTotalService.GetShippingByTotalRecordById(id);
            if (shippingByTotalRecord != null)
            {
                _shippingByTotalService.DeleteShippingByTotalRecord(shippingByTotalRecord);
            }
            return RatesList(command);
        }

        [HttpPost]
        public ActionResult AddShippingRate(ShippingByTotalListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
            {
                return Json(new { Result = false, Message = _localizationService.GetResource("Plugins.Shipping.ByTotal.ManageShippingSettings.AccessDenied") });
            }

            var zipPostalCode = model.AddZipPostalCode;

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
                ShippingMethodId = model.AddShippingMethodId,
                StoreId = model.AddStoreId,
                CountryId = model.AddCountryId,
                StateProvinceId = model.AddStateProvinceId,
                ZipPostalCode = zipPostalCode,
                DisplayOrder = model.AddDisplayOrder,
                From = model.AddFrom,
                To = model.AddTo,
                UsePercentage = model.AddUsePercentage,
                ShippingChargePercentage = (model.AddUsePercentage) ? model.AddShippingChargePercentage : 0,
                ShippingChargeAmount = (model.AddUsePercentage) ? 0 : model.AddShippingChargeAmount
            };
            _shippingByTotalService.InsertShippingByTotalRecord(shippingByTotalRecord);

            return Json(new { Result = true });
        }

        [HttpPost]
        public ActionResult SaveGeneralSettings(ShippingByTotalListModel model)
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
