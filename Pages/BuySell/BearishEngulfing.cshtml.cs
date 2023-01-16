using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Data;
using MarketAnalytics.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace MarketAnalytics.Pages.BuySell
{
    public class BearishEngulfing : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public List<SelectListItem> symbolList { get; set; }
        public List<SelectListItem> currentSymbolList { get; set; }

        public string BuySignalSort { get; set; }
        public string SymbolSort { get; set; }
        public string SellSignalSort { get; set; }
        public string FromDtSort { get; set; }
        public string ToDtSort { get; set; }
        public string CurrentFilter { get; set; }
        public int TrendSpan { get; set; }
        public int DaysBehind { get; set; }
public int SMAValue { get; set; }
        public string CurrentSort { get; set; }

        //public bool RefreshAllStocks { get; set; } = false;
        public int CurrentID { get; set; }
        public PaginatedList<BEARISH_ENGULFING> listBEARISH_ENGULFING { get; set; } = default!;

        public BearishEngulfing(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            symbolList = new List<SelectListItem>();
            currentSymbolList = new List<SelectListItem>();
            TrendSpan = 10;
            DaysBehind = 180;
            SMAValue = 30;
        }

        //public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex, int? id, bool? refreshAll, bool? getQuote, bool? updateBuySell, int? symbolToUpdate,
        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex, int? id, bool? getQuote, bool? updateBuySell, int? symbolToUpdate,
            int? trendSpan, int? daysBehind, int? smaValue)
        {
            symbolList.Clear();
            symbolList = _context.StockMaster.Where(x => ((x.V200 == true) || (x.V40 == true) || (x.V40N == true))).Select(a =>
                                                          new SelectListItem
                                                          {
                                                              Value = a.StockMasterID.ToString(),
                                                              Text = a.Symbol
                                                          }).ToList();

            SelectListItem selectAll = new SelectListItem("-- Select All --", "-99");
            symbolList.Insert(0,selectAll);

            if (_context.BEARISH_ENGULFING != null)
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

                if((trendSpan!= null) && (trendSpan > 0))
                {
                    TrendSpan = (int)trendSpan;
                }
                else
                {
                    TrendSpan = 10;
                }
                if((daysBehind != null) && (daysBehind > 0))
                {
                    DaysBehind = (int)daysBehind;
                }
                else
                {
                    DaysBehind = 180;
                }
                if((smaValue != null) && (smaValue >= 0))
                {
                    SMAValue = (int)smaValue;
                }
                else
                {
                    SMAValue = 30;
                }

                //if (refreshAll == true)
                if((symbolToUpdate != null) &&  (symbolToUpdate == -99))
                {
                    RefreshAllBuySellIndicators();
                    //RefreshAllStocks = false;
                    //refreshAll = false;
                }

                if ( (id != null) || ((symbolToUpdate != null) && (symbolToUpdate != -99)))
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
                            DbInitializer.GetQuote(selectedRecord.Symbol + (selectedRecord.Exchange.Length == 0 ? "" : ("." + selectedRecord.Exchange)), out quoteDate, out open,
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
                            DbInitializer.GetBearishEngulfingBuySellList(_context, selectedRecord, DateTime.Today.AddDays(-DaysBehind), TrendSpan);
                            if (symbolToUpdate != null)
                            {
                                searchString = selectedRecord.Symbol;
                            }
                        }
                    }
                }

                CurrentFilter = searchString;

                IQueryable<BEARISH_ENGULFING> bearishengulfingCandleIQ = _context.BEARISH_ENGULFING.Where(s => ((s.StockMaster.V200 == true)
                                                                              || (s.StockMaster.V40 == true) || (s.StockMaster.V40N == true))
                                                                              && (s.StockMaster.Close <= s.BUY_PRICE));
                currentSymbolList.Clear();

                currentSymbolList = bearishengulfingCandleIQ //.Where(x => x.StockMaster.V200 == true)
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
                    bearishengulfingCandleIQ = bearishengulfingCandleIQ.Where(s => s.StockMaster.Symbol.ToUpper().Contains(searchString.ToUpper())
                                                            || s.StockMaster.CompName.ToUpper().Contains(searchString.ToUpper()));
                    if(bearishengulfingCandleIQ.Count() == 0)
                    {
                        //bearishengulfingCandleIQ = _context.BEARISH_ENGULFING.Where(s => ((s.StockMaster.V200 == true)
                        //                            || (s.StockMaster.V40N == true) || (s.StockMaster.V40 == true)));
                        bearishengulfingCandleIQ = _context.BEARISH_ENGULFING.Where(s => ((s.StockMaster.V200 == true)
                                                                              || (s.StockMaster.V40 == true) || (s.StockMaster.V40N == true))
                                                                              && (s.StockMaster.Close <= s.BUY_PRICE));
                    }
                    if (currentSymbolList.Exists(a => (a.Value.Equals(searchString) == true)))
                    {
                        currentSymbolList.FirstOrDefault(a => a.Value.Equals(searchString)).Selected = true;
                    }
                }

                switch (sortOrder)
                {
                    case "symbol_desc":
                        bearishengulfingCandleIQ = bearishengulfingCandleIQ.OrderByDescending(s => s.StockMaster.Symbol);
                        break;
                    case "BUY_PRICE":
                        bearishengulfingCandleIQ = bearishengulfingCandleIQ.OrderBy(s => s.BUY_PRICE);
                        break;
                    case "buyprice_desc":
                        bearishengulfingCandleIQ = bearishengulfingCandleIQ.OrderByDescending(s => s.BUY_PRICE);
                        break;
                    case "SELL_PRICE":
                        bearishengulfingCandleIQ = bearishengulfingCandleIQ.OrderBy(s => s.SELL_PRICE);
                        break;
                    case "sellprice_desc":
                        bearishengulfingCandleIQ = bearishengulfingCandleIQ.OrderByDescending(s => s.SELL_PRICE);
                        break;
                    case "FROM_DT":
                        bearishengulfingCandleIQ = bearishengulfingCandleIQ.OrderBy(s => s.BUY_CANDLE_DATE);
                        break;
                    case "fromdt_desc":
                        bearishengulfingCandleIQ = bearishengulfingCandleIQ.OrderByDescending(s => s.BUY_CANDLE_DATE);
                        break;
                    case "TO_DT":
                        bearishengulfingCandleIQ = bearishengulfingCandleIQ.OrderBy(s => s.SELL_CANDLE_DATE);
                        break;
                    case "todt_desc":
                        bearishengulfingCandleIQ = bearishengulfingCandleIQ.OrderByDescending(s => s.SELL_CANDLE_DATE);
                        break;
                    default:
                        bearishengulfingCandleIQ = bearishengulfingCandleIQ.OrderBy(s => s.StockMaster.Symbol);
                        break;
                }
                var pageSize = Configuration.GetValue("PageSize", 10);
                listBEARISH_ENGULFING = await PaginatedList<BEARISH_ENGULFING>.CreateAsync(bearishengulfingCandleIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
            }
        }

        public void RefreshAllBuySellIndicators()
        {
            IQueryable<StockMaster> stockmasterIQ = _context.StockMaster.Where(s => ((s.V200 == true) || (s.V40 == true) || (s.V40N == true)));
            try
            {
                foreach (var item in stockmasterIQ)
                {
                    DbInitializer.GetBearishEngulfingBuySellList(_context, item, DateTime.Today.AddDays(-DaysBehind), TrendSpan);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
