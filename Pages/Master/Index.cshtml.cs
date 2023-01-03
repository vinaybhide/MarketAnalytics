using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MarketAnalytics.Data;
using MarketAnalytics.Models;
using System.Data;
using System.Net;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MarketAnalytics.Pages.Master
{
    public class IndexModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;

        public IndexModel(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
        }

        public string ExchangeSort { get; set; }
        public string SymbolSort { get; set; }
        public string CompNameSort { get; set; }
        public string V40Sort { get; set; }
        public string V40NSort { get; set; }
        public string V200Sort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }

        public bool RefreshAllStocks { get; set; } = false;
        [BindProperty]
        public int CurrentID { get; set; }
        public PaginatedList<StockMaster> StockMaster { get; set; } = default!;

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex, int? id,
                    bool? refreshAll, bool? history, bool? getQuote, bool? v40, bool? v40N,
                    bool? v200, bool? lifetimeHighLow)
        {
            if (_context.StockMaster != null)
            {
                //StockMaster = await _context.StockMaster.ToListAsync();
                //Commented above line and Added following for sorting, searching, paging
                CurrentSort = sortOrder;
                SymbolSort = String.IsNullOrEmpty(sortOrder) ? "symbol_desc" : "";
                ExchangeSort = sortOrder == "Exchange" ? "exchange_desc" : "Exchange";
                CompNameSort = sortOrder == "CompName" ? "compname_desc" : "CompName";
                V40Sort = sortOrder == "V40" ? "v40_desc" : "V40";
                V40NSort = sortOrder == "V40N" ? "v40n_desc" : "V40N";
                V200Sort = sortOrder == "V200" ? "v200_desc" : "V200";

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
                    string fetchedData = await DbInitializer.FetchMasterData();
                    DbInitializer.Initialize(_context, fetchedData);

                    //RefreshAllStockMaster();
                    RefreshAllStocks = false;
                }

                if (id != null)
                {
                    var selectedRecord = await _context.StockMaster.FirstOrDefaultAsync(m => m.StockMasterID == id);
                    if (selectedRecord != null)
                    {
                        if (getQuote == true)
                        {
                            //DateTime quoteDate;
                            //double open, high, low, close, volume, change, changepercent, prevclose;
                            DateTime[] quoteDate = null;
                            double[] open, high, low, close, volume, change, changepercent, prevclose = null;

                            DbInitializer.GetQuote(selectedRecord.Symbol + "." + selectedRecord.Exchange, out quoteDate, out open,
                                out high, out low, out close,
                                out volume, out change, out changepercent, out prevclose);
                            if (quoteDate != null)
                            {
                                selectedRecord.QuoteDateTime = quoteDate[0];
                                selectedRecord.Open = open[0];
                                selectedRecord.High = high[0];
                                selectedRecord.Low = low[0];
                                selectedRecord.Close = close[0];
                                selectedRecord.Volume = volume[0];
                                selectedRecord.ChangePercent = changepercent[0];
                                selectedRecord.Change = change[0];
                                selectedRecord.PrevClose = prevclose[0];
                                _context.StockMaster.Update(selectedRecord);
                                _context.SaveChanges();
                            }

                            //List<BULLISH_ENGULFING_STRATEGY> listEngulfing = DbInitializer.GetBullishEngulfingBuySellList(_context, selectedRecord,
                            //    DateTime.Today.AddDays(-180), 30);
                        }
                        if ((lifetimeHighLow != null) && (lifetimeHighLow == true))
                        {
                            double high, low = 0;

                            DbInitializer.GetLifetimeHighLow(_context, selectedRecord);
                            //selectedRecord.LIFETIME_HIGH = high;
                            //selectedRecord.LIFETIME_LOW = low;
                            //_context.StockMaster.Update(selectedRecord);
                            //_context.SaveChanges();
                        }
                    }
                }

                CurrentFilter = searchString;

                IQueryable<StockMaster> stockmasterIQ = from s in _context.StockMaster select s;

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
                    case "Exchange":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.Exchange);
                        break;
                    case "exchange_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.Exchange);
                        break;
                    case "CompName":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.CompName);
                        break;
                    case "compname_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.CompName);
                        break;
                    case "V40":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.V40);
                        break;
                    case "v40_desc":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.V40);
                        break;
                    case "V40N":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.V40N);
                        break;
                    case "v40n_desc":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.V40N);
                        break;
                    case "V200":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.V200);
                        break;
                    case "v200_desc":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.V200);
                        break;

                    default:
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.Symbol);
                        break;
                }
                var pageSize = Configuration.GetValue("PageSize", 10);
                StockMaster = await PaginatedList<StockMaster>.CreateAsync(stockmasterIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
            }
        }
        public IActionResult SelectedRow(int id, int stockid)
        {
            CurrentID = stockid;
            return Page();
        }

    }
}
