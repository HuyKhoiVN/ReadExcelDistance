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

        public virtual DbSet<Contract> Contracts { get; set; } = null!;
        public virtual DbSet<Device> Devices { get; set; } = null!;
        public virtual DbSet<DeviceMaintenanceSchedule> DeviceMaintenanceSchedules { get; set; } = null!;
        public virtual DbSet<DeviceTravelTime> DeviceTravelTimes { get; set; } = null!;
        public virtual DbSet<Officer> Officers { get; set; } = null!;
        public virtual DbSet<Province> Provinces { get; set; } = null!;
        public virtual DbSet<ProvinceTravelTime> ProvinceTravelTimes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contract>(entity =>
            {
                entity.ToTable("Contract");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ContractNumberChildren)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContractNumberParent)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.EndDate)
                    .HasColumnType("date")
                    .HasColumnName("end_date");

                entity.Property(e => e.StartDate)
                    .HasColumnType("date")
                    .HasColumnName("start_date");
            });

            modelBuilder.Entity<Device>(entity =>
            {
                entity.ToTable("Device");

                entity.Property(e => e.Address).HasMaxLength(200);

                entity.Property(e => e.Area).HasMaxLength(100);

                entity.Property(e => e.Class).HasMaxLength(100);

                entity.Property(e => e.Contact)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.ContractNumber).HasMaxLength(100);

                entity.Property(e => e.CreatedBy).HasMaxLength(100);

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DeviceIdNumber)
                    .HasMaxLength(8)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.DeviceStatus).HasMaxLength(50);

                entity.Property(e => e.Family)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.LastChange).HasColumnType("datetime");

                entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");

                entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");

                entity.Property(e => e.Name)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Province).HasMaxLength(100);

                entity.Property(e => e.SerialNumber).HasMaxLength(50);

                entity.Property(e => e.Support1).HasMaxLength(255);

                entity.Property(e => e.Support2).HasMaxLength(255);

                entity.Property(e => e.UpdatedBy).HasMaxLength(100);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.Zone)
                    .HasMaxLength(5)
                    .IsUnicode(false)
                    .IsFixedLength();
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

                entity.HasIndex(e => e.ProvinceName, "IX_Province_ProvinceName");

                entity.Property(e => e.CreatedBy).HasMaxLength(100);

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Fax).HasMaxLength(20);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");

                entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");

                entity.Property(e => e.Phone).HasMaxLength(20);

                entity.Property(e => e.ProvinceName).HasMaxLength(100);

                entity.Property(e => e.UpdatedBy).HasMaxLength(100);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<ProvinceTravelTime>(entity =>
            {
                entity.ToTable("ProvinceTravelTime");

                entity.HasIndex(e => new { e.ProvinceId, e.DeviceId }, "IX_ProvinceTravelTime_ProvinceDevice");

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

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
