using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Data;
using MarketAnalytics.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MarketAnalytics.Pages.PortfolioPages
{
    [Authorize(Roles = "Registered, Administrator")]
    public class PortfolioTxnIndex : PageModel
    {
        const string constShowTxn = "0";
        const string constGetQuote = "1";
        const string constUpdateStrategy = "2";
        const string constHistoryData = "3";
        const string constChart_History = "4";
        const string constChart_SMARSISTOCH = "5";
        const string constRsiTrendV40 = "6";
        const string constStochV40 = "7";
        const string constSmaV40 = "8";
        const string constV20 = "9";
        const string constBullishEngulfing = "10";
        const string constBearishEngulfing = "11";

        const string constClosePosition = "0";
        const string constEditTxn = "1";
        const string constDeleteTxn = "2";
        const string constDetailsTxn = "3";


        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public List<SelectListItem> symbolList { get; set; }
        public List<SelectListItem> menuList { get; set; }
        public List<SelectListItem> summarymenuList { get; set; }

        [BindProperty]
        public string UserName { get; set; }

        [BindProperty]
        public string UserId { get; set; }

        [BindProperty]
        public string DateSort { get; set; }
        [BindProperty]
        public string SymbolSort { get; set; }
        [BindProperty]
        public string ClosedSymbolSort { get; set; }
        [BindProperty]
        public string CurrentFilter { get; set; }
        [BindProperty]
        public string CurrentSummarySort { get; set; }
        [BindProperty]
        public string CurrentOpenSort { get; set; }
        [BindProperty]
        public string CurrentClosedSort { get; set; }

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

        public PortfolioTxnIndex(MarketAnalytics.Data.DBContext context, IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            Configuration = configuration;
            symbolList = new List<SelectListItem>();
            menuList = new List<SelectListItem>();
            summarymenuList = new List<SelectListItem> { };
            UserId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            UserName = httpContextAccessor.HttpContext.User.Identity.Name;
        }

        public async Task OnGetAsync(string openSortOrder, string summarySortOrder, string closedSortOrder, string currentFilter, string searchString, int? pageSummaryIndex, int? pageIndex,
            int? pageIndexClosed, int? masterid, bool? refreshAll, bool? getQuote, int? stockid, bool? updateBuySell, bool? lifetimeHighLow)
        {
            if ((_context.PORTFOLIOTXN != null) && (masterid != null))
            {
                MasterId = (int)masterid;

                var masterRec = await _context.PORTFOLIO_MASTER.AsSplitQuery().FirstOrDefaultAsync(m => (m.PORTFOLIO_MASTER_ID == masterid));
                portfolioMasterName = masterRec.PORTFOLIO_NAME;
                if (masterRec.collectionTxn.Any())
                {
                    //create ordered list of transaction based on stock master id
                    IQueryable<PORTFOLIOTXN> txnOpenIQ = null;
                    if (stockid != null)
                    {
                        StockId = stockid;
                        txnOpenIQ = _context.PORTFOLIOTXN.Include(a => a.stockMaster).AsSplitQuery()
                            .Where(x => (x.PORTFOLIO_MASTER_ID == masterid) && (x.TXN_TYPE.Equals("B")) && (x.StockMasterID == stockid));
                        var selectedRecord = txnOpenIQ.FirstOrDefault(m => (m.StockMasterID == stockid));
                        if((updateBuySell != null) && (updateBuySell == true) && (selectedRecord != null))
                        {
                            DbInitializer.GetQuoteAndUpdateAllPortfolioTxn(_context, UserId, null, selectedRecord, buysell: true);
                        }
                    }
                    else if ((refreshAll != null) && (refreshAll == true))
                    {
                        DbInitializer.GetQuoteAndUpdateAllPortfolioTxn(_context, UserId, masterid, null, buysell:true);
                    }

                    IQueryable<PORTFOLIOTXN> txnSummaryOpenIQ = _context.PORTFOLIOTXN
                                                            .Include(a => a.stockMaster)
                                                            .AsSplitQuery()
                                                            .Where(x => (x.PORTFOLIO_MASTER_ID == masterid)
                                                                && (x.TXN_TYPE.Equals("B")))
                                                            .OrderBy(a => a.StockMasterID);

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
                            TotalGainPCT =  x.Sum(s => s.GAIN_AMT) / x.Sum(s => s.TOTAL_COST) * 100,
                            CMP = x.First().CMP,
                            TotalValue = x.Sum(s => s.VALUE)
                        }
                        );

                    summarymenuList.Clear();
                    SelectListItem summarymenuItem = new SelectListItem("Show Txn", constShowTxn);
                    summarymenuList.Add(summarymenuItem);

                    summarymenuItem = new SelectListItem("Get Quote", constGetQuote);
                    summarymenuList.Add(summarymenuItem);

                    summarymenuItem = new SelectListItem("Update Strategy", constUpdateStrategy);
                    summarymenuList.Add(summarymenuItem);

                    summarymenuItem = new SelectListItem("History Data", constHistoryData);
                    summarymenuList.Add(summarymenuItem);

                    summarymenuItem = new SelectListItem("Chart-History", constChart_History);
                    summarymenuList.Add(summarymenuItem);

                    summarymenuItem = new SelectListItem("Chart-SMA/RSI/STOCH", constChart_SMARSISTOCH);
                    summarymenuList.Add(summarymenuItem);

                    summarymenuItem = new SelectListItem("RSI Trend V40", constRsiTrendV40);
                    summarymenuList.Add(summarymenuItem);

                    summarymenuItem = new SelectListItem("Stoch V40", constStochV40);
                    summarymenuList.Add(summarymenuItem);

                    summarymenuItem = new SelectListItem("SMA V40", constSmaV40);
                    summarymenuList.Add(summarymenuItem);

                    summarymenuItem = new SelectListItem("V20", constV20);
                    summarymenuList.Add(summarymenuItem);

                    summarymenuItem = new SelectListItem("Bullish Engulfing", constBullishEngulfing);
                    summarymenuList.Add(summarymenuItem);

                    summarymenuItem = new SelectListItem("Bearish Engulfing", constBearishEngulfing);
                    summarymenuList.Add(summarymenuItem);


                    menuList.Clear();
                    //SelectListItem menuItem = new SelectListItem("-- Select Action --", listCntr.ToString());
                    //menuList.Add(menuItem);

                    SelectListItem menuItem = new SelectListItem("Close", constClosePosition);
                    menuList.Add(menuItem);

                    menuItem = new SelectListItem("Edit", constEditTxn);
                    menuList.Add(menuItem);

                    menuItem = new SelectListItem("Delete", constDeleteTxn);
                    menuList.Add(menuItem);

                    menuItem = new SelectListItem("Details", constDetailsTxn);
                    menuList.Add(menuItem);



                    IQueryable<PORTFOLIOTXN> txnClosedIQ = _context.PORTFOLIOTXN
                                            .Include(a => a.stockMaster)
                                            .AsSplitQuery()
                                            .Where(x => (x.PORTFOLIO_MASTER_ID == masterid) && (x.TXN_TYPE.Equals("S")));

                    symbolList.Clear();

                    symbolList = listofSummaryTxn.OrderBy(a => a.Symbol)
                                                            .Select(a =>
                                                              new SelectListItem
                                                              {
                                                                  Value = a.StockMasterId.ToString(),
                                                                  Text = a.Symbol + "." + a.Exchange
                                                              }
                                                          ).ToList();

                    CurrentOpenSort = openSortOrder;
                    DateSort = String.IsNullOrEmpty(openSortOrder) ? "date_desc" : "";
                    CurrentSummarySort = summarySortOrder;
                    SymbolSort = String.IsNullOrEmpty(summarySortOrder) ? "symbol_desc" : "";
                    CurrentClosedSort = closedSortOrder;
                    ClosedSymbolSort = String.IsNullOrEmpty(closedSortOrder) ? "closedsymbol_desc" : "";

                    //SymbolSort = sortOrder == "Symbol" ? "symbol_desc" : "Symbol";
                    //ExchangeSort = sortOrder == "Exchange" ? "exchange_desc" : "Exchange";
                    //CompNameSort = sortOrder == "CompName" ? "compname_desc" : "CompName";
                    if (searchString != null)
                    {
                        pageIndex = 1;
                    }
                    else
                    {
                        searchString = currentFilter;
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
                    switch(summarySortOrder)
                    {
                        case "symbol_desc":
                            listofSummaryTxn = listofSummaryTxn.OrderByDescending(s => s.Symbol);
                            break;
                        default:
                            listofSummaryTxn = listofSummaryTxn.OrderBy(s => s.Symbol);
                            break;
                    }
                    switch (openSortOrder)
                    {
                        case "date_desc":
                            if (txnOpenIQ != null)
                            {
                                txnOpenIQ = txnOpenIQ.OrderByDescending(s => s.TXN_BUY_DATE);
                            }
                            break;
                        default:
                            if (txnOpenIQ != null)
                            {
                                txnOpenIQ = txnOpenIQ.OrderBy(s => s.TXN_BUY_DATE);
                            }
                            break;
                    }
                    switch (closedSortOrder)
                    {
                        case "closedsymbol_desc":
                            if (txnClosedIQ != null)
                            {
                                txnClosedIQ = txnClosedIQ.OrderByDescending(s => s.stockMaster.Symbol);
                            }
                            break;
                        default:
                            if (txnClosedIQ != null)
                            {
                                txnClosedIQ = txnClosedIQ.OrderBy(s => s.stockMaster.Symbol);
                            }
                            break;
                    }

                    var pageSize = Configuration.GetValue("PageSize", 10);

                    CurrentSummaryPageIndex = pageSummaryIndex;
                    portfolioSummaryOpenTxn = await PaginatedList<PORTFOLIOTXN_SUMMARY>.CreateAsync(listofSummaryTxn, pageSummaryIndex ?? 1, pageSize);

                    if (txnOpenIQ != null)
                    {

                        CurrentPageIndex = pageIndex;
                        portfolioOpenTxn = await PaginatedList<PORTFOLIOTXN>.CreateAsync(txnOpenIQ, pageIndex ?? 1, pageSize);
                    }

                    CurrentClosedPageIndex = pageIndexClosed;
                    portfolioClosedTxn = await PaginatedList<PORTFOLIOTXN>.CreateAsync(txnClosedIQ, pageIndexClosed ?? 1, pageSize);
                }
            }
        }

        public IActionResult OnPostSummaryAction(string summarymenuitemsel, int? masterid, int? stockid, 
            int? pageSummaryIndex, int? pageIndex, int? pageClosedIndex, 
            string openSortOrder, string summarySortOrder, string closedSortOrder, string currentFilter)
        {
            if ((summarymenuitemsel.Equals("-1") == false) && (masterid != null) && (stockid != null))
            {
                switch (summarymenuitemsel)
                {
                    case constShowTxn://case of show all txn
                        return RedirectToPage("./portfolioTxnIndex", new
                        {
                            openSortOrder = openSortOrder,
                            summarySortOrder = summarySortOrder,
                            closedSortOrder = closedSortOrder,
                            currentFilter = currentFilter,
                            pageSummaryIndex = pageSummaryIndex,
                            pageIndex = pageIndex,
                            masterid = masterid,
                            pageClosedIndex = pageClosedIndex,
                            getQuote = false,
                            stockid = stockid,
                            refreshAll = false,
                            lifetimeHighLow = false
                        });
                    case constGetQuote://case of update get quote, buy sell, high low
                        return RedirectToPage("./portfolioTxnIndex", new
                        {
                            masterid = masterid,
                            stockid = stockid,
                            openSortOrder = openSortOrder,
                            summarySortOrder = summarySortOrder,
                            closedSortOrder = closedSortOrder,
                            pageSummaryIndex = pageSummaryIndex,
                            pageIndex = pageIndex,
                            pageClosedIndex = pageClosedIndex,
                            currentFilter = currentFilter,
                            getQuote = true,
                            updateBuySell = false,
                            refreshAll = false,
                            lifetimeHighLow = false
                        });

                    case constUpdateStrategy://case of update get quote, buy sell, high low
                        return RedirectToPage("./portfolioTxnIndex", new
                        {
                            masterid = masterid,
                            stockid = stockid,
                            openSortOrder = openSortOrder,
                            summarySortOrder = summarySortOrder,
                            closedSortOrder = closedSortOrder,
                            pageSummaryIndex = pageSummaryIndex,
                            pageIndex = pageIndex,
                            pageClosedIndex = pageClosedIndex,
                            currentFilter = currentFilter,
                            getQuote = true,
                            updateBuySell = true,
                            refreshAll = false,
                            lifetimeHighLow = false
                        });
                    case constHistoryData://case of show history table
                        return RedirectToPage("/History/Index", new { id = stockid });
                    case constChart_History://case of show history chart
                        return RedirectToPage("/StandardIndicators/chartHistory", new
                        {
                            stockid = stockid,
                            onlyhistory = 0,
                            history = true
                        });
                    case constChart_SMARSISTOCH://case of show smarsistoch chart
                        return RedirectToPage("/StandardIndicators/chartSMARSISTOCH", new { id = stockid });
                    case constRsiTrendV40: //case of rsi trend
                        return RedirectToPage("/BuySell/rsiv40", new { symbolToUpdate = stockid });
                    case constStochV40: //case of strategy stoch
                        return RedirectToPage("/BuySell/stochv40", new { symbolToUpdate = stockid });
                    case constSmaV40: //case of strategy SMA
                        return RedirectToPage("/BuySell/smav40", new { symbolToUpdate = stockid });
                    case constV20: //case of strategy V20
                        return RedirectToPage("/BuySell/v20BuySell", new { symbolToUpdate = stockid });
                    case constBullishEngulfing: //case of strategy Bullinsh engulfing
                        return RedirectToPage("/BuySell/BullishEngulfing", new { symbolToUpdate = stockid });
                    case constBearishEngulfing: //case of strategy Bearish Engulfing
                        return RedirectToPage("/BuySell/BearishEngulfing", new { symbolToUpdate = stockid });

                    default:
                        return RedirectToPage("./portfolioTxnIndex", new
                        {
                            openSortOrder = openSortOrder,
                            summarySortOrder = summarySortOrder,
                            closedSortOrder = closedSortOrder,
                            currentFilter = currentFilter,
                            pageSummaryIndex = pageSummaryIndex,
                            pageIndex = pageIndex,
                            masterid = masterid,
                            pageClosedIndex = pageClosedIndex,
                            getQuote = false,
                            refreshAll = false,
                            lifetimeHighLow = false
                        });
                }
            }
            return RedirectToPage("./portfolioTxnIndex", new
            {
                openSortOrder = openSortOrder,
                summarySortOrder = summarySortOrder,
                closedSortOrder = closedSortOrder,
                currentFilter = currentFilter,
                pageSummaryIndex = pageSummaryIndex,
                pageIndex = pageIndex,
                masterid = masterid,
                pageClosedIndex = pageClosedIndex,
                getQuote = false,
                refreshAll = false,
                lifetimeHighLow = false
            });

        }
        public IActionResult OnPostTransactionAction(string menuitemsel, int? masterid, int? txnid, int? stockid,
            int? pageSummaryIndex, int? pageIndex, int? pageClosedIndex, 
            string openSortOrder, string summarySortOrder, string closedSortOrder, string currentFilter)
        {
            if ((menuitemsel.Equals("-1") == false) && (txnid != null) && (masterid != null) && (stockid != null))
            {
                //PORTFOLIOTXN currentTxn = _context.PORTFOLIOTXN.FirstOrDefault(t => t.PORTFOLIOTXN_ID== txnid);
                switch (menuitemsel)
                {
                    case constClosePosition://case of sell txn
                        return RedirectToPage("./portfolioTxnCreate", new
                        {
                            txntype = "S",
                            masterid = masterid,
                            txnid = txnid,
                            stockid = stockid,
                            openSortOrder = openSortOrder,
                            summarySortOrder = summarySortOrder,
                            closedSortOrder = closedSortOrder,
                            pageSummaryIndex = pageSummaryIndex,
                            pageIndex = pageIndex,
                            pageClosedIndex = pageClosedIndex,
                            currentFilter = currentFilter
                        });

                    case constEditTxn://case of edit txn
                        return RedirectToPage("./portfolioTxnEdit", new
                        {
                            masterid = masterid,
                            txnid = txnid,
                            stockid = stockid,
                            openSortOrder = openSortOrder,
                            summarySortOrder = summarySortOrder,
                            closedSortOrder = closedSortOrder,
                            pageSummaryIndex = pageSummaryIndex,
                            pageIndex = pageIndex,
                            pageClosedIndex = pageClosedIndex,
                            currentFilter = currentFilter,
                            getQuote = false,
                            refreshAll = false,
                            lifetimeHighLow = false
                        });
                    case constDeleteTxn://case of delete txn
                        return RedirectToPage("./portfolioTxnDelete", new
                        {
                            masterid = masterid,
                            txnid = txnid,
                            stockid = stockid,
                            openSortOrder = openSortOrder,
                            summarySortOrder = summarySortOrder,
                            closedSortOrder = closedSortOrder,
                            pageSummaryIndex = pageSummaryIndex,
                            pageIndex = pageIndex,
                            pageClosedIndex = pageClosedIndex,
                            currentFilter = currentFilter,
                            getQuote = false,
                            refreshAll = false,
                            lifetimeHighLow = false
                        });
                    case constDetailsTxn://case of txn details
                        return RedirectToPage("./portfolioTxnDetails", new
                        {
                            masterid = masterid,
                            txnid = txnid,
                            stockid = stockid,
                            openSortOrder = openSortOrder,
                            summarySortOrder = summarySortOrder,
                            closedSortOrder = closedSortOrder,
                            pageSummaryIndex = pageSummaryIndex,
                            pageIndex = pageIndex,
                            pageClosedIndex = pageClosedIndex,
                            currentFilter = currentFilter,
                            getQuote = false,
                            refreshAll = false,
                            lifetimeHighLow = false
                        });
                    default:
                        return RedirectToPage("./portfolioTxnIndex", new
                        {
                            masterid = masterid,
                            openSortOrder = openSortOrder,
                            summarySortOrder = summarySortOrder,
                            closedSortOrder = closedSortOrder,
                            pageSummaryIndex = pageSummaryIndex,
                            pageIndex = pageIndex,
                            pageClosedIndex = pageClosedIndex,
                            currentFilter = currentFilter,
                            getQuote = false,
                            refreshAll = false,
                            lifetimeHighLow = false
                        });
                }
            }
            return RedirectToPage("./portfolioTxnIndex", new { masterid = masterid, txnid = txnid, stockid = stockid, openSortOrder = openSortOrder, summarySortOrder = summarySortOrder, closedSortOrder = closedSortOrder, pageSummaryIndex = pageSummaryIndex, pageIndex = pageIndex, pageClosedIndex = pageClosedIndex, currentFilter = currentFilter, getQuote = false, refreshAll = false, lifetimeHighLow = false });
        }

    }

}
