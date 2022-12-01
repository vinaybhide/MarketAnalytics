﻿using System;
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

                if(refreshAll == true)
                {
                    string fetchedData = await DbInitializer.FetchMasterData();
                    DbInitializer.Initialize(_context, fetchedData);

                    //RefreshAllStockMaster();
                    RefreshAllStocks = false;
                }

                if(id != null)
                {
                    var selectedRecord = await _context.StockMaster.FirstOrDefaultAsync(m => m.StockMasterID == id);
                    if (selectedRecord != null)
                    {
                        if(getQuote == true)
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

                            List<BULLISH_ENGULFING_STRATEGY> listEngulfing = DbInitializer.GetBullishEngulfingBuySellList(_context, selectedRecord,
                                DateTime.Today.AddDays(-180), 30);
                        }
                        if((lifetimeHighLow != null) && (lifetimeHighLow == true))
                        {
                            double high, low = 0;

                            DbInitializer.GetLifetimeHighLow(_context, selectedRecord, out high, out low);
                            selectedRecord.LIFETIME_HIGH = high;
                            selectedRecord.LIFETIME_LOW = low;
                            _context.StockMaster.Update(selectedRecord);
                            _context.SaveChanges();
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

        public void RefreshAllStockMaster()
        {

        }
        public void GetQuote(string symbol, out DateTime quoteDate, out double open, out double high, out double low, out double close, 
                            out double volume, out double change, out double changepercent, out double prevclose)
        {
            try
            {
                quoteDate = DateTime.Now;
                open = high = low = close = volume = change = changepercent = prevclose = 0;

                string webservice_url = "";
                WebResponse wr;
                Stream receiveStream = null;
                StreamReader reader = null;
                //DataRow r;

                //https://query1.finance.yahoo.com/v7/finance/chart/HDFC.BO?range=1m&interval=1m&indicators=quote&timestamp=true
                webservice_url = string.Format(DbInitializer.urlGlobalQuote, symbol);

                Uri url = new Uri(webservice_url);
                var webRequest = WebRequest.Create(url);
                webRequest.Method = "GET";
                webRequest.ContentType = "application/json";
                wr = webRequest.GetResponseAsync().Result;
                receiveStream = wr.GetResponseStream();
                reader = new StreamReader(receiveStream);

                getQuoteTableFromJSON(reader.ReadToEnd(), symbol, out quoteDate, out open, out high, out low, out close, out volume,
                                        out change, out changepercent, out prevclose);
                reader.Close();
                if (receiveStream != null)
                    receiveStream.Close();
            }
            catch (Exception ex)
            {
                quoteDate = DateTime.Now;
                open = high = low = close = volume = change = changepercent = prevclose = 0;
            }
        }

        public void getQuoteTableFromJSON(string record, string symbol, out DateTime quoteDate, out double open, out double high, 
                        out double low, out double close, out double volume, out double change, out double changepercent, out double prevclose)
        {
            quoteDate = DateTime.Now;
            open = high = low = close = volume = change = changepercent = prevclose = 0;

            if (record.ToUpper().Contains("NOT FOUND"))
            {
                return;
            }
            DateTime myDate;
            var errors = new List<string>();
            try
            {
                Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(record, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Populate,
                    Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                    {
                        errors.Add(args.ErrorContext.Error.Message);
                        args.ErrorContext.Handled = true;
                        //args.ErrorContext.Handled = false;
                    }
                    //Converters = { new IsoDateTimeConverter() }

                });

                Chart myChart = myDeserializedClass.chart;

                Result myResult = myChart.result[0];

                Meta myMeta = myResult.meta;

                Indicators myIndicators = myResult.indicators;

                //this will be typically only 1 row and quote will have list of close, high, low, open, volume
                Quote myQuote = myIndicators.quote[0];

                //this will be typically only 1 row and adjClose will have list of adjClose
                //Adjclose myAdjClose = null;
                //if (bIsDaily)
                //{
                //    myAdjClose = myIndicators.adjclose[0];
                //}

                if (myResult.timestamp != null)
                {
                    //for (int i = 0; i < myResult.timestamp.Count; i++)
                    for (int i = 0; i <= 0; i++)
                    {
                        if ((myQuote.close[i] == null) && (myQuote.high[i] == null) && (myQuote.low[i] == null) && (myQuote.open[i] == null)
                            && (myQuote.volume[i] == null))
                        {
                            continue;
                        }

                        //myDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(myResult.timestamp[i]).ToLocalTime();
                        myDate = convertUnixEpochToLocalDateTime(myResult.timestamp[i], myMeta.timezone);

                        //myDate = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(myResult.timestamp[i]);
                        //string formatedDate = myDate.ToString("dd-MM-yyyy");
                        //formatedDate = myDate.ToString("yyyy-dd-MM");

                        //myDate = System.Convert.ToDateTime(myResult.timestamp[i]);

                        //if all are null do not enter this row

                        if (myQuote.close[i] == null)
                        {
                            close = 0.00;
                        }
                        else
                        {
                            //close = (double)myQuote.close[i];
                            close = System.Convert.ToDouble(string.Format("{0:0.00}", myQuote.close[i]));
                        }

                        if (myQuote.high[i] == null)
                        {
                            high = 0.00;
                        }
                        else
                        {
                            //high = (double)myQuote.high[i];
                            high = System.Convert.ToDouble(string.Format("{0:0.00}", myQuote.high[i]));
                        }

                        if (myQuote.low[i] == null)
                        {
                            low = 0.00;
                        }
                        else
                        {
                            //low = (double)myQuote.low[i];
                            low = System.Convert.ToDouble(string.Format("{0:0.00}", myQuote.low[i]));
                        }

                        if (myQuote.open[i] == null)
                        {
                            open = 0.00;
                        }
                        else
                        {
                            //open = (double)myQuote.open[i];
                            open = System.Convert.ToDouble(string.Format("{0:0.00}", myQuote.open[i]));
                        }
                        if (myQuote.volume[i] == null)
                        {
                            volume = 0;
                        }
                        else
                        {
                            volume = (int)myQuote.volume[i];
                        }
                        prevclose = System.Convert.ToDouble(string.Format("{0:0.00}", myMeta.chartPreviousClose));
                        change = close - prevclose;
                        changepercent = (change / prevclose) * 100;
                        change = System.Convert.ToDouble(string.Format("{0:0.00}", change));
                        changepercent = System.Convert.ToDouble(string.Format("{0:0.00}", changepercent));
                    }
                }
            }
            catch (Exception ex)
            {
                quoteDate = DateTime.Now;
                open = high = low = close = volume = change = changepercent = prevclose = 0;
            }
        }
        public string findTimeZoneId(string zoneId)
        {
            string returnTimeZoneId = "";
            switch (zoneId)
            {
                case "IST":
                    returnTimeZoneId = "India Standard Time";
                    break;
                default:
                    returnTimeZoneId = "India Standard Time";
                    break;
            }
            return returnTimeZoneId;
        }

        public DateTime convertUnixEpochToLocalDateTime(long dateEpoch, string zoneId)
        {
            DateTime localDateTime;

            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(dateEpoch);
            string timeZoneId = findTimeZoneId(zoneId);
            TimeZoneInfo currentTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            localDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeOffset.UtcDateTime, currentTimeZone);

            return localDateTime;
        }

    }
}
