using MarketAnalytics.Models;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.ComponentModel;
using SQLitePCL;
using System.Net.NetworkInformation;
using NuGet.Versioning;
using MarketAnalytics.Pages.BuySell;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;

namespace MarketAnalytics.Data
{
    public class DbInitializer
    {
        public static string urlNSEStockMaster = "http://www1.nseindia.com/content/equities/EQUITY_L.csv";
        public static string urlGetHistoryQuote = "https://query1.finance.yahoo.com/v8/finance/chart/{0}?period1={1}&period2={2}&interval={3}&filter=history&frequency={4}&includeAdjustedClose={5}";

        public static string urlGlobalQuote = "https://query1.finance.yahoo.com/v8/finance/chart/{0}?range=1d&interval=1d&indicators=quote&timestamp=true";

        static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Fetches list of companies along with symbols from NSE
        /// </summary>
        /// <param name="sourceURL"></param>
        /// <param name="exchangeCode"></param>
        /// <returns></returns>
        public static async Task<string> FetchMasterData(string sourceURL = "http://www1.nseindia.com/content/equities/EQUITY_L.csv", string exchangeCode = "NS")
        {
            string responseBody = null;
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                if (exchangeCode.Equals("NS"))
                {
                    HttpResponseMessage response = await client.GetAsync(sourceURL);
                    response.EnsureSuccessStatusCode();
                    responseBody = await response.Content.ReadAsStringAsync();
                    // Above three lines can be replaced with new helper method below
                    // string responseBody = await client.GetStringAsync(uri);

                }
                else
                {
                    responseBody = null;
                }
            }
            catch (HttpRequestException e)
            {
                //throw e;
            }
            return responseBody;
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        /// <summary>
        /// Fetched data from NSE is checked with existing stock master records. If the stock does not exists in StockMaster
        /// then Inserts the new stock
        /// Updates all symbols with current quote
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fetchedData"></param>
        public static void Initialize(DBContext context, string fetchedData)
        {
            //if (context.StockMaster.Any())
            //{
            //    return;   // DB has been seeded
            //}
            StringBuilder sourceFile, record;
            string[] sourceLines;
            int recCounter = 0;
            string[] fields;
            List<StockMaster> newRecords = new List<StockMaster>();
            DateTime[] quoteDate = null;
            double[] open, high, low, close, volume, change, changepercent, prevclose = null;

            //sourceURL = urlNSEStockMaster;
            //string fetchedData = await FetchMasterData(sourceURL, exchangeCode);

            if (fetchedData != null)
            {
                //read first line which is list of fields
                sourceFile = new StringBuilder(fetchedData);
                sourceLines = sourceFile.ToString().Split('\n');
                sourceFile.Clear();
                //We want to skip the first line, which is column headings
                record = new StringBuilder(sourceLines[recCounter++].Trim());

                //IQueryable<StockMaster> stockmasterIQ = from s in context.StockMaster select s;
                IQueryable<StockMaster> currentMaster;
                while (recCounter < sourceLines.Length)
                {
                    record.Clear();
                    record.Append(sourceLines[recCounter++].Trim());
                    if (record.Length == 0)
                    {
                        continue;
                    }
                    fields = record.ToString().Split(',');
                    //if (IsMasterUpdated(context, fields[0], fields[1], "NS") == false)
                    {
                        GetQuote(fields[0] + ".NS", out quoteDate, out open, out high, out low, out close,
                                    out volume, out change, out changepercent, out prevclose);
                        if (quoteDate != null)
                        { //find if stock exist in StockMaster, if not add it to context
                            var recTOAdd = new StockMaster();
                            currentMaster = context.StockMaster.Where(s => s.Symbol.ToUpper().Equals(fields[0].ToUpper())
                                                && s.CompName.ToUpper().Equals(fields[1].ToUpper()));
                            if (currentMaster.Count() <= 0)
                            //if (!context.StockMaster.Any(o => o.Symbol.ToUpper().Equals(fields[0].ToUpper())
                            //                                    && o.CompName.ToUpper().Equals(fields[1].ToUpper())
                            //                                    && o.Exchange.ToUpper().Equals(fields[1].ToUpper())))
                            {
                                recTOAdd.Symbol = fields[0];
                                recTOAdd.CompName = fields[1];
                                recTOAdd.Exchange = "NS";

                                recTOAdd.QuoteDateTime = quoteDate[0];
                                recTOAdd.Open = open[0];
                                recTOAdd.High = high[0];
                                recTOAdd.Low = low[0];
                                recTOAdd.Close = close[0];
                                recTOAdd.Volume = volume[0];
                                recTOAdd.ChangePercent = changepercent[0];
                                recTOAdd.Change = change[0];
                                recTOAdd.PrevClose = prevclose[0];

                                newRecords.Add(recTOAdd);
                            }
                            else
                            {
                                var selectedRecord = (StockMaster)(currentMaster.First());
                                selectedRecord.QuoteDateTime = quoteDate[0];
                                selectedRecord.Open = open[0];
                                selectedRecord.High = high[0];
                                selectedRecord.Low = low[0];
                                selectedRecord.Close = close[0];
                                selectedRecord.Volume = volume[0];
                                selectedRecord.ChangePercent = changepercent[0];
                                selectedRecord.Change = change[0];
                                selectedRecord.PrevClose = prevclose[0];
                                context.StockMaster.Update(selectedRecord);
                            }
                        }
                    }
                }
                if (newRecords.Count > 0)
                {
                    context.StockMaster.AddRange(newRecords);
                }
                context.SaveChanges(true);
            }
        }

        /// <summary>
        /// For given StockMaster record fetches historical price data from given fromdate. if fromdate is null then fetches last
        /// 10 years price data
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stockMaster"></param>
        /// <param name="symbol"></param>
        /// <param name="compname"></param>
        /// <param name="exchange"></param>
        /// <param name="fromDate"></param>
        public static void InitializeHistory(DBContext context, StockMaster stockMaster, string fromDate = null)
        {
            //if (context.StockMaster.Any())
            //{
            //    return;   // DB has been seeded
            //}
            int recCounter = 0;
            List<StockPriceHistory> newRecords = new List<StockPriceHistory>();
            DateTime[] quoteDate;
            double[] open, high, low, close, volume, change, changepercent, prevclose;

            quoteDate = null;
            open = high = low = close = volume = change = changepercent = prevclose = null;

            try
            {
                //sourceURL = urlNSEStockMaster;
                if (string.IsNullOrEmpty(fromDate))
                {
                    GetHistoryQuote(stockMaster.Symbol + "." + stockMaster.Exchange, DateTime.Today.Date.AddYears(-10).ToString("yyyy-MM-dd"), DateTime.Today.Date.AddDays(1).ToString("yyyy-MM-dd"), out quoteDate, out open, out high, out low, out close,
                                    out volume, out change, out changepercent, out prevclose);
                }
                else
                {
                    GetHistoryQuote(stockMaster.Symbol + "." + stockMaster.Exchange, Convert.ToDateTime(fromDate).ToString("yyyy-MM-dd"), DateTime.Today.Date.AddDays(1).ToString("yyyy-MM-dd"), out quoteDate, out open, out high, out low, out close,
                                    out volume, out change, out changepercent, out prevclose);
                }
                //read first line which is list of fields
                if ((quoteDate != null) && (quoteDate.Length > 0))
                {
                    //IQueryable<StockPriceHistory> stockpriceIQ = from s in context.StockPriceHistory select s;
                    IQueryable<StockPriceHistory> currentPrice;
                    for (int i = 0; i < quoteDate.Length; i++)
                    {
                        if (quoteDate[i].ToShortDateString().Contains("0001"))
                        {
                            //this means we skipped few rows due to null values in price
                            continue;
                        }
                        //find if stock exist in StockMaster, if not add it to context
                        var recTOAdd = new StockPriceHistory();
                        //currentPrice = stockpriceIQ.Where(s => s.StockMaster.Symbol.ToUpper().Equals(symbol.ToUpper())
                        //                    && s.StockMaster.Exchange.ToUpper().Equals(exchange.ToUpper())
                        //                    && s.PriceDate.Equals(quoteDate[i]));
                        //currentPrice = context.StockPriceHistory.Where(s => ((s.StockMasterID == stockMaster.StockMasterID) &&
                        //                    (s.StockMaster.Symbol.ToUpper().Equals(symbol.ToUpper()))
                        //                    && (s.StockMaster.Exchange.ToUpper().Equals(exchange.ToUpper()))
                        //                    && (s.PriceDate.Date.Equals(quoteDate[i].Date))));
                        currentPrice = context.StockPriceHistory.Where(s => ((s.StockMasterID == stockMaster.StockMasterID) &&
                                            (s.PriceDate.Date.CompareTo(quoteDate[i].Date) == 0)));
                        if (currentPrice.Count() <= 0)
                        {
                            //this is new price history record
                            recTOAdd.PriceDate = quoteDate[i];
                            recTOAdd.Open = open[i];
                            recTOAdd.High = high[i];
                            recTOAdd.Low = low[i];
                            recTOAdd.Close = close[i];
                            recTOAdd.Volume = volume[i];
                            recTOAdd.ChangePercent = changepercent[i];
                            recTOAdd.Change = change[i];
                            recTOAdd.PrevClose = prevclose[i];
                            recTOAdd.StockMasterID = stockMaster.StockMasterID;
                            //recTOAdd.StockMaster = stockMaster;
                            newRecords.Add(recTOAdd);
                        }
                        else
                        {
                            recTOAdd = (StockPriceHistory)(currentPrice.First());

                            recTOAdd.PriceDate = quoteDate[i];
                            recTOAdd.Open = open[i];
                            recTOAdd.High = high[i];
                            recTOAdd.Low = low[i];
                            recTOAdd.Close = close[i];
                            recTOAdd.Volume = volume[i];
                            recTOAdd.ChangePercent = changepercent[i];
                            recTOAdd.Change = change[i];
                            recTOAdd.PrevClose = prevclose[i];

                            recTOAdd.StockMasterID = stockMaster.StockMasterID;
                            context.StockPriceHistory.Update(recTOAdd);
                        }
                    }
                    if (newRecords.Count > 0)
                    {
                        context.StockPriceHistory.AddRange(newRecords);
                    }
                    context.SaveChanges(true);
                }
            }
            catch (Exception ex)
            {
                //throw ex;

            }
        }

        /// <summary>
        /// Gets CMP for given symbol and returns all values in arrays
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="quoteDate"></param>
        /// <param name="open"></param>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="close"></param>
        /// <param name="volume"></param>
        /// <param name="change"></param>
        /// <param name="changepercent"></param>
        /// <param name="prevclose"></param>
        public static void GetQuote(string symbol, out DateTime[] quoteDate, out double[] open, out double[] high,
                    out double[] low, out double[] close, out double[] volume, out double[] change, out double[] changepercent,
                    out double[] prevclose)
        {
            quoteDate = null;
            open = high = low = close = volume = change = changepercent = prevclose = null;
            try
            {

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
                //throw ex;
            }
        }


