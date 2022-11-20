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
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MarketAnalytics.Pages.PortfolioPages
{
    public class PortfolioModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;

        public PortfolioModel(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
        }

        public string DateSort { get; set; }
        public string ExchangeSort { get; set; }
        public string SymbolSort { get; set; }
        public string CompNameSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }

        public bool RefreshAllStocks { get; set; } = false;
        public int CurrentID { get; set; }
        public PaginatedList<PORTFOLIO> portfolio { get; set; } = default!;

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex, int? id, bool? refreshAll, bool? history)
        {
            DateTime[] quoteDate = null;
            double[] open, high, low, close, volume, change, changepercent, prevclose = null;

            if (_context.PORTFOLIO != null)
            {
                //StockMaster = await _context.StockMaster.ToListAsync();
                //Commented above line and Added following for sorting, searching, paging
                CurrentSort = sortOrder;
                DateSort = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
                SymbolSort = sortOrder == "Symbol" ? "symbol_desc" : "Symbol";
                ExchangeSort = sortOrder == "Exchange" ? "exchange_desc" : "Exchange";
                CompNameSort = sortOrder == "CompName" ? "compname_desc" : "CompName";
                if (searchString != null)
                {
                    pageIndex = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                //if (refreshAll == true)
                //{
                //    string fetchedData = await DbInitializer.FetchMasterData();
                //    DbInitializer.Initialize(_context, fetchedData);

                //    //RefreshAllStockMaster();
                //    RefreshAllStocks = false;
                //}

                if (id != null)
                {
                    var selectedRecord = await _context.PORTFOLIO.FirstOrDefaultAsync(m => m.StockMasterID == id);
                    if (selectedRecord != null)
                    {
                        if ((history == null) || (history == false))
                        {
                            //DateTime quoteDate;
                            //double open, high, low, close, volume, change, changepercent, prevclose;
                            DbInitializer.GetQuote(selectedRecord.StockMaster.Symbol + "." + selectedRecord.StockMaster.Exchange, out quoteDate, out open,
                                out high, out low, out close,
                                out volume, out change, out changepercent, out prevclose);
                            if (quoteDate != null)
                            {
                                selectedRecord.CMP = close[0];
                                selectedRecord.VALUE = close[0] * selectedRecord.QUANTITY;
                                _context.PORTFOLIO.Update(selectedRecord);
                                _context.SaveChanges();
                            }
                        }
                    }
                }

                CurrentFilter = searchString;

                IQueryable<PORTFOLIO> portfolioIQ = from s in _context.PORTFOLIO select s;

                if (!String.IsNullOrEmpty(searchString))
                {
                    portfolioIQ = portfolioIQ.Where(s => s.StockMaster.Symbol.ToUpper().Contains(searchString.ToUpper())
                                                            || s.StockMaster.CompName.ToUpper().Contains(searchString.ToUpper()));
                }

                switch (sortOrder)
                {
                    case "date_desc":
                        portfolioIQ = portfolioIQ.OrderByDescending(s => s.PURCHASE_DATE);
                        break;
                    case "Symbol":
                        portfolioIQ = portfolioIQ.OrderBy(s => s.StockMaster.Symbol);
                        break;
                    case "symbol_desc":
                        portfolioIQ = portfolioIQ.OrderByDescending(s => s.StockMaster.Symbol);
                        break;
                    case "Exchange":
                        portfolioIQ = portfolioIQ.OrderBy(s => s.StockMaster.Exchange);
                        break;
                    case "exchange_desc":
                        portfolioIQ = portfolioIQ.OrderByDescending(s => s.StockMaster.Exchange);
                        break;
                    case "CompName":
                        portfolioIQ = portfolioIQ.OrderBy(s => s.StockMaster.CompName);
                        break;
                    case "compname_desc":
                        portfolioIQ = portfolioIQ.OrderByDescending(s => s.StockMaster.CompName);
                        break;
                    default:
                        portfolioIQ = portfolioIQ.OrderBy(s => s.PURCHASE_DATE);
                        break;
                }
                var pageSize = Configuration.GetValue("PageSize", 10);
                portfolio = await PaginatedList<PORTFOLIO>.CreateAsync(portfolioIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
                foreach (var item in portfolio)
                {
                    quoteDate = null;
                    open = high = low = close = volume = change = changepercent = prevclose = null;
                    DbInitializer.GetQuote(item.StockMaster.Symbol + ".NS", out quoteDate, out open, out high, out low, out close,
                                out volume, out change, out changepercent, out prevclose);
                    if (quoteDate != null)
                    {
                        var selectedRecord = _context.PORTFOLIO.Find(item.PORTFOLIO_ID);

                        item.CMP = close[0];
                        item.VALUE = item.QUANTITY * close[0];

                        if (selectedRecord != null)
                        {
                            selectedRecord.CMP = close[0];
                            selectedRecord.VALUE = close[0] * selectedRecord.QUANTITY;
                            _context.PORTFOLIO.Update(selectedRecord);
                        }
                    }
                    _context.SaveChanges();
                }
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
