using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Data;
using MarketAnalytics.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace MarketAnalytics.Pages.BuySell
{
    public class SMAV40Finder : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public List<SelectListItem> symbolList { get; set; }
        public List<SelectListItem> menuList { get; set; }

        public SMAV40Finder(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            symbolList = new List<SelectListItem>();
            menuList = new List<SelectListItem>();
        }

        public string BuySignalSort { get; set; }
        public string SymbolSort { get; set; }
        public string SellSignalSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }

        public bool RefreshAllStocks { get; set; } = false;
        public int CurrentID { get; set; }
        public PaginatedList<StockMaster> StockMaster { get; set; } = default!;

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex, int? id, bool? refreshAll, bool? getQuote, bool? updateBuySell, int? symbolToUpdate)
        {
            symbolList.Clear();
            symbolList = _context.StockMaster.AsSplitQuery().Where(x => x.V40 == true).OrderBy(a => a.Symbol).Select(a =>
                                                          new SelectListItem
                                                          {
                                                              Value = a.StockMasterID.ToString(),
                                                              Text = a.Symbol + "." + a.Exchange
                                                          }).ToList();

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

                //StockMaster = await _context.StockMaster.ToListAsync();
                //Commented above line and Added following for sorting, searching, paging
                CurrentSort = sortOrder;
                SymbolSort = String.IsNullOrEmpty(sortOrder) ? "symbol_desc" : "";
                BuySignalSort = sortOrder == "SMABUY" ? "smabuy_desc" : "SMABUY";
                SellSignalSort = sortOrder == "SMASELL" ? "smasell_desc" : "SMASELL";
                if(searchString != null)
                {
                    pageIndex = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                if(refreshAll == true)
                {
                    //string fetchedData = await DbInitializer.FetchMasterData();
                    //DbInitializer.Initialize(_context, fetchedData);

                    RefreshAllBuySellIndicators();
                    RefreshAllStocks = false;
                    refreshAll = false;
                }

                if ((id != null) || (symbolToUpdate != null))
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
                            DbInitializer.GetSMA_BUYSELL(_context, selectedRecord, 20, 50, 200);
                            if (symbolToUpdate != null)
                            {
                                searchString = selectedRecord.Symbol;
                            }
                        }
                    }
                }

                CurrentFilter = searchString;

                //IQueryable<StockMaster> stockmasterIQ = from s in _context.StockMaster select s;
                //stockmasterIQ = stockmasterIQ.Where(s => ((s.V40 == true) && (s.SMA_BUY_SIGNAL == true 
                //                                            || s.SMA_SELL_SIGNAL == true)));
                IQueryable<StockMaster> stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.V40 == true));
                if (!String.IsNullOrEmpty(searchString))
                {
                    stockmasterIQ = stockmasterIQ.AsSplitQuery().Where(s => s.Symbol.ToUpper().Contains(searchString.ToUpper())
                                                            || s.CompName.ToUpper().Contains(searchString.ToUpper()));
                }

                switch (sortOrder)
                {
                    case "symbol_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.Symbol);
                        break;
                    case "SMABUY":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.SMA_BUY_SIGNAL);
                        break;
                    case "smabuy_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.SMA_BUY_SIGNAL);
                        break;
                    case "SMASELL":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.SMA_SELL_SIGNAL);
                        break;
                    case "smasell_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.SMA_SELL_SIGNAL);
                        break;
                    default:
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.Symbol);
                        break;
                }
                var pageSize = Configuration.GetValue("PageSize", 10);
                StockMaster = await PaginatedList<StockMaster>.CreateAsync(stockmasterIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
            }
        }

        public void RefreshAllBuySellIndicators()
        {
            //IQueryable<StockMaster> stockmasterIQ = from s in _context.StockMaster select s;
            IQueryable<StockMaster> stockmasterIQ = _context.StockMaster.AsSplitQuery().Where(s => (s.V40 == true));

            foreach (var item in stockmasterIQ)
            {
                DbInitializer.GetSMA_BUYSELL(_context, item, 20, 50, 200);
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
                        return RedirectToPage("./smav40", new { id = id, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter, symbolToUpdate = symbolToUpdate });
                    case "1"://case get quote
                        return RedirectToPage("./smav40", new { id = id, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter, getquote = true });
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
