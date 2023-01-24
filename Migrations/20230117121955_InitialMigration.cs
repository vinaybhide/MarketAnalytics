using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketAnalytics.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockMaster",
                columns: table => new
                {
                    StockMasterID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Exchange = table.Column<string>(type: "TEXT", nullable: false),
                    Symbol = table.Column<string>(type: "TEXT", nullable: false),
                    CompName = table.Column<string>(type: "TEXT", nullable: false),
                    INVESTMENTTYPE = table.Column<string>(name: "INVESTMENT_TYPE", type: "TEXT", nullable: true),
                    QuoteDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Open = table.Column<double>(type: "REAL", nullable: false),
                    High = table.Column<double>(type: "REAL", nullable: false),
                    Low = table.Column<double>(type: "REAL", nullable: false),
                    Close = table.Column<double>(type: "REAL", nullable: false),
                    Volume = table.Column<double>(type: "REAL", nullable: false),
                    Change = table.Column<double>(type: "REAL", nullable: false),
                    ChangePercent = table.Column<double>(type: "REAL", nullable: false),
                    PrevClose = table.Column<double>(type: "REAL", nullable: false),
                    V40 = table.Column<bool>(type: "INTEGER", nullable: false),
                    V40N = table.Column<bool>(type: "INTEGER", nullable: false),
                    V200 = table.Column<bool>(type: "INTEGER", nullable: false),
                    SMABUYSIGNAL = table.Column<bool>(name: "SMA_BUY_SIGNAL", type: "INTEGER", nullable: false),
                    SMASELLSIGNAL = table.Column<bool>(name: "SMA_SELL_SIGNAL", type: "INTEGER", nullable: false),
                    STOCHBUYSIGNAL = table.Column<bool>(name: "STOCH_BUY_SIGNAL", type: "INTEGER", nullable: false),
                    STOCHSELLSIGNAL = table.Column<bool>(name: "STOCH_SELL_SIGNAL", type: "INTEGER", nullable: false),
                    RSIOVERBOUGHT = table.Column<bool>(name: "RSI_OVERBOUGHT", type: "INTEGER", nullable: false),
                    RSIOVERSOLD = table.Column<bool>(name: "RSI_OVERSOLD", type: "INTEGER", nullable: false),
                    SMAFAST = table.Column<double>(name: "SMA_FAST", type: "REAL", nullable: false),
                    SMAMID = table.Column<double>(name: "SMA_MID", type: "REAL", nullable: false),
                    SMASLOW = table.Column<double>(name: "SMA_SLOW", type: "REAL", nullable: false),
                    RSICLOSE = table.Column<double>(name: "RSI_CLOSE", type: "REAL", nullable: false),
                    STOCHBUYPRICE = table.Column<double>(name: "STOCH_BUY_PRICE", type: "REAL", nullable: false),
                    STOCHSELLPRICE = table.Column<double>(name: "STOCH_SELL_PRICE", type: "REAL", nullable: false),
                    YEARHI = table.Column<double>(name: "YEAR_HI", type: "REAL", nullable: false),
                    YEARLO = table.Column<double>(name: "YEAR_LO", type: "REAL", nullable: false),
                    LIFETIMEHIGH = table.Column<double>(name: "LIFETIME_HIGH", type: "REAL", nullable: false),
                    LIFETIMELOW = table.Column<double>(name: "LIFETIME_LOW", type: "REAL", nullable: false),
                    LESSTHAN67PCTON = table.Column<DateTime>(name: "LESSTHAN_67PCT_ON", type: "TEXT", nullable: false),
                    DIFFFROMYEARHI = table.Column<double>(name: "DIFF_FROM_YEAR_HI", type: "REAL", nullable: false),
                    DIFFFROMLIFETIMEHIGH = table.Column<double>(name: "DIFF_FROM_LIFETIME_HIGH", type: "REAL", nullable: false),
                    HILOW6750LastUpDt = table.Column<DateTime>(name: "HI_LOW_67_50_LastUpDt", type: "TEXT", nullable: false),
                    SMALastUpDt = table.Column<DateTime>(name: "SMA_LastUpDt", type: "TEXT", nullable: false),
                    RSILastUpDt = table.Column<DateTime>(name: "RSI_LastUpDt", type: "TEXT", nullable: false),
                    STOCHLastUpDt = table.Column<DateTime>(name: "STOCH_LastUpDt", type: "TEXT", nullable: false),
                    BULLENGULFLastUpDt = table.Column<DateTime>(name: "BULL_ENGULF_LastUpDt", type: "TEXT", nullable: false),
                    BEARENGULFLastUpDt = table.Column<DateTime>(name: "BEAR_ENGULF_LastUpDt", type: "TEXT", nullable: false),
                    V20LastUpDt = table.Column<DateTime>(name: "V20_LastUpDt", type: "TEXT", nullable: false),
                    SMABUYSELLLastUpDt = table.Column<DateTime>(name: "SMA_BUYSELL_LastUpDt", type: "TEXT", nullable: false),
                    STOCHBUYSELLLastUpDt = table.Column<DateTime>(name: "STOCH_BUYSELL_LastUpDt", type: "TEXT", nullable: false),
                    RSITRENDLastUpDt = table.Column<DateTime>(name: "RSI_TREND_LastUpDt", type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMaster", x => x.StockMasterID);
                });

            migrationBuilder.CreateTable(
                name: "UpdateTracker",
                columns: table => new
                {
                    UpdateTrackerID = table.Column<int>(name: "UpdateTracker_ID", type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    REFDATE = table.Column<DateTime>(name: "REF_DATE", type: "TEXT", nullable: false),
                    TYPE = table.Column<string>(type: "TEXT", nullable: true),
                    DATA = table.Column<string>(type: "TEXT", nullable: true),
                    StockMasterID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpdateTracker", x => x.UpdateTrackerID);
                });

            migrationBuilder.CreateTable(
                name: "USER_MASTER",
                columns: table => new
                {
                    USERMASTERID = table.Column<int>(name: "USER_MASTER_ID", type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    USERID = table.Column<string>(name: "USER_ID", type: "TEXT", nullable: false),
                    USERPWD = table.Column<string>(name: "USER_PWD", type: "TEXT", nullable: false),
                    USERTYPE = table.Column<int>(name: "USER_TYPE", type: "INTEGER", nullable: false),
                    UserMasterUSERMASTERID = table.Column<int>(name: "UserMasterUSER_MASTER_ID", type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_MASTER", x => x.USERMASTERID);
                    //table.ForeignKey(
                    //    name: "FK_USER_MASTER_USER_MASTER_UserMasterUSER_MASTER_ID",
                    //    column: x => x.UserMasterUSERMASTERID,
                    //    principalTable: "USER_MASTER",
                    //    principalColumn: "USER_MASTER_ID");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BEARISH_ENGULFING",
                columns: table => new
                {
                    BEARISHENGULFINGID = table.Column<int>(name: "BEARISH_ENGULFING_ID", type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BUYCANDLEDATE = table.Column<DateTime>(name: "BUY_CANDLE_DATE", type: "TEXT", nullable: false),
                    SELLCANDLEDATE = table.Column<DateTime>(name: "SELL_CANDLE_DATE", type: "TEXT", nullable: false),
                    BUYPRICE = table.Column<double>(name: "BUY_PRICE", type: "REAL", nullable: false),
                    SELLPRICE = table.Column<double>(name: "SELL_PRICE", type: "REAL", nullable: false),
                    StockMasterID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BEARISH_ENGULFING", x => x.BEARISHENGULFINGID);
                    table.ForeignKey(
                        name: "FK_BEARISH_ENGULFING_StockMaster_StockMasterID",
                        column: x => x.StockMasterID,
                        principalTable: "StockMaster",
                        principalColumn: "StockMasterID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BULLISH_ENGULFING_STRATEGY",
                columns: table => new
                {
                    BULLISHENGULFINGSTRATEGYID = table.Column<int>(name: "BULLISH_ENGULFING_STRATEGY_ID", type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BUYCANDLEDATE = table.Column<DateTime>(name: "BUY_CANDLE_DATE", type: "TEXT", nullable: false),
                    SELLCANDLEDATE = table.Column<DateTime>(name: "SELL_CANDLE_DATE", type: "TEXT", nullable: false),
                    BUYPRICE = table.Column<double>(name: "BUY_PRICE", type: "REAL", nullable: false),
                    SELLPRICE = table.Column<double>(name: "SELL_PRICE", type: "REAL", nullable: false),
                    AVGBUYPRICE = table.Column<double>(name: "AVG_BUY_PRICE", type: "REAL", nullable: false),
                    StockMasterID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BULLISH_ENGULFING_STRATEGY", x => x.BULLISHENGULFINGSTRATEGYID);
                    table.ForeignKey(
                        name: "FK_BULLISH_ENGULFING_STRATEGY_StockMaster_StockMasterID",
                        column: x => x.StockMasterID,
                        principalTable: "StockMaster",
                        principalColumn: "StockMasterID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockPriceHistory",
                columns: table => new
                {
                    StockPriceHistoryID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PriceDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Open = table.Column<double>(type: "REAL", nullable: false),
                    High = table.Column<double>(type: "REAL", nullable: false),
                    Low = table.Column<double>(type: "REAL", nullable: false),
                    Close = table.Column<double>(type: "REAL", nullable: false),
                    Volume = table.Column<double>(type: "REAL", nullable: false),
                    Change = table.Column<double>(type: "REAL", nullable: false),
                    ChangePercent = table.Column<double>(type: "REAL", nullable: false),
                    PrevClose = table.Column<double>(type: "REAL", nullable: false),
                    RSIOPEN = table.Column<double>(name: "RSI_OPEN", type: "REAL", nullable: true),
                    RSICLOSE = table.Column<double>(name: "RSI_CLOSE", type: "REAL", nullable: true),
                    RSIHIGH = table.Column<double>(name: "RSI_HIGH", type: "REAL", nullable: true),
                    RSILOW = table.Column<double>(name: "RSI_LOW", type: "REAL", nullable: true),
                    SMASMALL = table.Column<double>(name: "SMA_SMALL", type: "REAL", nullable: true),
                    SMAMID = table.Column<double>(name: "SMA_MID", type: "REAL", nullable: true),
                    SMALONG = table.Column<double>(name: "SMA_LONG", type: "REAL", nullable: true),
                    CROSSOVERFLAG = table.Column<string>(name: "CROSSOVER_FLAG", type: "TEXT", nullable: true),
                    LOWERTHANSMASMALL = table.Column<bool>(name: "LOWER_THAN_SMA_SMALL", type: "INTEGER", nullable: false),
                    BULLISHENGULFING = table.Column<bool>(name: "BULLISH_ENGULFING", type: "INTEGER", nullable: false),
                    BUYSMASTRATEGY = table.Column<double>(name: "BUY_SMA_STRATEGY", type: "REAL", nullable: true),
                    SELLSMASTRATEGY = table.Column<double>(name: "SELL_SMA_STRATEGY", type: "REAL", nullable: true),
                    V20CANDLE = table.Column<bool>(name: "V20_CANDLE", type: "INTEGER", nullable: false),
                    V20CANDLEBUYPRICE = table.Column<double>(name: "V20_CANDLE_BUY_PRICE", type: "REAL", nullable: true),
                    V20CANDLESELLPRICE = table.Column<double>(name: "V20_CANDLE_SELL_PRICE", type: "REAL", nullable: true),
                    SlowD = table.Column<double>(type: "REAL", nullable: true),
                    FastK = table.Column<double>(type: "REAL", nullable: true),
                    StockMasterID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockPriceHistory", x => x.StockPriceHistoryID);
                    table.ForeignKey(
                        name: "FK_StockPriceHistory_StockMaster_StockMasterID",
                        column: x => x.StockMasterID,
                        principalTable: "StockMaster",
                        principalColumn: "StockMasterID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "V20_CANDLE_STRATEGY",
                columns: table => new
                {
                    V20CANDLESTRATEGYID = table.Column<int>(name: "V20_CANDLE_STRATEGY_ID", type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FROMDATE = table.Column<DateTime>(name: "FROM_DATE", type: "TEXT", nullable: false),
                    TODATE = table.Column<DateTime>(name: "TO_DATE", type: "TEXT", nullable: false),
                    DIFFPCT = table.Column<double>(name: "DIFF_PCT", type: "REAL", nullable: false),
                    BUYPRICE = table.Column<double>(name: "BUY_PRICE", type: "REAL", nullable: false),
                    SELLPRICE = table.Column<double>(name: "SELL_PRICE", type: "REAL", nullable: false),
                    StockMasterID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_V20_CANDLE_STRATEGY", x => x.V20CANDLESTRATEGYID);
                    table.ForeignKey(
                        name: "FK_V20_CANDLE_STRATEGY_StockMaster_StockMasterID",
                        column: x => x.StockMasterID,
                        principalTable: "StockMaster",
                        principalColumn: "StockMasterID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PORTFOLIO_MASTER",
                columns: table => new
                {
                    PORTFOLIOMASTERID = table.Column<int>(name: "PORTFOLIO_MASTER_ID", type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PORTFOLIONAME = table.Column<string>(name: "PORTFOLIO_NAME", type: "TEXT", nullable: false),
                    //USERMASTERID = table.Column<int>(name: "USER_MASTER_ID", type: "INTEGER", nullable: false),
                    //userMasterUSERMASTERID = table.Column<int>(name: "userMasterUSER_MASTER_ID", type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PORTFOLIO_MASTER", x => x.PORTFOLIOMASTERID);
                    //table.ForeignKey(
                    //    name: "FK_PORTFOLIO_MASTER_USER_MASTER_userMasterUSER_MASTER_ID",
                    //    column: x => x.userMasterUSERMASTERID,
                    //    principalTable: "USER_MASTER",
                    //    principalColumn: "USER_MASTER_ID");
                });

            migrationBuilder.CreateTable(
                name: "PORTFOLIOTXN",
                columns: table => new
                {
                    PORTFOLIOTXNID = table.Column<int>(name: "PORTFOLIOTXN_ID", type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PORTFOLIOMASTERID = table.Column<int>(name: "PORTFOLIO_MASTER_ID", type: "INTEGER", nullable: false),
                    StockMasterID = table.Column<int>(type: "INTEGER", nullable: false),
                    TXN_BUY_DATE = table.Column<DateTime>(name: "TXN_BUY_DATE", type: "TEXT", nullable: false),
                    TXN_TYPE = table.Column<string>(name: "TXN_TYPE", type: "TEXT", nullable: true),
                    PURCHASE_QUANTITY = table.Column<int>(type: "INTEGER", nullable: false),
                    COST_PER_UNIT = table.Column<double>(name: "COST_PER_UNIT", type: "REAL", nullable: false),
                    TOTAL_COST = table.Column<double>(name: "TOTAL_COST", type: "REAL", nullable: false),
                    TXN_SELL_DATE = table.Column<DateTime>(name: "TXN_SELL_DATE", type: "TEXT", nullable: false),
                    SELL_QUANTITY = table.Column<int>(type: "INTEGER", nullable: false),
                    SELL_AMT_PER_UNIT = table.Column<double>(name: "SELL_AMT_PER_UNIT", type: "REAL", nullable: false),
                    TOTAL_SELL_AMT = table.Column<double>(name: "TOTAL_SELL_AMT", type: "REAL", nullable: false),
                    DAYS_SINCE = table.Column<int>(name: "DAYS_SINCE", type: "INTEGER", nullable: false),
                    SOLD_AFTER = table.Column<int>(name: "SOLD_AFTER", type: "INTEGER", nullable: false),
                    CMP = table.Column<double>(type: "REAL", nullable: true),
                    VALUE = table.Column<double>(type: "REAL", nullable: true),
                    CAGR = table.Column<double>(type: "REAL", nullable: true),
                    GAIN_PCT = table.Column<double>(name: "GAIN_PCT", type: "REAL", nullable: true),
                    SELL_GAIN_PCT = table.Column<double>(name: "SELL_GAIN_PCT", type: "REAL", nullable: true),
                    GAIN_AMT = table.Column<double>(name: "GAIN_AMT", type: "REAL", nullable: true),
                    SELL_GAIN_AMT = table.Column<double>(name: "SELL_GAIN_AMT", type: "REAL", nullable: true),
                    BUY_VS_52HI = table.Column<double>(name: "BUY_VS_52HI", type: "REAL", nullable: false),
                    LastUpDt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PORTFOLIOTXN", x => x.PORTFOLIOTXNID);
                    table.ForeignKey(
                        name: "FK_PORTFOLIOTXN_PORTFOLIO_MASTER_PORTFOLIO_MASTER_ID",
                        column: x => x.PORTFOLIOMASTERID,
                        principalTable: "PORTFOLIO_MASTER",
                        principalColumn: "PORTFOLIO_MASTER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PORTFOLIOTXN_StockMaster_StockMasterID",
                        column: x => x.StockMasterID,
                        principalTable: "StockMaster",
                        principalColumn: "StockMasterID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BEARISH_ENGULFING_StockMasterID",
                table: "BEARISH_ENGULFING",
                column: "StockMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_BULLISH_ENGULFING_STRATEGY_StockMasterID",
                table: "BULLISH_ENGULFING_STRATEGY",
                column: "StockMasterID");

            //migrationBuilder.CreateIndex(
            //    name: "IX_PORTFOLIO_MASTER_userMasterUSER_MASTER_ID",
            //    table: "PORTFOLIO_MASTER",
            //    column: "userMasterUSER_MASTER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PORTFOLIOTXN_PORTFOLIO_MASTER_ID",
                table: "PORTFOLIOTXN",
                column: "PORTFOLIO_MASTER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_PORTFOLIOTXN_StockMasterID",
                table: "PORTFOLIOTXN",
                column: "StockMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_StockPriceHistory_StockMasterID",
                table: "StockPriceHistory",
                column: "StockMasterID");

            migrationBuilder.CreateIndex(
                name: "IX_USER_MASTER_UserMasterUSER_MASTER_ID",
                table: "USER_MASTER",
                column: "UserMasterUSER_MASTER_ID");

            migrationBuilder.CreateIndex(
                name: "IX_V20_CANDLE_STRATEGY_StockMasterID",
                table: "V20_CANDLE_STRATEGY",
                column: "StockMasterID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BEARISH_ENGULFING");

            migrationBuilder.DropTable(
                name: "BULLISH_ENGULFING_STRATEGY");

            migrationBuilder.DropTable(
                name: "PORTFOLIOTXN");

            migrationBuilder.DropTable(
                name: "StockPriceHistory");

            migrationBuilder.DropTable(
                name: "UpdateTracker");

            migrationBuilder.DropTable(
                name: "V20_CANDLE_STRATEGY");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "PORTFOLIO_MASTER");

            migrationBuilder.DropTable(
                name: "StockMaster");

            migrationBuilder.DropTable(
                name: "USER_MASTER");
        }
    }
}
