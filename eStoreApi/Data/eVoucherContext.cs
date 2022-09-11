using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using eStoreApi.Model;

namespace eStoreApi.Data
{
    public partial class eVoucherContext : DbContext
    {
        public eVoucherContext()
        {
        }

        public eVoucherContext(DbContextOptions<eVoucherContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TblEvoucher> TblEvouchers { get; set; } = null!;
        public virtual DbSet<TblPurchasehistory> TblPurchasehistories { get; set; } = null!;
        public virtual DbSet<TblTokenuser> TblTokenusers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<TblEvoucher>(entity =>
            {
                entity.ToTable("tbl_evoucher");

                entity.Property(e => e.Id).HasMaxLength(50);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.ExpiryDate).HasColumnType("datetime");

                entity.Property(e => e.MidifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.PaymentType).HasMaxLength(50);

                entity.Property(e => e.Phone).HasMaxLength(50);

                entity.Property(e => e.Qrimage).HasColumnName("QRImage");

                entity.Property(e => e.Title).HasMaxLength(50);

                entity.Property(e => e.VoucherCodes).HasMaxLength(50);
            });

            modelBuilder.Entity<TblPurchasehistory>(entity =>
            {
                entity.HasKey(e => e.PurchaseId)
                    .HasName("PRIMARY");

                entity.ToTable("tbl_purchasehistory");

                entity.Property(e => e.PurchaseId)
                    .HasMaxLength(50)
                    .HasColumnName("PurchaseID");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.EvoucherId)
                    .HasMaxLength(50)
                    .HasColumnName("EVoucherID");

                entity.Property(e => e.Phone).HasMaxLength(20);

                entity.Property(e => e.PromoCodes).HasMaxLength(11);

                entity.Property(e => e.Qrimage).HasColumnName("QRImage");

                entity.Property(e => e.UserName).HasMaxLength(50);
            });

            modelBuilder.Entity<TblTokenuser>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("tbl_tokenuser");

                entity.Property(e => e.Password).HasMaxLength(30);

                entity.Property(e => e.User).HasMaxLength(30);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
