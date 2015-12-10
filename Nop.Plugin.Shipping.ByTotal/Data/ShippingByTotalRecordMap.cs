using Nop.Data.Mapping;
using Nop.Plugin.Shipping.ByTotal.Domain;

namespace Nop.Plugin.Shipping.ByTotal.Data
{
    public class ShippingByTotalRecordMap : NopEntityTypeConfiguration<ShippingByTotalRecord>
    {
        public ShippingByTotalRecordMap()
        {
            this.ToTable("ShippingByTotal");
            this.HasKey(x => x.Id);

            this.Property(x => x.ZipPostalCode).HasMaxLength(400);
        }
    }
}