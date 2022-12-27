using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketAnalytics.Models
{
    public class PORTFOLIOTXN
    {
        [Key]
        public int PORTFOLIOTXN_ID { get; set; }

        [DisplayFormat(DataFormatString ="{0:MM/dd/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime PURCHASE_DATE { get; set; } = default(DateTime);

        public int QUANTITY { get; set; } = default(int);

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double COST_PER_SHARE { get; set; } = default(double);

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double TOTAL_COST { get; set; } = default(double);
        public double? CMP { get; set; } = default(double);

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? VALUE { get; set; } = default(double);
        [DisplayFormat(DataFormatString = "{0:0.00}%", ApplyFormatInEditMode = true)]
        public double? GAIN_PCT { get; set; } = default(double);
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? GAIN_AMT { get; set; } = default(double);
        public DateTime LastUpDt { get; set; } = default(DateTime);

        public int PORTFOLIO_MASTER_ID { get; set; }

        public Portfolio_Master portfolioMaster { get; set; }
        //[Key]
        public int StockMasterID { get; set; }
        public StockMaster stockMaster { get; set; }
    }
}
