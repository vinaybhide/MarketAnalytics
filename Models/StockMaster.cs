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
        [DisplayName("SMA20")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double SMA_FAST { get; set; } = default(double);
        [DisplayName("SMA50")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double SMA_MID { get; set; } = default(double);
        [DisplayName("SMA200")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)] 
        public double SMA_SLOW { get; set; } = default(double);

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double LIFETIME_HIGH { get; set; } = default(double);

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double LIFETIME_LOW { get; set; } = default(double);
        public ICollection<V20_CANDLE_STRATEGY> collection_V20_buysell { get; set; }

        public ICollection<PORTFOLIOTXN> collectionTxn { get; set; }
        //public List<PortfolioTxn> portfolioTxns { get; set; }
        //public ICollection<BULLISH_ENGULFING_STRATEGY> collectionEngulfing { get; set; }
    }
}
