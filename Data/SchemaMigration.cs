using FluentMigrator;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Data.Migrations;
using Nop.Plugin.Shipping.ByTotal.Domain;

namespace Nop.Plugin.Shipping.ByTotal.Data
{


    public partial class ShippingByTotalRecordBuilder : NopEntityBuilder<ShippingByTotalRecord>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ShippingByTotalRecord.ZipPostalCode)).AsString(400);

        }

        #endregion
    }

    [SkipMigrationOnUpdate]
    [NopMigration("2020/06/11 09:09:17:6455442", "Nop.Plugin.Shipping.ByTotal base schema")]
    public class SchemaMigration : AutoReversingMigration
    {
        #region Fields

        protected IMigrationManager _migrationManager;

        #endregion

        #region Ctor

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            _migrationManager.BuildTable<ShippingByTotalRecord>(Create);
        }

        #endregion
    }
}