        /// <summary>
        /// Fetches price data for given symbol from periodDt1 to periodDt2
        /// Returns all data in arrays
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="periodDt1"></param>
        /// <param name="periodDt2"></param>
        /// <param name="quoteDate"></param>
        /// <param name="open"></param>
        /// <param name="high"></param>
        /// <param name="low"></param>
        /// <param name="close"></param>
        /// <param name="volume"></param>
        /// <param name="change"></param>
        /// <param name="changepercent"></param>
        /// <param name="prevclose"></param>
        /// <param name="interval"></param>
        /// <param name="frequency"></param>
        /// <param name="adjclose"></param>
        public static void GetHistoryQuote(string symbol, string periodDt1, string periodDt2, out DateTime[] quoteDate, out double[] open, out double[] high,
                    out double[] low, out double[] close, out double[] volume, out double[] change, out double[] changepercent,
                    out double[] prevclose, string interval = "1d",
                string frequency = "1d", string adjclose = "true")
        {
            quoteDate = null;
            open = high = low = close = volume = change = changepercent = prevclose = null;
            try
            {
                string webservice_url = "";
                WebResponse wr;
                Stream receiveStream = null;
                StreamReader reader = null;
                //DataRow r;

                //we need to convert the date first
                string period1 = convertDateTimeToUnixEpoch(System.Convert.ToDateTime(periodDt1)).ToString();
                string period2 = convertDateTimeToUnixEpoch(System.Convert.ToDateTime(periodDt2)).ToString();

                webservice_url = string.Format(urlGetHistoryQuote, symbol, period1, period2, interval, frequency, adjclose);

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
                //throw ex;
            }
        }

        public static bool IsStockUpdatedToday(StockMaster stockMaster, int whatToCheck)
        {
            bool breturn = true;
            DateTime todayDate = DateTime.Today;
            try
            {
                if ((whatToCheck == 1) && (stockMaster.HI_LOW_67_50_LastUpDt.Date.CompareTo(todayDate.Date) < 0))
                {
                    breturn = false;
                }
                else if ((whatToCheck == 2) && (stockMaster.SMA_LastUpDt.Date.CompareTo(todayDate.Date) < 0))
                {
                    breturn = false;
                }
                else if ((whatToCheck == 3) && (stockMaster.RSI_LastUpDt.Date.CompareTo(todayDate.Date) < 0))
                {
                    breturn = false;
                }
                else if ((whatToCheck == 4) && (stockMaster.STOCH_LastUpDt.Date.CompareTo(todayDate.Date) < 0))
                {
                    breturn = false;
                }
                else if ((whatToCheck == 5) && (stockMaster.BULL_ENGULF_LastUpDt.Date.CompareTo(todayDate.Date) < 0))
                {
                    breturn = false;
                }
                else if ((whatToCheck == 6) && (stockMaster.BEAR_ENGULF_LastUpDt.Date.CompareTo(todayDate.Date) < 0))
                {
                    breturn = false;
                }
                else if ((whatToCheck == 7) && (stockMaster.V20_LastUpDt.Date.CompareTo(todayDate.Date) < 0))
                {
                    breturn = false;
                }
                else if ((whatToCheck == 8) && (stockMaster.SMA_BUYSELL_LastUpDt.Date.CompareTo(todayDate.Date) < 0))
                {
                    breturn = false;
                }

            }
            catch
            {
                breturn = true;
            }
            return breturn;
        }
        public static bool IsMasterUpdated(DBContext context, string symbol, string compname, string exchange)
        {
            bool breturn = true;
            try
            {
                //IQueryable<StockPriceHistory> stockpriceIQ = from s in context.StockPriceHistory select s;
                //IQueryable<StockMaster> stockmasterIQ = from s in context.StockMaster select s;

                IQueryable<StockMaster> currentStockIQ = context.StockMaster.Where(s => (s.Symbol.Equals(symbol)) && (s.CompName.Equals(compname)) &&
                                                                                  (s.Exchange.Equals(exchange)));
                if (currentStockIQ.Count() > 0)
                {
                    StockMaster lastRec = currentStockIQ.OrderBy(p => p.QuoteDateTime).LastOrDefault();
                    if (lastRec != null)
                    {
                        //if (Convert.ToDateTime(lastRec.QuoteDateTime).Date.CompareTo(DateTime.Today.Date) < 0)
                        if (Convert.ToDateTime(lastRec.QuoteDateTime.Value.ToShortDateString()).CompareTo(Convert.ToDateTime(DateTime.Today.ToShortDateString())) < 0)
                        {
                            breturn = false;
                        }
                    }
                }
                else
                {
                    //this is new listed stock
                    breturn = false;
                }
            }
            catch //(Exception ex)
            {
                breturn = false;
            }
            return breturn;
        }

        public static void UpdateStockQuote(DBContext context, StockMaster stockMaster)
        {
            try
            {
                DateTime[] quoteDate = null;
                double[] open, high, low, close, volume, change, changepercent, prevclose = null;

                DbInitializer.GetQuote(stockMaster.Symbol + "." + stockMaster.Exchange, out quoteDate, out open,
                    out high, out low, out close,
                    out volume, out change, out changepercent, out prevclose);
                if (quoteDate != null)
                {
                    stockMaster.QuoteDateTime = quoteDate[0];
                    stockMaster.Open = open[0];
                    stockMaster.High = high[0];
                    stockMaster.Low = low[0];
                    stockMaster.Close = close[0];
                    stockMaster.Volume = volume[0];
                    stockMaster.ChangePercent = changepercent[0];
                    stockMaster.Change = change[0];
                    stockMaster.PrevClose = prevclose[0];
                    context.StockMaster.Update(stockMaster);
                    context.SaveChanges(true);
                }
            }
            catch (Exception ex)
            { }
        }

