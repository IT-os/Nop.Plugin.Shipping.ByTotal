using System;
using System.Collections.Generic;
using System.Web.Routing;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Plugin.Shipping.ByTotal.Data;
using Nop.Plugin.Shipping.ByTotal.Services;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Shipping;

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
        private readonly ILocalizationService _localizationService;
        private readonly ILanguageService _languageService;
        private readonly ILogger _logger;

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
        /// <param name="localizationService">Localization service</param>
        /// <param name="languageService">Language service</param>
        /// <param name="logger">Logger</param>
        public ByTotalShippingComputationMethod(IShippingService shippingService,
            IShippingByTotalService shippingByTotalService,
            ShippingByTotalSettings shippingByTotalSettings,
            ShippingByTotalObjectContext objectContext,
            IPriceCalculationService priceCalculationService,
            ILocalizationService localizationService,
            ILanguageService languageService,
            ILogger logger)
        {
            this._shippingService = shippingService;
            this._shippingByTotalService = shippingByTotalService;
            this._shippingByTotalSettings = shippingByTotalSettings;
            this._objectContext = objectContext;
            this._priceCalculationService = priceCalculationService;
            this._localizationService = localizationService;
            this._languageService = languageService;
            this._logger = logger;
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

        /// <summary>
        /// Inserts or updates language resources for the plugin
        /// </summary>
        /// <param name="localeStringResources">collection of resources used by the plugin</param>
        /// <param name="languages">collection of languages for which to install the resources</param>
        private void InstallPluginLocaleStringResources(IList<LocaleStringResource> localeStringResources, IEnumerable<Language> languages)
        {
            foreach (var language in languages)
            {
                var languageId = language.Id;

                foreach (var lsr in localeStringResources)
                {
                    lsr.LanguageId = languageId;

                    var resource = _localizationService.GetLocaleStringResourceByName(lsr.ResourceName, languageId, false);

                    if (resource != null)
                    {
                        resource.ResourceName = lsr.ResourceName;
                        resource.ResourceValue = lsr.ResourceValue;

                        _localizationService.UpdateLocaleStringResource(resource);
                    }
                    else
                    {
                        _localizationService.InsertLocaleStringResource(lsr);
                    }
                }
            }
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
            _objectContext.Install();
            base.Install();

            var allLanguages = _languageService.GetAllLanguages();
            var localeStringResources = new List<LocaleStringResource>()
                {
                    new LocaleStringResource { ResourceName = "Plugins.Shipping.ByTotal.Fields.Country", ResourceValue = "Country" },    
                    new LocaleStringResource { ResourceName = "Plugins.Shipping.ByTotal.Fields.Country.Hint", ResourceValue = "If an asterisk is selected, then this tax rate will apply to all customers, regardless of the country." },
                    new LocaleStringResource { ResourceName = "Plugins.Shipping.ByTotal.Fields.ShippingMethod", ResourceValue = "Shipping method" },
                    new LocaleStringResource { ResourceName = "Plugins.Shipping.ByTotal.Fields.ShippingMethod.Hint", ResourceValue = "The shipping method." },
                    new LocaleStringResource { ResourceName = "Plugins.Shipping.ByTotal.Fields.From", ResourceValue = "Order total From" },
                    new LocaleStringResource { ResourceName = "Plugins.Shipping.ByTotal.Fields.From.Hint", ResourceValue = "Order total from." },
                    new LocaleStringResource { ResourceName = "Plugins.Shipping.ByTotal.Fields.To", ResourceValue = "Order total To" },
                    new LocaleStringResource { ResourceName = "Plugins.Shipping.ByTotal.Fields.To.Hint", ResourceValue = "Order total to." },
                    new LocaleStringResource { ResourceName = "Plugins.Shipping.ByTotal.Fields.UsePercentage", ResourceValue = "Use percentage" },
                    new LocaleStringResource { ResourceName = "Plugins.Shipping.ByTotal.Fields.UsePercentage.Hint", ResourceValue = "Check to use 'charge percentage' value." },
                    new LocaleStringResource { ResourceName = "Plugins.Shipping.ByTotal.Fields.ShippingChargePercentage", ResourceValue = "Charge percentage (of subtotal)" },
                    new LocaleStringResource { ResourceName = "Plugins.Shipping.ByTotal.Fields.ShippingChargePercentage.Hint", ResourceValue = "Charge percentage (of subtotal)." },
                    new LocaleStringResource { ResourceName = "Plugins.Shipping.ByTotal.Fields.ShippingChargeAmount", ResourceValue = "Charge amount" },
                    new LocaleStringResource { ResourceName = "Plugins.Shipping.ByTotal.Fields.ShippingChargeAmount.Hint", ResourceValue = "Charge amount." },
                    new LocaleStringResource { ResourceName = "Plugins.Shipping.ByTotal.Fields.LimitMethodsToCreated", ResourceValue = "Limit shipping methods to configured ones" },
                    new LocaleStringResource { ResourceName = "Plugins.Shipping.ByTotal.Fields.LimitMethodsToCreated.Hint", ResourceValue = "If you check this option, then your customers will be limited to shipping options configured here. Otherwise, they'll be able to choose any existing shipping options even if they're not configured here (zero shipping fee in this case)." },
                    new LocaleStringResource { ResourceName = "Plugins.Shipping.ByTotal.AddNewRecordTitle", ResourceValue = "Add new 'Shipping By Total' record" },
                    new LocaleStringResource { ResourceName = "Plugins.Shipping.ByTotal.SettingsTitle", ResourceValue = "Shipping By Total Settings" }                    
                };

            InstallPluginLocaleStringResources(localeStringResources, allLanguages);
            _logger.Information(string.Format("Plugin installed: SystemName: {0}, Version: {1}, Description: '{2}'", PluginDescriptor.SystemName, PluginDescriptor.Version, PluginDescriptor.FriendlyName));
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            _objectContext.Uninstall();
            base.Uninstall();
        }

        #endregion
    }
}
