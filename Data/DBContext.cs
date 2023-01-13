using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Models;
using System.Reflection.Metadata;
using System.Diagnostics;

namespace MarketAnalytics.Data
{
    public class DBContext : DbContext
    {
        public DBContext (DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        public DbSet<MarketAnalytics.Models.StockMaster> StockMaster { get; set; } = default!;
        public DbSet<MarketAnalytics.Models.UpdateTracker> UpdateTracker { get; set; } = default;
        public DbSet<MarketAnalytics.Models.StockPriceHistory> StockPriceHistory { get; set; } = default!;
        public DbSet<MarketAnalytics.Models.Portfolio_Master> PORTFOLIO_MASTER { get; set; } = default!;
        //public DbSet<MarketAnalytics.Models.PORTFOLIO> PORTFOLIO { get; set; } = default!;
        public DbSet<MarketAnalytics.Models.PORTFOLIOTXN> PORTFOLIOTXN { get; set; } = default!;
        public DbSet<MarketAnalytics.Models.V20_CANDLE_STRATEGY> V20_CANDLE_STRATEGY { get; set; } = default!;
        public DbSet<MarketAnalytics.Models.BULLISH_ENGULFING_STRATEGY> BULLISH_ENGULFING_STRATEGY { get; set; } = default!;
        public DbSet<MarketAnalytics.Models.BEARISH_ENGULFING> BEARISH_ENGULFING { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UpdateTracker>().ToTable(nameof(UpdateTracker));
            modelBuilder.Entity<StockMaster>().ToTable("StockMaster");
            //modelBuilder.Entity<StockPriceHistory>().ToTable("StockPriceHistory");
            modelBuilder.Entity<StockPriceHistory>().ToTable("StockPriceHistory").Navigation(e => e.StockMaster).AutoInclude();

            modelBuilder.Entity<Portfolio_Master>().ToTable("PORTFOLIO_MASTER");

            modelBuilder.Entity<PORTFOLIOTXN>()
                 .HasOne(d => d.portfolioMaster)
                 .WithMany(dm => dm.collectionTxn)
                 .HasForeignKey(dkey => dkey.PORTFOLIO_MASTER_ID);

            modelBuilder.Entity<PORTFOLIOTXN>()
                 .HasOne(d => d.stockMaster)
                 .WithMany(dm => dm.collectionTxn)
                 .HasForeignKey(dkey => dkey.StockMasterID);

            modelBuilder.Entity<PORTFOLIOTXN>().ToTable("PORTFOLIOTXN").Navigation(e => e.stockMaster).AutoInclude();
            modelBuilder.Entity<PORTFOLIOTXN>().ToTable("PORTFOLIOTXN").Navigation(e => e.portfolioMaster).AutoInclude();


            //Autoinclude will automatically include the StockMaster object
            //modelBuilder.Entity<PORTFOLIO>().ToTable("PORTFOLIO").Navigation(e => e.StockMaster).AutoInclude();

            modelBuilder.Entity<V20_CANDLE_STRATEGY>().ToTable("V20_CANDLE_STRATEGY").Navigation(e => e.StockMaster).AutoInclude();

            modelBuilder.Entity<BULLISH_ENGULFING_STRATEGY>().ToTable("BULLISH_ENGULFING_STRATEGY").Navigation(e => e.StockMaster).AutoInclude();

            modelBuilder.Entity<BEARISH_ENGULFING>().ToTable("BEARISH_ENGULFING").Navigation(e => e.StockMaster).AutoInclude();
        }
    }
}
