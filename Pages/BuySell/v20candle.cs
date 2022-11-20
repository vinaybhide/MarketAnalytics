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
using System.Drawing.Printing;

namespace MarketAnalytics.Pages.BuySell
{
    public class V20Finder : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;

        public V20Finder(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
        }
        public List<StockMaster> StockMasters { get; set; } = default!;
        
        public string v20IndicatorSort { get; set; }
        public string SymbolSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }

        public bool RefreshAllStocks { get; set; } = false;
        public int CurrentID { get; set; }

        public async Task OnGetAsync()
        {
            IQueryable<StockMaster> stockmasterIQ = from s in _context.StockMaster select s;
            //stockmasterIQ = stockmasterIQ.Where(s => ((s.V40 == true) && (s.SMA_BUY_SIGNAL == true 
            //                                            || s.SMA_SELL_SIGNAL == true)));
            stockmasterIQ = stockmasterIQ.Where(s => (s.V200 == true));
            //StockMasters = await PaginatedList<StockMaster>.CreateAsync(stockmasterIQ.Include(c=>c.collection_V20_buysell).AsNoTracking(), pageIndex ?? 1, pageSize)
            StockMasters = await stockmasterIQ.Include(c => c.collection_V20_buysell).AsNoTracking().ToListAsync();
        }
        //public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex, int? id, bool? refreshAll, bool? history)
        //{
        //    if (_context.StockPriceHistory != null)
        //    {
        //        //StockMaster = await _context.StockMaster.ToListAsync();
        //        //Commented above line and Added following for sorting, searching, paging
        //        CurrentSort = sortOrder;
        //        SymbolSort = String.IsNullOrEmpty(sortOrder) ? "symbol_desc" : "";
        //        v20IndicatorSort = sortOrder == "V20_CANDLE" ? "v20_desc" : "V20_CANDLE";
        //        if(searchString != null)
        //        {
        //            pageIndex = 1;
        //        }
        //        else
        //        {
        //            searchString = currentFilter;
        //        }

        //        if(refreshAll == true)
        //        {
        //            RefreshAllBuySellIndicators();
        //            RefreshAllStocks = false;
        //            refreshAll = false;
        //        }

        //        if(id != null)
        //        {
        //            var selectedRecord = await _context.StockMaster.FirstOrDefaultAsync(m => m.StockMasterID == id);
        //            if (selectedRecord != null)
        //            {
        //                if((history == null) || (history == false))
        //                {
        //                    //DateTime quoteDate;
        //                    //double open, high, low, close, volume, change, changepercent, prevclose;
        //                    DateTime[] quoteDate = null;
        //                    double[] open, high, low, close, volume, change, changepercent, prevclose = null;

        //                    DbInitializer.GetQuote(selectedRecord.Symbol + "." + selectedRecord.Exchange, out quoteDate, out open,
        //                        out high, out low, out close,
        //                        out volume, out change, out changepercent, out prevclose);
        //                    if (quoteDate != null)
        //                    {
        //                        selectedRecord.QuoteDateTime = quoteDate[0];
        //                        selectedRecord.Open = open[0];
        //                        selectedRecord.High = high[0];
        //                        selectedRecord.Low = low[0];
        //                        selectedRecord.Close = close[0];
        //                        selectedRecord.Volume = volume[0];
        //                        selectedRecord.ChangePercent = changepercent[0];
        //                        selectedRecord.Change = change[0];
        //                        selectedRecord.PrevClose = prevclose[0];
        //                        _context.StockMaster.Update(selectedRecord);
        //                        _context.SaveChanges();
        //                    }
        //                    DbInitializer.V20CandlesticPatternFinder(_context, selectedRecord);
        //                }
        //            }
        //        }

        //        CurrentFilter = searchString;

        //        IQueryable<StockMaster> stockmasterIQ = from s in _context.StockMaster select s;
        //        //stockmasterIQ = stockmasterIQ.Where(s => ((s.V40 == true) && (s.SMA_BUY_SIGNAL == true 
        //        //                                            || s.SMA_SELL_SIGNAL == true)));
        //        stockmasterIQ = stockmasterIQ.Where(s => (s.V200 == true));
        //        if (!String.IsNullOrEmpty(searchString))
        //        {
        //            stockmasterIQ = stockmasterIQ.Where(s => s.Symbol.ToUpper().Contains(searchString.ToUpper())
        //                                                    || s.CompName.ToUpper().Contains(searchString.ToUpper()));
        //        }

        //        switch (sortOrder)
        //        {
        //            case "symbol_desc":
        //                stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.Symbol);
        //                break;
        //            case "V20_CANDLE":
        //                stockmasterIQ = stockhistoryIQ.OrderBy(s => s.V20_CANDLE);
        //                break;
        //            default:
        //                stockhistoryIQ = stockhistoryIQ.OrderBy(s => s.StockMaster.Symbol);
        //                break;
        //        }
        //        var pageSize = Configuration.GetValue("PageSize", 10);
        //        StockHistoryList = await PaginatedList<StockPriceHistory>.CreateAsync(stockhistoryIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
        //    }
        //}

        public void RefreshAllBuySellIndicators()
        {
            IQueryable<StockMaster> stockmasterIQ = from s in _context.StockMaster select s;
            stockmasterIQ = stockmasterIQ.Where(s => (s.V200 == true));

            foreach (var item in stockmasterIQ)
            {
                DbInitializer.V20CandlesticPatternFinder(_context, item);
            }
        }
    }
}
