using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace HotelCarga.Models;

public partial class HotelcargaContext : DbContext
{
    public HotelcargaContext()
    {
    }

    public HotelcargaContext(DbContextOptions<HotelcargaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingHistory> BookingHistories { get; set; }

    public virtual DbSet<BookingStatus> BookingStatuses { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<QueueStatus> QueueStatuses { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomAvailability> RoomAvailabilities { get; set; }

    public virtual DbSet<RoomCategory> RoomCategories { get; set; }

    public virtual DbSet<RoomStatus> RoomStatuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserStatus> UserStatuses { get; set; }

    public virtual DbSet<VwCustomerBooking> VwCustomerBookings { get; set; }

    public virtual DbSet<VwWaitingQueueReport> VwWaitingQueueReports { get; set; }

    public virtual DbSet<WaitingQueue> WaitingQueues { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("booking", tb => tb.HasComment("Booking reservations with customer, room, and pricing details"))
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.CheckIn, "idx_check_in");

            entity.HasIndex(e => e.CheckOut, "idx_check_out");

            entity.HasIndex(e => e.CustomerId, "idx_customer_id");

            entity.HasIndex(e => e.ReserveNumber, "idx_reserve_number").IsUnique();

            entity.HasIndex(e => e.RoomId, "idx_room_id");

            entity.HasIndex(e => e.StatusId, "idx_status_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CheckIn)
                .HasColumnType("datetime")
                .HasColumnName("check_in");
            entity.Property(e => e.CheckOut)
                .HasColumnType("datetime")
                .HasColumnName("check_out");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.NightlyRate)
                .HasPrecision(10, 2)
                .HasColumnName("nightly_rate");
            entity.Property(e => e.ReserveNumber)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("reserve_number");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("'1'")
                .HasColumnName("status_id");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(12, 2)
                .HasColumnName("total_price");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Customer).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_booking_customer");

            entity.HasOne(d => d.Room).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_booking_room");

            entity.HasOne(d => d.Status).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_booking_status");
        });

