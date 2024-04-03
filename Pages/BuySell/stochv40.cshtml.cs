using MarketAnalytics.Data;
using MarketAnalytics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MarketAnalytics.Pages.BuySell
{
    public class stochv40Model : PageModel
    {
        private const string constV40V40NV200 = "-99";
        private const string constV40 = "-98";
        private const string constV40N = "-97";
        private const string constV200 = "-96";
        private const string constAll = "-95";

        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public List<SelectListItem> groupList { get; set; }
        public List<SelectListItem> symbolList { get; set; }
        public List<SelectListItem> menuList { get; set; }

        public stochv40Model(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            symbolList = new List<SelectListItem>();
            menuList = new List<SelectListItem>();
            groupList = new List<SelectListItem>();
        }

        public string BuySignalSort { get; set; }
        public string SymbolSort { get; set; }
        public string SellSignalSort { get; set; }
        public string CurrentFilter { get; set; }
        [BindProperty]
        public string CurrentSort { get; set; }
        [BindProperty]
        public int? CurrentGroup { get; set; }
        [BindProperty]
        public int? CurrentPageIndex { get; set; }

        //public bool RefreshAllStocks { get; set; } = false;
        //public int CurrentID { get; set; }
        public PaginatedList<StockMaster> StockMaster { get; set; } = default!;

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex, int? id, bool? refreshAll,
            bool? getQuote, bool? updateBuySell, int? symbolToUpdate, int? groupsel, bool? updateStrategy, string filterCategory)
        {
            groupList.Clear();

            SelectListItem selectAll = new SelectListItem("V40+V40N+V200", constV40V40NV200);
            groupList.Add(selectAll);

            selectAll = new SelectListItem("V40", constV40, true);
            groupList.Add(selectAll);

            selectAll = new SelectListItem("V40N", constV40N);
            groupList.Add(selectAll);

            selectAll = new SelectListItem("V200", constV200);
            groupList.Add(selectAll);

            selectAll = new SelectListItem("All", constAll);
            groupList.Add(selectAll);

            if (_context.StockMaster != null)
            {
                menuList.Clear();
                SelectListItem menuItem = new SelectListItem("Update Strategy", "0");
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Get Quote", "1");
                menuList.Add(menuItem);
                menuItem = new SelectListItem("History", "2");
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Chart: History", "3");
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Chart: SMA-RSI-Stoch", "4");
                menuList.Add(menuItem);

                CurrentSort = sortOrder;
                SymbolSort = String.IsNullOrEmpty(sortOrder) ? "symbol_desc" : "";
                BuySignalSort = sortOrder == "STOCHBUY" ? "stochbuy_desc" : "STOCHBUY";
                SellSignalSort = sortOrder == "STOCHSELL" ? "stochsell_desc" : "STOCHSELL";
                if (searchString != null)
                {
                    pageIndex = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                IQueryable<StockMaster> stockmasterIQ = null;
                if(groupsel != null)
                {
                    CurrentGroup = groupsel;
                }
                else
                {
                    CurrentGroup = Int32.Parse(constV40);
                }

                if ((string.IsNullOrEmpty(filterCategory) == false) && filterCategory.Equals("Show & Update Category"))
                {
                    updateStrategy = true;
                }
                if (((id == null) || (id <= 0)) && (CurrentGroup != null))
                {
                    if ((updateStrategy != null) && (updateStrategy == true))
                    {
                        RefreshSTOCHBuySellForAll(CurrentGroup);
                    }
                }

                //if (refreshAll == true)
                //{
                //    RefreshSTOCHBuySellForAll(groupsel);
                //    RefreshAllStocks = false;
                //    refreshAll = false;
                //}


                if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constV40V40NV200)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => ((s.V40 == true) || (s.V40N == true) || (s.V200 == true))).AsNoTracking();
                    //groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constV40)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.V40 == true)).AsNoTracking();
                    //groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constV40N)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.V40N == true)).AsNoTracking();
                    //groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else if ((CurrentGroup != null) && (CurrentGroup == Int32.Parse(constV200)))
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.V200 == true)).AsNoTracking();
                    //groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else
                {
                    stockmasterIQ = _context.StockMaster.AsSplitQuery().AsNoTracking();//from s in _context.StockMaster select s;
                }
                groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;

                symbolList.Clear();
                symbolList = stockmasterIQ.OrderBy(a => a.Symbol).Select(a =>
                                                              new SelectListItem
                                                              {
                                                                  Value = a.StockMasterID.ToString(),
                                                                  Text = a.Symbol + "." + a.Exchange
                                                              }).ToList();

                if (((id != null) && (id > 0)) || (symbolToUpdate != null))
                {
                    if ((id == null) && (symbolToUpdate != null))
                    {
                        id = symbolToUpdate;
                    }

                    var selectedRecord = await _context.StockMaster.AsSplitQuery().FirstOrDefaultAsync(m => m.StockMasterID == id);
                    if (selectedRecord != null)
                    {
                        if ((getQuote == true) || (updateBuySell == true) || (symbolToUpdate != null))
                        {
                            DbInitializer.UpdateStockQuote(_context, selectedRecord);
                        }
                        if ((updateBuySell == true) || (symbolToUpdate != null))
                        {
                            DbInitializer.GetSTOCH_BUYSELL(_context, selectedRecord, "20", "20");
                            if (symbolToUpdate != null)
                            {
                                searchString = selectedRecord.Symbol;
                            }
                        }
                    }
                }

                CurrentFilter = searchString;

                //stockmasterIQ = _context.StockMaster.Where(s => (s.V40 == true));
                if (!String.IsNullOrEmpty(searchString))
                {
                    stockmasterIQ = stockmasterIQ.Where(s => s.Symbol.ToUpper().Contains(searchString.ToUpper())
                                                            || s.CompName.ToUpper().Contains(searchString.ToUpper()));

                    if(stockmasterIQ.Any())
                    {
                        symbolList.FirstOrDefault(a => a.Value.Equals(stockmasterIQ.First().StockMasterID.ToString())).Selected = true;
                    }
                }

                switch (sortOrder)
                {
                    case "symbol_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.Symbol);
                        break;
                    case "STOCHBUY":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.STOCH_BUY_SIGNAL);
                        break;
                    case "stochbuy_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.STOCH_BUY_SIGNAL);
                        break;
                    case "STOCHSELL":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.STOCH_SELL_SIGNAL);
                        break;
                    case "stochsell_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.STOCH_SELL_SIGNAL);
                        break;
                    default:
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.Symbol);
                        break;
                }
                var pageSize = Configuration.GetValue("PageSize", 10);
                CurrentPageIndex = pageIndex;
                StockMaster = await PaginatedList<StockMaster>.CreateAsync(stockmasterIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
            }
        }

        public void RefreshSTOCHBuySellForAll(int? currentSelectedGroup)
        {
            //IQueryable<StockMaster> stockmasterIQ = from s in _context.StockMaster select s;
            IQueryable<StockMaster> stockmasterIQ = null;
            if ((currentSelectedGroup != null) && (currentSelectedGroup == Int32.Parse(constV40V40NV200)))
            {
                stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => ((s.V40 == true) || (s.V40N == true) || (s.V200 == true))).AsNoTracking();
            }
            else if ((currentSelectedGroup != null) && (currentSelectedGroup == Int32.Parse(constV40)))
            {
                stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.V40 == true)).AsNoTracking();
            }
            else if ((currentSelectedGroup != null) && (currentSelectedGroup == Int32.Parse(constV40N)))
            {
                stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.V40N == true)).AsNoTracking();
            }
            else if ((currentSelectedGroup != null) && (currentSelectedGroup == Int32.Parse(constV200)))
            {
                stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.V200 == true)).AsNoTracking();
            }
            else 
            {
                stockmasterIQ = _context.StockMaster.AsSplitQuery().AsNoTracking();
            }

            foreach (var item in stockmasterIQ)
            {
                DbInitializer.GetSTOCH_BUYSELL(_context, item, "20", "20");
            }
        }

        public IActionResult OnPostStockAction(int? id, string menuitemsel, string sortOrder, int pageIndex,
                    string currentFilter, int? symbolToUpdate)
        {
            if ((id != null) && (menuitemsel.Equals("-1") == false))
            {
                StockMaster stockMaster = _context.StockMaster.AsSplitQuery().FirstOrDefault(m => m.StockMasterID == id);

                switch (menuitemsel)
                {
                    case "0"://update strategy
                        return RedirectToPage("./stochv40", new { id = id, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter, symbolToUpdate = symbolToUpdate });
                    case "1"://case get quote
                        return RedirectToPage("./stochv40", new { id = id, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter, getquote = true });
                    case "2": //case of history
                        return RedirectToPage("/History/Index", new { id = id });
                    case "3": //case of history chart
                        return RedirectToPage("/StandardIndicators/chartHistory", new { stockid = id, onlyhistory = 0, history = true });
                    case "4": //case of chart SMA RSI STOCH
                        return RedirectToPage("/StandardIndicators/chartSMARSISTOCH", new { id = id });
                    default:
                        return RedirectToPage("./smav40", new { id = id, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter });
                }
            }
            return RedirectToPage("./smav40", new { id = id, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter });
        }
    }
}