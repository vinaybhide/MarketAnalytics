using System.ComponentModel.DataAnnotations;

namespace MarketAnalytics.Models
{
    public class PORTFOLIO
    {
        [Key]
        public int PORTFOLIO_ID { get; set; }

        [Required]
        public DateTime PURCHASE_DATE { get; set; } = default(DateTime);

        public int QUANTITY { get; set; } = default(int);

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double COST_PER_SHARE { get; set; } = default(double);

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double TOTAL_COST { get; set; } = default(double);
        public double? CMP { get; set; } = default(double);

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? VALUE { get; set; } = default(double);

        //[DisplayName(displayName: "StockMaster")]
        //public virtual int StockMasterID { get; set; }
        public int StockMasterID { get; set; }

        //[ForeignKey("StockMasterID")]
        //public virtual StockMaster StockMaster { get; set; }
        public StockMaster StockMaster { get; set; }
    }
}
