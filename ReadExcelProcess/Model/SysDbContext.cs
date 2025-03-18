using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ReadExcelProcess.Model
{
    public partial class SysDbContext : DbContext
    {

        public SysDbContext(DbContextOptions<SysDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Device> Devices { get; set; } = null!;
        public virtual DbSet<DeviceMaintenanceSchedule> DeviceMaintenanceSchedules { get; set; } = null!;
        public virtual DbSet<DeviceTravelTime> DeviceTravelTimes { get; set; } = null!;
        public virtual DbSet<Officer> Officers { get; set; } = null!;
        public virtual DbSet<Province> Provinces { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>(entity =>
            {
                entity.ToTable("Device");

                entity.Property(e => e.Address).HasMaxLength(200);

                entity.Property(e => e.Area).HasMaxLength(100);

                entity.Property(e => e.ContractNumber).HasMaxLength(100);

                entity.Property(e => e.CreatedBy).HasMaxLength(100);

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Customer).HasMaxLength(100);

                entity.Property(e => e.DeviceStatus).HasMaxLength(50);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");

                entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");

                entity.Property(e => e.MaintenanceCycle).HasMaxLength(50);

                entity.Property(e => e.MaintenanceEndDate).HasColumnType("datetime");

                entity.Property(e => e.MaintenanceStartDate).HasColumnType("datetime");

                entity.Property(e => e.ManagementBranch).HasMaxLength(100);

                entity.Property(e => e.Manufacturer).HasMaxLength(100);

                entity.Property(e => e.Model).HasMaxLength(100);

                entity.Property(e => e.Province).HasMaxLength(100);

                entity.Property(e => e.SerialNumber).HasMaxLength(50);

                entity.Property(e => e.SubContractNumber).HasMaxLength(100);

                entity.Property(e => e.TimeMaintenance).HasDefaultValueSql("((2))");

                entity.Property(e => e.Type).HasMaxLength(50);

                entity.Property(e => e.UpdatedBy).HasMaxLength(100);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<DeviceMaintenanceSchedule>(entity =>
            {
                entity.ToTable("DeviceMaintenanceSchedule");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedBy).HasMaxLength(100);

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DeviceId).HasColumnName("device_id");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.MaintenanceEndDate)
                    .HasColumnType("datetime")
                    .HasColumnName("maintenance_end_date");

                entity.Property(e => e.MaintenanceStartDate)
                    .HasColumnType("datetime")
                    .HasColumnName("maintenance_start_date");

                entity.Property(e => e.UpdatedBy).HasMaxLength(100);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<DeviceTravelTime>(entity =>
            {
                entity.ToTable("DeviceTravelTime");

                entity.Property(e => e.CreatedBy).HasMaxLength(100);

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.TravelTime).HasColumnType("decimal(10, 4)");

                entity.Property(e => e.UpdatedBy).HasMaxLength(100);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Officer>(entity =>
            {
                entity.ToTable("Officer");

                entity.Property(e => e.Account).HasMaxLength(50);

                entity.Property(e => e.Branch).HasMaxLength(100);

                entity.Property(e => e.Cccd)
                    .HasMaxLength(50)
                    .HasColumnName("CCCD");

                entity.Property(e => e.DateOfIssue).HasColumnType("date");

                entity.Property(e => e.FullName).HasMaxLength(100);

                entity.Property(e => e.PlaceOfIssue).HasMaxLength(100);

                entity.Property(e => e.Region).HasMaxLength(100);

                entity.Property(e => e.Title).HasMaxLength(100);
            });

            modelBuilder.Entity<Province>(entity =>
            {
                entity.ToTable("Province");

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.Fax).HasMaxLength(20);

                entity.Property(e => e.Phone).HasMaxLength(20);

                entity.Property(e => e.ProvinceName).HasMaxLength(100);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
