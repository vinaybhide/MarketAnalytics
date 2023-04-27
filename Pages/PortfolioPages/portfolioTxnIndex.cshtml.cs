using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Data;
using MarketAnalytics.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IO.Compression;
using System.Globalization;
using System.Drawing.Drawing2D;
using Newtonsoft.Json.Linq;
using System.Numerics;
using System.Text;

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
        const string constEntityValuation = "12";

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
                        if ((updateBuySell != null) && (updateBuySell == true) && (selectedRecord != null))
                        {
                            DbInitializer.GetQuoteAndUpdateAllPortfolioTxn(_context, UserId, null, selectedRecord, buysell: true);
                        }
                    }
                    else if ((refreshAll != null) && (refreshAll == true))
                    {
                        DbInitializer.GetQuoteAndUpdateAllPortfolioTxn(_context, UserId, masterid, null, buysell: true);
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
                            TotalGainPCT = x.Sum(s => s.GAIN_AMT) / x.Sum(s => s.TOTAL_COST) * 100,
                            CMP = x.First().CMP,
                            TotalValue = x.Sum(s => s.VALUE)
                        }
                        );

                    summarymenuList.Clear();
                    SelectListItem summarymenuItem = new SelectListItem("Show Txn", constShowTxn);
                    summarymenuList.Add(summarymenuItem);

                    summarymenuItem = new SelectListItem("Get Quote", constGetQuote);
                    summarymenuList.Add(summarymenuItem);

                    summarymenuItem = new SelectListItem("Chart-Valuation", constEntityValuation);
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
                    switch (summarySortOrder)
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
            string openSortOrder, string summarySortOrder, string closedSortOrder, string currentFilter, string quantity)
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
                    case constEntityValuation://case of show entity valuation
                        return RedirectToPage("/StandardIndicators/chartEntityHistoryValuation", new
                        {
                            stockid = stockid,
                            masterid = masterid,
                            quantity = quantity
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

        //[HttpPost]
        public IActionResult OnPostUploadFile(IFormFile postedFile, int? masterid, int? stockid,
            int? pageSummaryIndex, int? pageIndex, int? pageClosedIndex,
            string openSortOrder, string summarySortOrder, string closedSortOrder, string currentFilter)
        {

            if ((masterid != null) && (postedFile.FileName.EndsWith(".csv")))
            {
                using (var sreader = new StreamReader(postedFile.OpenReadStream()))
                {
                    //Title 
                    //Symbol,Current Price,Date,Time,Change,Open,High,Low,Volume,Trade Date,Purchase Price,Quantity,Commission,High Limit,Low Limit,Comment
                    //Sample Data for Buy (note that Symbol must provide exchange code)
                    //0P00005WDJ.BO,,,,,,,,,20121220,119.2317,208.837,100,,,
                    //Sample Data for Sell (note that Symbol must provide exchange code)
                    //0P00005WDJ.BO,,,,,,,,,20121220,119.2317,-208.837,100,,,
                    string headerFormat = "Symbol,Current Price,Date,Time,Change,Open,High,Low,Volume,Trade Date,Purchase Price,Quantity,Commission,High Limit,Low Limit,Comment";
                    string strHeader = sreader.ReadLine();     //Title
                    DateTime[] quoteDate = null;
                    double[] open = null, high = null, low = null, close = null, volume = null, change = null, changepercent = null, prevclose = null;

                    if (strHeader.ToUpper().Equals(headerFormat.ToUpper()))
                    {
                        var masterRec = _context.PORTFOLIO_MASTER.AsSplitQuery().FirstOrDefault(m => (m.PORTFOLIO_MASTER_ID == masterid));
                        StockMaster stockRec = null;
                        //string[] headers = strHeader.Split(",");
                        string symbol = string.Empty;
                        string exchange = string.Empty;
                        double tradeQty = 0;
                        double existingAvgCost = 0;
                        List<long> listTicks = new List<long>();
                        long avgTicks = 0;
                        DateTime avgDate = DateTime.MinValue;
                        List<double> listCost = new List<double>();

                        while (!sreader.EndOfStream)                          //get all the content in rows 
                        {
                            try
                            {
                                string[] rows = sreader.ReadLine().Split(',');
                                symbol = rows[0].ToString().Split(".")[0];
                                exchange = rows[0].ToString().Split(".")[1];
                                if ((stockRec == null) || (stockRec.Symbol.ToUpper().Equals(symbol.ToUpper()) == false))
                                {
                                    stockRec = _context.StockMaster.AsSplitQuery().FirstOrDefault(s => (s.Symbol == symbol) && (s.Exchange == exchange));
                                    if (stockRec == null)
                                    {
                                        //try to find online the imported symbol
                                        if (DbInitializer.SearchOnlineInsertInDB(_context, symbol) == true)
                                        {
                                            stockRec = _context.StockMaster.AsSplitQuery().FirstOrDefault(s => (s.Symbol == symbol) && (s.Exchange == exchange));
                                            DbInitializer.GetQuote(stockRec.Symbol + (stockRec.Exchange.Length == 0 ? "" : ("." + stockRec.Exchange)), out quoteDate, out open, out high, out low, out close,
                                                out volume, out change, out changepercent, out prevclose);
                                        }
                                        else
                                        {
                                            //Ideally we should not come here but
                                            //this is added to handle MF that was not found on yahoo, Bandhan Midcap
                                            try
                                            {
                                                stockRec = new StockMaster();
                                                stockRec.Symbol = symbol;
                                                stockRec.CompName = symbol;
                                                stockRec.Exchange = exchange;
                                                stockRec.INVESTMENT_TYPE = "NA";

                                                stockRec.QuoteDateTime = DateTime.Today.Date;
                                                stockRec.Open = 1;
                                                stockRec.High = 1;
                                                stockRec.Low = 1;
                                                stockRec.Close = 1;
                                                stockRec.Volume = 0;
                                                stockRec.ChangePercent = 0;
                                                stockRec.Change = 0;
                                                stockRec.PrevClose = 1;
                                                _context.StockMaster.Add(stockRec);
                                                _context.SaveChangesAsync(true);

                                                //since we will not get quote for this non existent stock we will initialize 
                                                //arrays to 1 and 0 and for today
                                                quoteDate = new DateTime[1] { DateTime.Today.Date };
                                                open = new double[1] { 1 };
                                                high = new double[1] { 1 };
                                                low = new double[1] { 1 };
                                                close = new double[1] { 1 };
                                                volume = new double[1] { 0 };
                                                change = new double[1] { 0 };
                                                changepercent = new double[1] { 0 };
                                                prevclose = new double[1] { 1 };
                                            }
                                            catch
                                            {
                                                stockRec = null;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //we have a new stock entity, hence get current quote
                                        DbInitializer.GetQuote(stockRec.Symbol + (stockRec.Exchange.Length == 0 ? "" : ("." + stockRec.Exchange)), out quoteDate, out open, out high, out low, out close,
                                        out volume, out change, out changepercent, out prevclose);
                                    }
                                    if (stockRec == null)
                                    {
                                        continue;
                                    }
                                }

                                //now add to rec
                                PORTFOLIOTXN portfolioTxn = new PORTFOLIOTXN();
                                portfolioTxn.PORTFOLIO_MASTER_ID = (int)masterid;
                                portfolioTxn.portfolioMaster = masterRec;
                                portfolioTxn.StockMasterID = stockRec.StockMasterID;
                                portfolioTxn.stockMaster = stockRec;
                                tradeQty = double.Parse(rows[11].ToString());
                                IQueryable<PORTFOLIOTXN> existingTxnIQ = null;
                                if (tradeQty < 0)
                                {
                                    portfolioTxn.TXN_TYPE = "S";
                                    portfolioTxn.TXN_SELL_DATE = DateTime.ParseExact(rows[9].ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
                                    portfolioTxn.SELL_QUANTITY = double.Abs(tradeQty);
                                    portfolioTxn.SELL_AMT_PER_UNIT = double.Parse(rows[10].ToString());
                                }
                                else
                                {
                                    portfolioTxn.TXN_TYPE = "B";
                                    portfolioTxn.TXN_BUY_DATE = DateTime.ParseExact(rows[9].ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
                                    portfolioTxn.PURCHASE_QUANTITY = tradeQty;
                                    portfolioTxn.COST_PER_UNIT = double.Parse(rows[10].ToString());
                                }

                                //tradeCommission = double.Parse(rows[12].ToString());

                                //if (DbInitializer.GetQuote(portfolioTxn.stockMaster.Symbol + (portfolioTxn.stockMaster.Exchange.Length == 0 ? "" : ("." + portfolioTxn.stockMaster.Exchange)), out quoteDate, out open, out high, out low, out close,
                                //        out volume, out change, out changepercent, out prevclose))
                                {
                                    if (quoteDate != null)
                                    {
                                        portfolioTxn.CMP = close != null ? close[0] : 0;
                                        //portfolioTxn.VALUE = portfolioTxn.PURCHASE_QUANTITY * close[0];
                                    }
                                    if (portfolioTxn.TXN_TYPE.Equals("B"))
                                    {
                                        portfolioTxn.TOTAL_COST = portfolioTxn.PURCHASE_QUANTITY * portfolioTxn.COST_PER_UNIT;
                                        portfolioTxn.VALUE = portfolioTxn.PURCHASE_QUANTITY * close[0];
                                        portfolioTxn.GAIN_AMT = portfolioTxn.VALUE - portfolioTxn.TOTAL_COST;
                                        if (portfolioTxn.TOTAL_COST > 0)
                                        {
                                            portfolioTxn.GAIN_PCT = (portfolioTxn.GAIN_AMT / portfolioTxn.VALUE) * 100;
                                        }
                                        else
                                        {
                                            portfolioTxn.GAIN_PCT = 100;
                                        }
                                    }
                                    else if (portfolioTxn.TXN_TYPE.Equals("S"))
                                    {
                                        existingTxnIQ = _context.PORTFOLIOTXN.Where(t => (t.PORTFOLIO_MASTER_ID == masterid) &&
                                                    (t.StockMasterID == stockRec.StockMasterID) &&
                                                    (t.TXN_TYPE == "B")
                                                    ).OrderBy(t => t.TXN_BUY_DATE);
                                        if ((existingTxnIQ == null) || (existingTxnIQ.Count() <= 0))
                                        {
                                            continue;
                                        }
                                        if (existingTxnIQ.Sum(t => t.PURCHASE_QUANTITY) < double.Abs(tradeQty))
                                        {
                                            continue;
                                        }

                                        listCost.Clear();
                                        listTicks.Clear();
                                        tradeQty = double.Abs(tradeQty);

                                        foreach (var txnItem in existingTxnIQ)
                                        {
                                            if (tradeQty > 0)
                                            {
                                                txnItem.CMP = close[0];
                                                listCost.Add(txnItem.COST_PER_UNIT);
                                                listTicks.Add(txnItem.TXN_BUY_DATE.Date.Ticks);
                                                if (txnItem.PURCHASE_QUANTITY <= tradeQty)
                                                {
                                                    tradeQty -= txnItem.PURCHASE_QUANTITY;
                                                    txnItem.PURCHASE_QUANTITY = 0;
                                                }
                                                else if (txnItem.PURCHASE_QUANTITY > tradeQty)
                                                {
                                                    txnItem.PURCHASE_QUANTITY = txnItem.PURCHASE_QUANTITY - tradeQty;
                                                    tradeQty = 0;
                                                }
                                                if (txnItem.PURCHASE_QUANTITY > 0)
                                                {
                                                    //txnItem.CMP = close[0];
                                                    txnItem.TOTAL_COST = txnItem.PURCHASE_QUANTITY * txnItem.COST_PER_UNIT;
                                                    txnItem.VALUE = txnItem.PURCHASE_QUANTITY * close[0];
                                                    txnItem.GAIN_AMT = txnItem.VALUE - txnItem.TOTAL_COST;
                                                    txnItem.GAIN_PCT = (txnItem.GAIN_AMT / txnItem.TOTAL_COST) * 100;
                                                    _context.PORTFOLIOTXN.Update(txnItem);
                                                }
                                                else
                                                {
                                                    _context.PORTFOLIOTXN.Remove(txnItem);
                                                }
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }

                                        existingAvgCost = listCost.Average();
                                        //avgTicks = Convert.ToInt64( listTicks.Average());
                                        var total = BigInteger.Zero;
                                        var count = 0;
                                        foreach (var t in listTicks)
                                        {
                                            count += 1;
                                            total += t;
                                        }
                                        //avgTicks = Convert.ToInt64(total / count);
                                        avgDate = new DateTime(((long)(total / count)));

                                        portfolioTxn.TOTAL_SELL_AMT = portfolioTxn.SELL_QUANTITY * portfolioTxn.SELL_AMT_PER_UNIT;
                                        //sell value - buy value for sold number of stocks
                                        portfolioTxn.SELL_GAIN_AMT = portfolioTxn.TOTAL_SELL_AMT - (portfolioTxn.SELL_QUANTITY * existingAvgCost);
                                        if (existingAvgCost > 0)
                                        {
                                            portfolioTxn.SELL_GAIN_PCT = (portfolioTxn.SELL_GAIN_AMT / (portfolioTxn.SELL_QUANTITY * existingAvgCost)) * 100;
                                        }
                                        else
                                        {
                                            portfolioTxn.SELL_GAIN_PCT = 100;
                                        }
                                        portfolioTxn.SOLD_AFTER = portfolioTxn.TXN_SELL_DATE.Date.Subtract(avgDate.Date).Days;

                                        portfolioTxn.TXN_BUY_DATE = avgDate.Date;
                                        portfolioTxn.COST_PER_UNIT = existingAvgCost;

                                        portfolioTxn.PURCHASE_QUANTITY = portfolioTxn.SELL_QUANTITY;
                                        portfolioTxn.TOTAL_COST = portfolioTxn.PURCHASE_QUANTITY * existingAvgCost;

                                        portfolioTxn.VALUE = portfolioTxn.PURCHASE_QUANTITY * close[0];
                                        portfolioTxn.GAIN_AMT = portfolioTxn.VALUE - portfolioTxn.TOTAL_COST;
                                        portfolioTxn.GAIN_PCT = (portfolioTxn.GAIN_AMT / portfolioTxn.TOTAL_COST) * 100;

                                        portfolioTxn.CMP = close[0];
                                    }

                                    _context.PORTFOLIOTXN.Add(portfolioTxn);
                                    _context.SaveChangesAsync(true);
                                }


                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        //_context.SaveChangesAsync(true);
                    }
                }
            }

            return RedirectToPage("./portfolioTxnIndex", new { masterid = masterid, stockid = stockid, openSortOrder = openSortOrder, summarySortOrder = summarySortOrder, closedSortOrder = closedSortOrder, pageSummaryIndex = pageSummaryIndex, pageIndex = pageIndex, pageClosedIndex = pageClosedIndex, currentFilter = currentFilter, getQuote = false, refreshAll = false, lifetimeHighLow = false });
        }

        //[HttpPost]
        /// <summary>
        /// Method to output poprtfolio data in following format
        /// Symbol,Current Price,Date,Time,Change,Open,High,Low,Volume,Trade Date,Purchase Price,Quantity,Commission,High Limit,Low Limit,Comment
        ///Sample Data for Buy (note that Symbol must provide exchange code)
        /// 0P00005WDJ.BO,,,,,,,,,20121220,119.2317,208.837,100,,,
        ///Sample Data for Sell (note that Symbol must provide exchange code)
        /// 0P00005WDJ.BO,,,,,,,,,20121220,119.2317,-208.837,100,,,
        /// </summary>
        /// <param name="masterid"></param>
        /// <param name="stockid"></param>
        /// <param name="pageSummaryIndex"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageClosedIndex"></param>
        /// <param name="openSortOrder"></param>
        /// <param name="summarySortOrder"></param>
        /// <param name="closedSortOrder"></param>
        /// <param name="currentFilter"></param>
        /// <returns></returns>
        public IActionResult OnPostDownloadFile(int? masterid, int? stockid,
            int? pageSummaryIndex, int? pageIndex, int? pageClosedIndex,
            string openSortOrder, string summarySortOrder, string closedSortOrder, string currentFilter)
        {
            try
            {
                if (masterid != null)
                {
                    var masterRec = _context.PORTFOLIO_MASTER.AsSplitQuery().FirstOrDefault(m => (m.PORTFOLIO_MASTER_ID == masterid));
                    if (masterRec != null)
                    {
                        string fileName = masterRec.PORTFOLIO_NAME + ".csv";
                        string fileContents = "Symbol,Current Price,Date,Time,Change,Open,High,Low,Volume,Trade Date,Purchase Price,Quantity,Commission,High Limit,Low Limit,Comment" + Environment.NewLine;
                        int originalLen = fileContents.Length;
                        string fileRecord = string.Empty;
                        //first add 'B' transactions
                        IQueryable<PORTFOLIOTXN> iqTxn = _context.PORTFOLIOTXN.Include(a => a.stockMaster).AsSplitQuery()
                                    .Where(x => (x.PORTFOLIO_MASTER_ID == masterid) && (x.TXN_TYPE.Equals("B")));
                        foreach (var itemOpen in iqTxn)
                        {
                            //write data in following format;
                            // 0P00005WDJ.BO,,,,,,,,,20121220,119.2317,208.837,100,,,
                            fileRecord = itemOpen.stockMaster.Symbol + "." + itemOpen.stockMaster.Exchange + ",,,,,,,,," +
                                itemOpen.TXN_BUY_DATE.ToString("yyyyMMdd") + "," +
                                //itemOpen.TXN_BUY_DATE.Year.ToString() + itemOpen.TXN_BUY_DATE.Month.ToString() + itemOpen.TXN_BUY_DATE.Day.ToString() + "," +
                                itemOpen.COST_PER_UNIT.ToString() + "," + itemOpen.PURCHASE_QUANTITY.ToString() + ",,,," + Environment.NewLine;
                            fileContents += fileRecord;
                            fileRecord = string.Empty;
                        }

                        iqTxn = _context.PORTFOLIOTXN.Include(a => a.stockMaster).AsSplitQuery()
                                    .Where(x => (x.PORTFOLIO_MASTER_ID == masterid) && (x.TXN_TYPE.Equals("S")));
                        foreach (var itemClose in iqTxn)
                        {
                            //first add purchase record so that the sell transaction gets inserted when file is imported
                            //else we can not match the sell to purchase.
                            //portfolioTxn.TXN_BUY_DATE = avgDate.Date;
                            //portfolioTxn.COST_PER_UNIT = existingAvgCost;
                            //portfolioTxn.PURCHASE_QUANTITY = portfolioTxn.SELL_QUANTITY;
                            fileRecord = itemClose.stockMaster.Symbol + "." + itemClose.stockMaster.Exchange + ",,,,,,,,," +
                                itemClose.TXN_BUY_DATE.ToString("yyyyMMdd") + "," +
                                //itemOpen.TXN_SELL_DATE.Year.ToString() + itemOpen.TXN_SELL_DATE.Month.ToString() + itemOpen.TXN_SELL_DATE.Day.ToString() + "," +
                                itemClose.COST_PER_UNIT.ToString() + "," + itemClose.PURCHASE_QUANTITY.ToString() + ",,,," + Environment.NewLine;

                            fileContents += fileRecord;
                            fileRecord = string.Empty;

                            //write data in following format;
                            // 0P00005WDJ.BO,,,,,,,,,20121220,119.2317,-208.837,100,,,
                            fileRecord = itemClose.stockMaster.Symbol + "." + itemClose.stockMaster.Exchange + ",,,,,,,,," +
                                itemClose.TXN_SELL_DATE.ToString("yyyyMMdd") + "," +
                                //itemOpen.TXN_SELL_DATE.Year.ToString() + itemOpen.TXN_SELL_DATE.Month.ToString() + itemOpen.TXN_SELL_DATE.Day.ToString() + "," +
                                itemClose.SELL_AMT_PER_UNIT.ToString() + ",-" + itemClose.SELL_QUANTITY.ToString() + ",,,," + Environment.NewLine;
                            fileContents += fileRecord;
                            fileRecord = string.Empty;
                        }

                        if(fileContents.Length > originalLen)
                        {
                            byte[] bytes = Encoding.ASCII.GetBytes(fileContents);
                            return File(bytes, "application/text", fileName);
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }
            return RedirectToPage("./portfolioTxnIndex", new { masterid = masterid, stockid = stockid, openSortOrder = openSortOrder, summarySortOrder = summarySortOrder, closedSortOrder = closedSortOrder, pageSummaryIndex = pageSummaryIndex, pageIndex = pageIndex, pageClosedIndex = pageClosedIndex, currentFilter = currentFilter, getQuote = false, refreshAll = false, lifetimeHighLow = false });
        }

    }

}
