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
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace MarketAnalytics.Pages.BuySell
{
    public class V20BuySell : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public List<SelectListItem> symbolList { get; set; }
        public List<SelectListItem> currentSymbolList { get; set; }

        public V20BuySell(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            symbolList = new List<SelectListItem>();
            currentSymbolList = new List<SelectListItem>();
        }
        public string BuySignalSort { get; set; }
        public string SymbolSort { get; set; }
        public string SellSignalSort { get; set; }
        public string FromDtSort { get; set; }
        public string ToDtSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }

        public bool RefreshAllStocks { get; set; } = false;
        public int CurrentID { get; set; }
        public PaginatedList<V20_CANDLE_STRATEGY> V20_CANDLE_STRATEGies { get; set; } = default!;

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex, int? id, bool? refreshAll, bool? getQuote, bool? updateBuySell, int? symbolToUpdate)
        {
            symbolList.Clear();
            symbolList = _context.StockMaster.Where(x => ((x.V200 == true) || (x.V40 == true) || (x.V40N == true))).Select(a =>
                                                          new SelectListItem
                                                          {
                                                              Value = a.StockMasterID.ToString(),
                                                              Text = a.Symbol
                                                          }).ToList();
            //symbolList = _context.StockMaster.Select(a =>
            //                                  new SelectListItem 
            //                                  {
            //                                      Value = a.StockMasterID.ToString(),
            //                                      Text = a.Symbol
            //                                  }).ToList();

            if (_context.V20_CANDLE_STRATEGY != null)
            {
                CurrentSort = sortOrder;
                SymbolSort = String.IsNullOrEmpty(sortOrder) ? "symbol_desc" : "";
                BuySignalSort = sortOrder == "BUY_PRICE" ? "buyprice_desc" : "BUY_PRICE";
                SellSignalSort = sortOrder == "SELL_PRICE" ? "sellprice_desc" : "SELL_PRICE";
                FromDtSort = sortOrder == "FROM_DT" ? "fromdt_desc" : "FROM_DT";
                ToDtSort = sortOrder == "TO_DT" ? "todt_desc" : "TO_DT";
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
                    var selectedRecord = await _context.StockMaster.FirstOrDefaultAsync(m => m.StockMasterID == id);
                    if (selectedRecord != null)
                    {
                        if ((getQuote == true) || (updateBuySell == true) || (symbolToUpdate != null))
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
                        }
                        if ((updateBuySell == true) || (symbolToUpdate != null))
                        {
                            DbInitializer.V20CandlesticPatternFinder(_context, selectedRecord);
                            if (symbolToUpdate != null)
                            {
                                searchString = selectedRecord.Symbol;
                            }
                        }
                    }
                }

                CurrentFilter = searchString;

                IQueryable<V20_CANDLE_STRATEGY> v20CandleIQ = _context.V20_CANDLE_STRATEGY.Where(s => ((s.StockMaster.V200 == true) || 
                                                                             (s.StockMaster.V40==true) || (s.StockMaster.V40N == true))
                                                                             && (s.StockMaster.Close < s.BUY_PRICE));
                currentSymbolList.Clear();
                //currentSymbolList = _context.V20_CANDLE_STRATEGY.Where(s => (s.StockMaster.V200 == true))
                //    .OrderBy(x => x.StockMaster.Symbol)
                //    .Distinct()
                //    .Select(a =>
                //        new SelectListItem
                //        {
                //            Value = a.StockMaster.Symbol.ToString(),
                //            Text = a.StockMaster.Symbol.ToString()
                //        }).ToList();

                currentSymbolList = v20CandleIQ //.Where(x => x.StockMaster.V200 == true)
                                        //.GroupBy(a => a.StockMaster.Symbol)
                                            .OrderBy(a => a.StockMasterID)
                                            .Distinct()
                                                .Select(a =>
                                                    new SelectListItem
                                                    {
                                                        Value = a.StockMaster.Symbol.ToString(),
                                                        Text = a.StockMaster.Symbol.ToString()
                                                    }).Distinct().ToList();
               
                if (!String.IsNullOrEmpty(searchString))
                {
                    v20CandleIQ = v20CandleIQ.Where(s => s.StockMaster.Symbol.ToUpper().Contains(searchString.ToUpper())
                                                            || s.StockMaster.CompName.ToUpper().Contains(searchString.ToUpper()));

                    if (v20CandleIQ.Count() == 0)
                    {
                        v20CandleIQ = _context.V20_CANDLE_STRATEGY.Where(s => (s.StockMaster.V200 == true));
                    }

                    if (currentSymbolList.Exists(a => (a.Value.Equals(searchString) == true)))
                    {
                        currentSymbolList.FirstOrDefault(a => a.Value.Equals(searchString)).Selected = true;
                    }
                }

                switch (sortOrder)
                {
                    case "symbol_desc":
                        v20CandleIQ = v20CandleIQ.OrderByDescending(s => s.StockMaster.Symbol);
                        break;
                    case "BUY_PRICE":
                        v20CandleIQ = v20CandleIQ.OrderBy(s => s.BUY_PRICE);
                        break;
                    case "buyprice_desc":
                        v20CandleIQ = v20CandleIQ.OrderByDescending(s => s.BUY_PRICE);
                        break;
                    case "SELL_PRICE":
                        v20CandleIQ = v20CandleIQ.OrderBy(s => s.SELL_PRICE);
                        break;
                    case "sellprice_desc":
                        v20CandleIQ = v20CandleIQ.OrderByDescending(s => s.SELL_PRICE);
                        break;
                    case "FROM_DT":
                        v20CandleIQ = v20CandleIQ.OrderBy(s => s.FROM_DATE);
                        break;
                    case "fromdt_desc":
                        v20CandleIQ = v20CandleIQ.OrderByDescending(s => s.FROM_DATE);
                        break;
                    case "TO_DT":
                        v20CandleIQ = v20CandleIQ.OrderBy(s => s.TO_DATE);
                        break;
                    case "todt_desc":
                        v20CandleIQ = v20CandleIQ.OrderByDescending(s => s.TO_DATE);
                        break;
                    default:
                        v20CandleIQ = v20CandleIQ.OrderBy(s => s.StockMaster.Symbol);
                        break;
                }
                var pageSize = Configuration.GetValue("PageSize", 10);
                V20_CANDLE_STRATEGies = await PaginatedList<V20_CANDLE_STRATEGY>.CreateAsync(v20CandleIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
            }
        }

        public void RefreshAllBuySellIndicators()
        {
            //IQueryable<StockMaster> stockmasterIQ = from s in _context.StockMaster select s;
            IQueryable<StockMaster> stockmasterIQ = _context.StockMaster.Where(s => ((s.V200 == true) || (s.V40 == true) || (s.V40N == true)));
            try
            {
                foreach (var item in stockmasterIQ)
                {
                    DbInitializer.V20CandlesticPatternFinder(_context, item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
