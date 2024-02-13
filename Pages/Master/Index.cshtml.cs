using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Data;
using MarketAnalytics.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MarketAnalytics.Pages.Master
{
    public class IndexModel : PageModel
    {
        private const string constV40V40NV200 = "-99";
        private const string constV40 = "-98";
        private const string constV40N = "-97";
        private const string constV200 = "-96";
        private const string constAll = "-95";
        private const string constMF = "-94";
        private const string constBSEMF = "-93";
        private const string constAMFIMF = "-92";
        private const string constAllStocks = "-91";
        private const string constAllETF = "-90";
        private const string constAllFuture = "-89";
        private const string constAllIndex = "-88";


        private const string constEditCategory = "0";
        private const string constShowDetails = "1";
        private const string constGetQuote = "2";
        private const string constUpdateAll = "3";
        private const string constHistory = "4";
        private const string constChartHistory = "5";
        private const string constChartSMARSISTOCH = "6";
        private const string constSMAV40 = "7";
        private const string constV20 = "8";
        private const string constBullishEngulf = "9";
        private const string constBearishEngul = "10";
        private const string constSTOCHV40 = "11";

        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public List<SelectListItem> menuList { get; set; }
        public List<SelectListItem> groupList { get; set; }


        public IndexModel(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            menuList = new List<SelectListItem>();
            groupList = new List<SelectListItem>();
        }

        public string ExchangeSort { get; set; }
        public string SymbolSort { get; set; }
        public string CompNameSort { get; set; }
        public string TypeSort { get; set; }
        public string V40Sort { get; set; }
        public string V40NSort { get; set; }
        public string V200Sort { get; set; }
        [BindProperty]
        public string CurrentFilter { get; set; }
        [BindProperty]
        public string CurrentSort { get; set; }

        //public bool RefreshAllStocks { get; set; } = false;
        [BindProperty]
        public int? CurrentGroup { get; set; }
        [BindProperty]
        public int CurrentSel { get; set; }
        [BindProperty]
        public int? CurrentPageIndex { get; set; }
        [BindProperty]
        public string CurrentGroupSelection { get; set; }
        public PaginatedList<StockMaster> StockMaster { get; set; } = default!;

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex, int? id,
                    bool? refreshAll, bool? history, bool? getQuote, bool? lifetimeHighLow, int? groupsel, bool? updateStrategy,
                    string filterCategory, string searchWhere)
        {
            if (_context.StockMaster != null)
            {
                groupList.Clear();
                SelectListItem selectAll = new SelectListItem("-- Show All --", constAll, true);
                groupList.Insert(0, selectAll);

                selectAll = new SelectListItem("-- Show: V40, V40N, V200 --", constV40V40NV200);
                groupList.Add(selectAll);

                selectAll = new SelectListItem("Show V40", constV40);
                groupList.Add(selectAll);

                selectAll = new SelectListItem("Show V40N", constV40N);
                groupList.Add(selectAll);

                selectAll = new SelectListItem("Show V200", constV200);
                groupList.Add(selectAll);

                selectAll = new SelectListItem("Show All Mutual Funds", constMF);
                groupList.Add(selectAll);

                selectAll = new SelectListItem("Show BSE Mutual Funds", constBSEMF);
                groupList.Add(selectAll);

                selectAll = new SelectListItem("Show AMFI Mutual Funds", constAMFIMF);
                groupList.Add(selectAll);

                selectAll = new SelectListItem("Show All Stocks", constAllStocks);
                groupList.Add(selectAll);

                selectAll = new SelectListItem("Show All ETF", constAllETF);
                groupList.Add(selectAll);

                selectAll = new SelectListItem("Show All Future", constAllFuture);
                groupList.Add(selectAll);

                selectAll = new SelectListItem("Show All Indexes", constAllIndex);
                groupList.Add(selectAll);

                menuList.Clear();

                //SelectListItem menuItem = new SelectListItem("-- Select Action --", "-1");
                //menuList.Add(menuItem);

                SelectListItem menuItem = new SelectListItem("Edit Category", constEditCategory);
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Show Details", constShowDetails);
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Get Quote", constGetQuote);
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Update All", constUpdateAll);
                menuList.Add(menuItem);
                menuItem = new SelectListItem("History", constHistory);
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Chart: History", constChartHistory);
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Chart: SMA-RSI-STOCH", constChartSMARSISTOCH);
                menuList.Add(menuItem);
                menuItem = new SelectListItem("SMA V40", constSMAV40);
                menuList.Add(menuItem);
                menuItem = new SelectListItem("V20", constV20);
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Bullish Engulfing", constBullishEngulf);
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Bearish Engulfing", constBearishEngul);
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Stochastics V40", constSTOCHV40);
                menuList.Add(menuItem);


                //StockMaster = await _context.StockMaster.ToListAsync();
                //Commented above line and Added following for sorting, searching, paging
                CurrentSort = sortOrder;
                SymbolSort = String.IsNullOrEmpty(sortOrder) ? "symbol_desc" : "";
                ExchangeSort = sortOrder == "Exchange" ? "exchange_desc" : "Exchange";
                CompNameSort = sortOrder == "CompName" ? "compname_desc" : "CompName";
                TypeSort = sortOrder == "TypeSort" ? "typesort_desc" : "TypeSort";

                V40Sort = sortOrder == "V40" ? "v40_desc" : "V40";
                V40NSort = sortOrder == "V40N" ? "v40n_desc" : "V40N";
                V200Sort = sortOrder == "V200" ? "v200_desc" : "V200";

                if (searchString != null)
                {
                    pageIndex = 1;
                    if (string.IsNullOrEmpty(searchWhere) == false)
                    {
                        if(searchWhere.Equals("Search Online"))
                        {
                            DbInitializer.SearchOnlineInsertInDB(_context, searchString);
                        }
                    }
                    else
                    {
                        DbInitializer.SearchOnlineInsertInDB(_context, searchString);
                    }
                }
                else
                {
                    searchString = currentFilter;
                }

                if (refreshAll == true)
                {
                    string fetchedData = await DbInitializer.FetchMasterData();
                    DbInitializer.Initialize(_context, fetchedData);
                    string fetchedMFData = await DbInitializer.FetchAMFIMFMasterData();
                    DbInitializer.InitializeAMFIMF(_context, fetchedMFData);
                    //RefreshAllStockMaster();
                    //RefreshAllStocks = false;
                }

                IQueryable<StockMaster> stockmasterIQ = null;

                CurrentGroup = groupsel;
                if ((string.IsNullOrEmpty(filterCategory) == false) && filterCategory.Equals("Show & Update Category"))
                {
                    updateStrategy = true;
                }
                if (((id == null) || (id <= 0)) && (CurrentGroup != null))
                {
                    if ((updateStrategy != null) && (updateStrategy == true))
                    {
                        DbInitializer.UpdateQuoteStrategy(_context, (int)CurrentGroup);
                    }
                }
                if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constV40V40NV200)))
                {
                    stockmasterIQ = _context.StockMaster
                        .AsSplitQuery()
                        .Where(s => ((s.V40 == true) || (s.V40N == true) || (s.V200 == true))).AsNoTracking();
                    groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constV40)))
                {
                    stockmasterIQ = _context.StockMaster
                                                .AsSplitQuery()
                                                .Where(s => (s.V40 == true)).AsNoTracking();
                    groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constV40N)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery()
                        .Where(s => (s.V40N == true)).AsNoTracking();
                    groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constV200)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.V200 == true)).AsNoTracking();
                    groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constMF)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.INVESTMENT_TYPE.Equals("Mutual Fund"))).AsNoTracking();
                    groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constBSEMF)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.INVESTMENT_TYPE.Equals("Mutual Fund")) && (s.Exchange.Equals("BO"))).AsNoTracking();
                    groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constAMFIMF)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.INVESTMENT_TYPE.Equals("Mutual Fund")) && (s.Exchange.Equals("AMFI"))).AsNoTracking();
                    groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constAllStocks)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.INVESTMENT_TYPE.Equals("Stocks"))).AsNoTracking();
                    groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constAllETF)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.INVESTMENT_TYPE.Equals("ETF"))).AsNoTracking();
                    groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constAllFuture)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.INVESTMENT_TYPE.Equals("Future"))).AsNoTracking();
                    groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constAllFuture)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.INVESTMENT_TYPE.Equals("Mutual Fund"))).AsNoTracking();
                    groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constAllIndex)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.INVESTMENT_TYPE.Equals("Index"))).AsNoTracking();
                    groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().AsNoTracking();//from s in _context.StockMaster select s;
                }

                if ((id != null) && (id > 0))
                {
                    var selectedRecord = await _context.StockMaster.AsSplitQuery().FirstOrDefaultAsync(m => m.StockMasterID == id);
                    if (selectedRecord != null)
                    {
                        if (getQuote == true)
                        {
                            DbInitializer.UpdateStockQuote(_context, selectedRecord);
                        }
                        if ((updateStrategy != null) && (updateStrategy == true))
                        {
                            DbInitializer.UpdateQuoteStrategy(_context, (int)id);
                        }
                    }
                }


                CurrentFilter = searchString;


                if (!String.IsNullOrEmpty(searchString))
                {
                    stockmasterIQ = stockmasterIQ
                        .AsSplitQuery()
                        .Where(s => s.Symbol.ToUpper().Contains(searchString.ToUpper())
                                                            || s.CompName.ToUpper().Contains(searchString.ToUpper())).AsNoTracking();
                    if (stockmasterIQ.Count() <= 0)
                    {
                        CurrentGroup = groupsel = -95;
                        groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                        stockmasterIQ = _context.StockMaster.AsSplitQuery().AsNoTracking();//from s in _context.StockMaster select s;
                    }
                }
                switch (sortOrder)
                {
                    case "symbol_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.Symbol).AsNoTracking();
                        break;
                    case "Exchange":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.Exchange).AsNoTracking();
                        break;
                    case "exchange_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.Exchange).AsNoTracking();
                        break;
                    case "CompName":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.CompName).AsNoTracking();
                        break;
                    case "compname_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.CompName).AsNoTracking();
                        break;
                    case "TypeSort":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.INVESTMENT_TYPE).AsNoTracking();
                        break;
                    case "typesort_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.INVESTMENT_TYPE).AsNoTracking();
                        break;
                    case "V40":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.V40).AsNoTracking();
                        break;
                    case "v40_desc":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.V40).AsNoTracking();
                        break;
                    case "V40N":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.V40N).AsNoTracking();
                        break;
                    case "v40n_desc":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.V40N).AsNoTracking();
                        break;
                    case "V200":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.V200).AsNoTracking();
                        break;
                    case "v200_desc":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.V200).AsNoTracking();
                        break;

                    default:
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.Symbol).AsNoTracking();
                        break;
                }
                var pageSize = Configuration.GetValue("PageSize", 10);
                CurrentPageIndex = pageIndex;
                StockMaster = await PaginatedList<StockMaster>.CreateAsync(stockmasterIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
            }
        }

        public IActionResult OnPostStockAction(int? id, string menuitemsel, string sortOrder, int pageIndex, string currentFilter, int? groupsel)
        {
            if ((id != null) && (menuitemsel.Equals("-1") == false))
            {
                StockMaster stockMaster = _context.StockMaster.AsSplitQuery().FirstOrDefault(m => m.StockMasterID == id);

                switch (menuitemsel)
                {
                    case "0"://case of edit category
                        return RedirectToPage("./Edit", new { id = id, groupsel = groupsel, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter });
                    case "1"://case of show details
                        return RedirectToPage("./Details", new { id = id, groupsel = groupsel, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter });
                    case "2"://case of Get Quote
                             //DbInitializer.UpdateStockQuote(_context, stockMaster);
                        return RedirectToPage("./Index", new { id = id, groupsel = groupsel, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter, getQuote = true, updateStrategy = false });
                    case "3": //case of update high low & strategy
                              //DbInitializer.GetLifetimeHighLow(_context, stockMaster);
                        return RedirectToPage("./Index", new { id = id, groupsel = groupsel, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter, getQuote = false, updateStrategy = true });

                    case "4": //case of history
                        return RedirectToPage("/History/Index", new { id = id });

                    case "5": //case of history chart
                        return RedirectToPage("/StandardIndicators/chartHistory", new { stockid = id, onlyhistory = 0, history = true });
                    case "6": //case of chart SMA RSI STOCH
                        return RedirectToPage("/StandardIndicators/chartSMARSISTOCH", new { id = id });
                    case "7": //case of strategy SMA
                        return RedirectToPage("/BuySell/smav40", new { symbolToUpdate = id });
                    case "8": //case of strategy V20
                        return RedirectToPage("/BuySell/v20BuySell", new { symbolToUpdate = id });
                    case "9": //case of strategy Bullinsh engulfing
                        return RedirectToPage("/BuySell/BullishEngulfing", new { symbolToUpdate = id });
                    case "10": //case of strategy Bearish Engulfing
                        return RedirectToPage("/BuySell/BearishEngulfing", new { symbolToUpdate = id });
                    case "11": //case of strategy Bearish Engulfing
                        return RedirectToPage("/BuySell/stochv40", new { symbolToUpdate = id });
                    default:
                        return RedirectToPage("./Index", new { id = id, groupsel = groupsel, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter });
                }
            }
            return RedirectToPage("./Index", new { id = id, groupsel = groupsel, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter });
        }
    }
}
