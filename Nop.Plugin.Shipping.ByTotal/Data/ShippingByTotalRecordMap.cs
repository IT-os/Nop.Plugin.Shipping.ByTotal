using System.Data.Entity.ModelConfiguration;
using Nop.Plugin.Shipping.ByTotal.Domain;

namespace Nop.Plugin.Shipping.ByTotal.Data
{
    public class ShippingByTotalRecordMap : EntityTypeConfiguration<ShippingByTotalRecord>
    {
        public ShippingByTotalRecordMap()
        {
            this.ToTable("ShippingByTotal");
            this.HasKey(x => x.Id);
        }
    }
}
