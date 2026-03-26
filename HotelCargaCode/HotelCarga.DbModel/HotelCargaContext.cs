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

        // TODO: Scaffold the real entities after connecting to your MySQL DB:
        // dotnet ef dbcontext scaffold "name=myConnectionString" Pomelo.EntityFrameworkCore.MySql --context HotelCargaContext --output-dir Entities --use-database-names --force
        
        // Example:
        // public virtual DbSet<Hotel> Hotels { get; set; }

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

            // Customize each property/trigger as needed, e.g.:
            // modelBuilder.Entity<YourEntity>()
            //     .Property(e => e.UpdatedAt)
            //     .ValueGeneratedOnAddOrUpdate()
            //     .HasDefaultValueSql("CURRENT_TIMESTAMP")
            //     .Metadata.SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
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
            // optional hook for pre-save values for triggers, if needed.
        }
    }
}
