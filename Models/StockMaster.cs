﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MarketAnalytics.Models
{
    public class StockMaster
    {
        [Key]
        public int StockMasterID { get; set; }

        [Required]
        [DisplayName("EXCH")]
        public string Exchange { get; set; } = string.Empty;

        [Required]
        public string Symbol { get; set; } = String.Empty;

        [Required]
        [DisplayName("Company")]
        public string CompName { get; set; } = String.Empty;

        [DisplayName("Type")]
        public string INVESTMENT_TYPE { get; set; } = String.Empty;

        [DisplayName("Date")]
        public DateTime? QuoteDateTime { get; set; } = default(DateTime);
        public double Open { get; set; } = default(double);
        public double High { get; set; } = default(double);
        public double Low { get; set; } = default(double);
        public double Close { get; set; } = default(double);
        public double Volume { get; set; } = default(double);
        public double Change { get; set; } = default(double);
        
        [DisplayName("%Change")]
        public double ChangePercent { get; set; } = default(double);
        public double PrevClose { get; set; } = default(double);

        public bool V40 { get; set; } = false;
        public bool V40N { get; set; } = false;
        public bool V200 { get; set;} = false;
        [DisplayName("SMABUY")]
        public bool SMA_BUY_SIGNAL { get; set; } = false;
        [DisplayName("SMASELL")]
        public bool SMA_SELL_SIGNAL { get; set; } = false;
        [DisplayName("STOCH BUY")]
        public bool STOCH_BUY_SIGNAL { get; set; } = false;
        [DisplayName("STOCH SELL")]
        public bool STOCH_SELL_SIGNAL { get; set; } = false;

        [DisplayName("Overbought")]
        public bool RSI_OVERBOUGHT { get; set; } = false;
        [DisplayName("Oversold")]
        public bool RSI_OVERSOLD { get; set; } = false;

        [DisplayName("SMA20")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double SMA_FAST { get; set; } = default(double);
        [DisplayName("SMA50")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double SMA_MID { get; set; } = default(double);
        [DisplayName("SMA200")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)] 
        public double SMA_SLOW { get; set; } = default(double);
        
        [DisplayName("RSI_CLOSE")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double RSI_CLOSE { get; set; } = default(double);

        [DisplayName("SlowD")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double SlowD { get; set; } = default(double);

        [DisplayName("FastK")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double FastK { get; set; } = default(double);

        [DisplayName("STOCH BUY PRICE")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double STOCH_BUY_PRICE { get; set; } = default(double);
        
        [DisplayName("STOCH SELL PRICE")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double STOCH_SELL_PRICE { get; set; } = default(double);

        [DisplayName("52 Week Hi")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double YEAR_HI { get; set; } = default(double);
        [DisplayName("52 Week Lo")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double YEAR_LO { get; set; } = default(double);
        [DisplayName("Lifetime Hi")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double LIFETIME_HIGH { get; set; } = default(double);
        [DisplayName("Lifetime Lo")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double LIFETIME_LOW { get; set; } = default(double);
        [DisplayName("LT 67% On")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime LESSTHAN_67PCT_ON { get; set; } = default(DateTime);
        [DisplayName("%Change From Year Hi")]
        [DisplayFormat(DataFormatString = "{0:0.00}%", ApplyFormatInEditMode = true)]
        public double DIFF_FROM_YEAR_HI { get; set; } = default(double);
        [DisplayName("%Change From Lifetime Hi")]
        [DisplayFormat(DataFormatString = "{0:0.00}%", ApplyFormatInEditMode = true)]
        public double DIFF_FROM_LIFETIME_HIGH { get; set; } = default(double);
        public DateTime HI_LOW_67_50_LastUpDt { get; set; } = default(DateTime);
        public DateTime SMA_LastUpDt { get; set; } = default(DateTime);
        public DateTime RSI_LastUpDt { get; set; } = default(DateTime);
        public DateTime STOCH_LastUpDt { get; set; } = default(DateTime);
        public DateTime BULL_ENGULF_LastUpDt { get; set; } = default(DateTime);
        public DateTime BEAR_ENGULF_LastUpDt { get; set; } = default(DateTime);
        public DateTime V20_LastUpDt { get; set; } = default(DateTime);
        public DateTime SMA_BUYSELL_LastUpDt { get; set; } = default(DateTime);
        public DateTime STOCH_BUYSELL_LastUpDt { get; set; } = default(DateTime);
        public DateTime RSI_TREND_LastUpDt { get; set; } = default(DateTime);

        public ICollection<V20_CANDLE_STRATEGY> collection_V20_buysell { get; set; }

        public IList<PORTFOLIOTXN> collectionTxn { get; set; }

        public ICollection<StockPriceHistory> collectionStockPriceHistory { get; set;}
        //public List<PortfolioTxn> portfolioTxns { get; set; }
        public ICollection<BULLISH_ENGULFING_STRATEGY> collectionBullishEngulfing { get; set; }
        public ICollection<BEARISH_ENGULFING> collectionBearishEngulfing { get; set; }
    }
}
