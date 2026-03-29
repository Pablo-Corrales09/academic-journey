using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HotelCarga.DbModel
{
    public class HotelCargaContext : DbContext
    {
        public HotelCargaContext(DbContextOptions<HotelCargaContext> options)
            : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set default behavior for DB-generated values (including trigger-updated columns):
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.IsPrimaryKey() || property.ValueGenerated != Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never)
                    {
                        property.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAddOrUpdate;
                    }
                }
            }

        }

        public override int SaveChanges()
        {
            UpdateTriggerAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTriggerAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTriggerAuditFields()
        {

        }
    }
}
