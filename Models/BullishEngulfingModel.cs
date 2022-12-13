using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace MarketAnalytics.Models
{
    public class BULLISH_ENGULFING_STRATEGY
    {
        [Key]
        public int BULLISH_ENGULFING_STRATEGY_ID { get; set; }
        [Required]
        [DisplayName("Buy Ref Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = false)]

        public DateTime BUY_CANDLE_DATE { get; set; } = default(DateTime);
        [Required]
        [DisplayName("Sell Ref Date")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = false)]

        public DateTime SELL_CANDLE_DATE { get; set; } = default(DateTime);

        [Required]
        public double BUY_PRICE { get; set; } = default(double);
        [Required]
        public double SELL_PRICE { get; set; } = default(double);
        [Required]
        public double AVG_BUY_PRICE { get; set; } = default(double);

        public int StockMasterID { get; set; }
        public StockMaster StockMaster { get; set; }

    }
}
