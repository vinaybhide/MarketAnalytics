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

        public V20BuySell(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            symbolList = new List<SelectListItem>();
        }
        public string BuySignalSort { get; set; }
        public string SymbolSort { get; set; }
        public string SellSignalSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }

        public bool RefreshAllStocks { get; set; } = false;
        public int CurrentID { get; set; }
        public PaginatedList<V20_CANDLE_STRATEGY> V20_CANDLE_STRATEGies { get; set; } = default!;

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex, int? id, bool? refreshAll, bool? getQuote, bool? updateBuySell, int? symbolToUpdate)
        {
            symbolList.Clear();
            symbolList = _context.StockMaster.Where(x => x.V200 == true).Select(a =>
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
                    var selectedRecord = await _context.V20_CANDLE_STRATEGY.FirstOrDefaultAsync(m => m.StockMaster.StockMasterID == id);
                    if (selectedRecord != null)
                    {
                        if ((getQuote == true) || (updateBuySell == true) || (symbolToUpdate != null))
                        {
                            //DateTime quoteDate;
                            //double open, high, low, close, volume, change, changepercent, prevclose;
                            DateTime[] quoteDate = null;
                            double[] open, high, low, close, volume, change, changepercent, prevclose = null;

                            DbInitializer.GetQuote(selectedRecord.StockMaster.Symbol + "." + selectedRecord.StockMaster.Exchange, out quoteDate, out open,
                                out high, out low, out close,
                                out volume, out change, out changepercent, out prevclose);
                            if (quoteDate != null)
                            {
                                selectedRecord.StockMaster.QuoteDateTime = quoteDate[0];
                                selectedRecord.StockMaster.Open = open[0];
                                selectedRecord.StockMaster.High = high[0];
                                selectedRecord.StockMaster.Low = low[0];
                                selectedRecord.StockMaster.Close = close[0];
                                selectedRecord.StockMaster.Volume = volume[0];
                                selectedRecord.StockMaster.ChangePercent = changepercent[0];
                                selectedRecord.StockMaster.Change = change[0];
                                selectedRecord.StockMaster.PrevClose = prevclose[0];
                                _context.StockMaster.Update(selectedRecord.StockMaster);
                                _context.SaveChanges();
                            }
                        }
                        if ((updateBuySell == true) || (symbolToUpdate != null))
                        {
                            DbInitializer.V20CandlesticPatternFinder(_context, selectedRecord.StockMaster);
                            if(symbolToUpdate != null)
                            {
                                searchString = selectedRecord.StockMaster.Symbol;
                            }
                        }
                    }
                }

                CurrentFilter = searchString;

                IQueryable<V20_CANDLE_STRATEGY> v20CandleIQ = from s in _context.V20_CANDLE_STRATEGY select s;
                v20CandleIQ = v20CandleIQ.Where(s => (s.StockMaster.V200 == true));
                if (!String.IsNullOrEmpty(searchString))
                {
                    v20CandleIQ = v20CandleIQ.Where(s => s.StockMaster.Symbol.ToUpper().Contains(searchString.ToUpper())
                                                            || s.StockMaster.CompName.ToUpper().Contains(searchString.ToUpper()));
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
            IQueryable<StockMaster> stockmasterIQ = from s in _context.StockMaster select s;
            stockmasterIQ = stockmasterIQ.Where(s => (s.V200 == true));
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
        //public void GetQuote(string symbol, out DateTime quoteDate, out double open, out double high, out double low, out double close, 
        //                    out double volume, out double change, out double changepercent, out double prevclose)
        //{
        //    try
        //    {
        //        quoteDate = DateTime.Now;
        //        open = high = low = close = volume = change = changepercent = prevclose = 0;

        //        string webservice_url = "";
        //        WebResponse wr;
        //        Stream receiveStream = null;
        //        StreamReader reader = null;
        //        //DataRow r;

        //        //https://query1.finance.yahoo.com/v7/finance/chart/HDFC.BO?range=1m&interval=1m&indicators=quote&timestamp=true
        //        webservice_url = string.Format(DbInitializer.urlGlobalQuote, symbol);

        //        Uri url = new Uri(webservice_url);
        //        var webRequest = WebRequest.Create(url);
        //        webRequest.Method = "GET";
        //        webRequest.ContentType = "application/json";
        //        wr = webRequest.GetResponseAsync().Result;
        //        receiveStream = wr.GetResponseStream();
        //        reader = new StreamReader(receiveStream);

        //        getQuoteTableFromJSON(reader.ReadToEnd(), symbol, out quoteDate, out open, out high, out low, out close, out volume,
        //                                out change, out changepercent, out prevclose);
        //        reader.Close();
        //        if (receiveStream != null)
        //            receiveStream.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        quoteDate = DateTime.Now;
        //        open = high = low = close = volume = change = changepercent = prevclose = 0;
        //    }
        //}

        //public void getQuoteTableFromJSON(string record, string symbol, out DateTime quoteDate, out double open, out double high, 
        //                out double low, out double close, out double volume, out double change, out double changepercent, out double prevclose)
        //{
        //    quoteDate = DateTime.Now;
        //    open = high = low = close = volume = change = changepercent = prevclose = 0;

        //    if (record.ToUpper().Contains("NOT FOUND"))
        //    {
        //        return;
        //    }
        //    DateTime myDate;
        //    var errors = new List<string>();
        //    try
        //    {
        //        Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(record, new JsonSerializerSettings
        //        {
        //            NullValueHandling = NullValueHandling.Ignore,
        //            DefaultValueHandling = DefaultValueHandling.Populate,
        //            Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
        //            {
        //                errors.Add(args.ErrorContext.Error.Message);
        //                args.ErrorContext.Handled = true;
        //                //args.ErrorContext.Handled = false;
        //            }
        //            //Converters = { new IsoDateTimeConverter() }

        //        });

        //        Chart myChart = myDeserializedClass.chart;

        //        Result myResult = myChart.result[0];

        //        Meta myMeta = myResult.meta;

        //        Indicators myIndicators = myResult.indicators;

        //        //this will be typically only 1 row and quote will have list of close, high, low, open, volume
        //        Quote myQuote = myIndicators.quote[0];

        //        //this will be typically only 1 row and adjClose will have list of adjClose
        //        //Adjclose myAdjClose = null;
        //        //if (bIsDaily)
        //        //{
        //        //    myAdjClose = myIndicators.adjclose[0];
        //        //}

        //        if (myResult.timestamp != null)
        //        {
        //            //for (int i = 0; i < myResult.timestamp.Count; i++)
        //            for (int i = 0; i <= 0; i++)
        //            {
        //                if ((myQuote.close[i] == null) && (myQuote.high[i] == null) && (myQuote.low[i] == null) && (myQuote.open[i] == null)
        //                    && (myQuote.volume[i] == null))
        //                {
        //                    continue;
        //                }

        //                //myDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(myResult.timestamp[i]).ToLocalTime();
        //                myDate = convertUnixEpochToLocalDateTime(myResult.timestamp[i], myMeta.timezone);

        //                //myDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(myResult.timestamp[i]);
        //                //string formatedDate = myDate.ToString("dd-MM-yyyy");
        //                //formatedDate = myDate.ToString("yyyy-dd-MM");

        //                //myDate = System.Convert.ToDateTime(myResult.timestamp[i]);

        //                //if all are null do not enter this row

        //                if (myQuote.close[i] == null)
        //                {
        //                    close = 0.00;
        //                }
        //                else
        //                {
        //                    //close = (double)myQuote.close[i];
        //                    close = System.Convert.ToDouble(string.Format("{0:0.00}", myQuote.close[i]));
        //                }

        //                if (myQuote.high[i] == null)
        //                {
        //                    high = 0.00;
        //                }
        //                else
        //                {
        //                    //high = (double)myQuote.high[i];
        //                    high = System.Convert.ToDouble(string.Format("{0:0.00}", myQuote.high[i]));
        //                }

        //                if (myQuote.low[i] == null)
        //                {
        //                    low = 0.00;
        //                }
        //                else
        //                {
        //                    //low = (double)myQuote.low[i];
        //                    low = System.Convert.ToDouble(string.Format("{0:0.00}", myQuote.low[i]));
        //                }

        //                if (myQuote.open[i] == null)
        //                {
        //                    open = 0.00;
        //                }
        //                else
        //                {
        //                    //open = (double)myQuote.open[i];
        //                    open = System.Convert.ToDouble(string.Format("{0:0.00}", myQuote.open[i]));
        //                }
        //                if (myQuote.volume[i] == null)
        //                {
        //                    volume = 0;
        //                }
        //                else
        //                {
        //                    volume = (int)myQuote.volume[i];
        //                }
        //                prevclose = System.Convert.ToDouble(string.Format("{0:0.00}", myMeta.chartPreviousClose));
        //                change = close - prevclose;
        //                changepercent = (change / prevclose) * 100;
        //                change = System.Convert.ToDouble(string.Format("{0:0.00}", change));
        //                changepercent = System.Convert.ToDouble(string.Format("{0:0.00}", changepercent));
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        quoteDate = DateTime.Now;
        //        open = high = low = close = volume = change = changepercent = prevclose = 0;
        //    }
        //}
        //public string findTimeZoneId(string zoneId)
        //{
        //    string returnTimeZoneId = "";
        //    switch (zoneId)
        //    {
        //        case "IST":
        //            returnTimeZoneId = "India Standard Time";
        //            break;
        //        default:
        //            returnTimeZoneId = "India Standard Time";
        //            break;
        //    }
        //    return returnTimeZoneId;
        //}

        //public DateTime convertUnixEpochToLocalDateTime(long dateEpoch, string zoneId)
        //{
        //    DateTime localDateTime;

        //    DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(dateEpoch);
        //    string timeZoneId = findTimeZoneId(zoneId);
        //    TimeZoneInfo currentTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        //    localDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeOffset.UtcDateTime, currentTimeZone);

        //    return localDateTime;
        //}

    }
}
