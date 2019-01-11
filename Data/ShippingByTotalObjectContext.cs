using Microsoft.EntityFrameworkCore;
using Nop.Core;
using Nop.Data;
using Nop.Data.Extensions;
using Nop.Plugin.Shipping.ByTotal.Domain;
using System;
using System.Linq;

namespace Nop.Plugin.Shipping.ByTotal.Data
{
    /// <summary>
    /// Object context
    /// </summary>
    public class ShippingByTotalObjectContext : DbContext, IDbContext
    {
        #region Ctor

        public ShippingByTotalObjectContext(DbContextOptions<ShippingByTotalObjectContext> options)
            : base(options)
        {
        }

        #endregion Ctor

        #region Utilities

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ShippingByTotalRecordMap());
            base.OnModelCreating(modelBuilder);
        }

        #endregion Utilities

        #region Methods

        public virtual string GenerateCreateScript()
        {
            return this.Database.GenerateCreateScript();
        }

        public virtual new DbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        /// <summary>
        /// Install
        /// </summary>
        public void Install()
        {
            // create table
            this.ExecuteSqlScript(this.GenerateCreateScript());
        }

        /// <summary>
        /// Uninstall
        /// </summary>
        public void Uninstall()
        {
            // drop table
            this.DropPluginTable(nameof(ShippingByTotalRecord));
        }

        public virtual IQueryable<TQuery> QueryFromSql<TQuery>(string sql) where TQuery : class
        {
            throw new NotImplementedException();
        }

        public virtual IQueryable<TEntity> EntityFromSql<TEntity>(string sql, params object[] parameters) where TEntity : BaseEntity
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes the given DDL/DML command against the database.
        /// </summary>
        /// <param name="sql">The command string</param>
        /// <param name="doNotEnsureTransaction">false - the transaction creation is not ensured; true - the transaction creation is ensured.</param>
        /// <param name="timeout">Timeout value, in seconds. A null value indicates that the default value of the underlying provider will be used</param>
        /// <param name="parameters">The parameters to apply to the command string.</param>
        /// <returns>The result returned by the database after executing the command.</returns>
        public virtual int ExecuteSqlCommand(RawSqlString sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            using (var transaction = this.Database.BeginTransaction())
            {
                var result = this.Database.ExecuteSqlCommand(sql, parameters);
                transaction.Commit();

                return result;
            }
        }

        /// <summary>
        /// Detach an entity
        /// </summary>
        /// <param name="entity">Entity</param>
        public virtual void Detach<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}