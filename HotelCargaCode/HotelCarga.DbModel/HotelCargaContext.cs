using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HotelCarga.HotelCarga.DbModel.Entities;

namespace HotelCarga.DbModel
{
    public class HotelCargaContext : DbContext
    {
        public HotelCargaContext(DbContextOptions<HotelCargaContext> options)
            : base(options)
        {
        }

        public virtual DbSet<booking> bookings { get; set; }
        public virtual DbSet<booking_history> booking_histories { get; set; }
        public virtual DbSet<booking_status> booking_statuses { get; set; }
        public virtual DbSet<customer> customers { get; set; }
        public virtual DbSet<queue_status> queue_statuses { get; set; }
        public virtual DbSet<role> roles { get; set; }
        public virtual DbSet<room> rooms { get; set; }
        public virtual DbSet<room_availability> room_availabilities { get; set; }
        public virtual DbSet<room_category> room_categories { get; set; }
        public virtual DbSet<room_status> room_statuses { get; set; }
        public virtual DbSet<user> users { get; set; }
        public virtual DbSet<user_status> user_statuses { get; set; }
        public virtual DbSet<vw_customer_booking> vw_customer_bookings { get; set; }
        public virtual DbSet<vw_waiting_queue_report> vw_waiting_queue_reports { get; set; }
        public virtual DbSet<waiting_queue> waiting_queues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<booking>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PRIMARY");

                entity
                    .ToTable("booking", tb => tb.HasComment("Booking reservations with customer, room, and pricing details"))
                    .UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.check_in, "idx_check_in");
                entity.HasIndex(e => e.check_out, "idx_check_out");
                entity.HasIndex(e => e.customer_id, "idx_customer_id");
                entity.HasIndex(e => e.reserve_number, "idx_reserve_number").IsUnique();
                entity.HasIndex(e => e.room_id, "idx_room_id");
                entity.HasIndex(e => e.status_id, "idx_status_id");

