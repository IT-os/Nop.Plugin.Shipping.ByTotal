using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
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
        #region Constants

        private const string SHIPPINGBYTOTAL_ALL_KEY = "Nop.shippingbytotal.all";
        private const string SHIPPINGBYTOTAL_PATTERN_KEY = "Nop.shippingbytotal.";

        #endregion

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
        /// <param name="pageIndex">page index</param>
        /// <param name="pageSize">page size</param>
        /// <returns>ShippingByTotalRecord collection</returns>
        public virtual IPagedList<ShippingByTotalRecord> GetAllShippingByTotalRecords(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            string key = SHIPPINGBYTOTAL_ALL_KEY;
            return _cacheManager.Get(key, () =>
            {
                var query = from sbt in _sbtRepository.Table
                            orderby sbt.StoreId, sbt.CountryId, sbt.StateProvinceId, sbt.DisplayOrder, sbt.ShippingMethodId, sbt.From, sbt.Id
                            select sbt;

                var records = new PagedList<ShippingByTotalRecord>(query, pageIndex, pageSize);

                return records;
            });
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
        /// <param name="storeId">store identifier</param>
        /// <param name="subtotal">order's subtotal</param>
        /// <param name="countryId">country identifier</param>
        /// <param name="subtotal">subtotal</param>
        /// <param name="stateProvinceId">state province identifier</param>
        /// <param name="zipPostalCode">ZIP / postal code</param>
        /// <returns>ShippingByTotalRecord</returns>
        public virtual ShippingByTotalRecord FindShippingByTotalRecord(int shippingMethodId, int storeId,
            int countryId, decimal subtotal, int stateProvinceId, string zipPostalCode)
        {
            if (zipPostalCode == null)
            {
                zipPostalCode = string.Empty;
            }
            else
            {
                zipPostalCode = zipPostalCode.Trim();
            }

            // filter by shipping method and subtotal
            var existingRates = GetAllShippingByTotalRecords()
                .Where(sbt => sbt.ShippingMethodId == shippingMethodId && subtotal >= sbt.From && subtotal <= sbt.To)
                .ToList();

            // filter by store
            var matchedByStore = new List<ShippingByTotalRecord>();
            foreach (var sbtr in existingRates)
            {
                if (storeId == sbtr.StoreId)
                {
                    matchedByStore.Add(sbtr);
                }
            }
            if (matchedByStore.Count == 0)
            {
                foreach (var sbtr in existingRates)
                {
                    if (sbtr.StoreId == 0)
                    {
                        matchedByStore.Add(sbtr);
                    }
                }
            }

            // filter by country
            var matchedByCountry = new List<ShippingByTotalRecord>();
            foreach (var sbtr in matchedByStore)
            {
                if (countryId == sbtr.CountryId)
                {
                    matchedByCountry.Add(sbtr);
                }
            }
            if (matchedByCountry.Count == 0)
            {
                foreach (var sbtr in matchedByStore)
                {
                    if (sbtr.CountryId == 0)
                    {
                        matchedByCountry.Add(sbtr);
                    }
                }
            }

            // filter by state/province
            var matchedByStateProvince = new List<ShippingByTotalRecord>();
            foreach (var sbtr in matchedByCountry)
            {
                if (stateProvinceId == sbtr.StateProvinceId)
                {
                    matchedByStateProvince.Add(sbtr);
                }
            }
            if (matchedByStateProvince.Count == 0)
            {
                foreach (var sbtr in matchedByCountry)
                {
                    if (sbtr.StateProvinceId == 0)
                    {
                        matchedByStateProvince.Add(sbtr);
                    }
                }
            }

            // filter by ZIP / postal code
            var matchedByZipPostalCode = new List<ShippingByTotalRecord>();
            foreach (var sbtr in matchedByStateProvince)
            {
                if (CheckZipPostalCode(zipPostalCode, sbtr.ZipPostalCode))
                {
                    matchedByZipPostalCode.Add(sbtr);
                }
            }

            if (matchedByZipPostalCode.Count == 0)
            {
                foreach (var sbtr in matchedByStateProvince)
                {
                    if (String.IsNullOrWhiteSpace(sbtr.ZipPostalCode))
                    {
                        matchedByZipPostalCode.Add(sbtr);
                    }
                }
            }

            return matchedByZipPostalCode.FirstOrDefault();
        }

        /// <summary>
        /// Checks if the request ZIP postal code matches the ZIP postal code of the ShippingByTotalRecord
        /// </summary>
        /// <param name="zipPostalCode">ZIP / postal code</param>
        /// <param name="shippingByTotalRecordZip">ShippingByTotalRecord ZIP postal code</param>
        /// <returns>Whether the request ZIP postal code matches the record's ZIP postal code</returns>
        private static bool CheckZipPostalCode(string zipPostalCode, string shippingByTotalRecordZip)
        {
            if (string.IsNullOrEmpty(zipPostalCode) && !string.IsNullOrEmpty(shippingByTotalRecordZip))
            {
                return false;
            }

            if (String.IsNullOrEmpty(zipPostalCode) && String.IsNullOrEmpty(shippingByTotalRecordZip))
            {
                return true;
            }

            if (zipPostalCode.Contains('-'))
            {
                zipPostalCode = RemovePlus4(zipPostalCode);
            }

            if (zipPostalCode.Equals(shippingByTotalRecordZip, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (shippingByTotalRecordZip != null &&
               (shippingByTotalRecordZip.Contains(',') || shippingByTotalRecordZip.Contains(':') || shippingByTotalRecordZip.Contains('?')))
            {
                var allZips = shippingByTotalRecordZip.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var entry in allZips)
                {
                    var zipEntry = entry.Trim();

                    if (zipEntry.Contains('-'))
                    {
                        zipEntry = RemovePlus4(zipEntry);
                    }

                    if (zipPostalCode.Equals(zipEntry, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }

                    if (zipEntry.Contains('?') && zipPostalCode.Length == zipEntry.Length)
                    {
                        var targetZip = zipPostalCode.ToLower().ToArray();
                        var wildcardZip = zipEntry.ToLower().ToArray();

                        for (int x = 0; x < wildcardZip.Length; x++)
                        {
                            if (wildcardZip[x] != '?' && targetZip[x] != wildcardZip[x])
                            {
                                continue;
                            }

                            if (x + 1 == wildcardZip.Length)
                            {
                                return true;
                            }
                        }
                    }

                    if (zipEntry.Contains(':'))
                    {
                        var zipRanges = zipEntry.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        int range1 = 0, range2 = 0, lower = 0, upper = 0, targetZip = 0;
                        bool range1Numeric = false, range2Numeric = false;

                        if (zipRanges.Any())
                        {
                            if (int.TryParse(zipPostalCode, out targetZip))
                            {
                                if (zipRanges[0] != null)
                                {
                                    range1Numeric = int.TryParse(zipRanges[0], out range1);

                                    if (range1Numeric && zipRanges[1] != null)
                                    {
                                        range2Numeric = int.TryParse(zipRanges[1], out range2);
                                    }
                                }

                                if (range1Numeric && range2Numeric)
                                {
                                    lower = range1;
                                    upper = range2;
                                    if (range1 > range2)
                                    {
                                        lower = range2;
                                        upper = range1;
                                    }

                                    if (targetZip >= lower & targetZip <= upper)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Remvoves +4 section of ZIP+4
        /// </summary>
        /// <param name="zip">ZIP code with +4 (contains '-' character)</param>
        /// <returns>ZIP code without +4</returns>
        private static string RemovePlus4(string zip)
        {
            int i = zip.IndexOf('-');

            zip = zip.Substring(0, (i > 0) ? i : zip.Length);

            return zip;
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

            _cacheManager.RemoveByPattern(SHIPPINGBYTOTAL_PATTERN_KEY);
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

            _cacheManager.RemoveByPattern(SHIPPINGBYTOTAL_PATTERN_KEY);
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

            _cacheManager.RemoveByPattern(SHIPPINGBYTOTAL_PATTERN_KEY);
        }

        #endregion
    }
}
