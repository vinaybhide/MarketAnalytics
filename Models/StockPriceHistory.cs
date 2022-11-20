using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketAnalytics.Models
{
    public class StockPriceHistory
    {
        [Key]
        public int StockPriceHistoryID { get; set; }

        [Required]
        public DateTime PriceDate { get; set; } = default(DateTime);

        public double Open { get; set; } = default(double);
        public double High { get; set; } = default(double);
        public double Low { get; set; } = default(double);
        public double Close { get; set; } = default(double);
        public double Volume { get; set; } = default(double);
        public double Change { get; set; } = default(double);
        public double ChangePercent { get; set; } = default(double);
        public double PrevClose { get; set; } = default(double);

        public double? RSI_OPEN { get; set; } = default(double);
        public double? RSI_CLOSE{ get; set; } = default(double);
        public double? RSI_HIGH { get; set; } = default(double);
        public double? RSI_LOW { get; set; } = default(double);

        public double? SMA_SMALL { get; set; } = default(double);
        public double? SMA_MID { get; set; } = default(double);
        public double? SMA_LONG { get; set; } = default(double);
        public string CROSSOVER_FLAG { get; set; } = "LT";
        public bool LOWER_THAN_SMA_SMALL { get; set; } = false;

        public bool BULLISH_ENGULFING { get; set; } = false;
        public double? BUY_SMA_STRATEGY { get; set; } = default(double);
        public double? SELL_SMA_STRATEGY { get; set; } = default(double);

        public bool V20_CANDLE { get; set; } = false;
        public double? V20_CANDLE_BUY_PRICE { get; set; } = default(double);
        public double? V20_CANDLE_SELL_PRICE { get; set; } = default(double);

        //[DisplayName(displayName:  "StockMaster")]
        //public virtual int StockMasterID { get; set; }
        public int StockMasterID { get; set; }

        //[ForeignKey("StockMasterID")]
        //public virtual StockMaster StockMaster { get; set; }
        public StockMaster StockMaster { get; set; }
    }
}