                entity.Property(e => e.check_in).HasColumnType("datetime");
                entity.Property(e => e.check_out).HasColumnType("datetime");
                entity.Property(e => e.created_at)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
                entity.Property(e => e.nightly_rate).HasPrecision(10, 2);
                entity.Property(e => e.reserve_number)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.status_id).HasDefaultValueSql("'1'");
                entity.Property(e => e.total_price).HasPrecision(12, 2);
                entity.Property(e => e.updated_at)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");

                entity.HasOne(d => d.customer).WithMany(p => p.bookings)
                    .HasForeignKey(d => d.customer_id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_booking_customer");

                entity.HasOne(d => d.room).WithMany(p => p.bookings)
                    .HasForeignKey(d => d.room_id)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false)
                    .HasConstraintName("fk_booking_room");

                entity.HasOne(d => d.status).WithMany(p => p.bookings)
                    .HasForeignKey(d => d.status_id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_booking_status");
            });

            modelBuilder.Entity<booking_status>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PRIMARY");
                entity
                    .ToTable("booking_status", tb => tb.HasComment("Reference table for booking statuses"))
                    .UseCollation("utf8mb4_unicode_ci");
                entity.HasIndex(e => e.status_name, "status_name").IsUnique();
                entity.Property(e => e.id).ValueGeneratedOnAdd();
                entity.Property(e => e.created_at)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
                entity.Property(e => e.description).HasMaxLength(255);
                entity.Property(e => e.status_name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<role>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PRIMARY");
                entity
                    .ToTable("role", tb => tb.HasComment("Reference table for user roles"))
                    .UseCollation("utf8mb4_unicode_ci");
                entity.HasIndex(e => e.role_name, "role_name").IsUnique();
                entity.Property(e => e.id).ValueGeneratedOnAdd();
                entity.Property(e => e.created_at)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
                entity.Property(e => e.description).HasMaxLength(255);
                entity.Property(e => e.role_name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<user>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PRIMARY");
                entity
                    .ToTable("user", tb => tb.HasComment("User accounts with authentication and role assignment"))
                    .UseCollation("utf8mb4_unicode_ci");
                entity.HasIndex(e => e.email, "email").IsUnique();
                entity.HasIndex(e => e.role_id, "fk_user_role");
                entity.HasIndex(e => e.status_id, "idx_status_id");
                entity.HasIndex(e => e.username, "idx_username").IsUnique();
                entity.Property(e => e.created_at)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
                entity.Property(e => e.email)
                    .IsRequired()
                    .HasMaxLength(150);
                entity.Property(e => e.password_hash)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.Property(e => e.role_id).HasDefaultValueSql("'1'");
                entity.Property(e => e.status_id).HasDefaultValueSql("'1'");
                entity.Property(e => e.updated_at)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
                entity.Property(e => e.username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.role).WithMany(p => p.users)
                    .HasForeignKey(d => d.role_id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_user_role");

                entity.HasOne(d => d.status).WithMany(p => p.users)
                    .HasForeignKey(d => d.status_id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_user_status");
            });

            modelBuilder.Entity<customer>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PRIMARY");
                entity
                    .ToTable("customer", tb => tb.HasComment("Customer profiles with personal identification and contact information"))
                    .UseCollation("utf8mb4_unicode_ci");
                entity.HasIndex(e => e.document_number, "document_number").IsUnique();
                entity.HasIndex(e => e.last_name, "idx_last_name");
                entity.HasIndex(e => e.user_id, "idx_user_id").IsUnique();

                entity.Property(e => e.address).HasMaxLength(255);
                entity.Property(e => e.city).HasMaxLength(100);
                entity.Property(e => e.country).HasMaxLength(100);
                entity.Property(e => e.created_at)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
                entity.Property(e => e.document_number)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.first_name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.last_name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.phone).HasMaxLength(20);
                entity.Property(e => e.updated_at)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");

                entity.HasOne(d => d.user).WithOne(p => p.customer)
                    .HasForeignKey<customer>(d => d.user_id)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_customer_user");
            });

            // Configure View Entities (Keyless)
            modelBuilder.Entity<vw_customer_booking>()
                .HasNoKey()
                .ToView("vw_customer_bookings");

            modelBuilder.Entity<vw_waiting_queue_report>()
                .HasNoKey()
                .ToView("vw_waiting_queue_report");

            // Configure remaining entities
            modelBuilder.Entity<booking_history>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PRIMARY");

                entity
                    .ToTable("booking_history", tb => tb.HasComment("Audit trail for booking status changes"))
                    .UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.booking_id, "idx_booking_id");
                entity.HasIndex(e => e.customer_id, "idx_customer_id");
                entity.HasIndex(e => e.room_id, "fk_booking_history_room");
                entity.HasIndex(e => e.status_id, "fk_booking_history_status");

                entity.Property(e => e.action_type)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.check_in).HasColumnType("datetime");
                entity.Property(e => e.check_out).HasColumnType("datetime");
                entity.Property(e => e.created_at)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
                entity.Property(e => e.nightly_rate).HasPrecision(10, 2);
                entity.Property(e => e.total_price).HasPrecision(12, 2);

                entity.HasOne(d => d.booking).WithMany(p => p.booking_histories)
                    .HasForeignKey(d => d.booking_id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_booking_history_booking");

                entity.HasOne(d => d.customer).WithMany(p => p.booking_histories)
                    .HasForeignKey(d => d.customer_id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_booking_history_customer");

                entity.HasOne(d => d.room).WithMany(p => p.booking_histories)
                    .HasForeignKey(d => d.room_id)
                    .OnDelete(DeleteBehavior.SetNull)
                    .IsRequired(false)
                    .HasConstraintName("fk_booking_history_room");

                entity.HasOne(d => d.status).WithMany(p => p.booking_histories)
                    .HasForeignKey(d => d.status_id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_booking_history_status");
            });

            modelBuilder.Entity<booking_status>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PRIMARY");

                entity
                    .ToTable("booking_status", tb => tb.HasComment("Reference table for booking statuses"))
                    .UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.status_name, "status_name").IsUnique();

                entity.Property(e => e.id).ValueGeneratedOnAdd();
                entity.Property(e => e.created_at)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
                entity.Property(e => e.description).HasMaxLength(255);
                entity.Property(e => e.status_name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<queue_status>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PRIMARY");

                entity
                    .ToTable("queue_status", tb => tb.HasComment("Reference table for waiting queue statuses"))
                    .UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.status_name, "status_name").IsUnique();

                entity.Property(e => e.id).ValueGeneratedOnAdd();
                entity.Property(e => e.created_at)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
                entity.Property(e => e.description).HasMaxLength(255);
                entity.Property(e => e.status_name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<role>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PRIMARY");

                entity
                    .ToTable("role", tb => tb.HasComment("Reference table for user roles"))
                    .UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.role_name, "role_name").IsUnique();

                entity.Property(e => e.id).ValueGeneratedOnAdd();
                entity.Property(e => e.created_at)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
                entity.Property(e => e.description).HasMaxLength(255);
                entity.Property(e => e.role_name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<room>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PRIMARY");

                entity
                    .ToTable("room", tb => tb.HasComment("Physical rooms with pricing and status tracking"))
                    .UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.category_id, "fk_room_category");

                entity.HasIndex(e => e.room_number, "room_number").IsUnique();

                entity.HasIndex(e => e.status_id, "idx_status_id");

                entity.Property(e => e.created_at)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
                entity.Property(e => e.nightly_rate).HasPrecision(10, 2);
                entity.Property(e => e.status_id).HasDefaultValueSql("'1'");

                entity.HasOne(d => d.category).WithMany(p => p.rooms)
                    .HasForeignKey(d => d.category_id)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_room_category");

                entity.HasOne(d => d.status).WithMany(p => p.rooms)
                    .HasForeignKey(d => d.status_id)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_room_status");
            });

            modelBuilder.Entity<room_availability>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PRIMARY");

                entity
                    .ToTable("room_availability", tb => tb.HasComment("Room availability calendar for date-specific bookings"))
                    .UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => new { e.room_id, e.start_schedule, e.end_schedule }, "uk_room_availability").IsUnique();
                entity.HasIndex(e => e.start_schedule, "idx_start_schedule");
                entity.HasIndex(e => e.end_schedule, "idx_end_schedule");

                entity.Property(e => e.creation_at)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
                entity.Property(e => e.end_schedule).HasColumnType("datetime");
                entity.Property(e => e.start_schedule).HasColumnType("datetime");

                entity.HasOne(d => d.room).WithMany(p => p.room_availabilities)
                    .HasForeignKey(d => d.room_id)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_room_availability_room");
            });

            modelBuilder.Entity<room_category>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PRIMARY");

                entity
                    .ToTable("room_category", tb => tb.HasComment("Reference table for room categories and types"))
                    .UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.category_name, "category_name").IsUnique();

                entity.Property(e => e.id).ValueGeneratedOnAdd();
                entity.Property(e => e.amenities).HasMaxLength(500);
                entity.Property(e => e.category_name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.created_at)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
                entity.Property(e => e.description).HasColumnType("text");
            });

            modelBuilder.Entity<room_status>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PRIMARY");

                entity
                    .ToTable("room_status", tb => tb.HasComment("Reference table for room availability states"))
                    .UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.status_name, "status_name").IsUnique();

                entity.Property(e => e.id).ValueGeneratedOnAdd();
                entity.Property(e => e.created_at)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
                entity.Property(e => e.description).HasMaxLength(255);
                entity.Property(e => e.status_name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<user>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PRIMARY");

                entity
                    .ToTable("user", tb => tb.HasComment("User accounts with authentication and role assignment"))
                    .UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.email, "email").IsUnique();

                entity.HasIndex(e => e.role_id, "fk_user_role");

                entity.HasIndex(e => e.status_id, "idx_status_id");

                entity.HasIndex(e => e.username, "idx_username").IsUnique();

                entity.Property(e => e.created_at)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
                entity.Property(e => e.email)
                    .IsRequired()
                    .HasMaxLength(150);
                entity.Property(e => e.password_hash)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.Property(e => e.role_id).HasDefaultValueSql("'1'");
                entity.Property(e => e.status_id).HasDefaultValueSql("'1'");
                entity.Property(e => e.updated_at)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
                entity.Property(e => e.username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.role).WithMany(p => p.users)
                    .HasForeignKey(d => d.role_id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_user_role");

                entity.HasOne(d => d.status).WithMany(p => p.users)
                    .HasForeignKey(d => d.status_id)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_user_status");
            });

            modelBuilder.Entity<user_status>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PRIMARY");

                entity
                    .ToTable("user_status", tb => tb.HasComment("Reference table for user account statuses"))
                    .UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.status_name, "status_name").IsUnique();

                entity.Property(e => e.id).ValueGeneratedOnAdd();
                entity.Property(e => e.created_at)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
                entity.Property(e => e.description).HasMaxLength(255);
                entity.Property(e => e.status_name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<waiting_queue>(entity =>
            {
                entity.HasKey(e => e.id).HasName("PRIMARY");

                entity
                    .ToTable("waiting_queue", tb => tb.HasComment("Waiting list for room requests when preferred rooms are unavailable"))
                    .UseCollation("utf8mb4_unicode_ci");

                entity.HasIndex(e => e.customer_id, "idx_customer_id");
                entity.HasIndex(e => e.room_category_id, "fk_waiting_queue_category");
                entity.HasIndex(e => e.status_id, "fk_waiting_queue_status");
                entity.HasIndex(e => e.requested_check_in, "idx_requested_check_in");

                entity.Property(e => e.check_out).HasColumnType("datetime");
                entity.Property(e => e.created_at)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");
                entity.Property(e => e.requested_check_in).HasColumnType("datetime");
                entity.Property(e => e.status_id).HasDefaultValueSql("'1'");
                entity.Property(e => e.updated_at)
                    .ValueGeneratedOnAddOrUpdate()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP")
                    .HasColumnType("timestamp");

                entity.HasOne(d => d.customer).WithMany(p => p.waiting_queues)
                    .HasForeignKey(d => d.customer_id)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_waiting_queue_customer");

                entity.HasOne(d => d.room_category).WithMany(p => p.waiting_queues)
                    .HasForeignKey(d => d.room_category_id)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_waiting_queue_category");

                entity.HasOne(d => d.status).WithMany(p => p.waiting_queues)
                    .HasForeignKey(d => d.status_id)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("fk_waiting_queue_status");
            });
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
