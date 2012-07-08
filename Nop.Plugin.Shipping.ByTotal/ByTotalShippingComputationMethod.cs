using System;
using System.Web.Routing;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Plugin.Shipping.ByTotal.Data;
using Nop.Plugin.Shipping.ByTotal.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;

namespace Nop.Plugin.Shipping.ByTotal
{
    public class ByTotalShippingComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Fields

        private readonly IShippingService _shippingService;
        private readonly IShippingByTotalService _shippingByTotalService;
        private readonly ShippingByTotalSettings _shippingByTotalSettings;
        private readonly ShippingByTotalObjectContext _objectContext;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="shippingService">Shipping service</param>
        /// <param name="shippingByTotalService">ShippingByTotal service</param>
        /// <param name="shippingByTotalSettings">ShippingByTotal settings</param>
        /// <param name="objectContext">ShippingByTotal object context</param>
        /// <param name="priceCalculationService">PriceCalculation service</param>
        /// <param name="settingService">Settings service</param>
        /// <param name="logger">Logger</param>
        public ByTotalShippingComputationMethod(IShippingService shippingService,
            IShippingByTotalService shippingByTotalService,
            ShippingByTotalSettings shippingByTotalSettings,
            ShippingByTotalObjectContext objectContext,
            IPriceCalculationService priceCalculationService,
            ILogger logger,
            ISettingService settingService)
        {
            this._shippingService = shippingService;
            this._shippingByTotalService = shippingByTotalService;
            this._shippingByTotalSettings = shippingByTotalSettings;
            this._objectContext = objectContext;
            this._priceCalculationService = priceCalculationService;
            this._logger = logger;
            this._settingService = settingService;
        }

        #endregion

        #region Properties

