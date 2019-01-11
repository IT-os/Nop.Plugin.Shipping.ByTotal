using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Shipping.ByTotal.Domain;

namespace Nop.Plugin.Shipping.ByTotal.Data
{
    /// <summary>
    /// Entity mapping
    /// </summary>
    public class ShippingByTotalRecordMap : NopEntityTypeConfiguration<ShippingByTotalRecord>
    {
        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<ShippingByTotalRecord> builder)
        {
            builder.ToTable(nameof(ShippingByTotalRecord));
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ZipPostalCode).HasMaxLength(400);
        }
    }
}