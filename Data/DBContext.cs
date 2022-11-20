using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Models;
using System.Reflection.Metadata;

namespace MarketAnalytics.Data
{
    public class DBContext : DbContext
    {
        public DBContext (DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        public DbSet<MarketAnalytics.Models.StockMaster> StockMaster { get; set; } = default!;
        public DbSet<MarketAnalytics.Models.StockPriceHistory> StockPriceHistory { get; set; } = default!;
        public DbSet<MarketAnalytics.Models.PORTFOLIO> PORTFOLIO { get; set; } = default!;

        public DbSet<MarketAnalytics.Models.V20_CANDLE_STRATEGY> V20_CANDLE_STRATEGY { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StockMaster>().ToTable("StockMaster");
            //modelBuilder.Entity<StockPriceHistory>().ToTable("StockPriceHistory");
            modelBuilder.Entity<StockPriceHistory>().ToTable("StockPriceHistory").Navigation(e => e.StockMaster).AutoInclude();

            //Autoinclude will automatically include the StockMaster object
            modelBuilder.Entity<PORTFOLIO>().ToTable("PORTFOLIO").Navigation(e => e.StockMaster).AutoInclude();

            modelBuilder.Entity<V20_CANDLE_STRATEGY>().ToTable("V20_CANDLE_STRATEGY").Navigation(e => e.StockMaster).AutoInclude(); ;
        }
    }
}
