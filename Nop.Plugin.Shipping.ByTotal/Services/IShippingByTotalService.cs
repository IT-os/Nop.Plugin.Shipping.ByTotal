using Nop.Core;
using Nop.Plugin.Shipping.ByTotal.Domain;

namespace Nop.Plugin.Shipping.ByTotal.Services
{
    public partial interface IShippingByTotalService
    {
        /// <summary>
        /// Gets all the ShippingByTotalRecords
        /// </summary>
        /// <param name="pageIndex">page index</param>
        /// <param name="pageSize">page size</param>
        /// <returns>ShippingByTotalRecord collection</returns>
        IPagedList<ShippingByTotalRecord> GetAllShippingByTotalRecords(int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Finds the ShippingByTotalRecord by its identifier
        /// </summary>
        /// <param name="shippingByTotalRecordId">ShippingByTotalRecord identifier</param>
        /// <returns>ShippingByTotalRecord</returns>
        ShippingByTotalRecord GetShippingByTotalRecordById(int shippingByTotalRecordId);

        /// <summary>
        /// Finds the ShippingByTotalRecord
        /// </summary>
        /// <param name="shippingMethodId">shipping method identifier</param>
        /// <param name="storeId">store identifier</param>
        /// <param name="subtotal">order's subtotal</param>
        /// <param name="countryId">country identifier</param>
        /// <param name="subtotal">subtotal</param>
        /// <param name="stateProvinceId">state province identifier</param>
        /// <param name="zip">Zip code</param>
        /// <returns>ShippingByTotalRecord</returns>
        ShippingByTotalRecord FindShippingByTotalRecord(int shippingMethodId, int storeId,
            int countryId, decimal subtotal, int stateProvinceId, string zip);

        /// <summary>
        /// Deletes the ShippingByTotalRecord
        /// </summary>
        /// <param name="shippingByTotalRecord">ShippingByTotalRecord</param>
        void DeleteShippingByTotalRecord(ShippingByTotalRecord shippingByTotalRecord);

        /// <summary>
        /// Inserts the ShippingByTotalRecord
        /// </summary>
        /// <param name="shippingByTotalRecord">ShippingByTotalRecord</param>
        void InsertShippingByTotalRecord(ShippingByTotalRecord shippingByTotalRecord);

        /// <summary>
        /// Updates the ShippingByTotalRecord
        /// </summary>
        /// <param name="shippingByTotalRecord">ShippingByTotalRecord</param>
        void UpdateShippingByTotalRecord(ShippingByTotalRecord shippingByTotalRecord);
    }
}
