using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Data;
using MarketAnalytics.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace MarketAnalytics.Pages.BuySell
{
    public class rsiv40Model : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public List<SelectListItem> symbolList { get; set; }
        public List<SelectListItem> menuList { get; set; }

        public rsiv40Model(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
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
            symbolList = _context.StockMaster.Where(x => x.V40 == true).OrderBy(a => a.Symbol).Select(a =>
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

                CurrentSort = sortOrder;
                SymbolSort = String.IsNullOrEmpty(sortOrder) ? "symbol_desc" : "";
                BuySignalSort = sortOrder == "RSIOVERSOLD" ? "rsioversold_desc" : "RSIOVERSOLD";
                SellSignalSort = sortOrder == "RSIOVERBOUGHT" ? "rsioverbought_desc" : "RSIOVERBOUGHT";
                if (searchString != null)
                {
                    pageIndex = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                if (refreshAll == true)
                {
                    RefreshTrendForAll();
                    RefreshAllStocks = false;
                    refreshAll = false;
                }

                if ((id != null) || (symbolToUpdate != null))
                {
                    if ((id == null) && (symbolToUpdate != null))
                    {
                        id = symbolToUpdate;
                    }

                    var selectedRecord = await _context.StockMaster.FirstOrDefaultAsync(m => m.StockMasterID == id);
                    if (selectedRecord != null)
                    {
                        if ((getQuote == true) || (updateBuySell == true) || (symbolToUpdate != null))
                        {
                            DbInitializer.UpdateStockQuote(_context, selectedRecord);
                        }
                        if ((updateBuySell == true) || (symbolToUpdate != null))
                        {
                            DbInitializer.GetRSI_Trend(_context, selectedRecord, "14");
                            if (symbolToUpdate != null)
                            {
                                searchString = selectedRecord.Symbol;
                            }
                        }
                    }
                }

                CurrentFilter = searchString;

                IQueryable<StockMaster> stockmasterIQ = _context.StockMaster.Where(s => (s.V40 == true));
                if (!String.IsNullOrEmpty(searchString))
                {
                    stockmasterIQ = stockmasterIQ.Where(s => s.Symbol.ToUpper().Contains(searchString.ToUpper())
                                                            || s.CompName.ToUpper().Contains(searchString.ToUpper()));
                }

                switch (sortOrder)
                {
                    case "symbol_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.Symbol);
                        break;
                    case "RSIOVERSOLD":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.RSI_OVERSOLD);
                        break;
                    case "rsioversold_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.RSI_OVERSOLD);
                        break;
                    case "RSIOVERBOUGHT":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.RSI_OVERBOUGHT);
                        break;
                    case "rsioverbought_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.RSI_OVERBOUGHT);
                        break;
                    default:
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.Symbol);
                        break;
                }
                var pageSize = Configuration.GetValue("PageSize", 10);
                StockMaster = await PaginatedList<StockMaster>.CreateAsync(stockmasterIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
            }
        }

        public void RefreshTrendForAll()
        {
            //IQueryable<StockMaster> stockmasterIQ = from s in _context.StockMaster select s;
            IQueryable<StockMaster> stockmasterIQ = _context.StockMaster.Where(s => (s.V40 == true));

            foreach (var item in stockmasterIQ)
            {
                DbInitializer.GetRSI_Trend(_context, item, "14");
            }
        }
        public IActionResult OnPostStockAction(int? id, string menuitemsel, string sortOrder, int pageIndex,
                            string currentFilter, int? symbolToUpdate)
        {
            if ((id != null) && (menuitemsel.Equals("-1") == false))
            {
                StockMaster stockMaster = _context.StockMaster.FirstOrDefault(m => m.StockMasterID == id);

                switch (menuitemsel)
                {
                    case "0"://update strategy
                        return RedirectToPage("./rsiv40", new { id = id, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter, symbolToUpdate = symbolToUpdate });
                    case "1"://case get quote
                        return RedirectToPage("./rsiv40", new { id = id, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter, getquote = true });
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
