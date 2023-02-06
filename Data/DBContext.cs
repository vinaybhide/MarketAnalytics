using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Models;
using System.Reflection.Metadata;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MarketAnalytics.Data
{
    public class DBContext : IdentityDbContext<IdentityUser, IdentityRole, string> //DbContext
    {
        public DBContext (DbContextOptions<DBContext> options)
            : base(options)
        {
        }
        public DbSet<MarketAnalytics.Models.UserMaster> UserMasters { get; set; } = default!;

        public DbSet<MarketAnalytics.Models.StockMaster> StockMaster { get; set; } = default!;
        public DbSet<MarketAnalytics.Models.UpdateTracker> UpdateTracker { get; set; } = default;
        public DbSet<MarketAnalytics.Models.StockPriceHistory> StockPriceHistory { get; set; } = default!;
        public DbSet<MarketAnalytics.Models.Portfolio_Master> PORTFOLIO_MASTER { get; set; } = default!;
        public DbSet<MarketAnalytics.Models.PORTFOLIOTXN> PORTFOLIOTXN { get; set; } = default!;
        public DbSet<MarketAnalytics.Models.V20_CANDLE_STRATEGY> V20_CANDLE_STRATEGY { get; set; } = default!;
        public DbSet<MarketAnalytics.Models.BULLISH_ENGULFING_STRATEGY> BULLISH_ENGULFING_STRATEGY { get; set; } = default!;
        public DbSet<MarketAnalytics.Models.BEARISH_ENGULFING> BEARISH_ENGULFING { get; set; } = default!;

        //ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
        //public virtual Microsoft.EntityFrameworkCore.DbContextOptionsBuilder ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserMaster>().ToTable("USER_MASTER");
            modelBuilder.Entity<UpdateTracker>().ToTable(nameof(UpdateTracker));

            modelBuilder.Entity<StockPriceHistory>()
                .HasOne<StockMaster>(s => s.StockMaster)
                .WithMany(g => g.collectionStockPriceHistory)
                .HasForeignKey(s => s.StockMasterID);

            modelBuilder.Entity<BULLISH_ENGULFING_STRATEGY>()
                .HasOne<StockMaster>(s => s.StockMaster)
                .WithMany(g => g.collectionBullishEngulfing)
                .HasForeignKey(s => s.StockMasterID);

            modelBuilder.Entity<BEARISH_ENGULFING>()
                .HasOne<StockMaster>(s => s.StockMaster)
                .WithMany(g => g.collectionBearishEngulfing)
                .HasForeignKey(s => s.StockMasterID);

            modelBuilder.Entity<V20_CANDLE_STRATEGY>()
                .HasOne<StockMaster>(s => s.StockMaster)
                .WithMany(g => g.collection_V20_buysell)
                .HasForeignKey(s => s.StockMasterID);


            modelBuilder.Entity<StockMaster>().ToTable("StockMaster").Navigation(e => e.collectionStockPriceHistory).AutoInclude();
            modelBuilder.Entity<StockMaster>().ToTable("StockMaster").Navigation(e => e.collectionBullishEngulfing).AutoInclude();
            modelBuilder.Entity<StockMaster>().ToTable("StockMaster").Navigation(e => e.collectionBearishEngulfing).AutoInclude();
            modelBuilder.Entity<StockMaster>().ToTable("StockMaster").Navigation(e => e.collection_V20_buysell).AutoInclude();
            
            //modelBuilder.Entity<StockMaster>().ToTable("StockMaster").Navigation(e => e.collectionTxn).AutoInclude();

            //modelBuilder.Entity<StockPriceHistory>().ToTable("StockPriceHistory").Navigation(e => e.StockMaster).AutoInclude();


            ////for many to many relationship you can use following code or the below commented single statement
            modelBuilder.Entity<PORTFOLIOTXN>()
                 .HasOne(d => d.portfolioMaster)
                 .WithMany(dm => dm.collectionTxn)
                 .HasForeignKey(dkey => dkey.PORTFOLIO_MASTER_ID);

            ////for many to many relationship you can use following or the below single commented code 
            modelBuilder.Entity<PORTFOLIOTXN>()
                 .HasOne(d => d.stockMaster)
                 .WithMany(dm => dm.collectionTxn)
                 .HasForeignKey(dkey => dkey.StockMasterID);

            //modelBuilder.Entity<Portfolio_Master>().ToTable("PORTFOLIO_MASTER");
            modelBuilder.Entity<Portfolio_Master>().ToTable("Portfolio_Master").Navigation(e => e.collectionTxn).AutoInclude();
            modelBuilder.Entity<StockMaster>().ToTable("StockMaster").Navigation(e => e.collectionTxn).AutoInclude();

            //for many to many relationship you can use commented following code or above code for portfolio_master
            //& StockMaster table
            //modelBuilder.Entity<PORTFOLIOTXN>().HasKey(p => new { p.StockMasterID, p.PORTFOLIO_MASTER_ID });

            //modelBuilder.Entity<PORTFOLIOTXN>().ToTable("PORTFOLIOTXN").Navigation(e => e.stockMaster).AutoInclude();
            //modelBuilder.Entity<PORTFOLIOTXN>().ToTable("PORTFOLIOTXN").Navigation(e => e.portfolioMaster).AutoInclude();

            //modelBuilder.Entity<V20_CANDLE_STRATEGY>().ToTable("V20_CANDLE_STRATEGY").Navigation(e => e.StockMaster).AutoInclude();

            //modelBuilder.Entity<BULLISH_ENGULFING_STRATEGY>().ToTable("BULLISH_ENGULFING_STRATEGY").Navigation(e => e.StockMaster).AutoInclude();

            //modelBuilder.Entity<BEARISH_ENGULFING>().ToTable("BEARISH_ENGULFING").Navigation(e => e.StockMaster).AutoInclude();
        }
    }
}
