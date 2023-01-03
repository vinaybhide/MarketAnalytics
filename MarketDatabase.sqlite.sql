BEGIN TRANSACTION;
DROP TABLE IF EXISTS "PORTFOLIO";
CREATE TABLE IF NOT EXISTS "PORTFOLIO" (
	"PORTFOLIO_ID"	INTEGER NOT NULL,
	"PURCHASE_DATE"	TEXT NOT NULL,
	"QUANTITY"	INT NOT NULL,
	"COST_PER_SHARE"	REAL NOT NULL,
	"TOTAL_COST"	REAL NOT NULL,
	"CMP"	REAL,
	"VALUE"	REAL,
	"PORTFOLIO_MASTER_ID"	INTEGER NOT NULL,
	"StockMasterID"	INTEGER NOT NULL,
	CONSTRAINT "FK_PORTFOLIO_StockMaster_StockMasterID" FOREIGN KEY("StockMasterID") REFERENCES "StockMaster"("StockMasterID") ON DELETE CASCADE,
	CONSTRAINT "FK_PORTFOLIO_PORTFOLIO_MASTER_PORTFOLIO_MASTER_ID" FOREIGN KEY("PORTFOLIO_MASTER_ID") REFERENCES "PORTFOLIO_MASTER"("PORTFOLIO_MASTER_ID") ON DELETE CASCADE,
	CONSTRAINT "PK_PORTFOLIO" PRIMARY KEY("PORTFOLIO_ID" AUTOINCREMENT)
);
DROP TABLE IF EXISTS "PORTFOLIOTXN";
CREATE TABLE IF NOT EXISTS "PORTFOLIOTXN" (
	"PORTFOLIOTXN_ID"	INTEGER NOT NULL,
	"PORTFOLIO_MASTER_ID"	INTEGER NOT NULL,
	"StockMasterID"	INTEGER NOT NULL,
	"PURCHASE_DATE"	TEXT NOT NULL,
	"QUANTITY"	INT NOT NULL,
	"COST_PER_SHARE"	REAL NOT NULL,
	"TOTAL_COST"	REAL NOT NULL,
	"CMP"	REAL,
	"VALUE"	REAL,
	"GAIN_PCT"	REAL DEFAULT 0,
	"GAIN_AMT"	REAL DEFAULT 0,
	"LastUpDt"	TEXT NOT NULL DEFAULT '0001-01-01',
	CONSTRAINT "FK_StockMasterID" FOREIGN KEY("StockMasterID") REFERENCES "StockMaster"("StockMasterID") ON DELETE CASCADE,
	CONSTRAINT "FK_PORTFOLIO_MASTER_ID" FOREIGN KEY("PORTFOLIO_MASTER_ID") REFERENCES "PORTFOLIO_MASTER"("PORTFOLIO_MASTER_ID") ON DELETE CASCADE,
	CONSTRAINT "PK_PORTFOLIOTXN" PRIMARY KEY("PORTFOLIOTXN_ID" AUTOINCREMENT)
);
DROP TABLE IF EXISTS "BEARISH_ENGULFING";
CREATE TABLE IF NOT EXISTS "BEARISH_ENGULFING" (
	"BEARISH_ENGULFING_ID"	INTEGER NOT NULL,
	"BUY_CANDLE_DATE"	TEXT NOT NULL,
	"SELL_CANDLE_DATE"	TEXT NOT NULL,
	"BUY_PRICE"	REAL NOT NULL DEFAULT 0,
	"SELL_PRICE"	REAL NOT NULL DEFAULT 0,
	"StockMasterID"	INTEGER NOT NULL,
	CONSTRAINT "FK_BEARISH_ENGULFING_StockMaster_StockMasterID" FOREIGN KEY("StockMasterID") REFERENCES "StockMaster"("StockMasterID") ON DELETE CASCADE,
	CONSTRAINT "PK_BEARISH_ENGULFING" PRIMARY KEY("BEARISH_ENGULFING_ID" AUTOINCREMENT)
);
DROP TABLE IF EXISTS "BULLISH_ENGULFING_STRATEGY";
CREATE TABLE IF NOT EXISTS "BULLISH_ENGULFING_STRATEGY" (
	"BULLISH_ENGULFING_STRATEGY_ID"	INTEGER NOT NULL,
	"BUY_CANDLE_DATE"	TEXT NOT NULL,
	"SELL_CANDLE_DATE"	TEXT NOT NULL,
	"BUY_PRICE"	REAL NOT NULL,
	"SELL_PRICE"	REAL NOT NULL,
	"AVG_BUY_PRICE"	REAL NOT NULL DEFAULT 0,
	"StockMasterID"	INTEGER NOT NULL,
	CONSTRAINT "FK_BULLISH_ENGULFING_STRATEGY_StockMaster_StockMasterID" FOREIGN KEY("StockMasterID") REFERENCES "StockMaster"("StockMasterID") ON DELETE CASCADE,
	CONSTRAINT "PK_BULLISH_ENGULFING_STRATEGY" PRIMARY KEY("BULLISH_ENGULFING_STRATEGY_ID" AUTOINCREMENT)
);
DROP TABLE IF EXISTS "SMA_BUYSELL";
CREATE TABLE IF NOT EXISTS "SMA_BUYSELL" (
	"SMA_BUYSELL_ID"	INTEGER NOT NULL,
	"SMA_FAST"	REAL,
	"SMA_MID"	REAL,
	"SMA_SLOW"	REAL,
	"CLOSE"	REAL,
	"BUY_INDICATOR"	INTEGER,
	"SELL_INDICATOR"	NUMERIC,
	"StockMasterID"	INTEGER NOT NULL,
	CONSTRAINT "FK_SMA_BUYSELL_StockMaster_StockMasterID" FOREIGN KEY("StockMasterID") REFERENCES "StockMaster"("StockMasterID") ON DELETE CASCADE,
	CONSTRAINT "PK_SMA_BUYSELL" PRIMARY KEY("SMA_BUYSELL_ID" AUTOINCREMENT)
);
DROP TABLE IF EXISTS "StockPriceHistory";
CREATE TABLE IF NOT EXISTS "StockPriceHistory" (
	"StockPriceHistoryID"	INTEGER NOT NULL,
	"PriceDate"	TEXT NOT NULL,
	"Open"	REAL NOT NULL,
	"High"	REAL NOT NULL,
	"Low"	REAL NOT NULL,
	"Close"	REAL NOT NULL,
	"Volume"	REAL NOT NULL,
	"Change"	REAL NOT NULL,
	"ChangePercent"	REAL NOT NULL,
	"PrevClose"	REAL NOT NULL,
	"RSI_OPEN"	REAL NOT NULL DEFAULT 0,
	"RSI_CLOSE"	REAL NOT NULL DEFAULT 0,
	"RSI_HIGH"	REAL NOT NULL DEFAULT 0,
	"RSI_LOW"	REAL NOT NULL DEFAULT 0,
	"SMA_SMALL"	REAL NOT NULL DEFAULT 0,
	"SMA_MID"	REAL NOT NULL DEFAULT 0,
	"SMA_LONG"	REAL NOT NULL DEFAULT 0,
	"CROSSOVER_FLAG"	TEXT NOT NULL DEFAULT '',
	"LOWER_THAN_SMA_SMALL"	INTEGER NOT NULL DEFAULT 0,
	"BULLISH_ENGULFING"	INTEGER NOT NULL DEFAULT 0,
	"BUY_SMA_STRATEGY"	REAL NOT NULL DEFAULT 0,
	"SELL_SMA_STRATEGY"	REAL NOT NULL DEFAULT 0,
	"V20_CANDLE"	INTEGER NOT NULL DEFAULT 0,
	"V20_CANDLE_BUY_PRICE"	REAL NOT NULL DEFAULT 0,
	"V20_CANDLE_SELL_PRICE"	REAL NOT NULL DEFAULT 0,
	"SlowD"	REAL NOT NULL DEFAULT 0,
	"FastK"	REAL NOT NULL DEFAULT 0,
	"StockMasterID"	INTEGER NOT NULL,
	CONSTRAINT "FK_StockPriceHistory_StockMaster_StockMasterID" FOREIGN KEY("StockMasterID") REFERENCES "StockMaster"("StockMasterID") ON DELETE CASCADE,
	CONSTRAINT "PK_StockPriceHistory" PRIMARY KEY("StockPriceHistoryID" AUTOINCREMENT)
);
DROP TABLE IF EXISTS "V20_CANDLE_STRATEGY";
CREATE TABLE IF NOT EXISTS "V20_CANDLE_STRATEGY" (
	"V20_CANDLE_STRATEGY_ID"	INTEGER NOT NULL,
	"FROM_DATE"	TEXT NOT NULL,
	"TO_DATE"	TEXT NOT NULL,
	"DIFF_PCT"	REAL NOT NULL DEFAULT 0,
	"BUY_PRICE"	REAL NOT NULL,
	"SELL_PRICE"	REAL NOT NULL,
	"StockMasterID"	INTEGER NOT NULL,
	CONSTRAINT "FK_V20_CANDLE_STRATEGY_StockMaster_StockMasterID" FOREIGN KEY("StockMasterID") REFERENCES "StockMaster"("StockMasterID") ON DELETE CASCADE,
	CONSTRAINT "PK_V20_CANDLE_STRATEGY" PRIMARY KEY("V20_CANDLE_STRATEGY_ID" AUTOINCREMENT)
);
DROP TABLE IF EXISTS "PORTFOLIO_MASTER";
CREATE TABLE IF NOT EXISTS "PORTFOLIO_MASTER" (
	"PORTFOLIO_MASTER_ID"	INTEGER NOT NULL,
	"PORTFOLIO_NAME"	TEXT NOT NULL UNIQUE,
	PRIMARY KEY("PORTFOLIO_MASTER_ID")
);
DROP TABLE IF EXISTS "StockMaster";
CREATE TABLE IF NOT EXISTS "StockMaster" (
	"StockMasterID"	INTEGER NOT NULL,
	"Exchange"	TEXT NOT NULL,
	"Symbol"	TEXT NOT NULL,
	"CompName"	TEXT NOT NULL,
	"QuoteDateTime"	TEXT,
	"Open"	REAL NOT NULL,
	"High"	REAL NOT NULL,
	"Low"	REAL NOT NULL,
	"Close"	REAL NOT NULL,
	"Volume"	REAL NOT NULL,
	"Change"	REAL NOT NULL,
	"ChangePercent"	REAL NOT NULL,
	"PrevClose"	REAL NOT NULL,
	"V40"	INTEGER DEFAULT 0,
	"V40N"	INTEGER DEFAULT 0,
	"V200"	INTEGER DEFAULT 0,
	"SMA_BUY_SIGNAL"	INTEGER DEFAULT 0,
	"SMA_SELL_SIGNAL"	INTEGER DEFAULT 0,
	"SMA_FAST"	REAL DEFAULT 0,
	"SMA_MID"	REAL DEFAULT 0,
	"SMA_SLOW"	REAL DEFAULT 0,
	"LIFETIME_HIGH"	REAL DEFAULT 0,
	"LIFETIME_LOW"	REAL DEFAULT 0,
	"LESSTHAN_67PCT_ON"	TEXT NOT NULL DEFAULT '0001-01-01',
	"DIFF_FROM_LIFETIME_HIGH"	REAL DEFAULT 0,
	"HI_LOW_67_50_LastUpDt"	TEXT NOT NULL DEFAULT '0001-01-01',
	"SMA_LastUpDt"	TEXT NOT NULL DEFAULT '0001-01-01',
	"RSI_LastUpDt"	TEXT NOT NULL DEFAULT '0001-01-01',
	"STOCH_LastUpDt"	TEXT NOT NULL DEFAULT '0001-01-01',
	"BULL_ENGULF_LastUpDt"	TEXT NOT NULL DEFAULT '0001-01-01',
	"BEAR_ENGULF_LastUpDt"	TEXT NOT NULL DEFAULT '0001-01-01',
	"V20_LastUpDt"	TEXT NOT NULL DEFAULT '0001-01-01',
	"SMA_BUYSELL_LastUpDt"	TEXT NOT NULL DEFAULT '0001-01-01',
	CONSTRAINT "PK_StockMaster" PRIMARY KEY("StockMasterID" AUTOINCREMENT)
);
DROP TABLE IF EXISTS "UpdateTracker";
CREATE TABLE IF NOT EXISTS "UpdateTracker" (
	"UpdateTracker_ID"	INTEGER NOT NULL,
	"REF_DATE"	TEXT NOT NULL DEFAULT '0001-01-01',
	"TYPE"	TEXT DEFAULT '',
	"DATA"	INTEGER DEFAULT '',
	PRIMARY KEY("UpdateTracker_ID" AUTOINCREMENT)
);
DROP INDEX IF EXISTS "IX_SMA_BUYSELL_StockMasterID";
CREATE INDEX IF NOT EXISTS "IX_SMA_BUYSELL_StockMasterID" ON "SMA_BUYSELL" (
	"StockMasterID"
);
DROP INDEX IF EXISTS "IX_StockPriceHistory_StockMasterID";
CREATE INDEX IF NOT EXISTS "IX_StockPriceHistory_StockMasterID" ON "StockPriceHistory" (
	"StockMasterID"
);
COMMIT;
