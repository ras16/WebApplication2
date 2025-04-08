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

        //Production Data ebtries
        public DbSet<FormData> FormDataEntries { get; set; }

        //WellTestSummaryData Entries
        public DbSet<WellData> WellDataEntries { get;  set; }

        //Wells Entries
        public DbSet<Wells> WellsEntries { get; set; }

        //CompressorInfo Entries
        public DbSet<CompressorInfo> CompressorInfoEntries { get; set; }

        //HP Gas Entries
        public DbSet<HP_Gas> HP_GasEntries { get; set; }

        //Compressors Entries
        public DbSet<Compressors> CompressorsEntries { get; set; }

        //ProdAllocation Main Entries
        public DbSet<ProdAllocationMain> ProdAllocationMain { get; set; }

        //AcceptedValues Entries
        public DbSet<AcceptedValues> AcceptedValuesEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Database Conection

            //Production Table
            modelBuilder.Entity<FormData>().ToTable("Production");

            //Wells Table 
            modelBuilder.Entity<Wells>().ToTable("Wells");
            modelBuilder.Entity<Wells>().HasKey(w => w.WellID);
            
            //WellTestSummary Table
            modelBuilder.Entity<WellData>().ToTable("WellTestSummary");
            modelBuilder.Entity<WellData>().HasKey(w => w.id);
            base.OnModelCreating(modelBuilder);

            //CompressorInfo Table
            modelBuilder.Entity<CompressorInfo>().ToTable("CompressorInfo");
            modelBuilder.Entity<CompressorInfo>().HasKey(w => w.ID);

            //HP Gas Table 
            modelBuilder.Entity<HP_Gas>().ToTable("HPGasTBL");

            //Compressors Table 
            modelBuilder.Entity<Compressors>().ToTable("Compressors");

            //ProdAllocation Main
            modelBuilder.Entity<ProdAllocationMain>().ToTable("ProdAllocationMain");
            modelBuilder.Entity<ProdAllocationMain>().HasKey(w => w.PAID);

            //AcceptedParameters (AcceptedValues in our code)
            modelBuilder.Entity<AcceptedValues>().ToTable("AcceptedParameters");
            modelBuilder.Entity<AcceptedValues>().HasKey(w => w.id);
        }
    }
}