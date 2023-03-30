using MarketAnalytics.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarketAnalytics.Models
{
    public class PORTFOLIOTXN_SUMMARY
    {
        public int StockMasterId { get; set; }
        public int MasterId { get; set; }

        [DisplayName("Symbol")]
        public string Symbol { get; set; }
        public string CompName { get; set; }
        public string Exchange { get; set; }
        [DisplayName("Total QTY")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double TotalQty { get; set; }
        [DisplayName("Total Cost")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double TotalCost { get; set; }
        
        [DisplayName("Total Gain")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? TotalGain { get; set; }
        
        [DisplayName("Total Gain%")]
        public double? GAIN_PCT { get; set; } = default(double);

        [DisplayFormat(DataFormatString = "{0:0.00}%", ApplyFormatInEditMode = true)]
        public double? TotalGainPCT { get; set; }

        [DisplayName("CMP")]
        public double? CMP { get; set;}
        
        [DisplayName("Total Value")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)] public double? TotalValue { get; set; }
    }
    public class PORTFOLIOTXN
    {
        [Key]
        public int PORTFOLIOTXN_ID { get; set; }

        [DisplayFormat(DataFormatString ="{0:MM/dd/yyyy}", ApplyFormatInEditMode = false)]
        [DisplayName("Buy Date")]
        public DateTime TXN_BUY_DATE { get; set; } = default(DateTime);
        [DisplayName("TXN Type")]
        public string TXN_TYPE { get; set; }
        
        [DisplayName("BUY QTY")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double PURCHASE_QUANTITY { get; set; } = default(double);
        
        [DisplayName("Cost/Unit")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double COST_PER_UNIT { get; set; } = default(double);

        [DisplayName("Investment")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double TOTAL_COST { get; set; } = default(double);

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = false)]
        
        [DisplayName("Sell Date")]
        public DateTime TXN_SELL_DATE { get; set; } = default(DateTime);

        [DisplayName("SELL QTY")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double SELL_QUANTITY { get; set; } = default(double);

        [DisplayName("Sell Value/Unit")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double SELL_AMT_PER_UNIT { get; set; } = default(double);

        [DisplayName("Total Sell Amt")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double TOTAL_SELL_AMT { get; set; } = default(double);

        [DisplayName("Holding Days")]
        public int DAYS_SINCE { get; set; } = default(int);
        [DisplayName("Sold After")]
        public int SOLD_AFTER { get; set; } = default(int);

        [DisplayName("CMP")]
        public double? CMP { get; set; } = default(double);

        [DisplayName("Value")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? VALUE { get; set; } = default(double);

        [DisplayName("CAGR")]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double CAGR { get; set; } = default(double);

        [DisplayFormat(DataFormatString = "{0:0.00}%", ApplyFormatInEditMode = true)]
        [DisplayName("Gain%")]
        public double? GAIN_PCT { get; set; } = default(double);

        [DisplayFormat(DataFormatString = "{0:0.00}%", ApplyFormatInEditMode = true)]
        [DisplayName("%Sell Gain")]
        public double? SELL_GAIN_PCT { get; set; } = default(double);

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        [DisplayName("Gain")]
        public double? GAIN_AMT { get; set; } = default(double);

        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        [DisplayName("Sell Gain")]
        public double? SELL_GAIN_AMT { get; set; } = default(double);

        [DisplayName("Cost Vs 52Hi")]
        [DisplayFormat(DataFormatString = "{0:0.00}%", ApplyFormatInEditMode = true)]
        public double BUY_VS_52HI { get; set; } = default(double);
        public DateTime LastUpDt { get; set; } = default(DateTime);

        public int PORTFOLIO_MASTER_ID { get; set; }

        public Portfolio_Master portfolioMaster { get; set; }
        //[Key]
        public int StockMasterID { get; set; }
        public StockMaster stockMaster { get; set; }

        public StockMaster GetStockMaster(DBContext context)
        {
            StockMaster stockMaster = null;
            try
            {
                if (this.stockMaster == null)
                {
                    stockMaster = context.StockMaster.Where(s => s.StockMasterID == this.StockMasterID).FirstOrDefault();//Find(this.StockMasterID);//.First(s => s.StockMasterID == this.StockMasterID);
                }
                else
                {
                    return this.stockMaster;
                }
            }
            catch(Exception ex)
            {
                stockMaster= null;
            }
            return stockMaster;
        }
    }
}
