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
        public List<SelectListItem> symbolList { get; set; }

        public List<classEntityValuationGraphItems> graphItems;

        public IQueryable<StockPriceHistory> iqHistory;
        public IQueryable<StockPriceHistory> iqIndexHistory;
        public IQueryable<PORTFOLIOTXN> iqPurchaseTxn;

        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public int? CurrentIndex { get; set; }
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
            symbolList = new List<SelectListItem>();
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

                IQueryable<PORTFOLIOTXN> txnIQ = _context.PORTFOLIOTXN
                                                            .Include(a => a.stockMaster)
                                                            .AsSplitQuery()
                                                            .Where(x => (x.PORTFOLIO_MASTER_ID == masterid)
                                                                && (x.TXN_TYPE.Equals("B")))
                                                            .OrderBy(a => a.StockMasterID);
                symbolList.Clear();
                symbolList = txnIQ.GroupBy(x => x.StockMasterID)
                                        .Select(a =>
                                                new SelectListItem
                                                {
                                                    Value = a.First().stockMaster.StockMasterID.ToString(),
                                                    Text = a.First().stockMaster.Symbol + " (" + a.First().stockMaster.CompName + ")"
                                                }).ToList();
                symbolList.FirstOrDefault(a => a.Value.Equals(stockid.ToString())).Selected = true;
                if (string.IsNullOrEmpty(fromDate) == false)
                {
                    FromDate = System.Convert.ToDateTime(fromDate).Date;
                }
                else
                {
                    FromDate = DateTime.MinValue;
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
                CurrentIndex = selectedindex;
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
                            .Where(x => (x.PORTFOLIO_MASTER_ID == MasterID) && (x.TXN_TYPE.Equals("B")) && (x.StockMasterID == CurrentID)).OrderBy(x => x.TXN_BUY_DATE);
                if (string.IsNullOrEmpty(quantity))
                {
                    QuantityHeld = iqPurchaseTxn.Sum(s => s.PURCHASE_QUANTITY);
                }
                else
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
                DateTime firstTxnDate = iqPurchaseTxn.FirstOrDefault().TXN_BUY_DATE;
                string firstPurchaseDate = firstTxnDate.ToString("yyyy-MM-dd");//iqPurchaseTxn.FirstOrDefault().TXN_BUY_DATE.ToString("yyyy-MM-dd");
                //from given date or from the begining, get price history for given stock & also get the index price
                string lastPriceDate = DbInitializer.IsHistoryUpdated(_context, tempSM, bForceUpdate: true, firstPurchaseDate);
                if (string.IsNullOrEmpty(lastPriceDate) == false)
                {
                    DbInitializer.InitializeHistory(_context, tempSM, lastPriceDate);
                }

                StockMaster indexRec = _context.StockMaster.Where(a => a.StockMasterID == CurrentIndex).FirstOrDefault();
                if (indexRec != null)
                {
                    lastPriceDate = DbInitializer.IsHistoryUpdated(_context, indexRec, bForceUpdate: true, firstPurchaseDate);
                    if (string.IsNullOrEmpty(lastPriceDate) == false)
                    {
                        DbInitializer.InitializeHistory(_context, indexRec, lastPriceDate);
                    }
                }

                if (FromDate != DateTime.MinValue)
                {
                    iqHistory = _context.StockPriceHistory
                        //.Include(a => a.StockMaster)
                        .AsSplitQuery().Where(a => (a.StockMasterID == CurrentID) && (a.PriceDate.Date.CompareTo(FromDate.Date) >= 0)).OrderBy(a => a.PriceDate);

                    if ((selectedindex != null) && (selectedindex != -1))
                    {
                        iqIndexHistory = _context.StockPriceHistory
                        //.Include(a => a.StockMaster)
                        .AsSplitQuery().Where(a => (a.StockMasterID == selectedindex) && (a.PriceDate.Date.CompareTo(FromDate.Date) >= 0)).OrderBy(a => a.PriceDate);
                    }
                }
                else
                {
                    iqHistory = _context.StockPriceHistory
                        //.Include(a => a.StockMaster)
                        //.AsSplitQuery().Where(a => a.StockMasterID == CurrentID).OrderBy(a => a.PriceDate);
                        .AsSplitQuery().Where(a => (a.StockMasterID == CurrentID) && (a.PriceDate.Date.CompareTo(firstTxnDate.Date) >=0)).OrderBy(a => a.PriceDate);
                    if ((selectedindex != null) && (selectedindex != -1))
                    {
                        iqIndexHistory = _context.StockPriceHistory
                        //.Include(a => a.StockMaster)
                        .AsSplitQuery().Where(a => (a.StockMasterID == selectedindex) && (a.PriceDate.Date.CompareTo(firstTxnDate.Date) >=0)).OrderBy(a => a.PriceDate);
                    }
                }

                DateTime firstHistoryDate = iqHistory.FirstOrDefault().PriceDate;
                DateTime firstIndexHistoryDate = iqIndexHistory.FirstOrDefault().PriceDate;
                IQueryable<PORTFOLIOTXN> txnSummaryOpenIQ = null;
                IQueryable<PORTFOLIOTXN_SUMMARY> listofSummaryTxn = null;
                StockPriceHistory indexHistoryItem;

                if (firstTxnDate.Date.CompareTo(firstHistoryDate.Date) < 0)
                {
                    // mens we do not have history records from first transaction
                    txnSummaryOpenIQ = _context.PORTFOLIOTXN.Include(a => a.stockMaster).AsSplitQuery()
                            .Where(x => (x.PORTFOLIO_MASTER_ID == MasterID) &&
                                        (x.TXN_TYPE.Equals("B")) &&
                                        (x.StockMasterID == CurrentID) &&
                                        (x.TXN_BUY_DATE.Date.CompareTo(firstHistoryDate.Date) < 0)
                                        );
                    foreach (var txn in txnSummaryOpenIQ)
                    {
                        classEntityValuationGraphItems currentgraphItem = new classEntityValuationGraphItems();
                        currentgraphItem.historyDate = txn.TXN_BUY_DATE;
                        currentgraphItem.historyPrice = txn.COST_PER_UNIT;
                        indexHistoryItem = iqIndexHistory
                            .Where(a => ((a.PriceDate == txn.TXN_BUY_DATE) && (a.StockMasterID == selectedindex)))
                            .FirstOrDefault();
                        if (indexHistoryItem != null)
                        {
                            currentgraphItem.indexClose = indexHistoryItem.Close;
                        }
                        currentgraphItem.totalPurchaseQty = QuantityHeld;
                        currentgraphItem.totalValuation = System.Convert.ToDouble(string.Format("{0:0.00}", QuantityHeld * txn.COST_PER_UNIT));
                        currentgraphItem.tipTotalValuation = "Date: " + txn.TXN_BUY_DATE.ToShortDateString() + " Valuation = " + currentgraphItem.totalValuation + " (Quantity: " + QuantityHeld.ToString() + " and Price: " + txn.COST_PER_UNIT + ")";
                        currentgraphItem.txnTotalPurchaseQty = txn.PURCHASE_QUANTITY;
                        currentgraphItem.txnTotalPurchaseCost = txn.TOTAL_COST;
                        currentgraphItem.txnTotalGain = txn.GAIN_AMT;
                        currentgraphItem.txnTotalGainPCT = txn.GAIN_PCT;
                        currentgraphItem.txnTotalValuation = txn.VALUE;

                        currentgraphItem.tipTxnTotalPurchaseCost = "Purchase Date: " + txn.TXN_BUY_DATE +
                            " QTY: " + currentgraphItem.txnTotalPurchaseQty +
                            " Cost: " + currentgraphItem.txnTotalPurchaseCost;

                        currentgraphItem.tipTxnTotalValuation = "Purchase Date: " + txn.TXN_BUY_DATE +
                            " QTY: " + currentgraphItem.txnTotalPurchaseQty +
                            " Value: " + currentgraphItem.txnTotalValuation +
                            " Gain: " + currentgraphItem.txnTotalGain +
                            " %Gain: " + currentgraphItem.txnTotalGainPCT;
                        graphItems.Add(currentgraphItem);
                    }
                }
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

                    currentgraphItem.totalPurchaseQty = _context.PORTFOLIOTXN.Where(x => (x.PORTFOLIO_MASTER_ID == MasterID) &&
                                (x.TXN_TYPE.Equals("B")) &&
                                (x.StockMasterID == CurrentID) &&
                                (x.TXN_BUY_DATE.Date.CompareTo(historyItem.PriceDate.Date) <= 0)
                                ).Sum(x => x.PURCHASE_QUANTITY);

                    //currentgraphItem.totalPurchaseQty = QuantityHeld;
                    //currentgraphItem.totalValuation = System.Convert.ToDouble(string.Format("{0:0.00}", QuantityHeld * historyItem.Close));
                    //currentgraphItem.tipTotalValuation = "Date: " + historyItem.PriceDate.ToShortDateString() + " Valuation = " + currentgraphItem.totalValuation + " (Quantity: " + QuantityHeld.ToString() + " and Price: " + historyItem.Close + ")";
                    currentgraphItem.totalValuation = System.Convert.ToDouble(string.Format("{0:0.00}", currentgraphItem.totalPurchaseQty * historyItem.Close));
                    currentgraphItem.tipTotalValuation = "Date: " + historyItem.PriceDate.ToShortDateString() + " Valuation = " + currentgraphItem.totalValuation + " (Quantity: " + currentgraphItem.totalPurchaseQty.ToString() + " and Price: " + historyItem.Close + ")";

                    //now get all transactions for the current date and if found do the sum
                    txnSummaryOpenIQ = _context.PORTFOLIOTXN.Include(a => a.stockMaster).AsSplitQuery()
                    .Where(x => (x.PORTFOLIO_MASTER_ID == MasterID) &&
                                (x.TXN_TYPE.Equals("B")) &&
                                (x.StockMasterID == CurrentID) &&
                                (x.TXN_BUY_DATE.Date.CompareTo(historyItem.PriceDate.Date) == 0)
                                );
                    if ((txnSummaryOpenIQ != null) && (txnSummaryOpenIQ.Count() > 0))
                    {
                        listofSummaryTxn = txnSummaryOpenIQ.GroupBy(x => x.StockMasterID)
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

                                currentgraphItem.txnTotalPurchaseQty = Math.Round(currentSummary.TotalQty, 2);
                                currentgraphItem.txnTotalPurchaseCost = Math.Round(currentSummary.TotalCost,2) ;
                                currentgraphItem.txnTotalGain = Math.Round((double)currentSummary.TotalGain, 2);
                                currentgraphItem.txnTotalGainPCT = Math.Round((double)currentSummary.TotalGainPCT, 2);
                                currentgraphItem.txnTotalValuation = Math.Round((double)currentSummary.TotalValue,2);

                                currentgraphItem.tipTxnTotalPurchaseCost = "Investment - Purchase Date: " + historyItem.PriceDate +
                                    " QTY: " + currentgraphItem.txnTotalPurchaseQty +
                                    " Cost: " + currentgraphItem.txnTotalPurchaseCost;

                                currentgraphItem.tipTxnTotalValuation = "Valuation On - Purchase Date: " + historyItem.PriceDate +
                                    " QTY: " + currentgraphItem.txnTotalPurchaseQty +
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
        public string tipTxnTotalValuation { get; set; } = null;
    }
}
