using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace MarketAnalytics.Models
{
    public class V20_CANDLE_STRATEGY
    {
        [Key]
        public int V20_CANDLE_STRATEGY_ID { get; set; }
        
        [Required]
        [DisplayName("From")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime FROM_DATE { get; set; } = default(DateTime);

        [Required]
        [DisplayName("To")]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime TO_DATE { get; set; } = default(DateTime);
        [Required]
        [DisplayName("DiffPct")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double DIFF_PCT { get; set; } = default(double);

        [Required]
        public double BUY_PRICE { get; set; } = default(double);
        [Required]
        public double SELL_PRICE { get; set; } = default(double);

        public int StockMasterID { get; set; }
        public StockMaster StockMaster { get; set; }
    }
}
