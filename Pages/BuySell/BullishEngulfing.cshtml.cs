﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Data;
using MarketAnalytics.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

namespace MarketAnalytics.Pages.BuySell
{
    public class BullishEngulfing : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public List<SelectListItem> symbolList { get; set; }
        public List<SelectListItem> currentSymbolList { get; set; }

        public BullishEngulfing(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
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
        public PaginatedList<BULLISH_ENGULFING_STRATEGY> BULLISH_ENGULFING_STRATEGYies { get; set; } = default!;

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex, int? id, bool? refreshAll, bool? getQuote, bool? updateBuySell, int? symbolToUpdate)
        {
            symbolList.Clear();
            symbolList = _context.StockMaster.Where(x => x.V200 == true).Select(a =>
                                                          new SelectListItem
                                                          {
                                                              Value = a.StockMasterID.ToString(),
                                                              Text = a.Symbol
                                                          }).ToList();

            if (_context.BULLISH_ENGULFING_STRATEGY != null)
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
                            searchString = "";
                        }
                        if ((updateBuySell == true) || (symbolToUpdate != null))
                        {
                            DbInitializer.GetBullishEngulfingBuySellList(_context, selectedRecord, DateTime.Today.AddDays(-180), 30);
                            if (symbolToUpdate != null)
                            {
                                searchString = selectedRecord.Symbol;
                            }
                        }
                    }
                }

                CurrentFilter = searchString;

                IQueryable<BULLISH_ENGULFING_STRATEGY> bullishengulfingCandleIQ = _context.BULLISH_ENGULFING_STRATEGY.Where(s => (s.StockMaster.V200 == true));
                currentSymbolList.Clear();

                currentSymbolList = bullishengulfingCandleIQ //.Where(x => x.StockMaster.V200 == true)
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
                    bullishengulfingCandleIQ = bullishengulfingCandleIQ.Where(s => s.StockMaster.Symbol.ToUpper().Contains(searchString.ToUpper())
                                                            || s.StockMaster.CompName.ToUpper().Contains(searchString.ToUpper()));
                    if(bullishengulfingCandleIQ.Count() == 0)
                    {
                        bullishengulfingCandleIQ = _context.BULLISH_ENGULFING_STRATEGY.Where(s => (s.StockMaster.V200 == true));
                    }
                    if (currentSymbolList.Exists(a => (a.Value.Equals(searchString) == true)))
                    {
                        currentSymbolList.FirstOrDefault(a => a.Value.Equals(searchString)).Selected = true;
                    }
                }

                switch (sortOrder)
                {
                    case "symbol_desc":
                        bullishengulfingCandleIQ = bullishengulfingCandleIQ.OrderByDescending(s => s.StockMaster.Symbol);
                        break;
                    case "BUY_PRICE":
                        bullishengulfingCandleIQ = bullishengulfingCandleIQ.OrderBy(s => s.BUY_PRICE);
                        break;
                    case "buyprice_desc":
                        bullishengulfingCandleIQ = bullishengulfingCandleIQ.OrderByDescending(s => s.BUY_PRICE);
                        break;
                    case "SELL_PRICE":
                        bullishengulfingCandleIQ = bullishengulfingCandleIQ.OrderBy(s => s.SELL_PRICE);
                        break;
                    case "sellprice_desc":
                        bullishengulfingCandleIQ = bullishengulfingCandleIQ.OrderByDescending(s => s.SELL_PRICE);
                        break;
                    case "FROM_DT":
                        bullishengulfingCandleIQ = bullishengulfingCandleIQ.OrderBy(s => s.BIG_CANDLE_DATE);
                        break;
                    case "fromdt_desc":
                        bullishengulfingCandleIQ = bullishengulfingCandleIQ.OrderByDescending(s => s.BIG_CANDLE_DATE);
                        break;
                    case "TO_DT":
                        bullishengulfingCandleIQ = bullishengulfingCandleIQ.OrderBy(s => s.SMALL_CANDLE_DATE);
                        break;
                    case "todt_desc":
                        bullishengulfingCandleIQ = bullishengulfingCandleIQ.OrderByDescending(s => s.SMALL_CANDLE_DATE);
                        break;
                    default:
                        bullishengulfingCandleIQ = bullishengulfingCandleIQ.OrderBy(s => s.StockMaster.Symbol);
                        break;
                }
                var pageSize = Configuration.GetValue("PageSize", 10);
                BULLISH_ENGULFING_STRATEGYies = await PaginatedList<BULLISH_ENGULFING_STRATEGY>.CreateAsync(bullishengulfingCandleIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
            }
        }

        public void RefreshAllBuySellIndicators()
        {
            IQueryable<StockMaster> stockmasterIQ = _context.StockMaster.Where(s => (s.V200 == true));
            try
            {
                foreach (var item in stockmasterIQ)
                {
                    DbInitializer.GetBullishEngulfingBuySellList(_context, item, DateTime.Today.AddDays(-180), 30);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}