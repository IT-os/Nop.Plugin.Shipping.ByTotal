using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Plugin.Shipping.ByTotal.Domain;

namespace Nop.Plugin.Shipping.ByTotal.Services
{
    /// <summary>
    /// Shipping By Total Service
    /// </summary>
    public partial class ShippingByTotalService : IShippingByTotalService
    {
        #region Fields

        private readonly IRepository<ShippingByTotalRecord> _sbtRepository;
        private readonly ICacheManager _cacheManager;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="sbtRepository">ShippingByTotal Repository</param>
        public ShippingByTotalService(ICacheManager cacheManager,
            IRepository<ShippingByTotalRecord> sbtRepository)
        {
            this._cacheManager = cacheManager;
            this._sbtRepository = sbtRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all the ShippingByTotalRecords
        /// </summary>
        /// <returns>ShippingByTotalRecord collection</returns>
        public virtual IList<ShippingByTotalRecord> GetAllShippingByTotalRecords()
        {
            var query = from sbt in _sbtRepository.Table
                        orderby sbt.CountryId, sbt.ShippingMethodId, sbt.From
                        select sbt;

            var records = query.ToList();

            return records;
        }

        /// <summary>
        /// Finds the ShippingByTotalRecord by its identifier
        /// </summary>
        /// <param name="shippingByTotalRecordId">ShippingByTotalRecord identifier</param>
        /// <returns>ShippingByTotalRecord</returns>
        public virtual ShippingByTotalRecord GetShippingByTotalRecordById(int shippingByTotalRecordId)
        {
            if (shippingByTotalRecordId == 0)
            {
                return null;
            }

            var record = _sbtRepository.GetById(shippingByTotalRecordId);

            return record;
        }

        /// <summary>
        /// Finds the ShippingByTotalRecord
        /// </summary>
        /// <param name="shippingMethodId">shipping method identifier</param>
        /// <param name="countryId">country identifier</param>
        /// <param name="subtotal">subtotal</param>
        /// <returns>ShippingByTotalRecord</returns> 
        public virtual ShippingByTotalRecord FindShippingByTotalRecord(int shippingMethodId, int countryId, decimal subtotal)
        {
            var query = from sbt in _sbtRepository.Table
                        where sbt.ShippingMethodId == shippingMethodId && subtotal >= sbt.From && subtotal <= sbt.To
                        orderby sbt.CountryId, sbt.ShippingMethodId, sbt.From
                        select sbt;

            var existingRecords = query.ToList();

            //filter by country
            foreach (var sbt in existingRecords)
            {
                if (countryId == sbt.CountryId)
                {
                    return sbt;
                }
            }

            foreach (var sbt in existingRecords)
            {
                if (sbt.CountryId == 0)
                {
                    return sbt;
                }
            }

            return null;
        }

        /// <summary>
        /// Deletes the ShippingByTotalRecord
        /// </summary>
        /// <param name="shippingByTotalRecord">ShippingByTotalRecord</param>
        public virtual void DeleteShippingByTotalRecord(ShippingByTotalRecord shippingByTotalRecord)
        {
            if (shippingByTotalRecord == null)
            {
                throw new ArgumentNullException("shippingByTotalRecord");
            }

            _sbtRepository.Delete(shippingByTotalRecord);
        }

        /// <summary>
        /// Inserts the ShippingByTotalRecord
        /// </summary>
        /// <param name="shippingByTotalRecord">ShippingByTotalRecord</param>
        public virtual void InsertShippingByTotalRecord(ShippingByTotalRecord shippingByTotalRecord)
        {
            if (shippingByTotalRecord == null)
            {
                throw new ArgumentNullException("shippingByTotalRecord");
            }

            _sbtRepository.Insert(shippingByTotalRecord);
        }

        /// <summary>
        /// Updates the ShippingByTotalRecord
        /// </summary>
        /// <param name="shippingByTotalRecord">ShippingByTotalRecord</param>
        public virtual void UpdateShippingByTotalRecord(ShippingByTotalRecord shippingByTotalRecord)
        {
            if (shippingByTotalRecord == null)
            {
                throw new ArgumentNullException("shippingByTotalRecord");
            }

            _sbtRepository.Update(shippingByTotalRecord);
        }

        #endregion
    }
}
