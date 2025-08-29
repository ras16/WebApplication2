using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using System.Collections.Generic;
using WebApplication2.Pages.Shared.Models;

namespace Production.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        //Data Entries
        //Production Data entries
        public DbSet<DailyFieldProduction> DailyFieldProductionEntries { get; set; }
        //WellTestSummaryData Entries
        public DbSet<WellData> WellDataEntries { get; set; }
        //Wells Entries
        public DbSet<Wells> WellsEntries { get; set; }
        //CompressorInfo Entries
        public DbSet<CompressorInfo> CompressorInfoEntries { get; set; }
        //HP Gas Entries
        public DbSet<HPGasTBL> HPGasTBLEntries { get; set; }
        //Compressors Entries
        public DbSet<Compressors> CompressorsEntries { get; set; }
        //ProdAllocation Main Entries
        public DbSet<ProdAllocationMain> ProdAllocationMain { get; set; }
        //AcceptedValues Entries
        public DbSet<AcceptedValues> AcceptedValuesEntries { get; set; }
        //Accepted Parameters Entries
        public DbSet<AcceptedParameters> AcceptedParametersEntries { get; set; }
        //Calculations1 Entries
        public DbSet<Calculations1> Calculations1 { get; set; }
        //DailyWellData Entries
        public DbSet<DailyWellData> DailyWellData { get; set; }
        //ProdAreasTBL Entries 
        public DbSet<Prod_Areas_TBL> Prod_Areas_TBL { get; set; }
        //FlayerAloc Entries 
        public DbSet<FlayerAloc> FlayerAloc { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Database Connection
            //Production Table
            modelBuilder.Entity<DailyFieldProduction>().ToTable("DailyFieldProduction");

            //Wells Table 
            modelBuilder.Entity<Wells>().ToTable("Wells");
            modelBuilder.Entity<Wells>().HasKey(w => w.WellID);

            //WellTestSummary Table
            modelBuilder.Entity<WellData>().ToTable("WellTestSummary");
            modelBuilder.Entity<WellData>().HasKey(w => w.id);

            //CompressorInfo Table
            modelBuilder.Entity<CompressorInfo>().ToTable("CompressorInfo");
            modelBuilder.Entity<CompressorInfo>().HasKey(w => w.ID);

            //HP Gas Table 
            modelBuilder.Entity<HPGasTBL>().ToTable("HPGasTBL");

            //Compressors Table 
            modelBuilder.Entity<Compressors>().ToTable("Compressors");

            //ProdAllocation Main
            modelBuilder.Entity<ProdAllocationMain>().ToTable("ProdAllocationMain");
            modelBuilder.Entity<ProdAllocationMain>().HasKey(w => w.PAID);

            //AcceptedValues
            //modelBuilder.Entity<AcceptedValues>().ToTable("AcceptedValues");
            //modelBuilder.Entity<AcceptedValues>().HasKey(w => w.id);

            //AcceptedParameters
            modelBuilder.Entity<AcceptedParameters>().ToTable("AcceptedParameters");
            modelBuilder.Entity<AcceptedParameters>().HasKey(w => w.ID);

            //Calculations1 Table
            modelBuilder.Entity<Calculations1>().ToTable("Calculations1");
            modelBuilder.Entity<Calculations1>().HasKey(c => c.Id);

            //DailyWellData Table
            modelBuilder.Entity<DailyWellData>().ToTable("DailyWellData");
            modelBuilder.Entity<DailyWellData>().HasKey(c => c.ID);

            //ProdArea Table 
            modelBuilder.Entity<Prod_Areas_TBL>().ToTable("Prod_Areas_Tbl");
            modelBuilder.Entity<Prod_Areas_TBL>().HasKey(c => c.prod_area_id);

            //FlayerAloc Table 
            modelBuilder.Entity<FlayerAloc>().ToTable("FlayerAloc");
            modelBuilder.Entity<FlayerAloc>().HasKey(c => c.id);

            //
            modelBuilder.Entity<AcceptedParameters>()
                .HasOne(ap => ap.Wells)
                .WithMany(w => w.AcceptedParameters)
                .HasForeignKey(ap => ap.WellID);

            //
            modelBuilder.Entity<ProdAllocationMain>()
            .HasOne(pam => pam.AcceptedParameters)
            .WithOne(ap => ap.ProdAllocationMain)
            .HasForeignKey<ProdAllocationMain>(pam => pam.PAID);

            base.OnModelCreating(modelBuilder);
        }
    }
}