        modelBuilder.Entity<BookingHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("booking_history", tb => tb.HasComment("Audit trail tracking all booking changes and historical events"))
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.RoomId, "fk_booking_history_room");

            entity.HasIndex(e => e.StatusId, "fk_booking_history_status");

            entity.HasIndex(e => e.BookingId, "idx_booking_id");

            entity.HasIndex(e => e.CreatedAt, "idx_created_at");

            entity.HasIndex(e => e.CustomerId, "idx_customer_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActionType)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("action_type");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CheckIn)
                .HasColumnType("datetime")
                .HasColumnName("check_in");
            entity.Property(e => e.CheckOut)
                .HasColumnType("datetime")
                .HasColumnName("check_out");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.NightlyRate)
                .HasPrecision(10, 2)
                .HasColumnName("nightly_rate");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(12, 2)
                .HasColumnName("total_price");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingHistories)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_booking_history_booking");

            entity.HasOne(d => d.Customer).WithMany(p => p.BookingHistories)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_booking_history_customer");

            entity.HasOne(d => d.Room).WithMany(p => p.BookingHistories)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_booking_history_room");

            entity.HasOne(d => d.Status).WithMany(p => p.BookingHistories)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_booking_history_status");
        });

        modelBuilder.Entity<BookingStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("booking_status", tb => tb.HasComment("Reference table for booking statuses"))
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.StatusName, "status_name").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.StatusName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("customer", tb => tb.HasComment("Customer profiles with personal identification and contact information"))
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.DocumentNumber, "document_number").IsUnique();

            entity.HasIndex(e => e.LastName, "idx_last_name");

            entity.HasIndex(e => e.UserId, "idx_user_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .HasColumnName("country");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.DocumentNumber)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("document_number");
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.Customer)
                .HasForeignKey<Customer>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_customer_user");
        });

        modelBuilder.Entity<QueueStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("queue_status", tb => tb.HasComment("Reference table for waiting queue entry statuses"))
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.StatusName, "status_name").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.StatusName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("role", tb => tb.HasComment("Reference table for user roles"))
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.RoleName, "role_name").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.RoleName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("room", tb => tb.HasComment("Physical rooms with pricing, category, and status information"))
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.CategoryId, "idx_category_id");

            entity.HasIndex(e => e.RoomNumber, "idx_room_number").IsUnique();

            entity.HasIndex(e => e.StatusId, "idx_status_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.FloorNumber).HasColumnName("floor_number");
            entity.Property(e => e.NightlyRate)
                .HasPrecision(10, 2)
                .HasColumnName("nightly_rate");
            entity.Property(e => e.RoomNumber).HasColumnName("room_number");
            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("'1'")
                .HasColumnName("status_id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Category).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_room_category");

            entity.HasOne(d => d.Status).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_room_status");
        });

        modelBuilder.Entity<RoomAvailability>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("room_availability", tb => tb.HasComment("Room availability schedule with date range management"))
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.EndSchedule, "idx_end_schedule");

            entity.HasIndex(e => e.StartSchedule, "idx_start_schedule");

            entity.HasIndex(e => new { e.RoomId, e.StartSchedule, e.EndSchedule }, "uk_room_availability").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreationAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("creation_at");
            entity.Property(e => e.EndSchedule)
                .HasColumnType("datetime")
                .HasColumnName("end_schedule");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.StartSchedule)
                .HasColumnType("datetime")
                .HasColumnName("start_schedule");

            entity.HasOne(d => d.Room).WithMany(p => p.RoomAvailabilities)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_room_availability_room");
        });

        modelBuilder.Entity<RoomCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("room_category", tb => tb.HasComment("Reference table for room categories and types"))
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.CategoryName, "category_name").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Amenities)
                .HasMaxLength(500)
                .HasColumnName("amenities");
            entity.Property(e => e.CategoryName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("category_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
        });

        modelBuilder.Entity<RoomStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("room_status", tb => tb.HasComment("Reference table for room availability states"))
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.StatusName, "status_name").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.StatusName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("user", tb => tb.HasComment("User accounts with authentication and role assignment"))
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Email, "email").IsUnique();

            entity.HasIndex(e => e.RoleId, "fk_user_role");

            entity.HasIndex(e => e.StatusId, "idx_status_id");

            entity.HasIndex(e => e.Username, "idx_username").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.RoleId)
                .HasDefaultValueSql("'1'")
                .HasColumnName("role_id");
            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("'1'")
                .HasColumnName("status_id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_role");

            entity.HasOne(d => d.Status).WithMany(p => p.Users)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_status");
        });

        modelBuilder.Entity<UserStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("user_status", tb => tb.HasComment("Reference table for user account statuses"))
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.StatusName, "status_name").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.StatusName)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<VwCustomerBooking>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_customer_bookings");

            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.BookingCreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("booking_created_at");
            entity.Property(e => e.BookingId)
                .HasDefaultValueSql("'0'")
                .HasColumnName("booking_id");
            entity.Property(e => e.BookingStatus)
                .HasMaxLength(50)
                .HasColumnName("booking_status")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.BookingUpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("booking_updated_at");
            entity.Property(e => e.CheckIn)
                .HasColumnType("datetime")
                .HasColumnName("check_in");
            entity.Property(e => e.CheckOut)
                .HasColumnType("datetime")
                .HasColumnName("check_out");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .HasColumnName("country")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DocumentNumber)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("document_number")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnName("email")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("first_name")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.FloorNumber).HasColumnName("floor_number");
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("last_name")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.NightlyRate)
                .HasPrecision(10, 2)
                .HasColumnName("nightly_rate");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.ReserveNumber)
                .HasMaxLength(50)
                .HasColumnName("reserve_number")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.RoomCategory)
                .HasMaxLength(50)
                .HasColumnName("room_category")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.RoomNumber).HasColumnName("room_number");
            entity.Property(e => e.RoomStatus)
                .HasMaxLength(50)
                .HasColumnName("room_status")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(12, 2)
                .HasColumnName("total_price");
        });

        modelBuilder.Entity<VwWaitingQueueReport>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_waiting_queue_report");

            entity.Property(e => e.CheckOut)
                .HasColumnType("datetime")
                .HasColumnName("check_out");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentStatus)
                .IsRequired()
                .HasMaxLength(9)
                .HasDefaultValueSql("''")
                .HasColumnName("current_status")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DocumentNumber)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("document_number")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("first_name")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("last_name")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.QueueId).HasColumnName("queue_id");
            entity.Property(e => e.QueueStatus)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("queue_status")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.RequestedCheckIn)
                .HasColumnType("datetime")
                .HasColumnName("requested_check_in");
            entity.Property(e => e.RequestedRoomCategory)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("requested_room_category")
                .UseCollation("utf8mb4_unicode_ci");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<WaitingQueue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("waiting_queue", tb => tb.HasComment("Waiting queue for customers seeking room availability"))
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.RoomCategoryId, "fk_waiting_queue_category");

            entity.HasIndex(e => e.CustomerId, "idx_customer_id");

            entity.HasIndex(e => e.RequestedCheckIn, "idx_requested_check_in");

            entity.HasIndex(e => e.StatusId, "idx_status_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CheckOut)
                .HasColumnType("datetime")
                .HasColumnName("check_out");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.RequestedCheckIn)
                .HasColumnType("datetime")
                .HasColumnName("requested_check_in");
            entity.Property(e => e.RoomCategoryId).HasColumnName("room_category_id");
            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("'1'")
                .HasColumnName("status_id");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Customer).WithMany(p => p.WaitingQueues)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_waiting_queue_customer");

            entity.HasOne(d => d.RoomCategory).WithMany(p => p.WaitingQueues)
                .HasForeignKey(d => d.RoomCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_waiting_queue_category");

            entity.HasOne(d => d.Status).WithMany(p => p.WaitingQueues)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_waiting_queue_status");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
