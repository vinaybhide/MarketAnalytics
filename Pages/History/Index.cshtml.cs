using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Data;
using MarketAnalytics.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace MarketAnalytics.Pages.History
{
    public class IndexModel : PageModel
    {
        private const string constV40V40NV200 = "-99";
        private const string constV40 = "-98";
        private const string constV40N = "-97";
        private const string constV200 = "-96";
        private const string constAll = "-95";
        private const string constMF = "-94";

        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;

        public List<SelectListItem> symbolList { get; set; }
        public List<SelectListItem> groupList { get; set; }

        public string PriceDateSort { get; set; }
        public string SymbolSort { get; set; }
        [BindProperty]
        public int? CurrentID { get; set; }
        public string CurrentSort { get; set; }
        public string CurrentFilter { get; set; }
        public bool RefreshAllStocks { get; set; } = false;
        [BindProperty]
        public int? CurrentGroup { get; set; }

        public string CompanyName { get; set; }
        public string InvestmenType { get; set; }
        //public IList<StockPriceHistory> StockPriceHistory { get;set; } = default!;
        public PaginatedList<StockPriceHistory> StockPriceHistory { get; set; } = default!;

        public IndexModel(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            symbolList = new List<SelectListItem>();
            groupList = new List<SelectListItem>();
        }

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex,
                                    int? id, bool? refreshAll, string filterCategory, int? groupsel)
        {
            if (_context.StockPriceHistory != null)
            {
                groupList.Clear();

                SelectListItem selectAll = new SelectListItem("V40+V40N+V200", constV40V40NV200);
                groupList.Add(selectAll);

                selectAll = new SelectListItem("V40", constV40);
                groupList.Add(selectAll);

                selectAll = new SelectListItem("V40N", constV40N);
                groupList.Add(selectAll);

                selectAll = new SelectListItem("V200", constV200);
                groupList.Add(selectAll);

                selectAll = new SelectListItem("All Stocks", constAll);
                groupList.Add(selectAll);

                selectAll = new SelectListItem("All Mutual Funds", constMF);
                groupList.Add(selectAll);

                IQueryable<StockPriceHistory> stockpriceIQ = null;

                //CurrentID = id;
                CurrentSort = sortOrder;
                SymbolSort = String.IsNullOrEmpty(sortOrder) ? "symbol_desc" : "";
                PriceDateSort = sortOrder == "PriceDate" ? "pricedate_desc" : "PriceDate";

                PriceDateSort = String.IsNullOrEmpty(sortOrder) ? "pricedate_desc" : "";
                if(groupsel == null)
                {
                    groupsel = Int32.Parse(constAll);
                }

                CurrentGroup = groupsel;

                if (searchString != null)
                {
                    pageIndex = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                IQueryable<StockMaster> stockmasterIQ = null;
                if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constV40V40NV200)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => ((s.V40 == true) || (s.V40N == true) || (s.V200 == true))).AsNoTracking();
                    CompanyName = "History for V40+V40N+V200";

                }
                else if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constV40)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.V40 == true)).AsNoTracking();
                    CompanyName = "History for V40";
                }
                else if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constV40N)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.V40N == true)).AsNoTracking();
                    CompanyName = "History for V40N";
                }
                else if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constV200)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.V200 == true)).AsNoTracking();
                    CompanyName = "History for V200";
                }
                else if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constMF)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.INVESTMENT_TYPE.Equals("Mutual Fund"))).AsNoTracking();
                    CompanyName = "History for all Mutual Funds";
                }
                else //if (id == Int32.Parse(constAll))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().AsNoTracking();
                    CompanyName = "History for all stocks";
                }
                
                groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;

                symbolList.Clear();
                if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constMF)))
                {
                    symbolList = stockmasterIQ.OrderBy(a => a.Symbol)
                                            .Select(a =>
                                                new SelectListItem
                                                {
                                                    Value = a.StockMasterID.ToString(),
                                                    Text = a.CompName
                                                }
                                            ).ToList();
                }
                else
                {
                    symbolList = stockmasterIQ.OrderBy(a => a.Symbol)
                                            .Select(a =>
                                                new SelectListItem
                                                {
                                                    Value = a.StockMasterID.ToString(),
                                                    Text = a.Symbol + "." + a.Exchange
                                                }
                                            ).ToList();
                }
                //SelectListItem selectAll = new SelectListItem("-Select Symbol-", "-1");
                //symbolList.Insert(0, selectAll);
                if ((string.IsNullOrEmpty(filterCategory) == false) && filterCategory.Equals("Show & Update Category") && (CurrentGroup != null))
                {
                    DbInitializer.UpdateQuoteStrategy(_context, (int)CurrentGroup);
                }

                CurrentID = id;

                if ((id != null) && (id > 0))
                {
                    StockMaster StockMasterRec = null;
                    //CurrentID = id;
                    //var selectedRecord = await _context.StockMaster.FirstOrDefaultAsync(m => m.StockMasterID == id);
                    StockMasterRec = await _context.StockMaster.Include(a => a.collectionStockPriceHistory).AsSplitQuery().FirstOrDefaultAsync(m => m.StockMasterID == id);
                    symbolList.FirstOrDefault(a => a.Value.Equals(CurrentID.ToString())).Selected = true;

                    CompanyName = StockMasterRec.CompName;
                    InvestmenType = StockMasterRec.INVESTMENT_TYPE;

                    if ((refreshAll == true) && (StockMasterRec != null) && (sortOrder == null) && (currentFilter == null) && (searchString == null) && (pageIndex == null))
                    {
                        //we have found a matching record from StockMaster, from where we can get id, symbol, company
                        DbInitializer.UpdateQuoteStrategy(_context, (int)id);
                        RefreshAllStocks = false;
                    }

                    CurrentFilter = searchString;

                    stockpriceIQ = _context.StockPriceHistory.Where(s => (s.StockMasterID == CurrentID)).AsNoTracking();
                    //stockpriceIQ = StockMasterRec.collectionStockPriceHistory.AsQueryable();
                    if (!String.IsNullOrEmpty(searchString))
                    {
                        stockpriceIQ = stockpriceIQ.Where(s => (s.PriceDate.Date >= (Convert.ToDateTime(searchString).Date))
                                                                && (s.StockMasterID == CurrentID)).AsNoTracking();
                    }
                    if (stockpriceIQ != null)
                    {
                        switch (sortOrder)
                        {
                            case "symbol_desc":
                                stockpriceIQ = stockpriceIQ.OrderByDescending(s => s.StockMaster.Symbol);
                                break;
                            case "PriceDate":
                                stockpriceIQ = stockpriceIQ.OrderBy(s => s.PriceDate);
                                break;
                            case "pricedate_desc":
                                stockpriceIQ = stockpriceIQ.OrderByDescending(s => s.PriceDate);
                                break;
                            default:
                                stockpriceIQ = stockpriceIQ.OrderBy(s => s.StockMaster.Symbol);
                                break;
                        }
                        var pageSize = Configuration.GetValue("PageSize", 10);
                        StockPriceHistory = await PaginatedList<StockPriceHistory>.CreateAsync(stockpriceIQ.AsNoTracking(), pageIndex ?? 1, pageSize, CurrentID);
                    }
                }
            }
        }

        public void RefreshHistoryForGroup(int groupId)
        {
            IQueryable<StockMaster> stockmasterIQ = null;
            string lastPriceDate = string.Empty;
            try
            {
                if (groupId == -99)
                {
                    stockmasterIQ = _context.StockMaster.Where(s => ((s.V200 == true) || (s.V40 == true) || (s.V40N == true)));
                }
                else if (groupId == -98)
                {
                    stockmasterIQ = _context.StockMaster.Where(s => (s.V40 == true));
                }
                else if (groupId == -97)
                {
                    stockmasterIQ = _context.StockMaster.Where(s => (s.V40N == true));
                }
                else if (groupId == -96)
                {
                    stockmasterIQ = _context.StockMaster.Where(s => (s.V200 == true));
                }
                else if (groupId == -95)
                {
                    stockmasterIQ = _context.StockMaster;
                }
                foreach (var item in stockmasterIQ)
                {
                    lastPriceDate = DbInitializer.IsHistoryUpdated(_context, item);
                    if (string.IsNullOrEmpty(lastPriceDate) == false)
                    {
                        DbInitializer.InitializeHistory(_context, item, lastPriceDate);
                    }
                    DbInitializer.GetSMA_EMA_MACD_BBANDS_Table(_context, item);

                    DbInitializer.getRSIDataTableFromDaily(_context, item, period: "14");

                    DbInitializer.getStochasticDataTableFromDaily(_context, item, fastkperiod: "20", slowdperiod: "20");

                    DbInitializer.V20CandlesticPatternFinder(_context, item);

                    DbInitializer.GetSMA_BUYSELL(_context, item, 20, 50, 200);

                    DbInitializer.GetBullishEngulfingBuySellList(_context, item, DateTime.Today.AddDays(-180), 10);
                    DbInitializer.GetBearishEngulfingBuySellList(_context, item, DateTime.Today.AddDays(-180), 10);
                    DbInitializer.GetLifetimeHighLow(_context, item);

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

    }
}
