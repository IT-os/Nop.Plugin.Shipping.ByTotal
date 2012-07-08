﻿using System.Collections.Generic;
using Nop.Plugin.Shipping.ByTotal.Domain;

namespace Nop.Plugin.Shipping.ByTotal.Services
{
    public partial interface IShippingByTotalService
    {
        /// <summary>
        /// Gets all the ShippingByTotalRecords
        /// </summary>
        /// <returns>ShippingByTotalRecord collection</returns>
        IList<ShippingByTotalRecord> GetAllShippingByTotalRecords();

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
        /// <param name="countryId">country identifier</param>
        /// <param name="subtotal">subtotal</param>
        /// <returns>ShippingByTotalRecord</returns>
        ShippingByTotalRecord FindShippingByTotalRecord(int shippingMethodId, int countryId, decimal subTotal);

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