        /// <summary>
        ///  Gets a shipping rate computation method type
        /// </summary>
        public ShippingRateComputationMethodType ShippingRateComputationMethodType
        {
            get
            {
                return ShippingRateComputationMethodType.Offline;
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets the rate for the shipping method
        /// </summary>
        /// <param name="subtotal">the order's subtotal</param>
        /// <param name="shippingMethodId">the shipping method identifier</param>
        /// <param name="countryId">country identifier</param>
        /// <returns>the rate for the shipping method</returns>
        private decimal? GetRate(decimal subtotal, int shippingMethodId, int countryId)
        {
            decimal? shippingTotal = null;

            var shippingByTotalRecord = _shippingByTotalService.FindShippingByTotalRecord(shippingMethodId, countryId, subtotal);

            if (shippingByTotalRecord == null)
            {
                if (_shippingByTotalSettings.LimitMethodsToCreated)
                {
                    return null;
                }
                else
                {
                    return decimal.Zero;
                }
            }

            if (shippingByTotalRecord.UsePercentage && shippingByTotalRecord.ShippingChargePercentage <= decimal.Zero)
            {
                return decimal.Zero;
            }

            if (!shippingByTotalRecord.UsePercentage && shippingByTotalRecord.ShippingChargeAmount <= decimal.Zero)
            {
                return decimal.Zero;
            }

            if (shippingByTotalRecord.UsePercentage)
            {
                shippingTotal = Math.Round((decimal)((((float)subtotal) * ((float)shippingByTotalRecord.ShippingChargePercentage)) / 100f), 2);
            }
            else
            {
                shippingTotal = shippingByTotalRecord.ShippingChargeAmount;
            }

            if (shippingTotal < decimal.Zero)
            {
                shippingTotal = decimal.Zero;
            }

            return shippingTotal;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Represents a response of getting shipping rate options</returns>
        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
            {
                throw new ArgumentNullException("getShippingOptionRequest");
            }

            var response = new GetShippingOptionResponse();

            if (getShippingOptionRequest.Items == null || getShippingOptionRequest.Items.Count == 0)
            {
                response.AddError("No shipment items");
                return response;
            }
            if (getShippingOptionRequest.ShippingAddress == null)
            {
                response.AddError("Shipping address is not set");
                return response;
            }

            int countryId = getShippingOptionRequest.ShippingAddress.CountryId.HasValue ? getShippingOptionRequest.ShippingAddress.CountryId.Value : 0;

            decimal subTotal = decimal.Zero;
            foreach (var shoppingCartItem in getShippingOptionRequest.Items)
            {
                if (shoppingCartItem.IsFreeShipping || !shoppingCartItem.IsShipEnabled)
                {
                    continue;
                }
                subTotal += _priceCalculationService.GetSubTotal(shoppingCartItem, true);
            }

            var shippingMethods = _shippingService.GetAllShippingMethods(countryId);
            foreach (var shippingMethod in shippingMethods)
            {
                decimal? rate = GetRate(subTotal, shippingMethod.Id, countryId);
                if (rate.HasValue)
                {
                    var shippingOption = new ShippingOption();
                    shippingOption.Name = shippingMethod.Name;
                    shippingOption.Description = shippingMethod.Description;
                    shippingOption.Rate = rate.Value;
                    response.ShippingOptions.Add(shippingOption);
                }
            }

            return response;
        }

        /// <summary>
        /// Gets fixed shipping rate (if shipping rate computation method allows it and the rate can be calculated before checkout).
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Fixed shipping rate; or null in case there's no fixed shipping rate</returns>
        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return null;
        }

        /// <summary>
        /// Gets a shipment tracker
        /// </summary>
        public IShipmentTracker ShipmentTracker
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out System.Web.Routing.RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "ShippingByTotal";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Shipping.ByTotal.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {            
            var settings = new ShippingByTotalSettings()
            {                
                LimitMethodsToCreated = false
            };
            _settingService.SaveSetting(settings);

            _objectContext.Install();

            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.Country", "Country");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.Country.Hint", "If an asterisk is selected, then this shipping rate will apply to all customers, regardless of the country.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.ShippingMethod", "Shipping method");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.ShippingMethod.Hint", "The shipping method.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.From", "Order total From");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.From.Hint", "Order total from.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.To", "Order total To");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.To.Hint", "Order total to.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.UsePercentage", "Use percentage");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.UsePercentage.Hint", "Check to use 'charge percentage' value.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.ShippingChargePercentage", "Charge percentage (of subtotal)");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.ShippingChargePercentage.Hint", "Charge percentage (of subtotal).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.ShippingChargeAmount", "Charge amount");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.ShippingChargeAmount.Hint", "Charge amount.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.LimitMethodsToCreated", "Limit shipping methods to configured ones");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.LimitMethodsToCreated.Hint", "If you check this option, then your customers will be limited to shipping options configured here. Otherwise, they'll be able to choose any existing shipping options even if they're not configured here (zero shipping fee in this case).");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByTotal.AddNewRecordTitle", "Add new 'Shipping By Total' record");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.ByTotal.SettingsTitle", "Shipping By Total Settings");

            base.Install();

            _logger.Information(string.Format("Plugin installed: SystemName: {0}, Version: {1}, Description: '{2}'", PluginDescriptor.SystemName, PluginDescriptor.Version, PluginDescriptor.FriendlyName));
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            _settingService.DeleteSetting<ShippingByTotalSettings>();

            _objectContext.Uninstall();

            this.DeletePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.Country");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.Country.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.ShippingMethod");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.ShippingMethod.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.From");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.From.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.To");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.To.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.UsePercentage");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.UsePercentage.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.ShippingChargePercentage");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.ShippingChargePercentage.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.ShippingChargeAmount");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.ShippingChargeAmount.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.LimitMethodsToCreated");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByTotal.Fields.LimitMethodsToCreated.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByTotal.AddNewRecordTitle");
            this.DeletePluginLocaleResource("Plugins.Shipping.ByTotal.SettingsTitle");

            base.Uninstall();
        }

        #endregion
    }
}
