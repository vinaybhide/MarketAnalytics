﻿// <auto-generated />
using System;
using MarketAnalytics.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MarketAnalytics.Migrations
{
    [DbContext(typeof(DBContext))]
    partial class DBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.2");

            modelBuilder.Entity("MarketAnalytics.Models.BEARISH_ENGULFING", b =>
                {
                    b.Property<int>("BEARISH_ENGULFING_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("BUY_CANDLE_DATE")
                        .HasColumnType("TEXT");

                    b.Property<double>("BUY_PRICE")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("SELL_CANDLE_DATE")
                        .HasColumnType("TEXT");

                    b.Property<double>("SELL_PRICE")
                        .HasColumnType("REAL");

                    b.Property<int>("StockMasterID")
                        .HasColumnType("INTEGER");

                    b.HasKey("BEARISH_ENGULFING_ID");

                    b.HasIndex("StockMasterID");

                    b.ToTable("BEARISH_ENGULFING", (string)null);
                });

            modelBuilder.Entity("MarketAnalytics.Models.BULLISH_ENGULFING_STRATEGY", b =>
                {
                    b.Property<int>("BULLISH_ENGULFING_STRATEGY_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("AVG_BUY_PRICE")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("BUY_CANDLE_DATE")
                        .HasColumnType("TEXT");

                    b.Property<double>("BUY_PRICE")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("SELL_CANDLE_DATE")
                        .HasColumnType("TEXT");

                    b.Property<double>("SELL_PRICE")
                        .HasColumnType("REAL");

                    b.Property<int>("StockMasterID")
                        .HasColumnType("INTEGER");

                    b.HasKey("BULLISH_ENGULFING_STRATEGY_ID");

                    b.HasIndex("StockMasterID");

                    b.ToTable("BULLISH_ENGULFING_STRATEGY", (string)null);
                });

            modelBuilder.Entity("MarketAnalytics.Models.PORTFOLIOTXN", b =>
                {
                    b.Property<int>("PORTFOLIOTXN_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("PORTFOLIO_MASTER_ID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("StockMasterID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("TXN_BUY_DATE")
                        .HasColumnType("TEXT");

                    b.Property<string>("TXN_TYPE")
                        .HasColumnType("TEXT");

                    b.Property<int>("PURCHASE_QUANTITY")
                        .HasColumnType("INTEGER");

                    b.Property<double>("COST_PER_UNIT")
                        .HasColumnType("REAL");

                    b.Property<double>("TOTAL_COST")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("TXN_SELL_DATE")
                        .HasColumnType("TEXT");

                    b.Property<int>("SELL_QUANTITY")
                        .HasColumnType("INTEGER");

                    b.Property<double>("SELL_AMT_PER_UNIT")
                        .HasColumnType("REAL");

                    b.Property<double>("TOTAL_SELL_AMT")
                        .HasColumnType("REAL");

                    b.Property<int>("DAYS_SINCE")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SOLD_AFTER")
                        .HasColumnType("INTEGER");

                    b.Property<double?>("CMP")
                        .HasColumnType("REAL");

                    b.Property<double?>("VALUE")
                        .HasColumnType("REAL");

                    b.Property<double?>("CAGR")
                        .HasColumnType("REAL");

                    b.Property<double?>("GAIN_PCT")
                        .HasColumnType("REAL");

                    b.Property<double>("SELL_GAIN_PCT")
                        .HasColumnType("REAL");

                    b.Property<double?>("GAIN_AMT")
                        .HasColumnType("REAL");

                    b.Property<double>("SELL_GAIN_AMT")
                        .HasColumnType("REAL");

                    b.Property<double>("BUY_VS_52HI")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("LastUpDt")
                        .HasColumnType("TEXT");

                    b.HasKey("PORTFOLIOTXN_ID");

                    b.HasIndex("PORTFOLIO_MASTER_ID");

                    b.HasIndex("StockMasterID");

                    b.ToTable("PORTFOLIOTXN", (string)null);
                });

            modelBuilder.Entity("MarketAnalytics.Models.Portfolio_Master", b =>
                {
                    b.Property<int>("PORTFOLIO_MASTER_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("PORTFOLIO_NAME")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("PORTFOLIO_MASTER_ID");

                    b.ToTable("PORTFOLIO_MASTER", (string)null);
                });

            modelBuilder.Entity("MarketAnalytics.Models.StockMaster", b =>
                {
                    b.Property<int>("StockMasterID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("BEAR_ENGULF_LastUpDt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("BULL_ENGULF_LastUpDt")
                        .HasColumnType("TEXT");

                    b.Property<double>("Change")
                        .HasColumnType("REAL");

                    b.Property<double>("ChangePercent")
                        .HasColumnType("REAL");

                    b.Property<double>("Close")
                        .HasColumnType("REAL");

                    b.Property<string>("CompName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("DIFF_FROM_LIFETIME_HIGH")
                        .HasColumnType("REAL");

                    b.Property<double>("DIFF_FROM_YEAR_HI")
                        .HasColumnType("REAL");

                    b.Property<string>("Exchange")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("HI_LOW_67_50_LastUpDt")
                        .HasColumnType("TEXT");

                    b.Property<double>("High")
                        .HasColumnType("REAL");

                    b.Property<string>("INVESTMENT_TYPE")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LESSTHAN_67PCT_ON")
                        .HasColumnType("TEXT");

                    b.Property<double>("LIFETIME_HIGH")
                        .HasColumnType("REAL");

                    b.Property<double>("LIFETIME_LOW")
                        .HasColumnType("REAL");

                    b.Property<double>("Low")
                        .HasColumnType("REAL");

                    b.Property<double>("Open")
                        .HasColumnType("REAL");

                    b.Property<double>("PrevClose")
                        .HasColumnType("REAL");

                    b.Property<DateTime?>("QuoteDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("RSI_LastUpDt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("SMA_BUYSELL_LastUpDt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("STOCH_BUYSELL_LastUpDt")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("RSI_TREND_LastUpDt")
                        .HasColumnType("TEXT");

                    b.Property<bool>("SMA_BUY_SIGNAL")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("STOCH_BUY_SIGNAL")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("STOCH_SELL_SIGNAL")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("RSI_OVERBOUGHT")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("RSI_OVERSOLD")
                        .HasColumnType("INTEGER");

                    b.Property<double>("SMA_FAST")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("SMA_LastUpDt")
                        .HasColumnType("TEXT");

                    b.Property<double>("SMA_MID")
                        .HasColumnType("REAL");

                    b.Property<bool>("SMA_SELL_SIGNAL")
                        .HasColumnType("INTEGER");

                    b.Property<double>("SMA_SLOW")
                        .HasColumnType("REAL");

                    b.Property<double>("RSI_CLOSE")
                        .HasColumnType("REAL");

                    b.Property<double>("SlowD")
                        .HasColumnType("REAL");

                    b.Property<double>("FastK")
                        .HasColumnType("REAL");

                    b.Property<double>("STOCH_BUY_PRICE")
                        .HasColumnType("REAL");

                    b.Property<double>("STOCH_SELL_PRICE")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("STOCH_LastUpDt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("V200")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("V20_LastUpDt")
                        .HasColumnType("TEXT");

                    b.Property<bool>("V40")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("V40N")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Volume")
                        .HasColumnType("REAL");

                    b.Property<double>("YEAR_HI")
                        .HasColumnType("REAL");

                    b.Property<double>("YEAR_LO")
                        .HasColumnType("REAL");

                    b.HasKey("StockMasterID");

                    b.ToTable("StockMaster", (string)null);
                });

            modelBuilder.Entity("MarketAnalytics.Models.StockPriceHistory", b =>
                {
                    b.Property<int>("StockPriceHistoryID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("BULLISH_ENGULFING")
                        .HasColumnType("INTEGER");

                    b.Property<double?>("BUY_SMA_STRATEGY")
                        .HasColumnType("REAL");

                    b.Property<string>("CROSSOVER_FLAG")
                        .HasColumnType("TEXT");

                    b.Property<double>("Change")
                        .HasColumnType("REAL");

                    b.Property<double>("ChangePercent")
                        .HasColumnType("REAL");

                    b.Property<double>("Close")
                        .HasColumnType("REAL");

                    b.Property<double?>("FastK")
                        .HasColumnType("REAL");

                    b.Property<double>("High")
                        .HasColumnType("REAL");

                    b.Property<bool>("LOWER_THAN_SMA_SMALL")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Low")
                        .HasColumnType("REAL");

                    b.Property<double>("Open")
                        .HasColumnType("REAL");

                    b.Property<double>("PrevClose")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("PriceDate")
                        .HasColumnType("TEXT");

                    b.Property<double?>("RSI_CLOSE")
                        .HasColumnType("REAL");

                    b.Property<double?>("RSI_HIGH")
                        .HasColumnType("REAL");

                    b.Property<double?>("RSI_LOW")
                        .HasColumnType("REAL");

                    b.Property<double?>("RSI_OPEN")
                        .HasColumnType("REAL");

                    b.Property<double?>("SELL_SMA_STRATEGY")
                        .HasColumnType("REAL");

                    b.Property<double?>("SMA_LONG")
                        .HasColumnType("REAL");

                    b.Property<double?>("SMA_MID")
                        .HasColumnType("REAL");

                    b.Property<double?>("SMA_SMALL")
                        .HasColumnType("REAL");

                    b.Property<double?>("SlowD")
                        .HasColumnType("REAL");

                    b.Property<int>("StockMasterID")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("V20_CANDLE")
                        .HasColumnType("INTEGER");

                    b.Property<double?>("V20_CANDLE_BUY_PRICE")
                        .HasColumnType("REAL");

                    b.Property<double?>("V20_CANDLE_SELL_PRICE")
                        .HasColumnType("REAL");

                    b.Property<double>("Volume")
                        .HasColumnType("REAL");

                    b.HasKey("StockPriceHistoryID");

                    b.HasIndex("StockMasterID");

                    b.ToTable("StockPriceHistory", (string)null);
                });

            modelBuilder.Entity("MarketAnalytics.Models.UpdateTracker", b =>
                {
                    b.Property<int>("UpdateTracker_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DATA")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("REF_DATE")
                        .HasColumnType("TEXT");

                    b.Property<int>("StockMasterID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TYPE")
                        .HasColumnType("TEXT");

                    b.HasKey("UpdateTracker_ID");

                    b.ToTable("UpdateTracker", (string)null);
                });

            modelBuilder.Entity("MarketAnalytics.Models.UserMaster", b =>
                {
                    b.Property<int>("USER_MASTER_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("USER_ID")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("USER_PWD")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("USER_TYPE")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("UserMasterUSER_MASTER_ID")
                        .HasColumnType("INTEGER");

                    b.HasKey("USER_MASTER_ID");

                    b.HasIndex("UserMasterUSER_MASTER_ID");

                    b.ToTable("USER_MASTER", (string)null);
                });

            modelBuilder.Entity("MarketAnalytics.Models.V20_CANDLE_STRATEGY", b =>
                {
                    b.Property<int>("V20_CANDLE_STRATEGY_ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("BUY_PRICE")
                        .HasColumnType("REAL");

                    b.Property<double>("DIFF_PCT")
                        .HasColumnType("REAL");

                    b.Property<DateTime>("FROM_DATE")
                        .HasColumnType("TEXT");

                    b.Property<double>("SELL_PRICE")
                        .HasColumnType("REAL");

                    b.Property<int>("StockMasterID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("TO_DATE")
                        .HasColumnType("TEXT");

                    b.HasKey("V20_CANDLE_STRATEGY_ID");

                    b.HasIndex("StockMasterID");

                    b.ToTable("V20_CANDLE_STRATEGY", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("TEXT");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("TEXT");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("TEXT");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("MarketAnalytics.Models.BEARISH_ENGULFING", b =>
                {
                    b.HasOne("MarketAnalytics.Models.StockMaster", "StockMaster")
                        .WithMany()
                        .HasForeignKey("StockMasterID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("StockMaster");
                });

            modelBuilder.Entity("MarketAnalytics.Models.BULLISH_ENGULFING_STRATEGY", b =>
                {
                    b.HasOne("MarketAnalytics.Models.StockMaster", "StockMaster")
                        .WithMany()
                        .HasForeignKey("StockMasterID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("StockMaster");
                });

            modelBuilder.Entity("MarketAnalytics.Models.PORTFOLIOTXN", b =>
                {
                    b.HasOne("MarketAnalytics.Models.Portfolio_Master", "portfolioMaster")
                        .WithMany("collectionTxn")
                        .HasForeignKey("PORTFOLIO_MASTER_ID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MarketAnalytics.Models.StockMaster", "stockMaster")
                        .WithMany("collectionTxn")
                        .HasForeignKey("StockMasterID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("portfolioMaster");

                    b.Navigation("stockMaster");
                });

            modelBuilder.Entity("MarketAnalytics.Models.Portfolio_Master", b =>
                {
                    b.HasOne("MarketAnalytics.Models.UserMaster", "userMaster")
                        .WithMany()
                        .HasForeignKey("userMasterUSER_MASTER_ID");

                    b.Navigation("userMaster");
                });

            modelBuilder.Entity("MarketAnalytics.Models.StockPriceHistory", b =>
                {
                    b.HasOne("MarketAnalytics.Models.StockMaster", "StockMaster")
                        .WithMany()
                        .HasForeignKey("StockMasterID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("StockMaster");
                });

            modelBuilder.Entity("MarketAnalytics.Models.UserMaster", b =>
                {
                    b.HasOne("MarketAnalytics.Models.UserMaster", null)
                        .WithMany("portfolioCollection")
                        .HasForeignKey("UserMasterUSER_MASTER_ID");
                });

            modelBuilder.Entity("MarketAnalytics.Models.V20_CANDLE_STRATEGY", b =>
                {
                    b.HasOne("MarketAnalytics.Models.StockMaster", "StockMaster")
                        .WithMany("collection_V20_buysell")
                        .HasForeignKey("StockMasterID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("StockMaster");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MarketAnalytics.Models.Portfolio_Master", b =>
                {
                    b.Navigation("collectionTxn");
                });

            modelBuilder.Entity("MarketAnalytics.Models.StockMaster", b =>
                {
                    b.Navigation("collectionTxn");

                    b.Navigation("collection_V20_buysell");
                });

            modelBuilder.Entity("MarketAnalytics.Models.UserMaster", b =>
                {
                    b.Navigation("portfolioCollection");
                });
#pragma warning restore 612, 618
        }
    }
}
