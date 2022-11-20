﻿using MarketAnalytics.Models;
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

namespace MarketAnalytics.Data
{
    public class DbInitializer
    {
        public static string urlNSEStockMaster = "http://www1.nseindia.com/content/equities/EQUITY_L.csv";
        public static string urlGetHistoryQuote = "https://query1.finance.yahoo.com/v8/finance/chart/{0}?period1={1}&period2={2}&interval={3}&filter=history&frequency={4}&includeAdjustedClose={5}";

        public static string urlGlobalQuote = "https://query1.finance.yahoo.com/v8/finance/chart/{0}?range=1d&interval=1d&indicators=quote&timestamp=true";

        static readonly HttpClient client = new HttpClient();

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
                throw e;
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

                IQueryable<StockMaster> stockmasterIQ = from s in context.StockMaster select s;
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
                    if (IsMasterUpdated(context, fields[0], fields[1], "NS"))
                    {
                        GetQuote(fields[0] + ".NS", out quoteDate, out open, out high, out low, out close,
                                    out volume, out change, out changepercent, out prevclose);
                        if (quoteDate != null)
                        { //find if stock exist in StockMaster, if not add it to context
                            var recTOAdd = new StockMaster();
                            currentMaster = stockmasterIQ.Where(s => s.Symbol.ToUpper().Equals(fields[0].ToUpper())
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
                context.SaveChanges();
            }
        }

        public static void InitializeHistory(DBContext context, StockMaster stockMaster, string symbol, string compname, string exchange, string fromDate = null)
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
                    GetHistoryQuote(symbol + "." + exchange, DateTime.Today.Date.AddYears(-10).ToString("yyyy-MM-dd"), DateTime.Today.Date.AddDays(1).ToString("yyyy-MM-dd"), out quoteDate, out open, out high, out low, out close,
                                    out volume, out change, out changepercent, out prevclose);
                }
                else
                {
                    GetHistoryQuote(symbol + "." + exchange, Convert.ToDateTime(fromDate).ToString("yyyy-MM-dd"), DateTime.Today.Date.AddDays(1).ToString("yyyy-MM-dd"), out quoteDate, out open, out high, out low, out close,
                                    out volume, out change, out changepercent, out prevclose);
                }
                //read first line which is list of fields
                if ((quoteDate != null) && (quoteDate.Length > 0))
                {
                    IQueryable<StockPriceHistory> stockpriceIQ = from s in context.StockPriceHistory select s;
                    IQueryable<StockPriceHistory> currentPrice;
                    for (int i = 0; i < quoteDate.Length; i++)
                    {
                        //find if stock exist in StockMaster, if not add it to context
                        var recTOAdd = new StockPriceHistory();
                        //currentPrice = stockpriceIQ.Where(s => s.StockMaster.Symbol.ToUpper().Equals(symbol.ToUpper())
                        //                    && s.StockMaster.Exchange.ToUpper().Equals(exchange.ToUpper())
                        //                    && s.PriceDate.Equals(quoteDate[i]));
                        currentPrice = stockpriceIQ.Where(s => ((s.StockMasterID == stockMaster.StockMasterID) &&
                                            (s.StockMaster.Symbol.ToUpper().Equals(symbol.ToUpper()))
                                            && (s.StockMaster.Exchange.ToUpper().Equals(exchange.ToUpper()))
                                            && (s.PriceDate.Date.Equals(quoteDate[i].Date))));
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
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

            }
        }
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
            }
        }

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
            }
        }

        public static bool IsMasterUpdated(DBContext context, string symbol, string compname, string exchange)
        {
            bool breturn = true;
            try
            {
                //IQueryable<StockPriceHistory> stockpriceIQ = from s in context.StockPriceHistory select s;
                IQueryable<StockMaster> stockmasterIQ = from s in context.StockMaster select s;

                IQueryable<StockMaster> currentStockIQ = stockmasterIQ.Where(s => (s.Symbol.Equals(symbol)) && (s.CompName.Equals(compname)) &&
                                                                                  (s.Exchange.Equals(exchange)));

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
            catch (Exception ex)
            {
                breturn = true;
            }
            return breturn;
        }

        public static string IsHistoryUpdated(DBContext context, StockMaster stockMaster, int? stockMasterID)
        {
            string lastPriceDate = string.Empty;
            try
            {
                IQueryable<StockPriceHistory> stockpriceIQ = from s in context.StockPriceHistory select s;

                IQueryable<StockPriceHistory> givenStockIQ = stockpriceIQ.Where(s => (s.StockMasterID == stockMasterID));

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
            catch (Exception ex)
            {
                lastPriceDate = string.Empty;
            }
            return lastPriceDate;
        }

        //public static void InitializeHistory(DBContext context, StockMaster stockMaster, string symbol, string compname, string exchange)
        public static void getRSIDataTableFromDaily(DBContext context, StockMaster stockMaster, string symbol, string exchange, int? stockMasterID, string compname,
                                    string seriestype = "CLOSE", string time_interval = "1d", string fromDate = null,
                                    string period = "14", bool stochRSI = false, bool refreshHistory = false)
        {
            //DataTable rsiDataTable = null;
            int iPeriod;
            double change, gain, loss, avgGain = 0.00, avgLoss = 0.00, rs, rsi;
            double sumOfGain = 0.00, sumOfLoss = 0.00;
            //DateTime dateCurrentRow = DateTime.Today;
            List<string> seriesNameList;
            StockPriceHistory currentHist = null, prevHist = null;
            try
            {
                //if (refreshHistory)
                //{
                //    InitializeHistory(context, stockMaster, symbol, compname, exchange);
                //}
                string lastPriceDate = IsHistoryUpdated(context, stockMaster, stockMasterID);
                if (string.IsNullOrEmpty(lastPriceDate) == false)
                {
                    InitializeHistory(context, stockMaster, symbol, compname, exchange, lastPriceDate);
                }

                IQueryable<StockPriceHistory> stockpriceIQ = from s in context.StockPriceHistory select s;
                //List<StockPriceHistory> chartDataList = (stockpriceIQ.Where(s => (s.StockMasterID == CurrentID))).ToList();

                //IQueryable<StockPriceHistory> rsiIQ = stockpriceIQ.Where(s => (s.StockMasterID == stockMasterID));

                IQueryable<StockPriceHistory> rsiIQ = stockpriceIQ.Where(s => (s.StockMasterID == stockMasterID) &&
                                s.PriceDate.Date >= (Convert.ToDateTime(fromDate).Date));

                //StockPriceHistory lastHistoryRec = symbolIQ.OrderBy(p => p.PriceDate).LastOrDefault();

                //if (Convert.ToDateTime(lastHistoryRec.PriceDate).Date.CompareTo(DateTime.Today.Date) < 0)
                //{
                //    InitializeHistory(context, stockMaster, symbol, compname, exchange, lastHistoryRec.PriceDate.ToString("yyyy-MM-dd"));
                //}

                //stockpriceIQ = from s in context.StockPriceHistory select s;
                ////List<StockPriceHistory> chartDataList = (stockpriceIQ.Where(s => (s.StockMasterID == CurrentID))).ToList();

                //rsiIQ = stockpriceIQ.Where(s => (s.StockMasterID == stockMasterID) &&
                //                s.PriceDate.Date >= (Convert.ToDateTime(fromDate).Date));

                //rsiList = (stockpriceIQ.Where(s => (s.StockMasterID == stockMasterID))).ToList();
                if ((rsiIQ != null) && (rsiIQ.Count() > 0))
                {
                    iPeriod = System.Convert.ToInt32(period);
                    if (stochRSI == false)
                    {
                        seriesNameList = new List<string> { seriestype };
                    }
                    else
                    {
                        seriesNameList = new List<string> { "Close", "Open", "High", "Low" };
                    }
                    foreach (var item in seriesNameList)
                    {
                        change = gain = loss = avgGain = avgLoss = rs = rsi = 0.00;
                        sumOfGain = sumOfLoss = 0.00;
                        for (int rownum = 1; rownum < rsiIQ.Count(); rownum++)
                        {


                            currentHist = rsiIQ.AsEnumerable().ElementAt(rownum);
                            prevHist = rsiIQ.AsEnumerable().ElementAt(rownum - 1);

                            if (item.ToString().Equals("Low"))
                            {
                                change = System.Convert.ToDouble(currentHist.Low) - System.Convert.ToDouble(prevHist.Low);
                            }
                            else if (item.ToString().Equals("Open"))
                            {
                                change = System.Convert.ToDouble(currentHist.Open) - System.Convert.ToDouble(prevHist.Open);
                            }
                            else if (item.ToString().Equals("High"))
                            {
                                change = System.Convert.ToDouble(currentHist.High) - System.Convert.ToDouble(prevHist.High);
                            }
                            else //if (item.ToString().Equals("Close"))
                            {
                                change = System.Convert.ToDouble(currentHist.Close) - System.Convert.ToDouble(prevHist.Close);
                            }

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
                                    rs = avgGain / avgLoss;
                                    rsi = 100 - (100 / (1 + rs));
                                }
                                else
                                {
                                    avgGain = ((avgGain * (iPeriod - 1)) + gain) / iPeriod;
                                    avgLoss = ((avgLoss * (iPeriod - 1)) + loss) / iPeriod;
                                    rs = avgGain / avgLoss;
                                    rsi = 100 - (100 / (1 + rs));
                                }
                            }
                            //dailyTable.Rows[rownum]["RSI"] = Math.Round(rsi, 2);
                            if (item.ToString().Equals("Low"))
                            {
                                currentHist.RSI_LOW = Math.Round(rsi, 2);
                            }
                            else if (item.ToString().Equals("High"))
                            {
                                currentHist.RSI_HIGH = Math.Round(rsi, 2);
                            }
                            else if (item.ToString().Equals("Open"))
                            {
                                currentHist.RSI_OPEN = Math.Round(rsi, 2);
                            }
                            else
                            {
                                currentHist.RSI_CLOSE = Math.Round(rsi, 2);
                            }
                            context.StockPriceHistory.Update(currentHist);

                        }
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("getRSIDataTableFromDaily exception: " + ex.Message);
            }
        }

        public static void GetSMA_EMA_MACD_BBANDS_Table(DBContext context, StockMaster stockMaster, string symbol, string exchange,
                                    int? stockMasterID, string compname,
                                    string seriestype = "CLOSE", string time_interval = "1d", string fromDate = null,
                                    int small_fast_Period = 20, int mid_period = 50, int long_slow_Period = 200, bool emaRequired = false, bool macdRequired = false,
                                    //string fastperiod = "12", string slowperiod = "26", 
                                    int signalperiod = 9, bool bbandsRequired = false, int stddeviation = 2, bool refreshHistory = false)
        {
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

            double sumLong = 0;
            double[] valuesLong = (long_slow_Period > 0) ? new double[long_slow_Period] : null;
            int indexLong = 0;
            StockPriceHistory currentHist = null, prevHist = null;

            bool bBuyFlagSet = false;
            bool bSellFlagSet = false;
            try
            {
                //if (refreshHistory)
                //{
                //    InitializeHistory(context, stockMaster, symbol, compname, exchange);
                //}
                string lastPriceDate = IsHistoryUpdated(context, stockMaster, stockMasterID);
                if (string.IsNullOrEmpty(lastPriceDate) == false)
                {
                    InitializeHistory(context, stockMaster, symbol, compname, exchange, lastPriceDate);
                }

                IQueryable<StockPriceHistory> stockpriceIQ = from s in context.StockPriceHistory select s;
                //List<StockPriceHistory> chartDataList = (stockpriceIQ.Where(s => (s.StockMasterID == CurrentID))).ToList();

                IQueryable<StockPriceHistory> symbolIQ = stockpriceIQ.Where(s => (s.StockMasterID == stockMasterID) &&
                                s.PriceDate.Date >= (Convert.ToDateTime(fromDate).Date));



                ////find last date and if its not TODAY then refreshHistory
                //StockPriceHistory lastHistoryRec = symbolIQ.OrderBy(p => p.PriceDate).LastOrDefault();

                //if (Convert.ToDateTime(lastHistoryRec.PriceDate).Date.CompareTo(DateTime.Today.Date) < 0)
                //{
                //    InitializeHistory(context, stockMaster, symbol, compname, exchange, lastHistoryRec.PriceDate.ToString("yyyy-MM-dd"));
                //}

                //stockpriceIQ = from s in context.StockPriceHistory select s;
                ////List<StockPriceHistory> chartDataList = (stockpriceIQ.Where(s => (s.StockMasterID == CurrentID))).ToList();

                //symbolIQ = stockpriceIQ.Where(s => (s.StockMasterID == stockMasterID) &&
                //                s.PriceDate.Date >= (Convert.ToDateTime(fromDate).Date));

                if ((symbolIQ != null) && (symbolIQ.Count() > 0))
                {
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
                        }

                        currentClosePrice = System.Convert.ToDouble(currentHist.Close);
                        if (small_fast_Period > 0)
                        {    //subtract the oldest CLOSE PRICE from the previous SUM and then add the current CLOSE PRICE
                            sumSmall = sumSmall - valuesSmall[indexSmall] + currentClosePrice;
                            valuesSmall[indexSmall] = currentClosePrice;

                            currentHist.SMA_SMALL = smallSMA = Math.Round((sumSmall / small_fast_Period), 2);
                            indexSmall = (indexSmall + 1) % small_fast_Period;

                            if ((currentHist.Open < smallSMA) && (currentHist.Close < smallSMA))
                            {
                                //means current candle is below SMA_SMALL
                                currentHist.LOWER_THAN_SMA_SMALL = true;

                                if ((prevHist != null) && (prevHist.Open < smallSMA) && (prevHist.Close < smallSMA) && (prevHist.Close < prevHist.Open) &&
                                    (prevHist.Close > currentHist.Open) && (prevHist.Close < currentHist.Close) && (prevHist.Open < currentHist.Close) &&
                                    (prevHist.Open > currentHist.Close))
                                {
                                    //means previous day candle below SMA_SMALL and stock closed below open indicating a red candle
                                    //also the prev candle is engulfed by current candle as prev open/close are withing current open/close
                                    currentHist.BULLISH_ENGULFING = true;
                                }
                            }
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

                            currentHist.CROSSOVER_FLAG = (smallSMA > longSMA) ? "GT" : "LT";
                        }

                        //check if buy flag can be set to 1
                        if ((!bBuyFlagSet) && (midSMA < longSMA) && (smallSMA < midSMA) && (currentClosePrice < smallSMA))
                        {
                            currentHist.BUY_SMA_STRATEGY = currentHist.Close;
                            bBuyFlagSet = true;
                            bSellFlagSet = false;
                        }
                        if ((!bSellFlagSet) && (midSMA > longSMA) && (smallSMA > midSMA) && (currentClosePrice > smallSMA))
                        {
                            currentHist.SELL_SMA_STRATEGY = currentHist.Close;
                            bSellFlagSet = true;
                            bBuyFlagSet = false;
                        }
                        context.StockPriceHistory.Update(currentHist);
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetSMA_EMA_MACD_BBANDS_Table exception: " + ex.Message);
            }
        }

        public static void GetSMA_BUYSELL(DBContext context, StockMaster stockMaster, string symbol, string exchange,
                            int? stockMasterID, string compname, int periodsmall, int periodmid, int periodlong)
        {
            //first get the SMA for three periods
            GetSMA(context, stockMaster, symbol, exchange, stockMasterID, compname, periodsmall, 0);
            GetSMA(context, stockMaster, symbol, exchange, stockMasterID, compname, periodmid, 1);
            GetSMA(context, stockMaster, symbol, exchange, stockMasterID, compname, periodlong, 2);

            IQueryable<StockMaster> stockmasterIQ = from s in context.StockMaster select s;
            IQueryable<StockMaster> symbolmasterIQ = stockmasterIQ.Where(s => (s.StockMasterID == stockMasterID));

            StockMaster currentMaster = symbolmasterIQ.First();

            IQueryable<StockPriceHistory> stockpriceIQ = from s in context.StockPriceHistory select s;
            IQueryable<StockPriceHistory> symbolIQ = stockpriceIQ.Where(s => (s.StockMasterID == stockMasterID));

            //StockPriceHistory currentHist = symbolIQ.AsEnumerable().ElementAt(symbolIQ.Count() - 1);
            StockPriceHistory currentHist = symbolIQ.AsEnumerable().Last();

            //now set BUY FLGA if the current close < smallsma < midsma < longsma
            currentHist.BUY_SMA_STRATEGY = 0;
            currentHist.SELL_SMA_STRATEGY = 0;
            currentMaster.SMA_SLOW = (double)currentHist.SMA_LONG;
            currentMaster.SMA_MID = (double)currentHist.SMA_MID;
            currentMaster.SMA_FAST = (double)currentHist.SMA_SMALL;
            currentMaster.SMA_BUY_SIGNAL = false;
            currentMaster.SMA_SELL_SIGNAL = false;

            if ((currentHist.Close < currentHist.SMA_SMALL) && (currentHist.SMA_SMALL < currentHist.SMA_MID) &&
                (currentHist.SMA_MID < currentHist.SMA_LONG))
            {
                currentHist.BUY_SMA_STRATEGY = currentHist.Close;
                currentMaster.SMA_SLOW = (double)currentHist.SMA_LONG;
                currentMaster.SMA_MID = (double)currentHist.SMA_MID;
                currentMaster.SMA_FAST = (double)currentHist.SMA_SMALL;
                currentMaster.SMA_BUY_SIGNAL = true;
            }
            //now set SELL FLGA if the current close > smallsma > midsma > longsma
            else if ((currentHist.Close > currentHist.SMA_SMALL) && (currentHist.SMA_SMALL > currentHist.SMA_MID) &&
                (currentHist.SMA_MID > currentHist.SMA_LONG))
            {
                currentHist.SELL_SMA_STRATEGY = currentHist.Close;
                currentMaster.SMA_SLOW = (double)currentHist.SMA_LONG;
                currentMaster.SMA_MID = (double)currentHist.SMA_MID;
                currentMaster.SMA_FAST = (double)currentHist.SMA_SMALL;
                currentMaster.SMA_SELL_SIGNAL = true;

            }
            context.StockPriceHistory.Update(currentHist);
            context.StockMaster.Update(currentMaster);

            context.SaveChanges();
        }
        //whichSMA = 0 then fast, 1 = mid, 2 = slow
        public static void GetSMA(DBContext context, StockMaster stockMaster, string symbol, string exchange,
                                    int? stockMasterID, string compname, int period, int whichSMA)
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
                string lastPriceDate = IsHistoryUpdated(context, stockMaster, stockMasterID);
                if (string.IsNullOrEmpty(lastPriceDate) == false)
                {
                    InitializeHistory(context, stockMaster, symbol, compname, exchange, lastPriceDate);
                }

                IQueryable<StockPriceHistory> stockpriceIQ = from s in context.StockPriceHistory select s;
                //List<StockPriceHistory> chartDataList = (stockpriceIQ.Where(s => (s.StockMasterID == CurrentID))).ToList();

                IQueryable<StockPriceHistory> symbolIQ = stockpriceIQ.Where(s => (s.StockMasterID == stockMasterID));


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

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("GetSMA_EMA_MACD_BBANDS_Table exception: " + ex.Message);
            }
        }

        public static void V20CandlesticPatternFinder(DBContext context, StockMaster stockMaster)
        {
            double low = 0, high = 0, lowestlow = 0, highesthigh = 0, twentypct = 0;
            StockPriceHistory nextHist = null, currentHist = null;
            List<V20_CANDLE_STRATEGY> newRecords = new List<V20_CANDLE_STRATEGY>();

            try
            {
                IQueryable<V20_CANDLE_STRATEGY> v20CandleIQ = from s in context.V20_CANDLE_STRATEGY select s;
                IQueryable<V20_CANDLE_STRATEGY> stockCandleIQ = v20CandleIQ.Where(s => (s.StockMasterID == stockMaster.StockMasterID));

                context.V20_CANDLE_STRATEGY.RemoveRange(stockCandleIQ.AsEnumerable());
                context.SaveChanges();

                //we need to make sure that price history is upto date till yesterday
                string lastPriceDate = IsHistoryUpdated(context, stockMaster, stockMaster.StockMasterID);
                if (string.IsNullOrEmpty(lastPriceDate) == false)
                {
                    InitializeHistory(context, stockMaster, stockMaster.Symbol, stockMaster.CompName, stockMaster.Exchange, lastPriceDate);
                }

                IQueryable<StockPriceHistory> stockpriceIQ = from s in context.StockPriceHistory select s;
                IQueryable<StockPriceHistory> symbolIQ = stockpriceIQ.Where(s => (s.StockMasterID == stockMaster.StockMasterID));

                if ((symbolIQ != null) && (symbolIQ.Count() > 0))
                {
                    //we will only look back 1 year. hence our first check will be from a record 365 days before
                    for (int rownum = symbolIQ.Count() - 365; rownum < symbolIQ.Count(); rownum++)
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
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            { }

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

                    for (int i = 0; i < myResult.timestamp.Count; i++)
                    //for (int i = 0; i <= 0; i++)
                    {
                        if ((myQuote.close[i] == null) && (myQuote.high[i] == null) && (myQuote.low[i] == null) && (myQuote.open[i] == null)
                            && (myQuote.volume[i] == null))
                        {
                            continue;
                        }

                        quoteDate[i] = convertUnixEpochToLocalDateTime(myResult.timestamp[i], myMeta.timezone);

                        if (myQuote.close[i] == null)
                        {
                            close[i] = 0.00;
                        }
                        else
                        {
                            //close = (double)myQuote.close[i];
                            close[i] = System.Convert.ToDouble(string.Format("{0:0.00}", myQuote.close[i]));
                        }

                        if (myQuote.high[i] == null)
                        {
                            high[i] = 0.00;
                        }
                        else
                        {
                            //high = (double)myQuote.high[i];
                            high[i] = System.Convert.ToDouble(string.Format("{0:0.00}", myQuote.high[i]));
                        }

                        if (myQuote.low[i] == null)
                        {
                            low[i] = 0.00;
                        }
                        else
                        {
                            //low = (double)myQuote.low[i];
                            low[i] = System.Convert.ToDouble(string.Format("{0:0.00}", myQuote.low[i]));
                        }

                        if (myQuote.open[i] == null)
                        {
                            open[i] = 0.00;
                        }
                        else
                        {
                            //open = (double)myQuote.open[i];
                            open[i] = System.Convert.ToDouble(string.Format("{0:0.00}", myQuote.open[i]));
                        }
                        if (myQuote.volume[i] == null)
                        {
                            volume[i] = 0;
                        }
                        else
                        {
                            volume[i] = (int)myQuote.volume[i];
                        }
                        prevclose[i] = System.Convert.ToDouble(string.Format("{0:0.00}", myMeta.chartPreviousClose));
                        change[i] = close[i] - prevclose[i];
                        changepercent[i] = (change[i] / prevclose[i]) * 100;
                        change[i] = System.Convert.ToDouble(string.Format("{0:0.00}", change[i]));
                        changepercent[i] = System.Convert.ToDouble(string.Format("{0:0.00}", changepercent[i]));
                    }
                }
            }
            catch (Exception ex)
            {
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

    }
}
