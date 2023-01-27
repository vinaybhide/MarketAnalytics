using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Data;
using MarketAnalytics.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MarketAnalytics.Pages.PortfolioPages
{
    public class PortfolioTxnIndex : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public List<SelectListItem> symbolList { get; set; }
        public List<SelectListItem> menuList { get; set; }
        public List<SelectListItem> summarymenuList { get; set; }

        public PortfolioTxnIndex(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            symbolList = new List<SelectListItem>();
            menuList = new List<SelectListItem>();
            summarymenuList = new List<SelectListItem> { };
        }

        public string DateSort { get; set; }
        public string ExchangeSort { get; set; }
        public string SymbolSort { get; set; }
        public string CompNameSort { get; set; }
        [BindProperty]
        public string CurrentFilter { get; set; }
        [BindProperty]
        public string CurrentSort { get; set; }
        [BindProperty]
        public int? CurrentPageIndex { get; set; }

        [BindProperty]
        public int? CurrentSummaryPageIndex { get; set; }

        [BindProperty]
        public int? CurrentClosedPageIndex { get; set; }

        public bool RefreshAllStocks { get; set; } = false;
        public PaginatedList<PORTFOLIOTXN_SUMMARY> portfolioSummaryOpenTxn { get; set; } = default!;
        public PaginatedList<PORTFOLIOTXN> portfolioOpenTxn { get; set; } = default!;
        public PaginatedList<PORTFOLIOTXN> portfolioClosedTxn { get; set; } = default!;

        public string portfolioMasterName { get; set; } = string.Empty;
        public double portfolioTotalCost { get; set; } = default(double);
        public double portfolioTotalGain { get; set; } = default(double);
        public double portfolioTotalValue { get; set; } = default(double);

        [BindProperty]
        public int MasterId { get; set; }
        [BindProperty]
        public int? StockId { get; set; }
        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageSummaryIndex, int? pageIndex,
            int? pageIndexClosed, int? masterid, bool? refreshAll, bool? getQuote, int? stockid, bool? updateBuySell, bool? lifetimeHighLow)
        {
            if ((_context.PORTFOLIOTXN != null) && (masterid != null))
            {
                MasterId = (int)masterid;

                //create ordered list of transaction based on stock master id
                IQueryable<PORTFOLIOTXN> txnSummaryOpenIQ = _context.PORTFOLIOTXN.Where(x => (x.PORTFOLIO_MASTER_ID == masterid)
                                                            && (x.TXN_TYPE.Equals("B"))).OrderBy(a => a.StockMasterID);

                //create summary list for each symbol using all transactions for that symbol
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

                summarymenuList.Clear();
                SelectListItem summarymenuItem = new SelectListItem("Show Txn", "0");
                summarymenuList.Add(summarymenuItem);

                menuList.Clear();
                int listCntr = -1;
                //SelectListItem menuItem = new SelectListItem("-- Select Action --", listCntr.ToString());
                //menuList.Add(menuItem);

                listCntr++;
                SelectListItem menuItem = new SelectListItem("Close Position", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("Edit Transaction", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("Delete Transaction", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("Details", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("Update (Quote, Strategies, High/Low)", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("History Table", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("Chart-History", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("Chart-SMA/RSI/STOCH", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("SMA V40 Strategy", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("V20 Strategy", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("Bullish Engulfing Strategy", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("Bearish Engulfing STrategy", listCntr.ToString());
                menuList.Add(menuItem);

                var masterRec = await _context.PORTFOLIO_MASTER.FirstOrDefaultAsync(m => (m.PORTFOLIO_MASTER_ID == masterid));
                portfolioMasterName = masterRec.PORTFOLIO_NAME;

                IQueryable<PORTFOLIOTXN> txnClosedIQ = _context.PORTFOLIOTXN.Where(x => (x.PORTFOLIO_MASTER_ID == masterid) && (x.TXN_TYPE.Equals("S")));

                //IQueryable<PORTFOLIOTXN> txnOpenIQ = _context.PORTFOLIOTXN.Where(x => (x.PORTFOLIO_MASTER_ID == masterid) && (x.TXN_TYPE.Equals("B")));

                //IQueryable<PORTFOLIOTXN> distinctOpenIQ = txnOpenIQ.GroupBy(a => a.stockMaster.Symbol)
                //                                            .Select(x => x.FirstOrDefault());
                symbolList.Clear();

                //symbolList = distinctIQ.Select(a =>
                //                                new SelectListItem
                //                                {
                //                                    Value = a.StockMasterID.ToString(),
                //                                    Text = a.stockMaster.Symbol
                //                                }).AsEnumerable().ToList();

                //symbolList = _context.PORTFOLIOTXN.Where(x => (x.PORTFOLIO_MASTER_ID == MasterId) && (x.TXN_TYPE.Equals("B")))
                //                                      .OrderBy(a => a.stockMaster.Symbol)
                //                                      .Select(a =>
                //                                          new SelectListItem
                //                                          {
                //                                              Value = a.StockMasterID.ToString(),
                //                                              Text = a.stockMaster.Symbol
                //                                          }
                //                                      ).Distinct().ToList();

                symbolList = listofSummaryTxn.OrderBy(a => a.Symbol)
                                                        .Select(a =>
                                                          new SelectListItem
                                                          {
                                                              Value = a.StockMasterId.ToString(),
                                                              Text = a.Symbol
                                                          }
                                                      ).ToList();

                CurrentSort = sortOrder;
                DateSort = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
                SymbolSort = sortOrder == "Symbol" ? "symbol_desc" : "Symbol";
                ExchangeSort = sortOrder == "Exchange" ? "exchange_desc" : "Exchange";
                CompNameSort = sortOrder == "CompName" ? "compname_desc" : "CompName";
                if (searchString != null)
                {
                    pageIndex = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                IQueryable<PORTFOLIOTXN> txnOpenIQ = null; // _context.PORTFOLIOTXN.Where(x => (x.PORTFOLIO_MASTER_ID == masterid) && (x.TXN_TYPE.Equals("B")));

                if (stockid != null)
                {
                    StockId = stockid;
                    txnOpenIQ = _context.PORTFOLIOTXN.Where(x => (x.PORTFOLIO_MASTER_ID == masterid) && (x.TXN_TYPE.Equals("B")) && (x.StockMasterID == stockid));
                    var selectedRecord = txnOpenIQ.FirstOrDefault(m => (m.StockMasterID == stockid));
                    if (selectedRecord != null)
                    {
                        DbInitializer.GetQuoteAndUpdateAllPortfolioTxn(_context, selectedRecord);
                        //searchString = selectedRecord.stockMaster.Symbol;
                    }
                }

                if ((refreshAll != null) && (refreshAll == true))
                {
                    IQueryable<PORTFOLIOTXN> distinctOpenIQ = txnSummaryOpenIQ.GroupBy(a => a.stockMaster.Symbol)
                                                                .Select(x => x.FirstOrDefault());
                    foreach (var item in distinctOpenIQ)
                    {
                        DbInitializer.GetQuoteAndUpdateAllPortfolioTxn(_context, item);
                    }
                }

                portfolioTotalCost = listofSummaryTxn.Sum(a => a.TotalCost); //txnOpenIQ.Sum(a => a.TOTAL_COST);
                portfolioTotalGain = (double)listofSummaryTxn.Sum(a => a.TotalGain); //(double)txnOpenIQ.Sum(a => a.GAIN_AMT);
                portfolioTotalValue = (double)listofSummaryTxn.Sum(a => a.TotalValue); //(double)txnOpenIQ.Sum(a => a.VALUE);

                CurrentFilter = searchString;

                //IQueryable<PortfolioTxn> portfolioIQ = from s in _context.PORTFOLIOTxn select s;

                if (!String.IsNullOrEmpty(searchString))
                {
                    //txnOpenIQ = txnOpenIQ.Where(s => s.stockMaster.Symbol.ToUpper().Contains(searchString.ToUpper())
                    //                            || s.stockMaster.CompName.ToUpper().Contains(searchString.ToUpper()));
                    listofSummaryTxn = listofSummaryTxn.Where(s => s.Symbol.ToUpper().Contains(searchString.ToUpper())
                                                || s.CompName.ToUpper().Contains(searchString.ToUpper()));
                    var searchRecord = listofSummaryTxn.FirstOrDefault();
                    if (searchRecord == null)
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
                                                TotalGainPCT = x.Sum(s => s.GAIN_PCT),
                                                CMP = x.First().CMP,
                                                TotalValue = x.Sum(s => s.VALUE)
                                            }
                                            );
                    }
                    else if (symbolList.Exists(a => (a.Value.Equals(searchRecord.StockMasterId.ToString()) == true)))
                    {
                        symbolList.FirstOrDefault(a => a.Value.Equals(searchRecord.StockMasterId.ToString())).Selected = true;
                    }

                }
                switch (sortOrder)
                {
                    case "date_desc":
                        if (txnOpenIQ != null)
                        {
                            txnOpenIQ = txnOpenIQ.OrderByDescending(s => s.TXN_BUY_DATE);
                        }
                        break;
                    case "Symbol":
                        //txnOpenIQ = txnOpenIQ.OrderBy(s => s.stockMaster.Symbol);
                        listofSummaryTxn = listofSummaryTxn.OrderBy(s => s.Symbol);
                        break;
                    case "symbol_desc":
                        //txnOpenIQ = txnOpenIQ.OrderByDescending(s => s.stockMaster.Symbol);
                        listofSummaryTxn = listofSummaryTxn.OrderByDescending(s => s.Symbol);
                        break;
                    case "Exchange":
                        if (txnOpenIQ != null)
                        {
                            txnOpenIQ = txnOpenIQ.OrderBy(s => s.stockMaster.Exchange);
                        }
                        break;
                    case "exchange_desc":
                        if (txnOpenIQ != null)
                        {
                            txnOpenIQ = txnOpenIQ.OrderByDescending(s => s.stockMaster.Exchange);
                        }
                        break;
                    case "CompName":
                        if (txnOpenIQ != null)
                        {
                            txnOpenIQ = txnOpenIQ.OrderBy(s => s.stockMaster.CompName);
                        }
                        break;
                    case "compname_desc":
                        if (txnOpenIQ != null)
                        {
                            txnOpenIQ = txnOpenIQ.OrderByDescending(s => s.stockMaster.CompName);
                        }
                        break;
                    default:
                        if (txnOpenIQ != null)
                        {
                            txnOpenIQ = txnOpenIQ.OrderBy(s => s.TXN_BUY_DATE);
                        }
                        break;
                }
                var pageSize = Configuration.GetValue("PageSize", 10);

                CurrentSummaryPageIndex = pageSummaryIndex;
                portfolioSummaryOpenTxn = await PaginatedList<PORTFOLIOTXN_SUMMARY>.CreateAsync(listofSummaryTxn.AsNoTracking(), pageSummaryIndex ?? 1, pageSize);

                if (txnOpenIQ != null)
                {

                    CurrentPageIndex = pageIndex;
                    portfolioOpenTxn = await PaginatedList<PORTFOLIOTXN>.CreateAsync(txnOpenIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
                }

                CurrentClosedPageIndex = pageIndexClosed;
                portfolioClosedTxn = await PaginatedList<PORTFOLIOTXN>.CreateAsync(txnClosedIQ.AsNoTracking(), pageIndexClosed ?? 1, pageSize);
            }
        }

        public IActionResult OnPostSummaryAction(string summarymenuitemsel, int? masterid, int? stockid, int? pageSummaryIndex, int? pageIndex, int? pageClosedIndex, string sortOrder, string currentFilter)
        {
            if ((summarymenuitemsel.Equals("-1") == false) && (masterid != null) && (stockid != null))
            {
                switch (summarymenuitemsel)
                {
                    case "0"://case of show all txn
                        return RedirectToPage("./portfolioTxnIndex", new
                        {
                            sortOrder = sortOrder,
                            currentFilter = currentFilter,
                            pageSummaryIndex = pageSummaryIndex,
                            pageIndex = pageIndex,
                            masterid = masterid,
                            pageClosedIndex = pageClosedIndex,
                            getQuote = "true",
                            stockid = stockid,
                            refreshAll = "false",
                            lifetimeHighLow = "false"
                        });
                    default:
                        return RedirectToPage("./portfolioTxnIndex", new
                        {
                            sortOrder = sortOrder,
                            currentFilter = currentFilter,
                            pageSummaryIndex = pageSummaryIndex,
                            pageIndex = pageIndex,
                            masterid = masterid,
                            pageClosedIndex = pageClosedIndex,
                            getQuote = "false",
                            refreshAll = "false",
                            lifetimeHighLow = "false"
                        });
                }
            }
            return RedirectToPage("./portfolioTxnIndex", new
            {
                sortOrder = sortOrder,
                currentFilter = currentFilter,
                pageSummaryIndex = pageSummaryIndex,
                pageIndex = pageIndex,
                masterid = masterid,
                pageClosedIndex = pageClosedIndex,
                getQuote = "false",
                refreshAll = "false",
                lifetimeHighLow = "false"
            });

        }
        public IActionResult OnPostTransactionAction(string menuitemsel, int? masterid, int? txnid, int? stockid, int? pageSummaryIndex, int? pageIndex, int? pageClosedIndex, string sortOrder, string currentFilter)
        {
            if ((menuitemsel.Equals("-1") == false) && (txnid != null) && (masterid != null) && (stockid != null))
            {
                //PORTFOLIOTXN currentTxn = _context.PORTFOLIOTXN.FirstOrDefault(t => t.PORTFOLIOTXN_ID== txnid);
                switch (menuitemsel)
                {
                    case "0"://case of sell txn
                        return RedirectToPage("./portfolioTxnCreate", new
                        {
                            txntype = "S",
                            masterid = masterid,
                            txnid = txnid,
                            stockid = stockid,
                            sortOrder = sortOrder,
                            pageIndex = pageIndex,
                            pageClosedIndex = pageClosedIndex,
                            currentFilter = currentFilter
                        });

                    case "1"://case of edit txn
                        return RedirectToPage("./portfolioTxnEdit", new { masterid = masterid, txnid = txnid, stockid = stockid, sortOrder = sortOrder, pageIndex = pageIndex, pageClosedIndex = pageClosedIndex, currentFilter = currentFilter, getQuote = "false", refreshAll = "false", lifetimeHighLow = "false" });
                    case "2"://case of delete txn
                        return RedirectToPage("./portfolioTxnDelete", new { masterid = masterid, txnid = txnid, stockid = stockid, sortOrder = sortOrder, pageIndex = pageIndex, pageClosedIndex = pageClosedIndex, currentFilter = currentFilter, getQuote = "false", refreshAll = "false", lifetimeHighLow = "false" });
                    case "3"://case of txn details
                        return RedirectToPage("./portfolioTxnDetails", new { masterid = masterid, txnid = txnid, stockid = stockid, sortOrder = sortOrder, pageIndex = pageIndex, pageClosedIndex = pageClosedIndex, currentFilter = currentFilter, getQuote = "false", refreshAll = "false", lifetimeHighLow = "false" });
                    case "4"://case of update get quote, buy sell, high low
                        return RedirectToPage("./portfolioTxnIndex", new { masterid = masterid, txnid = txnid, stockid = stockid, sortOrder = sortOrder, pageIndex = pageIndex, pageClosedIndex = pageClosedIndex, currentFilter = currentFilter, getQuote = "true", refreshAll = "false", lifetimeHighLow = "false" });
                    case "5"://case of show history table
                        return RedirectToPage("/History/Index", new { id = stockid });
                    case "6"://case of show history chart
                        return RedirectToPage("/StandardIndicators/chartHistory", new { stockid = stockid, onlyhistory = 0, history = true });
                    case "7"://case of show smarsistoch chart
                        return RedirectToPage("/StandardIndicators/chartSMARSISTOCH", new { id = stockid });
                    case "8": //case of strategy SMA
                        return RedirectToPage("/BuySell/smav40", new { symbolToUpdate = stockid });
                    case "9": //case of strategy V20
                        return RedirectToPage("/BuySell/v20BuySell", new { symbolToUpdate = stockid });
                    case "10": //case of strategy Bullinsh engulfing
                        return RedirectToPage("/BuySell/BullishEngulfing", new { symbolToUpdate = stockid });
                    case "11": //case of strategy Bearish Engulfing
                        return RedirectToPage("/BuySell/BearishEngulfing", new { symbolToUpdate = stockid });

                    default:
                        return RedirectToPage("./portfolioTxnIndex", new { masterid = masterid, sortOrder = sortOrder, pageIndex = pageIndex, pageClosedIndex = pageClosedIndex, currentFilter = currentFilter, getQuote = "false", refreshAll = "false", lifetimeHighLow = "false" });
                }
            }
            return RedirectToPage("./portfolioTxnIndex", new { masterid = masterid, txnid = txnid, stockid = stockid, sortOrder = sortOrder, pageIndex = pageIndex, pageClosedIndex = pageClosedIndex, currentFilter = currentFilter, getQuote = "true", refreshAll = "false", lifetimeHighLow = "false" });
        }
        
    }
}
