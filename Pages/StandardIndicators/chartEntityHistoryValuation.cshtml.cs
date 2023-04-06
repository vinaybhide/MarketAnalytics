using MarketAnalytics.Data;
using MarketAnalytics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Model;
using Microsoft.CodeAnalysis.Text;
//using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MarketAnalytics.Pages.StandardIndicators
{
    public class chartEntityHistoryValuation : PageModel
    {
        public List<SelectListItem> indexList;
        public List<classEntityValuationGraphItems> graphItems;

        public IQueryable<StockPriceHistory> iqHistory;
        public IQueryable<StockPriceHistory> iqIndexHistory;
        public IQueryable<PORTFOLIOTXN> iqPurchaseTxn;

        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public int? CurrentID { get; set; }
        public int? MasterID { get; set; }
        public double? QuantityHeld { get; set; }
        public DateTime FromDate { get; set; }
        public string Symbol { get; set; }
        public string CompanyName { get; set; }
        public string InvestmentType { get; set; }
        public string IndexSymbol { get; set; }
        public string ChartContent { get; set; }
        public chartEntityHistoryValuation(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            indexList = new List<SelectListItem>();
            graphItems = new List<classEntityValuationGraphItems>();
        }

        //caller should send fromdate in buydate
        //selldate if not specified will be used as today or null
        public async Task OnGetAsync(int? stockid, int? masterid, string fromDate, int? selectedindex, string quantity)
        {
            if ((_context.StockPriceHistory != null) && (stockid != null) && (masterid != null))
            {
                CurrentID = stockid;
                MasterID = masterid;

                if (string.IsNullOrEmpty(fromDate) == false)
                {
                    FromDate = System.Convert.ToDateTime(fromDate).Date;
                }
                else
                {
                    FromDate = DateTime.MinValue;
                }

                if (string.IsNullOrEmpty(quantity) == false)
                {
                    try
                    {
                        QuantityHeld = double.Parse(quantity);
                    }
                    catch
                    {
                        QuantityHeld = 0;

                    }
                }

                //populate the Index list
                indexList.Clear();
                indexList = _context.StockMaster.Where(a => a.INVESTMENT_TYPE.Equals("Index")).OrderBy(a => a.Symbol)
                        .Select(a =>
                            new SelectListItem
                            {
                                Value = a.StockMasterID.ToString(),
                                Text = a.Symbol
                            }
                        ).ToList();

                if ((selectedindex == null) || (selectedindex <= 0))
                {
                    selectedindex = _context.StockMaster.Where(a => a.Symbol == "^NSEI").FirstOrDefault().StockMasterID;
                    indexList.FirstOrDefault(a => a.Text.Equals("^NSEI")).Selected = true;
                }
                indexList.FirstOrDefault(a => a.Value.Equals(selectedindex.ToString())).Selected = true;
                IndexSymbol = indexList.FirstOrDefault(a => a.Value.Equals(selectedindex.ToString())).Text;

                //get the stock master record from given stock id
                StockMaster tempSM = _context.StockMaster.AsSplitQuery().FirstOrDefault(a => a.StockMasterID == CurrentID);
                Symbol = tempSM.Symbol;
                CompanyName = tempSM.CompName;
                ChartContent = "Entity price history & valuation for ";
                InvestmentType = tempSM.INVESTMENT_TYPE;

                //get all transactions belonging to current portfolio
                iqPurchaseTxn = _context.PORTFOLIOTXN.Include(a => a.stockMaster).AsSplitQuery()
                            .Where(x => (x.PORTFOLIO_MASTER_ID == MasterID) && (x.TXN_TYPE.Equals("B")) && (x.StockMasterID == CurrentID));

                //from given date or from the begining, get price history for given stock & also get the index price
                if (FromDate != DateTime.MinValue)
                {
                    iqHistory = _context.StockPriceHistory
                        //.Include(a => a.StockMaster)
                        .AsSplitQuery().Where(a => (a.StockMasterID == CurrentID) && (a.PriceDate.Date.CompareTo(FromDate.Date) >= 0));

                    if ((selectedindex != null) && (selectedindex != -1))
                    {
                        iqIndexHistory = _context.StockPriceHistory
                        //.Include(a => a.StockMaster)
                        .AsSplitQuery().Where(a => (a.StockMasterID == selectedindex) && (a.PriceDate.Date.CompareTo(FromDate.Date) >= 0));
                    }
                }
                else
                {
                    iqHistory = _context.StockPriceHistory
                        //.Include(a => a.StockMaster)
                        .AsSplitQuery().Where(a => a.StockMasterID == CurrentID);
                    if ((selectedindex != null) && (selectedindex != -1))
                    {
                        iqIndexHistory = _context.StockPriceHistory
                        //.Include(a => a.StockMaster)
                        .AsSplitQuery().Where(a => a.StockMasterID == selectedindex);
                    }
                }

                StockPriceHistory indexHistoryItem;
                //now we have history & transaction list
                foreach (var historyItem in iqHistory)
                {
                    classEntityValuationGraphItems currentgraphItem = new classEntityValuationGraphItems();
                    currentgraphItem.historyDate = historyItem.PriceDate;
                    currentgraphItem.historyPrice = historyItem.Close;

                    indexHistoryItem = iqIndexHistory
                        .Where(a => ((a.PriceDate == historyItem.PriceDate) && (a.StockMasterID == selectedindex)))
                        .FirstOrDefault();
                    if (indexHistoryItem != null)
                    {
                        currentgraphItem.indexClose = indexHistoryItem.Close;
                    }
                    currentgraphItem.totalPurchaseQty = QuantityHeld;
                    currentgraphItem.totalValuation = System.Convert.ToDouble(string.Format("{0:0.00}", QuantityHeld * historyItem.Close));

                    currentgraphItem.tipTotalValuation = "Date: " + historyItem.PriceDate.ToShortDateString() + " Valuation = " + currentgraphItem.totalValuation + " (Quantity: " + QuantityHeld.ToString() + " and Price: " + historyItem.Close + ")";

                    //now get all transactions for the current date and if found do the sum
                    IQueryable<PORTFOLIOTXN> txnSummaryOpenIQ = _context.PORTFOLIOTXN.Include(a => a.stockMaster).AsSplitQuery()
                            .Where(x => (x.PORTFOLIO_MASTER_ID == MasterID) &&
                                        (x.TXN_TYPE.Equals("B")) &&
                                        (x.StockMasterID == CurrentID) &&
                                        (x.TXN_BUY_DATE.Date.CompareTo(historyItem.PriceDate.Date) == 0)
                                        );
                    if ((txnSummaryOpenIQ != null) && (txnSummaryOpenIQ.Count() > 0))
                    {
                        IQueryable<PORTFOLIOTXN_SUMMARY> listofSummaryTxn = txnSummaryOpenIQ.GroupBy(x => x.StockMasterID)
                                                    .Select(x => new PORTFOLIOTXN_SUMMARY
                                                    {
                                                        StockMasterId = x.Key,
                                                        MasterId = x.First().PORTFOLIO_MASTER_ID,
                                                        Symbol = x.First().stockMaster.Symbol,
                                                        CompName = x.First().stockMaster.CompName,
                                                        Exchange = x.First().stockMaster.Exchange,
                                                        TotalQty = x.Sum(s => s.PURCHASE_QUANTITY),
                                                        TotalCost = x.Sum(s => s.TOTAL_COST),
                                                        TotalGain = x.Sum(s => s.GAIN_AMT),
                                                        TotalGainPCT = x.Sum(s => s.GAIN_AMT) / x.Sum(s => s.TOTAL_COST) * 100,
                                                        CMP = x.First().CMP,
                                                        TotalValue = x.Sum(s => s.VALUE)
                                                    }
                                                );
                        if ((listofSummaryTxn != null) && (listofSummaryTxn.Count() > 0))
                        {
                            PORTFOLIOTXN_SUMMARY currentSummary = listofSummaryTxn.FirstOrDefault();
                            if (currentSummary != null)
                            {

                                currentgraphItem.txnTotalPurchaseQty = listofSummaryTxn.FirstOrDefault().TotalQty;
                                currentgraphItem.txnTotalPurchaseCost = listofSummaryTxn.FirstOrDefault().TotalCost;
                                currentgraphItem.txnTotalGain = listofSummaryTxn.FirstOrDefault().TotalGain;
                                currentgraphItem.txnTotalGainPCT = currentSummary.TotalGainPCT;
                                currentgraphItem.txnTotalValuation = currentSummary.TotalValue;

                                currentgraphItem.tipTxnTotalPurchaseCost = "Purchase Date: " + historyItem.PriceDate + 
                                    " QTY: " + currentgraphItem.txnTotalPurchaseQty +
                                    " Cost: " + currentgraphItem.txnTotalPurchaseCost + 
                                    " Value: " + currentgraphItem.txnTotalValuation + 
                                    " Gain: " + currentgraphItem.txnTotalGain +
                                    " %Gain: " + currentgraphItem.txnTotalGainPCT;
                            }
                        }
                    }

                    graphItems.Add(currentgraphItem);
                }
            }
        }
    }

    public class classEntityValuationGraphItems
    {
        public DateTime historyDate { get; set; }
        public double? historyPrice { get; set; }
        public double? indexClose { get; set; } = null;
        public double? totalPurchaseQty { get; set; } = null;
        public double? totalValuation { get; set; } = null;
        public string tipTotalValuation { get; set; } = null;
        public double? txnTotalPurchaseQty { get; set; } = null;
        public double? txnTotalPurchaseCost { get; set; } = null;
        public string tipTxnTotalPurchaseCost { get; set; } = null;
        public double? txnTotalGain { get; set; } = null;
        public double? txnTotalGainPCT { get; set; } = null;
        public double? txnTotalValuation { get; set; } = null;
    }
}