        public static void UpdateStockModel(DBContext context, StockMaster item)
        {
            string lastPriceDate = string.Empty;
            try
            {
                lastPriceDate = DbInitializer.IsHistoryUpdated(context, item);
                if (string.IsNullOrEmpty(lastPriceDate) == false)
                {
                    DbInitializer.InitializeHistory(context, item, lastPriceDate);
                }
                DbInitializer.GetSMA_EMA_MACD_BBANDS_Table(context, item);

                DbInitializer.getRSIDataTableFromDaily(context, item, period: "14");

                DbInitializer.getStochasticDataTableFromDaily(context, item, fastkperiod: "20", slowdperiod: "20");

                DbInitializer.V20CandlesticPatternFinder(context, item);

                DbInitializer.GetSMA_BUYSELL(context, item, 20, 50, 200);

                DbInitializer.GetBullishEngulfingBuySellList(context, item, DateTime.Today.AddDays(-180), 10);
                DbInitializer.GetBearishEngulfingBuySellList(context, item, DateTime.Today.AddDays(-180), 10);
                DbInitializer.GetLifetimeHighLow(context, item);
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// For given StockMaster finds lifetime high & low from history data 
        /// It also finds if stock closing price was <= 67% of lifetime high
        /// It also find %change of todays close price to lifetime high price
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stockMaster"></param>
        public static void GetLifetimeHighLow(DBContext context, StockMaster stockMaster)
        {
            double high = -1;
            double low = -1;
            double lt67pct = 0;
            double changefromhigh = 0;
            double year_hi = -1;
            double year_lo = -1;
            double changefromyearhi = 0;

            try
            {
                if (IsStockUpdatedToday(stockMaster, 1))
                {
                    return;
                }
                string lastPriceDate = IsHistoryUpdated(context, stockMaster);
                if (string.IsNullOrEmpty(lastPriceDate) == false)
                {
                    InitializeHistory(context, stockMaster, lastPriceDate);
                }

                IOrderedQueryable<StockPriceHistory> historyIQ = context.StockPriceHistory.Where(a => a.StockMasterID == stockMaster.StockMasterID)
                                                                                   .OrderBy(a => a.PriceDate);

                //high = context.StockPriceHistory.Where(a => a.StockMasterID == stockMaster.StockMasterID)
                //    .Max(a => a.High);
                high = historyIQ.Max(a => a.High);
                year_hi = historyIQ.Where(a => a.PriceDate.Date.CompareTo(DateTime.Today.AddYears(-1).Date) >= 0).OrderBy(a => a.PriceDate).Max(a => a.High);
                
                var highestRec = historyIQ.Where(a => a.High == high).First();

                low = historyIQ.Min(a => a.Low);
                year_lo = historyIQ.Where(a => a.PriceDate.Date.CompareTo(DateTime.Today.AddYears(-1).Date) >= 0).OrderBy(a => a.PriceDate).Min(a => a.Low);

                lt67pct = high * .67;
                var ltrecord = historyIQ.Where(a => (a.PriceDate.Date > highestRec.PriceDate.Date) && (a.Close <= lt67pct)).FirstOrDefault();

                var todayRec = historyIQ.LastOrDefault();

                changefromhigh = (todayRec.Close - high) / todayRec.Close * 100;
                changefromyearhi = (todayRec.Close - year_hi) / todayRec.Close * 100;
                //stockMaster.LIFETIME_HIGH = context.StockPriceHistory.Where(a => a.StockMasterID == stockMaster.StockMasterID)
                //                                                        .Max(a => a.Close);
                //stockMaster.LIFETIME_LOW = context.StockPriceHistory.Where(a => a.StockMasterID == stockMaster.StockMasterID)
                //                                                        .Min(a => a.Close);
                stockMaster.LIFETIME_HIGH = high;
                stockMaster.LIFETIME_LOW = low;
                stockMaster.YEAR_HI = year_hi;
                stockMaster.YEAR_LO = year_lo;
                if (ltrecord != null)
                {
                    stockMaster.LESSTHAN_67PCT_ON = ltrecord.PriceDate;
                }
                stockMaster.DIFF_FROM_LIFETIME_HIGH = changefromhigh;
                stockMaster.DIFF_FROM_YEAR_HI = changefromyearhi;

                stockMaster.HI_LOW_67_50_LastUpDt = DateTime.Today.Date;
                context.StockMaster.Update(stockMaster);
                context.SaveChanges(true);
            }
            catch
            { }
        }

        /// <summary>
        /// For given StockMaster checks if the history price data is updated till today
        /// If not gets the last history record date and returns to calles
        /// Caller should use the return date and update history if return data is not null or empty
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stockMaster"></param>
        /// <param name="stockMasterID"></param>
        /// <returns></returns>
        public static string IsHistoryUpdated(DBContext context, StockMaster stockMaster)
        {
            string lastPriceDate = string.Empty;
            try
            {
                //IQueryable<StockPriceHistory> stockpriceIQ = from s in context.StockPriceHistory select s;

                IQueryable<StockPriceHistory> givenStockIQ = context.StockPriceHistory.Where(s => (s.StockMasterID == stockMaster.StockMasterID));

                StockPriceHistory lastHistoryRec = givenStockIQ.OrderBy(p => p.PriceDate).LastOrDefault();
                if (lastHistoryRec != null)
                {
                    //if (Convert.ToDateTime(lastHistoryRec.PriceDate).Date.CompareTo(DateTime.Today.Date) < 0)
                    //if (lastHistoryRec.PriceDate.CompareTo(DateTime.Now) < 0)
                    if (Convert.ToDateTime(lastHistoryRec.PriceDate.ToShortDateString()).CompareTo(Convert.ToDateTime(DateTime.Today.ToShortDateString())) < 0)
                    {
                        lastPriceDate = lastHistoryRec.PriceDate.ToString("yyyy-MM-dd");
                    }
                }
                else
                {
                    lastPriceDate = DateTime.Today.AddYears(-10).ToString("yyyy-MM-dd");
                }
            }
            catch
            {
                lastPriceDate = string.Empty;
            }
            return lastPriceDate;
        }

        /// <summary>
        /// For give StockMaster and using the period given, calculates RSI value for each date.
        /// RSI value is found for all records starting from 'fromDate + period'
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stockMaster"></param>
        /// <param name="symbol"></param>
        /// <param name="exchange"></param>
        /// <param name="stockMasterID"></param>
        /// <param name="compname"></param>
        /// <param name="fromDate"></param>
        /// <param name="seriestype"></param>
        /// <param name="time_interval"></param>
        /// <param name="period"></param>
        /// <param name="stochRSI"></param>
        /// <param name="refreshHistory"></param>
        public static void getRSIDataTableFromDaily(DBContext context, StockMaster stockMaster,
                                    //DateTime fromDate, string seriestype = "CLOSE", string time_interval = "1d",
                                    string period = "14")
        {
            //DataTable rsiDataTable = null;
            int iPeriod;
            double change, gain, loss, avgGain, avgLoss, rs, rsi;
            double sumOfGain, sumOfLoss;

            change = gain = loss = avgGain = avgLoss = rs = rsi = 0.00;
            sumOfGain = sumOfLoss = 0.00;

            //DateTime dateCurrentRow = DateTime.Today;
            StockPriceHistory currentHist = null, prevHist = null;

            UpdateTracker tracker = null;

            int counterStart = 1;
            string[] trackerData = null;
            int rownum = 0;
            int savedPeriod = 0;

            try
            {

                string lastPriceDate = IsHistoryUpdated(context, stockMaster);
                if (string.IsNullOrEmpty(lastPriceDate) == false)
                {
                    InitializeHistory(context, stockMaster, lastPriceDate);
                }
                iPeriod = System.Convert.ToInt32(period);

                IQueryable<UpdateTracker> updateTrackerIQ = context.UpdateTracker.Where(s => (s.StockMasterID == stockMaster.StockMasterID) && (s.TYPE.Equals("RSI")));
                if (updateTrackerIQ.Count() > 0)
                {
                    tracker = updateTrackerIQ.FirstOrDefault();
                    //fromDate = tracker.REF_DATE.AddDays(1);
                    //tracker.DATA = rownum + "," + iPeriod + "," + gain + "," + loss + "," + sumOfGain + "," + sumOfLoss + avgGain + "," + avgLoss;

                    trackerData = tracker.DATA.Split(",");

                    counterStart = Int32.Parse(trackerData[0]);

                    savedPeriod = Int32.Parse(trackerData[1]);

                    if (savedPeriod != iPeriod)
                    {
                        counterStart = 0;
                    }
                    else
                    {
                        if (IsStockUpdatedToday(stockMaster, 3))
                        {
                            return;
                        }
                        gain = double.Parse(trackerData[2]);
                        loss = double.Parse(trackerData[3]);
                        sumOfGain = double.Parse(trackerData[4]);
                        sumOfLoss = double.Parse(trackerData[5]);
                        avgGain = double.Parse(trackerData[6]);
                        avgLoss = double.Parse(trackerData[7]); 
                        change = rs = rsi = 0.00;
                    }
                }

                IQueryable<StockPriceHistory> rsiIQ = context.StockPriceHistory.Where(s => (s.StockMasterID == stockMaster.StockMasterID));

                if ((rsiIQ != null) && ((rsiIQ.Count() - iPeriod) > 0))
                {
                    for (rownum = counterStart; rownum < rsiIQ.Count(); rownum++)
                    {
                        currentHist = rsiIQ.AsEnumerable().ElementAt(rownum);
                        //prevHist = rsiIQ.AsEnumerable().ElementAt(rownum - 1);


                        //change = System.Convert.ToDouble(currentHist.Close) - System.Convert.ToDouble(prevHist.Close);
                        change = System.Convert.ToDouble(currentHist.Change);
                        if (change < 0)
                        {
                            loss = Math.Abs(change);
                            gain = 0.00;
                        }
                        else
                        {
                            gain = change;
                            loss = 0.00;
                        }

                        //for the first iPeriod keep adding loss & gain
                        if (rownum < iPeriod)
                        {
                            sumOfGain += gain;
                            sumOfLoss += loss;
                            rsi = 0;
                        }
                        else
                        {
                            if (rownum == iPeriod)
                            {
                                sumOfGain += gain;
                                sumOfLoss += loss;
                                //we also find  other fields and SAVE
                                avgGain = sumOfGain / iPeriod;
                                avgLoss = sumOfLoss / iPeriod;
                                if ((avgGain == 0) && (avgLoss == 0))
                                {
                                    rs = 0;
                                }
                                else
                                {
                                    rs = avgGain / avgLoss;
                                }
                            }
                            else
                            {
                                avgGain = ((avgGain * (iPeriod - 1)) + gain) / iPeriod;
                                avgLoss = ((avgLoss * (iPeriod - 1)) + loss) / iPeriod;
                                if ((avgGain == 0) && (avgLoss == 0))
                                {
                                    rs = 0;
                                }
                                else
                                {
                                    rs = avgGain / avgLoss;
                                }
                            }
                            rsi = 100 - (100 / (1 + rs));
                        }
                        currentHist.RSI_CLOSE = Math.Round(rsi, 2);
                        context.StockPriceHistory.Update(currentHist);
                    }
                    if (currentHist != null)
                    {
                        stockMaster.RSI_LastUpDt = DateTime.Today.Date;
                        context.StockMaster.Update(stockMaster);

                        if (tracker == null)
                        {
                            tracker = new UpdateTracker();
                            tracker.StockMasterID = stockMaster.StockMasterID;
                            tracker.TYPE = "RSI";
                            tracker.REF_DATE = currentHist.PriceDate;
                            tracker.DATA = rownum + "," + iPeriod + "," + gain + "," + loss + "," + sumOfGain + "," + sumOfLoss + "," + avgGain + "," + avgLoss;
                            context.UpdateTracker.Add(tracker);
                        }
                        else
                        {
                            //tracker.TYPE = "SMA";
                            tracker.REF_DATE = currentHist.PriceDate;
                            tracker.DATA = rownum + "," + iPeriod + "," + gain + "," + loss + "," + sumOfGain + "," + sumOfLoss+ "," + avgGain + "," + avgLoss;
                            context.UpdateTracker.Update(tracker);
                        }
                        context.SaveChanges(true);
                    }
                }
            }
            catch (Exception ex)
            {
                //throw ex;
                //Console.WriteLine("getRSIDataTableFromDaily exception: " + ex.Message);
            }
        }

        /// <summary>
        /// For given StockMaster and period values, finds stockastic values
        /// The FastK and SlowD values are saved from fromDate + fastkoeriod + slowdperiod
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stockMaster"></param>
        /// <param name="fromDate"></param>
        /// <param name="seriestype"></param>
        /// <param name="time_interval"></param>
        /// <param name="fastkperiod"></param>
        /// <param name="slowdperiod"></param>
        public static void getStochasticDataTableFromDaily(DBContext context, StockMaster stockMaster,
            //DateTime fromDate,
            //string seriestype = "CLOSE", string time_interval = "1d", 
            string fastkperiod = "20", string slowdperiod = "20")
        {
            List<double> listHigh = new List<double>();
            List<double> listClose = new List<double>();
            List<double> listLow = new List<double>();
            List<double> listHighestHigh = new List<double>();
            List<double> listLowestLow = new List<double>();
            List<double> listSlowK = new List<double>();
            try
            {
                int iFastKPeriod, iSlowDPeriod;
                int startFastK, startSlowD;
                int saveFastk, saveSlowD;
                double fastK = 0.00, slowD = 0.00, highestHigh = 0.00, lowestLow = 0.00;
                DateTime dateCurrentRow = DateTime.Today;
                StockPriceHistory currentHist = null;

                UpdateTracker tracker = null;

                int counterStart = 0;
                string[] trackerData = null;
                int rownum = 0;
                int savedPeriod = 0;


                string lastPriceDate = IsHistoryUpdated(context, stockMaster);
                if (string.IsNullOrEmpty(lastPriceDate) == false)
                {
                    InitializeHistory(context, stockMaster, lastPriceDate);
                }

                iFastKPeriod = System.Convert.ToInt32(fastkperiod);
                iSlowDPeriod = System.Convert.ToInt32(slowdperiod);
                startFastK = 0; startSlowD = 0;
                saveFastk = 0; saveSlowD= 0;

                IOrderedQueryable<StockPriceHistory> stochIQ = context.StockPriceHistory.Where(s => (s.StockMasterID == stockMaster.StockMasterID)).OrderBy(s => s.PriceDate);
                //&& s.PriceDate.Date >= (fromDate.Date));

                IQueryable<UpdateTracker> updateTrackerIQ = context.UpdateTracker.Where(s => (s.StockMasterID == stockMaster.StockMasterID) && (s.TYPE.Equals("STOCH")));
                if (updateTrackerIQ.Count() > 0)
                {
                    tracker = updateTrackerIQ.FirstOrDefault();
                    //fromDate = tracker.REF_DATE.AddDays(1);
                    trackerData = tracker.DATA.Split(",");

                    counterStart = Int32.Parse(trackerData[0]);

                    startFastK = Int32.Parse(trackerData[1]);
                    saveFastk = startFastK;

                    startSlowD = Int32.Parse(trackerData[2]);
                    saveSlowD = startSlowD;

                    savedPeriod = Int32.Parse(trackerData[3]);
                    if (savedPeriod != iFastKPeriod)
                    {
                        counterStart = 0;
                    }
                    else
                    {
                        //caller request Stoch for same period
                        if (IsStockUpdatedToday(stockMaster, 4))
                        {
                            return;
                        }

                        //we need to load the Close, High, Low list
                        for (rownum = startSlowD; rownum < counterStart; rownum++)
                        {
                            currentHist = stochIQ.AsEnumerable().ElementAt(rownum);

                            listClose.Add(currentHist.Close);
                            listHigh.Add(currentHist.High);
                            listLow.Add(currentHist.Low);
                            listSlowK.Add((double)currentHist.FastK);
                        }
                        startFastK = savedPeriod - 1;//counterStart - startFastK;
                        startSlowD = savedPeriod - 1;
                    }

                }

                for (rownum = counterStart; rownum < stochIQ.Count(); rownum++)
                {
                    currentHist = stochIQ.AsEnumerable().ElementAt(rownum);

                    listClose.Add(currentHist.Close);
                    listHigh.Add(currentHist.High);
                    listLow.Add(currentHist.Low);
                    if ((rownum + 1) >= iFastKPeriod) //CASE of iFastKPeriod = 5: rownum = 4, 5th or higher row
                    {
                        highestHigh = FindHighestHigh(listHigh, startFastK, iFastKPeriod);
                        listHighestHigh.Add(highestHigh);

                        lowestLow = FindLowestLow(listLow, startFastK, iFastKPeriod);
                        listLowestLow.Add(lowestLow);

                        startFastK++;
                        saveFastk++;

                        fastK = FindSlowK(listClose, listHighestHigh, listLowestLow);
                        listSlowK.Add(fastK);

                        /*if (((rownum - rsiIndexAdjustor) + 1) >= (iFastKPeriod + iSlowDPeriod))*/ //CASE of iSlowDPeriod = 3: rownum = 7, 8th or higher row
                        if ((rownum + 2) >= (iFastKPeriod + iSlowDPeriod))
                        {
                            slowD = FindSlowD(listSlowK, startSlowD, iSlowDPeriod);
                            startSlowD++;
                            saveSlowD++;

                            //now save the datat
                            //dateCurrentRow = System.Convert.ToDateTime(dailyTable.Rows[rownum]["TIMESTAMP"]);

                            currentHist.SlowD = Math.Round(slowD, 2);
                            currentHist.FastK = Math.Round(fastK, 2);
                            context.StockPriceHistory.Update(currentHist);
                        }
                    }
                }
                if (currentHist != null)
                {
                    stockMaster.STOCH_LastUpDt = DateTime.Today.Date;
                    context.StockMaster.Update(stockMaster);

                    if (tracker == null)
                    {
                        tracker = new UpdateTracker();
                        tracker.StockMasterID = stockMaster.StockMasterID;
                        tracker.TYPE = "STOCH";
                        tracker.REF_DATE = currentHist.PriceDate;
                        tracker.DATA = rownum + "," + saveFastk + "," + saveSlowD + "," + iFastKPeriod;
                        context.UpdateTracker.Add(tracker);
                    }
                    else
                    {
                        //tracker.TYPE = "STOCH";
                        tracker.REF_DATE = currentHist.PriceDate;
                        tracker.DATA = rownum + "," + saveFastk + "," + saveSlowD + "," + iFastKPeriod;
                        context.UpdateTracker.Update(tracker);
                    }

                    context.SaveChanges(true);
                }
                //}
            }
            catch (Exception ex)
            {
                //throw ex;
            }
            finally
            {
                listHigh.Clear();
                listClose.Clear();
                listLow.Clear();
                listHighestHigh.Clear();
                listLowestLow.Clear();
                listSlowK.Clear();
            }
        }
        public static void GetLastRunData(IOrderedQueryable<StockPriceHistory> histQ, DateTime refDate, int smaPeriod, int index, out double[] prevClose)
        {
            int lenhistQ = histQ.Count();
            int start = lenhistQ - 1;
            int balance = smaPeriod - 1 - index;
            int end = lenhistQ - 1 - index;
            int arraycounter = index < 0 ? balance - 1 : index;
            int savearraycounter = arraycounter;

            prevClose = new double[smaPeriod];

            for (; arraycounter >= 0; arraycounter--)
            {
                prevClose[arraycounter] = histQ.AsEnumerable().ElementAt(start--).Close;
            }

            arraycounter = smaPeriod - 1;
            for (; savearraycounter < arraycounter; arraycounter--)
            {
                prevClose[arraycounter] = histQ.AsEnumerable().ElementAt(start--).Close;
            }
        }

        /// <summary>
        /// Currently this method calculates SMA only
        /// it will use history data to calculate Fast (default 20), Mid (default 50) and slow (default 200) SMA
        /// If you want to find only one of the SMA then pass 0 in other periods
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stockMaster"></param>
        /// <param name="symbol"></param>
        /// <param name="exchange"></param>
        /// <param name="stockMasterID"></param>
        /// <param name="compname"></param>
        /// <param name="fromDate"></param>
        /// <param name="seriestype"></param>
        /// <param name="time_interval"></param>
        /// <param name="small_fast_Period"></param>
        /// <param name="mid_period"></param>
        /// <param name="long_slow_Period"></param>
        /// <param name="emaRequired"></param>
        /// <param name="macdRequired"></param>
        /// <param name="signalperiod"></param>
        /// <param name="bbandsRequired"></param>
        /// <param name="stddeviation"></param>
        /// <param name="refreshHistory"></param>
        public static void GetSMA_EMA_MACD_BBANDS_Table(DBContext context, StockMaster stockMaster,
                                    //DateTime fromDate, string seriestype = "CLOSE", string time_interval = "1d",
                                    int small_fast_Period = 20, int mid_period = 50, int long_slow_Period = 200)
        {
            try
            {
                if (IsStockUpdatedToday(stockMaster, 2))
                {
                    return;
                }

                double currentClosePrice = 0;
                double smallSMA = 0;
                double midSMA = 0;
                double longSMA = 0;

                double sumSmall = 0;
                double[] valuesSmall = (small_fast_Period > 0) ? new double[small_fast_Period] : null; //array of CLOSE PRICE for the current iteration
                int indexSmall = 0; //we will increment it till specifid period and then reset it to 0

                double sumMid = 0;
                double[] valuesMid = (mid_period > 0) ? new double[mid_period] : null;
                int indexMid = 0;

                int counterStart = 0;
                string[] trackerData = null;
                int rownum = 0;

                double sumLong = 0;
                double[] valuesLong = (long_slow_Period > 0) ? new double[long_slow_Period] : null;
                int indexLong = 0;
                StockPriceHistory currentHist = null, prevHist = null;
                UpdateTracker tracker = null;

                bool bBuyFlagSet = false;
                bool bSellFlagSet = false;
                string lastPriceDate = IsHistoryUpdated(context, stockMaster);
                if (string.IsNullOrEmpty(lastPriceDate) == false)
                {
                    InitializeHistory(context, stockMaster, lastPriceDate);
                }

                IOrderedQueryable<StockPriceHistory> symbolIQ = context.StockPriceHistory.Where(s => (s.StockMasterID == stockMaster.StockMasterID)).OrderBy(s => s.PriceDate);

                IQueryable<UpdateTracker> updateTrackerIQ = context.UpdateTracker.Where(s => (s.StockMasterID == stockMaster.StockMasterID) && (s.TYPE.Equals("SMA")));
                if (updateTrackerIQ.Count() > 0)
                {
                    tracker = updateTrackerIQ.FirstOrDefault();

                    //fromDate = tracker.REF_DATE.AddDays(1);
                    trackerData = tracker.DATA.Split(",");

                    counterStart = Int32.Parse(trackerData[0]);

                    indexSmall = Int32.Parse(trackerData[1]);
                    sumSmall = Double.Parse(trackerData[2]);

                    indexMid = Int32.Parse(trackerData[3]);
                    sumMid = Double.Parse(trackerData[4]);

                    indexLong = Int32.Parse(trackerData[5]);
                    sumLong = Double.Parse(trackerData[6]);

                    IOrderedQueryable<StockPriceHistory> tempIQ = symbolIQ.Where(s => (s.PriceDate.Date.CompareTo(tracker.REF_DATE.Date) <= 0)).OrderBy(s => s.PriceDate);
                    //We use the index value to fill the arrays starting from REF DATE and go backwards
                    //We start filling the arrays from index as start counter
                    //
                    valuesSmall = null;
                    GetLastRunData(tempIQ, tracker.REF_DATE, small_fast_Period, indexSmall, out valuesSmall);
                    indexSmall++;

                    valuesMid = null;
                    GetLastRunData(tempIQ, tracker.REF_DATE, mid_period, indexMid, out valuesMid);
                    indexMid++;

                    valuesLong= null;
                    GetLastRunData(tempIQ, tracker.REF_DATE, long_slow_Period, indexLong, out valuesLong);
                    indexLong++;
                    //valuesSmall[indexSmall] = double.Parse(trackerData[7]);
                    //valuesLong[indexLong] = double.Parse(trackerData[7]);
                    //valuesMid[indexMid] = double.Parse(trackerData[7]);
                }

                //bulk update columns
                //db.tblTemp.Where(x => x.ProductID == parmProductID && x.IsActive == true).ToList().ForEach(x =>
                //{
                //    x.IsActive = false; x.UpdatedTimeStamp = DateTime.Now;
                //});
                //db.SaveChanges();

                //context.StockPriceHistory
                //    .Where(s => (s.StockMasterID == stockMasterID))
                //    .ToList()
                //    .ForEach(s =>
                //    { s.BUY_SMA_STRATEGY = 0; s.SELL_SMA_STRATEGY = 0; s.BULLISH_ENGULFING = false; 
                //        s.LOWER_THAN_SMA_SMALL = false; s.SMA_LONG = 0; s.SMA_SMALL = 0; s.SMA_MID= 0;
                //    });

                //context.SaveChanges();


                for (rownum = counterStart; rownum < symbolIQ.Count(); rownum++)
                {
                    currentHist = symbolIQ.AsEnumerable().ElementAt(rownum);

                    if (rownum > 0)
                    {
                        prevHist = symbolIQ.AsEnumerable().ElementAt(rownum - 1);
                    }
                    else
                    {
                        prevHist = null;
                    }

                    currentClosePrice = currentHist.Close;
                    if (small_fast_Period > 0)
                    {    //subtract the oldest CLOSE PRICE from the previous SUM and then add the current CLOSE PRICE
                        sumSmall = sumSmall - valuesSmall[indexSmall] + currentClosePrice;
                        valuesSmall[indexSmall] = currentClosePrice;

                        currentHist.SMA_SMALL = smallSMA = Math.Round((sumSmall / small_fast_Period), 2);
                        indexSmall = (indexSmall + 1) % small_fast_Period;

                        //if ((currentHist.Open < smallSMA) && (currentHist.Close < smallSMA))
                        //{
                        //    //means current candle is below SMA_SMALL
                        //    currentHist.LOWER_THAN_SMA_SMALL = true;

                        //    if ((prevHist != null) && (prevHist.Open < smallSMA) && (prevHist.Close < smallSMA) && (prevHist.Close < prevHist.Open) &&
                        //        (prevHist.Close > currentHist.Open) && (prevHist.Close < currentHist.Close) && (prevHist.Open < currentHist.Close) &&
                        //        (prevHist.Open > currentHist.Close))
                        //    {
                        //        //means previous day candle below SMA_SMALL and stock closed below open indicating a red candle
                        //        //also the prev candle is engulfed by current candle as prev open/close are withing current open/close
                        //        currentHist.BULLISH_ENGULFING = true;
                        //    }
                        //}
                    }

                    if (mid_period > 0)
                    {
                        sumMid = sumMid - valuesMid[indexMid] + currentClosePrice;
                        valuesMid[indexMid] = currentClosePrice;
                        currentHist.SMA_MID = midSMA = Math.Round((sumMid / mid_period), 2);
                        indexMid = (indexMid + 1) % mid_period;
                    }

                    if (long_slow_Period > 0)
                    {
                        sumLong = sumLong - valuesLong[indexLong] + currentClosePrice;
                        valuesLong[indexLong] = currentClosePrice;
                        currentHist.SMA_LONG = longSMA = Math.Round((sumLong / long_slow_Period), 2);
                        indexLong = (indexLong + 1) % long_slow_Period;

                        //currentHist.CROSSOVER_FLAG = (smallSMA > longSMA) ? "GT" : "LT";
                    }

                    //check if buy flag can be set to 1
                    //if ((!bBuyFlagSet) && (midSMA < longSMA) && (smallSMA < midSMA) && (currentClosePrice < smallSMA))
                    //{
                    //    currentHist.BUY_SMA_STRATEGY = currentHist.Close;
                    //    bBuyFlagSet = true;
                    //    bSellFlagSet = false;
                    //}
                    //if ((!bSellFlagSet) && (midSMA > longSMA) && (smallSMA > midSMA) && (currentClosePrice > smallSMA))
                    //{
                    //    currentHist.SELL_SMA_STRATEGY = currentHist.Close;
                    //    bSellFlagSet = true;
                    //    bBuyFlagSet = false;
                    //}
                    context.StockPriceHistory.Update(currentHist);
                }

                if (currentHist != null)
                {
                    stockMaster.SMA_LastUpDt = DateTime.Today.Date;
                    context.StockMaster.Update(stockMaster);
                    if (tracker == null)
                    {
                        tracker = new UpdateTracker();
                        tracker.StockMasterID = stockMaster.StockMasterID;
                        tracker.TYPE = "SMA";
                        tracker.REF_DATE = currentHist.PriceDate;
                        tracker.DATA = rownum + "," + (indexSmall - 1) + "," + sumSmall + "," + (indexMid - 1) + "," + sumMid + "," + (indexLong - 1) + "," + sumLong + "," + currentClosePrice;
                        context.UpdateTracker.Add(tracker);
                    }
                    else
                    {
                        //tracker.TYPE = "SMA";
                        tracker.REF_DATE = currentHist.PriceDate;
                        tracker.DATA = rownum + "," + (indexSmall - 1) + "," + sumSmall + "," + (indexMid - 1) + "," + sumMid + "," + (indexLong - 1) + "," + sumLong + "," + currentClosePrice;
                        context.UpdateTracker.Update(tracker);
                    }
                    context.SaveChanges(true);
                }
                //}
            }
            catch (Exception ex)
            {
                //throw ex;
                //Console.WriteLine("GetSMA_EMA_MACD_BBANDS_Table exception: " + ex.Message);
            }
        }

        /// <summary>
        /// This method uses existing SMA values for today and 
        ///     if current close is less than fast which is less than mid which less than slow it marks as BUY signal
        ///     if current close is above dast which is above mid which is above slow then it marks as SELL signal
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stockMaster"></param>
        /// <param name="symbol"></param>
        /// <param name="exchange"></param>
        /// <param name="stockMasterID"></param>
        /// <param name="compname"></param>
        /// <param name="periodsmall"></param>
        /// <param name="periodmid"></param>
        /// <param name="periodlong"></param>
        public static void GetSMA_BUYSELL(DBContext context, StockMaster stockMaster, int periodsmall = 20, int periodmid = 50, int periodlong = 200)
        {
            try
            {
                if (IsStockUpdatedToday(stockMaster, 8))
                {
                    return;
                }

                //first get the SMA for three periods
                DbInitializer.GetSMA_EMA_MACD_BBANDS_Table(context, stockMaster);

                //GetSMA(context, stockMaster, symbol, exchange, stockMasterID, compname, periodsmall, 0);
                //GetSMA(context, stockMaster, symbol, exchange, stockMasterID, compname, periodmid, 1);
                //GetSMA(context, stockMaster, symbol, exchange, stockMasterID, compname, periodlong, 2);

                //IQueryable<StockMaster> stockmasterIQ = from s in context.StockMaster select s;
                //IQueryable<StockMaster> symbolmasterIQ = context.StockMaster.Where(s => (s.StockMasterID == stockMasterID));

                //StockMaster currentMaster = symbolmasterIQ.First();

                //IQueryable<StockPriceHistory> stockpriceIQ = from s in context.StockPriceHistory select s;
                IQueryable<StockPriceHistory> symbolIQ = context.StockPriceHistory.Where(s => (s.StockMasterID == stockMaster.StockMasterID));

                //StockPriceHistory currentHist = symbolIQ.AsEnumerable().ElementAt(symbolIQ.Count() - 1);
                StockPriceHistory currentHist = symbolIQ.AsEnumerable().Last();

                //now set BUY FLGA if the current close < smallsma < midsma < longsma

                //currentHist.BUY_SMA_STRATEGY = 0;
                //currentHist.SELL_SMA_STRATEGY = 0;
                stockMaster.SMA_SLOW = (double)currentHist.SMA_LONG;
                stockMaster.SMA_MID = (double)currentHist.SMA_MID;
                stockMaster.SMA_FAST = (double)currentHist.SMA_SMALL;
                stockMaster.SMA_BUY_SIGNAL = false;
                stockMaster.SMA_SELL_SIGNAL = false;

                if ((currentHist.Close < currentHist.SMA_SMALL) && (currentHist.SMA_SMALL < currentHist.SMA_MID) &&
                    (currentHist.SMA_MID < currentHist.SMA_LONG))
                {
                    //currentHist.BUY_SMA_STRATEGY = currentHist.Close;
                    stockMaster.SMA_SLOW = (double)currentHist.SMA_LONG;
                    stockMaster.SMA_MID = (double)currentHist.SMA_MID;
                    stockMaster.SMA_FAST = (double)currentHist.SMA_SMALL;
                    stockMaster.SMA_BUY_SIGNAL = true;
                }
                //now set SELL FLGA if the current close > smallsma > midsma > longsma
                else if ((currentHist.Close > currentHist.SMA_SMALL) && (currentHist.SMA_SMALL > currentHist.SMA_MID) &&
                    (currentHist.SMA_MID > currentHist.SMA_LONG))
                {
                    //currentHist.SELL_SMA_STRATEGY = currentHist.Close;
                    stockMaster.SMA_SLOW = (double)currentHist.SMA_LONG;
                    stockMaster.SMA_MID = (double)currentHist.SMA_MID;
                    stockMaster.SMA_FAST = (double)currentHist.SMA_SMALL;
                    stockMaster.SMA_SELL_SIGNAL = true;

                }
                stockMaster.SMA_BUYSELL_LastUpDt = DateTime.Today.Date;
                //context.StockPriceHistory.Update(currentHist);
                context.StockMaster.Update(stockMaster);

                context.SaveChanges(true);
            }
            catch (Exception ex)
            {
                //throw ex;
            }
        }

        /// <summary>
        /// Currently not used
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stockMaster"></param>
        /// <param name="symbol"></param>
        /// <param name="exchange"></param>
        /// <param name="stockMasterID"></param>
        /// <param name="compname"></param>
        /// <param name="period"></param>
        /// <param name="whichSMA"></param>
        //whichSMA = 0 then fast, 1 = mid, 2 = slow
        public static void GetSMA(DBContext context, StockMaster stockMaster, int period, int whichSMA)
        {
            double currentClosePrice = 0;
            double smallSMA = 0;

            double sumSmall = 0;
            double[] valuesSmall = (period > 0) ? new double[period] : null; //array of CLOSE PRICE for the current iteration
            int indexSmall = 0; //we will increment it till specifid period and then reset it to 0

            StockPriceHistory currentHist = null, prevHist = null;

            //bool bBuyFlagSet = false;
            //bool bSellFlagSet = false;
            try
            {
                string lastPriceDate = IsHistoryUpdated(context, stockMaster);
                if (string.IsNullOrEmpty(lastPriceDate) == false)
                {
                    InitializeHistory(context, stockMaster, lastPriceDate);
                }

                //IQueryable<StockPriceHistory> stockpriceIQ = from s in context.StockPriceHistory select s;
                //List<StockPriceHistory> chartDataList = (stockpriceIQ.Where(s => (s.StockMasterID == CurrentID))).ToList();

                IQueryable<StockPriceHistory> symbolIQ = context.StockPriceHistory.Where(s => (s.StockMasterID == stockMaster.StockMasterID));


                if ((symbolIQ != null) && (symbolIQ.Count() > 0))
                {
                    for (int rownum = symbolIQ.Count() - period; rownum < symbolIQ.Count(); rownum++)
                    {
                        currentHist = symbolIQ.AsEnumerable().ElementAt(rownum);

                        if (rownum > 0)
                        {
                            prevHist = symbolIQ.AsEnumerable().ElementAt(rownum - 1);
                        }
                        else
                        {
                            prevHist = null;
                        }

                        currentClosePrice = System.Convert.ToDouble(currentHist.Close);
                        if (period > 0)
                        {    //subtract the oldest CLOSE PRICE from the previous SUM and then add the current CLOSE PRICE
                            sumSmall = sumSmall - valuesSmall[indexSmall] + currentClosePrice;
                            valuesSmall[indexSmall] = currentClosePrice;

                            if (whichSMA == 0)
                            {
                                currentHist.SMA_SMALL = smallSMA = Math.Round((sumSmall / period), 2);
                            }
                            else if (whichSMA == 1)
                            {
                                currentHist.SMA_MID = smallSMA = Math.Round((sumSmall / period), 2);
                            }
                            else if (whichSMA == 2)
                            {
                                currentHist.SMA_LONG = smallSMA = Math.Round((sumSmall / period), 2);
                            }
                            indexSmall = (indexSmall + 1) % period;

                            //commenting the engulfing logic for the time being
                            //if ((currentHist.Open < smallSMA) && (currentHist.Close < smallSMA))
                            //{
                            //    //means current candle is below SMA_SMALL
                            //    currentHist.LOWER_THAN_SMA_SMALL = 1;

                            //    if ((prevHist != null) && (prevHist.Open < smallSMA) && (prevHist.Close < smallSMA) && (prevHist.Close < prevHist.Open) &&
                            //        (prevHist.Close > currentHist.Open) && (prevHist.Close < currentHist.Close) && (prevHist.Open < currentHist.Close) &&
                            //        (prevHist.Open > currentHist.Close))
                            //    {
                            //        //means previous day candle below SMA_SMALL and stock closed below open indicating a red candle
                            //        //also the prev candle is engulfed by current candle as prev open/close are withing current open/close
                            //        currentHist.BULLISH_ENGULFING = 1;
                            //    }
                            //}
                        }

                        ////check if buy flag can be set to 1
                        //if ((!bBuyFlagSet) && (midSMA < longSMA) && (smallSMA < midSMA) && (currentClosePrice < smallSMA))
                        //{
                        //    currentHist.BUY_SMA_STRATEGY = currentHist.Close;
                        //    bBuyFlagSet = true;
                        //    bSellFlagSet = false;
                        //}
                        //if ((!bSellFlagSet) && (midSMA > longSMA) && (smallSMA > midSMA) && (currentClosePrice > smallSMA))
                        //{
                        //    currentHist.SELL_SMA_STRATEGY = currentHist.Close;
                        //    bSellFlagSet = true;
                        //    bBuyFlagSet = false;
                        //}
                        context.StockPriceHistory.Update(currentHist);
                    }

                    context.SaveChanges(true);
                }
            }
            catch (Exception ex)
            {
                //throw ex;
            }
        }


        /// <summary>
        /// We will first get 30 day SMA for last 6 months
        /// If we have found a record with engulfing = true
        /// Starting today
        ///     we will go to that record whoose engulfing flag = true
        ///Bullish Engulfing buy-Sell pattern
        /// Decision of swing
        /// We use 30 day SMA and find set of candles where 1st candle's (which can be green or red)
        /// open & close price is engulfed (enclosed) in 2nd candles (which has to be green) open & close
        /// If we found such a pair then check there is a downtrend till 1st candle
        ///BUY SIGNAL - 3rd day buy at opening. Ideally Close of 2nd candle becomes BUY price,
        ///but that may not always happen
        ///SELL SIGNAL - backtrack till a date from where price started falling
        ///This means their has to be downtrend from point A to 1st candle
        ///at the top where downtrend started, find highest closing price
        ///This closing price is the SELL price
        ///Averaging
        ///if we find a new engulfing pattern where the new 2nd candle open price is lower than
        /// earlier 2nd candle open price, then we can buy next day morning
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stockMaster"></param>
        /// <param name="periodsmall"></param>
        //public static List<BULLISH_ENGULFING_STRATEGY> GetBullishEngulfingBuySellList(DBContext context, StockMaster stockMaster, DateTime fromDate, int period)
        public static void GetBullishEngulfingBuySellList(DBContext context, StockMaster stockMaster, DateTime fromDate, int trendSpan)
        {
            List<BULLISH_ENGULFING_STRATEGY> recordList = new List<BULLISH_ENGULFING_STRATEGY>();
            try
            {
                if (IsStockUpdatedToday(stockMaster, 5))
                {
                    return;
                }
                //double currentClosePrice = 0;
                //double smaValue = 0;
                //double buyPrice = 0;
                double sellPrice = 0;
                //double sumSmall = 0;
                //double[] valuesSmall = (period > 0) ? new double[period] : null; //array of CLOSE PRICE for the current iteration
                //int indexSmall = 0; //we will increment it till specifid period and then reset it to 0
                int foundCounter = -1;


                StockPriceHistory currentHist = null, prevHist = null;

                //bool bBuyFlagSet = false;
                //bool bSellFlagSet = false;

                string lastPriceDate = IsHistoryUpdated(context, stockMaster);
                if (string.IsNullOrEmpty(lastPriceDate) == false)
                {
                    InitializeHistory(context, stockMaster, lastPriceDate);
                }

                //IQueryable<StockPriceHistory> stockpriceIQ = from s in context.StockPriceHistory select s;
                //List<StockPriceHistory> chartDataList = (stockpriceIQ.Where(s => (s.StockMasterID == CurrentID))).ToList();

                //IQueryable<StockPriceHistory> symbolIQ = stockpriceIQ.Where(s => (s.StockMasterID == stockMaster.StockMasterID));

                IQueryable<BULLISH_ENGULFING_STRATEGY> bullishengulfingIQ = context.BULLISH_ENGULFING_STRATEGY.Where(s => (s.StockMasterID == stockMaster.StockMasterID));
                DateTime lastBuyDate = DateTime.MinValue;
                if (bullishengulfingIQ.Count() > 0)
                {
                    //commenting delete rows as we will only check history post buy date
                    //context.BULLISH_ENGULFING_STRATEGY.RemoveRange(bullishengulfingIQ.AsEnumerable());
                    //context.SaveChanges();
                    lastBuyDate = bullishengulfingIQ.OrderBy(s => s.BUY_CANDLE_DATE).Last().BUY_CANDLE_DATE;
                }

                IQueryable<StockPriceHistory> symbolIQ = null;

                if (lastBuyDate == DateTime.MinValue)
                {
                    symbolIQ = context.StockPriceHistory.Where(s => (s.StockMasterID == stockMaster.StockMasterID) && (s.PriceDate.Date >= fromDate.Date));
                }
                else
                {
                    symbolIQ = context.StockPriceHistory.Where(s => (s.StockMasterID == stockMaster.StockMasterID) && (s.PriceDate.Date >= lastBuyDate.Date));
                }
                if ((symbolIQ != null) && (symbolIQ.Count() > 0))
                {
                    //for (int rownum = symbolIQ.Count() - period; rownum < symbolIQ.Count(); rownum++)
                    for (int rownum = 0; rownum < symbolIQ.Count(); rownum++)
                    {
                        currentHist = symbolIQ.AsEnumerable().ElementAt(rownum);

                        if (rownum > 0)
                        {
                            prevHist = symbolIQ.AsEnumerable().ElementAt(rownum - 1);
                        }
                        else
                        {
                            prevHist = null;
                            //sumSmall = (double)currentHist.Close * period;
                        }

                        //if (period >= 0)
                        {
                            //subtract the oldest CLOSE PRICE from the previous SUM and then add the current CLOSE PRICE
                            //commenting SMA generation code as we may want to see all values not just < sma
                            //if (period > 0)
                            //{
                            //    sumSmall = sumSmall - valuesSmall[indexSmall] + currentHist.Close;
                            //    valuesSmall[indexSmall] = currentHist.Close;
                            //    smaValue = Math.Round((sumSmall / period), 2);
                            //    indexSmall = (indexSmall + 1) % period;
                            //}
                            //if ((period == 0) || ((currentHist.Open < smaValue) && (currentHist.Close < smaValue)))
                            {

                                if ((prevHist != null) && (prevHist.Close < prevHist.Open) && (currentHist.Close > currentHist.Open) &&
                                    (prevHist.Close > currentHist.Open) && (prevHist.Close < currentHist.Close) && (prevHist.Open < currentHist.Close) &&
                                    (prevHist.Open > currentHist.Open))
                                {
                                    //means previous day candle below SMA_SMALL and stock closed below open indicating a red candle
                                    //also the prev candle is engulfed by current candle as prev open/close are withing current open/close
                                    //currentHist.BULLISH_ENGULFING = 1;
                                    //now backtrack & find the buy price

                                    if (FindIfDownTrendExist(context, stockMaster,
                                                        (rownum - 1), (lastBuyDate == DateTime.MinValue) ? fromDate : lastBuyDate, out sellPrice, out foundCounter, trendSpan: trendSpan))
                                    {
                                        var recEngulfing = new BULLISH_ENGULFING_STRATEGY();

                                        recEngulfing.BUY_CANDLE_DATE = currentHist.PriceDate;
                                        recEngulfing.SELL_CANDLE_DATE = symbolIQ.First(a => a.StockPriceHistoryID == foundCounter).PriceDate; //prevHist.PriceDate;
                                        recEngulfing.BUY_PRICE = currentHist.Close;
                                        recEngulfing.SELL_PRICE = sellPrice;
                                        recEngulfing.StockMasterID = currentHist.StockMasterID;
                                        recEngulfing.StockMaster = currentHist.StockMaster;
                                        recordList.Add(recEngulfing);
                                    }
                                }
                            }
                        }

                        //context.StockPriceHistory.Update(currentHist);
                    }

                    if (recordList.Count > 0)
                    {
                        context.BULLISH_ENGULFING_STRATEGY.AddRange(recordList);

                    }
                }
                stockMaster.BULL_ENGULF_LastUpDt = DateTime.Today.Date;
                context.StockMaster.Update(stockMaster);
                context.SaveChanges(true);
            }
            catch (Exception ex)
            {
                //throw ex;
            }
            finally
            {
                recordList.Clear();
            }
        }

        public static void GetBearishEngulfingBuySellList(DBContext context, StockMaster stockMaster, DateTime fromDate, int trendSpan)
        {
            List<BEARISH_ENGULFING> recordList = new List<BEARISH_ENGULFING>();
            try
            {

                if (IsStockUpdatedToday(stockMaster, 6))
                {
                    return;
                }
                //double smaValue = 0;
                //double sumSmall = 0;
                //double[] valuesSmall = (period > 0) ? new double[period] : null; //array of CLOSE PRICE for the current iteration
                //int indexSmall = 0; //we will increment it till specifid period and then reset it to 0

                double buyPrice = 0;
                int foundCounter = -1;


                StockPriceHistory currentHist = null, prevHist = null;

                //bool bBuyFlagSet = false;
                //bool bSellFlagSet = false;

                string lastPriceDate = IsHistoryUpdated(context, stockMaster);
                if (string.IsNullOrEmpty(lastPriceDate) == false)
                {
                    InitializeHistory(context, stockMaster, lastPriceDate);
                }

                //IQueryable<StockPriceHistory> stockpriceIQ = from s in context.StockPriceHistory select s;
                //List<StockPriceHistory> chartDataList = (stockpriceIQ.Where(s => (s.StockMasterID == CurrentID))).ToList();

                //IQueryable<StockPriceHistory> symbolIQ = stockpriceIQ.Where(s => (s.StockMasterID == stockMaster.StockMasterID));

                IQueryable<BEARISH_ENGULFING> bearishengulfingIQ = context.BEARISH_ENGULFING.Where(s => (s.StockMasterID == stockMaster.StockMasterID));
                DateTime lastBuyDate = DateTime.MinValue;
                if (bearishengulfingIQ.Count() > 0)
                {
                    //commenting delete rows as we will only check history post buy date
                    //context.BEARISH_ENGULFING.RemoveRange(bearishengulfingIQ.AsEnumerable());
                    //context.SaveChanges();
                    lastBuyDate = bearishengulfingIQ.OrderBy(s => s.BUY_CANDLE_DATE).Last().BUY_CANDLE_DATE;

                }
                IQueryable<StockPriceHistory> symbolIQ = null;

                if (lastBuyDate == DateTime.MinValue)
                {
                    symbolIQ = context.StockPriceHistory.Where(s => (s.StockMasterID == stockMaster.StockMasterID) && (s.PriceDate.Date >= fromDate.Date));
                }
                else
                {
                    symbolIQ = context.StockPriceHistory.Where(s => (s.StockMasterID == stockMaster.StockMasterID) && (s.PriceDate.Date >= lastBuyDate.Date));
                }

                if ((symbolIQ != null) && (symbolIQ.Count() > 0))
                {
                    //for (int rownum = symbolIQ.Count() - period; rownum < symbolIQ.Count(); rownum++)
                    for (int rownum = 0; rownum < symbolIQ.Count(); rownum++)
                    {
                        currentHist = symbolIQ.AsEnumerable().ElementAt(rownum);

                        if (rownum > 0)
                        {
                            prevHist = symbolIQ.AsEnumerable().ElementAt(rownum - 1);
                        }
                        else
                        {
                            prevHist = null;
                            //sumSmall = (double)currentHist.Close * period;

                        }
                        //if (period >= 0)
                        //{
                        //if (period > 0)
                        //{ //subtract the oldest CLOSE PRICE from the previous SUM and then add the current CLOSE PRICE
                        //    sumSmall = sumSmall - valuesSmall[indexSmall] + currentHist.Close;
                        //    valuesSmall[indexSmall] = currentHist.Close;
                        //    smaValue = Math.Round((sumSmall / period), 2);
                        //    indexSmall = (indexSmall + 1) % period;
                        //}
                        //if ((period == 0) || ((currentHist.Open < smaValue) && (currentHist.Close < smaValue)))
                        //{

                        if ((prevHist != null) && (prevHist.Close > prevHist.Open) && (currentHist.Close < currentHist.Open) &&
                            (prevHist.Close < currentHist.Open) && (prevHist.Close > currentHist.Close) && (prevHist.Open > currentHist.Close) &&
                            (prevHist.Open < currentHist.Open))
                        {
                            //this means prev candle is green and curent candle is red
                            //means previous day candle is green = stock closed above open indicating a red candle
                            //also the prev candle is engulfed by current candle as prev open/close are withing current open/close
                            //now backtrack & find the buy price

                            if (FindIfUpTrendExist(context, stockMaster,
                                                (rownum + 1), (lastBuyDate == DateTime.MinValue) ? fromDate : lastBuyDate, out buyPrice, out foundCounter, trendSpan: trendSpan))
                            {
                                var recEngulfing = new BEARISH_ENGULFING();

                                recEngulfing.BUY_CANDLE_DATE = symbolIQ.First(a => a.StockPriceHistoryID == foundCounter).PriceDate; //prevHist.PriceDate;
                                recEngulfing.SELL_CANDLE_DATE = prevHist.PriceDate;//currentHist.PriceDate;
                                recEngulfing.BUY_PRICE = buyPrice;
                                recEngulfing.SELL_PRICE = currentHist.Close;
                                recEngulfing.StockMasterID = currentHist.StockMasterID;
                                recEngulfing.StockMaster = currentHist.StockMaster;
                                recordList.Add(recEngulfing);
                            }
                        }
                        //}
                        //}
                    }
                    if (recordList.Count > 0)
                    {
                        context.BEARISH_ENGULFING.AddRange(recordList);
                    }
                }
                stockMaster.BEAR_ENGULF_LastUpDt = DateTime.Today.Date;
                context.StockMaster.Update(stockMaster);
                context.SaveChanges(true);
            }
            catch (Exception ex)
            {
                //throw ex;
            }
            finally
            {
                recordList.Clear();
            }
        }

        /// <summary>
        /// Given a date this method will backtrack and find if we have a down trend
        /// It will try to use "Change" field to check the difference current close & prev close
        ///   if the difference is negative then it will increment the negative counter
        ///   We will do this for given trendSpan and if we have a (trendspan /2) + 1 of negative "Change"
        ///     values then we will mark this as downtrend and do the same for next set of dates
        ///   if after checking Change for given sequence we have negative counter < (trendspan /2) + 1
        ///     we will treat this as break in downtrend and exit the process
        ///     While checking the Change values we will save the Highest Close and its record number
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stockMaster"></param>
        /// <param name="startCounter"></param>
        /// <param name="fromDate"></param>
        /// <param name="highestClose"></param>
        /// <param name="foundCounter"></param>
        /// <param name="trendSpan"></param>
        /// <returns></returns>
        public static bool FindIfDownTrendExist(DBContext context, StockMaster stockMaster,
                    int startCounter, DateTime fromDate, out double highestClose, out int foundCounter, int trendSpan = 5)
        {
            bool breturn = false;
            int negativediff = 0;
            int counter = 1;
            highestClose = 0.00;
            foundCounter = -1;
            StockPriceHistory currentRec = null;
            try
            {
                IQueryable<StockPriceHistory> symbolIQ = context.StockPriceHistory.Where(s => (s.StockMasterID == stockMaster.StockMasterID) && (s.PriceDate.Date >= fromDate.Date));
                for (int j = startCounter; j > 0; j--)
                {
                    currentRec = symbolIQ.AsEnumerable().ElementAt(j);
                    if (currentRec.Close > highestClose)
                    {
                        highestClose = currentRec.Close;
                        foundCounter = currentRec.StockPriceHistoryID;//j;
                    }
                    if (currentRec.Change <= 0)
                    {
                        negativediff++;
                    }
                    if (counter >= trendSpan)
                    {
                        if (negativediff >= ((trendSpan / 2) + 1))
                        {
                            breturn = true;
                        }
                        else
                        {
                            if (breturn == true)
                            {
                                break;
                            }
                            else
                            {
                                foundCounter = -1;
                                highestClose = 0.00;
                                breturn = false;
                                break;
                            }
                        }
                        counter = 1;
                        negativediff = 0;
                    }
                    else
                    {
                        counter++;
                    }
                }
            }
            catch
            {
                breturn = false;
            }
            return breturn;
        }

        /// <summary>
        /// Given a date this method will look ahead from the given date backtrack and 
        /// find if we have a up trend
        /// It will try to use "Change" field to check the difference current close & prev close
        ///   if the difference is positive then it will increment the positive counter
        ///   We will do this for given trendSpan and if we have a (trendspan /2) + 1 of positive "Change"
        ///     values then we will mark this as uptrend and do the same for next set of dates
        ///   if after checking Change for given sequence we have positive counter > (trendspan /2) + 1
        ///     we will treat this as break in uptrend and exit the process
        ///     While checking the Change values we will save the Highest Close and its record number
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stockMaster"></param>
        /// <param name="startCounter"></param>
        /// <param name="fromDate"></param>
        /// <param name="highestClose"></param>
        /// <param name="foundCounter"></param>
        /// <param name="trendSpan"></param>
        /// <returns></returns>
        public static bool FindIfUpTrendExist(DBContext context, StockMaster stockMaster,
                    int startCounter, DateTime fromDate, out double lowestClose, out int foundCounter, int trendSpan = 5)
        {
            bool breturn = false;
            int negativediff = 0;
            int counter = 1;
            lowestClose = 0.00;
            foundCounter = -1;
            StockPriceHistory currentRec = null;
            try
            {
                IQueryable<StockPriceHistory> symbolIQ = context.StockPriceHistory.Where(s => (s.StockMasterID == stockMaster.StockMasterID) && (s.PriceDate.Date >= fromDate.Date));
                for (int j = startCounter; j > 0; j++)
                {
                    currentRec = symbolIQ.AsEnumerable().ElementAt(j);
                    if ((lowestClose == 0.00) || (currentRec.Close < lowestClose))
                    {
                        lowestClose = currentRec.Close;
                        foundCounter = currentRec.StockPriceHistoryID;//j;
                    }
                    if (currentRec.Change <= 0)
                    {
                        negativediff++;
                    }
                    if (counter >= trendSpan)
                    {
                        if (negativediff >= ((trendSpan / 2) + 1))
                        {
                            breturn = true;
                        }
                        else
                        {
                            if (breturn == true)
                            {
                                break;
                            }
                            else
                            {
                                foundCounter = -1;
                                lowestClose = 0.00;
                                breturn = false;
                                break;
                            }
                        }
                        counter = 1;
                        negativediff = 0;
                    }
                    else
                    {
                        counter++;
                    }
                }
            }
            catch
            {
                breturn = false;
            }
            return breturn;
        }

        /// <summary>
        /// For given StockMaster stasting from a year ago it will find a continuous group of green candle or a single candle whoose
        /// change from low to high is >= 20% 
        /// If it finds such sequence then it saves the lowest price in sequence as buy price value and highest price as sell value
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stockMaster"></param>
        public static void V20CandlesticPatternFinder(DBContext context, StockMaster stockMaster)
        {
            List<V20_CANDLE_STRATEGY> newRecords = new List<V20_CANDLE_STRATEGY>();
            try
            {
                if (IsStockUpdatedToday(stockMaster, 7))
                {
                    return;
                }
                double low = 0, high = 0, lowestlow = 0, highesthigh = 0, twentypct = 0;
                StockPriceHistory nextHist = null, currentHist = null;

                string lastPriceDate = IsHistoryUpdated(context, stockMaster);
                if (string.IsNullOrEmpty(lastPriceDate) == false)
                {
                    InitializeHistory(context, stockMaster, lastPriceDate);
                }

                //IQueryable<V20_CANDLE_STRATEGY> v20CandleIQ = from s in context.V20_CANDLE_STRATEGY select s;
                IQueryable<V20_CANDLE_STRATEGY> stockCandleIQ = context.V20_CANDLE_STRATEGY.Where(s => (s.StockMasterID == stockMaster.StockMasterID));

                DateTime lastBuyDate = DateTime.MinValue;

                if (stockCandleIQ.Count() > 0)
                {
                    //context.V20_CANDLE_STRATEGY.RemoveRange(stockCandleIQ.AsEnumerable());
                    //context.SaveChanges();
                    lastBuyDate = stockCandleIQ.OrderBy(s => s.TO_DATE).Last().TO_DATE;
                    if (lastBuyDate.Date.CompareTo(DateTime.Today.Date.AddDays(-365).Date) <= 0)
                    {
                        //that means last buy date is beyond one year
                        lastBuyDate = DateTime.MinValue;
                    }
                }
                //we need to make sure that price history is upto date till yesterday

                //IQueryable<StockPriceHistory> stockpriceIQ = from s in context.StockPriceHistory select s;

                IQueryable<StockPriceHistory> symbolIQ = null;

                if (lastBuyDate == DateTime.MinValue)
                {
                    symbolIQ = context.StockPriceHistory.Where(s => ((s.StockMasterID == stockMaster.StockMasterID) &&
                                                                (s.PriceDate >= DateTime.Today.AddDays(-365))));
                }
                else
                {
                    symbolIQ = context.StockPriceHistory.Where(s => (s.StockMasterID == stockMaster.StockMasterID) &&
                                                                (s.PriceDate.Date >= lastBuyDate.Date));
                }


                if ((symbolIQ != null) && (symbolIQ.Count() > 0))
                {
                    //we will only look back 1 year. hence our first check will be from a record 365 days before
                    for (int rownum = 0; rownum < symbolIQ.Count(); rownum++)
                    {
                        //lowestlow will be stored as buy price within the range
                        low = lowestlow = 0;
                        //highesthigh will be stored as sell price within the range
                        high = highesthigh = 0;

                        currentHist = symbolIQ.AsEnumerable().ElementAt(rownum);
                        //first check if this is green candle. If it is not then we move to the next days candle
                        if (currentHist.Close > currentHist.Open)
                        {
                            var saveV20 = new V20_CANDLE_STRATEGY();
                            saveV20.StockMaster = stockMaster;
                            saveV20.StockMasterID = stockMaster.StockMasterID;
                            saveV20.FROM_DATE = currentHist.PriceDate;
                            twentypct = IsTwentyPctOrMore(currentHist.Low, currentHist.High);
                            //this is green candle
                            if (twentypct >= 20)
                            {
                                //we found a green candle which has price difference of 20% in one day
                                //we will save low & high to indicate buy & sell price & move to next candle
                                saveV20.TO_DATE = currentHist.PriceDate;
                                saveV20.BUY_PRICE = currentHist.Low;
                                saveV20.SELL_PRICE = currentHist.High;
                                saveV20.DIFF_PCT = twentypct;
                                newRecords.Add(saveV20);

                                continue;
                            }
                            //since this is the first green candle in the sequence
                            //we will initialize the low & high values with the first candle values
                            low = lowestlow = currentHist.Low;
                            high = highesthigh = currentHist.High;

                            //start from the next candle & iterate till we find a red candle
                            //If the next candle is red we will break out of for loop
                            //if next candle in the sequence is green
                            //  and if it is then check price difference (from first candle low to current candle high is >= 20%
                            //and if price diff is >= 20% then we have found a sequence of green candles (cup).
                            //We will store the lowest low & highest high in the first green candle
                            //and break out from the for loop
                            //if the price difference is < 20% we will continue with the for loop

                            for (rownum = rownum + 1; rownum < symbolIQ.Count(); rownum++)
                            {
                                nextHist = symbolIQ.AsEnumerable().ElementAt(rownum);
                                //check if the next is green candle
                                if (nextHist.Close < nextHist.Open)
                                {
                                    //this is red candle and we cant continue with this sequence
                                    break;
                                }
                                if (nextHist.Close > nextHist.Open)
                                {
                                    //this is green candle.
                                    //First check if we need to reset the lowestlow & highesthigh values
                                    if (lowestlow > nextHist.Low)
                                    { lowestlow = nextHist.Low; }

                                    if (highesthigh < nextHist.High)
                                    { highesthigh = nextHist.High; }
                                    //now check the price difference is >= 20%.
                                    //Here the low has low price of the first candle in the sequence
                                    twentypct = IsTwentyPctOrMore(low, nextHist.High);
                                    if (twentypct >= 20)
                                    {
                                        //if the price diff is >= 20% we will record the lowestlow & highesthigh in the
                                        //first recrod of the sequence and break out of for loop
                                        saveV20.TO_DATE = nextHist.PriceDate;
                                        saveV20.BUY_PRICE = lowestlow;
                                        saveV20.SELL_PRICE = highesthigh;
                                        saveV20.DIFF_PCT = twentypct;
                                        newRecords.Add(saveV20);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (newRecords.Count > 0)
                    {
                        context.V20_CANDLE_STRATEGY.AddRange(newRecords);
                    }
                }
                stockMaster.V20_LastUpDt = DateTime.Today.Date;
                context.StockMaster.Update(stockMaster);
                context.SaveChanges(true);

            }
            catch (Exception ex)
            {
                //throw ex;
            }
            finally
            {
                newRecords.Clear();
            }
        }

        public static double IsTwentyPctOrMore(double low, double nextlow)
        {
            double twentpct = 0;
            double diff = nextlow - low;
            if (diff > 0)
            {
                twentpct = diff / low * 100;
            }
            return twentpct;
        }
        public static void getQuoteTableFromJSON(string record, string symbol, out DateTime[] quoteDate, out double[] open,
                out double[] high, out double[] low, out double[] close, out double[] volume, out double[] change,
                out double[] changepercent, out double[] prevclose)
        {
            quoteDate = null;
            open = high = low = close = volume = change = changepercent = prevclose = null;

            if (record.ToUpper().Contains("NOT FOUND"))
            {
                return;
            }
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
                    quoteDate = new DateTime[myResult.timestamp.Count];

                    open = new double[myResult.timestamp.Count];
                    high = new double[myResult.timestamp.Count];
                    low = new double[myResult.timestamp.Count];
                    close = new double[myResult.timestamp.Count];
                    volume = new double[myResult.timestamp.Count];
                    change = new double[myResult.timestamp.Count];
                    changepercent = new double[myResult.timestamp.Count];
                    prevclose = new double[myResult.timestamp.Count];
                    int outCtr = 0;
                    for (int i = 0; i < myResult.timestamp.Count; i++)
                    //for (int i = 0; i <= 0; i++)
                    {
                        try
                        {
                            //if ((myQuote.close[i] == null) && (myQuote.high[i] == null) && (myQuote.low[i] == null) && (myQuote.open[i] == null)
                            //&& (myQuote.volume[i] == null))
                            //if(myResult.timestamp[i] == null)
                            if ((myQuote.close[i] == null) && (myQuote.high[i] == null) &&
                                (myQuote.low[i] == null) && (myQuote.open[i] == null) && (myQuote.volume[i] == null))
                            {
                                //I have seen cases where date is valid but rest of the price data including
                                //volume is null. Hence we will skip the record
                                continue;
                            }

                            quoteDate[outCtr] = convertUnixEpochToLocalDateTime(myResult.timestamp[i], myMeta.timezone);

                            if (myQuote.close[i] == null)
                            {
                                close[outCtr] = 0.00;
                            }
                            else
                            {
                                //close = (double)myQuote.close[i];
                                close[outCtr] = System.Convert.ToDouble(string.Format("{0:0.00}", myQuote.close[i]));
                            }

                            if (myQuote.high[i] == null)
                            {
                                high[outCtr] = 0.00;
                            }
                            else
                            {
                                //high = (double)myQuote.high[i];
                                high[outCtr] = System.Convert.ToDouble(string.Format("{0:0.00}", myQuote.high[i]));
                            }

                            if (myQuote.low[i] == null)
                            {
                                low[outCtr] = 0.00;
                            }
                            else
                            {
                                //low = (double)myQuote.low[i];
                                low[outCtr] = System.Convert.ToDouble(string.Format("{0:0.00}", myQuote.low[i]));
                            }

                            if (myQuote.open[i] == null)
                            {
                                open[outCtr] = 0.00;
                            }
                            else
                            {
                                //open = (double)myQuote.open[i];
                                open[outCtr] = System.Convert.ToDouble(string.Format("{0:0.00}", myQuote.open[i]));
                            }
                            if (myQuote.volume[i] == null)
                            {
                                volume[outCtr] = 0;
                            }
                            else
                            {
                                volume[outCtr] = (int)myQuote.volume[i];
                            }
                            if (outCtr == 0)
                            {
                                try
                                {
                                    prevclose[outCtr] = System.Convert.ToDouble(string.Format("{0:0.00}", myMeta.chartPreviousClose));
                                }
                                catch
                                {
                                    prevclose[outCtr] = System.Convert.ToDouble(string.Format("{0:0.00}", 0.00));
                                }
                            }
                            else
                            {
                                prevclose[outCtr] = System.Convert.ToDouble(string.Format("{0:0.00}", close[outCtr - 1]));
                            }
                            //change[outCtr] = close[outCtr] - prevclose[outCtr];
                            change[outCtr] = System.Convert.ToDouble(string.Format("{0:0.00}", (close[outCtr] - prevclose[outCtr])));
                            if (prevclose[outCtr] > 0)
                            {
                                //changepercent[outCtr] = (change[i] / prevclose[i]) * 100;
                                changepercent[outCtr] = System.Convert.ToDouble(string.Format("{0:0.00}", ((change[outCtr] / prevclose[outCtr]) * 100)));
                            }
                            else
                            {
                                changepercent[outCtr] = System.Convert.ToDouble(string.Format("{0:0.00}", 100));
                            }
                            outCtr++;
                        }
                        catch (Exception ex)
                        {
                            //throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //throw ex;
            }
        }

        public static string findTimeZoneId(string zoneId)
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

        public static DateTime convertUnixEpochToLocalDateTime(long dateEpoch, string zoneId)
        {
            DateTime localDateTime;

            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(dateEpoch);
            string timeZoneId = findTimeZoneId(zoneId);
            TimeZoneInfo currentTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            localDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeOffset.UtcDateTime, currentTimeZone);

            return localDateTime;
        }
        public static long convertDateTimeToUnixEpoch(DateTime dtToConvert)
        {
            DateTimeOffset dtoffset = new DateTimeOffset(new DateTime(dtToConvert.Year, dtToConvert.Month, dtToConvert.Day, 0, 0, 0, DateTimeKind.Utc));

            return dtoffset.ToUnixTimeSeconds();
        }

        public static double FindHighestHigh(List<double> listHigh, int start, int count)
        {
            double highestHigh = (listHigh.GetRange(start, count)).Max();
            return highestHigh;
        }
        public static double FindLowestLow(List<double> listLow, int start, int count)
        {
            double lowestLow = (listLow.GetRange(start, count)).Min();
            return lowestLow;
        }
        public static double FindSlowK(List<double> listClose, List<double> listHighestHigh, List<double> listLowestLow)
        {
            double diffcloselow = listClose.Last() - listLowestLow.Last();
            double diffhighlow = listHighestHigh.Last() - listLowestLow.Last();

            double slowK = 0; // ((listClose.Last() - listLowestLow.Last()) / (listHighestHigh.Last() - listLowestLow.Last())) * 100;
            if (diffhighlow > 0)
            {
                slowK = diffcloselow / diffhighlow * 100;
            }
            return Math.Round(slowK, 4);
        }

        public static double FindSlowD(List<double> listSlowK, int start, int count)
        {
            double slowD = listSlowK.GetRange(start, count).Average();
            return Math.Round(slowD, 4);
        }

        /// <summary>
        /// If txnRec is not null then for the symbol in txnRec quote is fetched & gain is updated for 
        ///     this as well as other txnrec with same symbol
        ///     and other operations are executed for that symbol. 
        /// If txnRec is null then for all portfolios 
        ///     unique symbols are found from the txn and for each symbol
        ///         quote is fetched and gain is updated for each txn rec with same symbol
        ///         all other operations are executed for each symbol
        /// </summary>
        /// <param name="context"></param>
        /// <param name="txnRec"></param>
        public static void GetQuoteAndUpdateAllPortfolioTxn(DBContext context, PORTFOLIOTXN txnRec)
        {
            //IQueryable<PORTFOLIOTXN> txnIQ = context.PORTFOLIOTXN.Where(x => x.PORTFOLIO_MASTER_ID == masterRec.PORTFOLIO_MASTER_ID);
            IQueryable<PORTFOLIOTXN> txnIQ = context.PORTFOLIOTXN;//.Where(x => x.PORTFOLIO_MASTER_ID == masterRec.PORTFOLIO_MASTER_ID);
            IQueryable<PORTFOLIOTXN> distinctIQ = null;
            string lastPriceDate = string.Empty;

            if (txnRec == null)
            {
                distinctIQ = txnIQ.GroupBy(a => a.stockMaster.Symbol)
                                    .Select(x => x.FirstOrDefault());
            }
            else //if ((txnRec != null) && (masterRec == null))
            {
                distinctIQ = txnIQ.Where(a => (a.stockMaster.Symbol == txnRec.stockMaster.Symbol))
                            .GroupBy(a => a.stockMaster.Symbol)
                            .Select(a => a.FirstOrDefault());
            }
            foreach (var item in distinctIQ)
            {
                DbInitializer.UpdateStockQuote(context, item.stockMaster);
                DbInitializer.UpdateStockModel(context, item.stockMaster);

                item.CMP = item.stockMaster.Close;
                item.VALUE = item.stockMaster.Close * item.QUANTITY;
                item.GAIN_AMT = item.VALUE - item.TOTAL_COST;
                item.GAIN_PCT = (item.GAIN_AMT / item.VALUE) * 100;
                item.DAYS_SINCE = DateTime.Today.Date.Subtract(item.TXN_DATE).Days;
                item.BUY_VS_52HI = (item.COST_PER_SHARE - item.stockMaster.YEAR_HI) / item.COST_PER_SHARE * 100;

                context.PORTFOLIOTXN.Update(item);
                //now find txn for same symbol in this portfolio
                IQueryable<PORTFOLIOTXN> duplicateIQ = context.PORTFOLIOTXN.Where(a => (a.stockMaster.Symbol == item.stockMaster.Symbol)
                                                                                    && (a.PORTFOLIOTXN_ID != item.PORTFOLIOTXN_ID));
                foreach (var duplicateitem in duplicateIQ)
                {
                    duplicateitem.CMP = item.stockMaster.Close;
                    duplicateitem.VALUE = duplicateitem.QUANTITY * item.stockMaster.Close;
                    duplicateitem.GAIN_AMT = duplicateitem.VALUE - duplicateitem.TOTAL_COST;
                    duplicateitem.GAIN_PCT = (duplicateitem.GAIN_AMT / duplicateitem.VALUE) * 100;
                    duplicateitem.DAYS_SINCE = DateTime.Today.Date.Subtract(duplicateitem.TXN_DATE).Days;
                    duplicateitem.BUY_VS_52HI = (duplicateitem.COST_PER_SHARE - duplicateitem.stockMaster.YEAR_HI) / duplicateitem.COST_PER_SHARE * 100;

                    context.PORTFOLIOTXN.Update(duplicateitem);
                }
                context.SaveChanges(true);
                //DbInitializer.UpdateStockModel(context, item.stockMaster);
            }
        }

        /// <summary>
        /// For given groupid this method gets quote and also executes other operations
        /// </summary>
        /// <param name="context"></param>
        /// <param name="groupId">if groupid >= 0 then indeividual stockid else if -99 = V40, V40N, V200 else if -98 = V40 else if -97 = V40N else if -96 = V200 else if -95 = ALL stocks</param>
        public static void UpdateQuoteStrategy(DBContext context, int groupId)
        {
            IQueryable<StockMaster> stockmasterIQ = null;
            string lastPriceDate = string.Empty;
            try
            {
                if (groupId == -99)
                {
                    stockmasterIQ = context.StockMaster.Where(s => ((s.V200 == true) || (s.V40 == true) || (s.V40N == true)));
                }
                else if (groupId == -98)
                {
                    stockmasterIQ = context.StockMaster.Where(s => (s.V40 == true));
                }
                else if (groupId == -97)
                {
                    stockmasterIQ = context.StockMaster.Where(s => (s.V40N == true));
                }
                else if (groupId == -96)
                {
                    stockmasterIQ = context.StockMaster.Where(s => (s.V200 == true));
                }
                else if (groupId == -95)
                {
                    stockmasterIQ = context.StockMaster;
                }
                else if (groupId > 0)
                {
                    stockmasterIQ = context.StockMaster.Where(s => (s.StockMasterID == groupId));
                }
                if ((stockmasterIQ != null) && (stockmasterIQ.Count() > 0))
                {
                    foreach (var item in stockmasterIQ)
                    {
                        DbInitializer.UpdateStockQuote(context, item);
                        DbInitializer.UpdateStockModel(context, item);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

    }
}
