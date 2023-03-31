using MarketAnalytics.Data;
using MarketAnalytics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Model;
using Microsoft.CodeAnalysis.Text;
//using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MarketAnalytics.Pages.StandardIndicators
{
    public class chartHistory : PageModel
    {
        public List<StockPriceHistory> listHistory;
        public List<StockPriceHistory> listIndexHistory;
        public List<SelectListItem> indexList;
        public List<DateTime> listBuyDates;
        public List<DateTime> listSellDates;
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public int? CurrentID { get; set; }
        public int? OnlyHistory { get; set; }
        public DateTime SellDate { get; set; }
        public DateTime BuyDate { get; set; }
        public DateTime FromDate { get; set; }
        public string Symbol { get; set; }
        public string CompanyName { get; set; }
        public string InvestmentType { get; set; }
        public string ChartContent { get; set; }
        public chartHistory(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            listHistory = new List<StockPriceHistory>();
            listIndexHistory = new List<StockPriceHistory>();
            listBuyDates = new List<DateTime>();
            listSellDates = new List<DateTime>();
            indexList = new List<SelectListItem>();
        }

        //caller should send fromdate in buydate
        //selldate if not specified will be used as today or null
        public async Task OnGetAsync(int? stockid, string buyDate, string sellDate, string fromDate, int? onlyHistory, bool history,
            int? selectedindex)
        {
            listHistory.Clear();
            listIndexHistory.Clear();
            if (_context.StockPriceHistory != null)
            {
                if (stockid != null)
                {
                    CurrentID = stockid;
                }
                else
                {
                    CurrentID = _context.StockPriceHistory
                        //.Include(a => a.StockMaster)
                        .AsSplitQuery().FirstOrDefault().StockMasterID;
                }

                StockMaster tempSM = _context.StockMaster.AsSplitQuery().FirstOrDefault(a => a.StockMasterID == CurrentID);
                Symbol = tempSM.Symbol;
                CompanyName = tempSM.CompName;

                OnlyHistory = 0;

                indexList.Clear();
                indexList = _context.StockMaster.Where(a => a.INVESTMENT_TYPE.Equals("Index")).OrderBy(a => a.Symbol)
                        .Select(a =>
                            new SelectListItem
                            {
                                Value = a.StockMasterID.ToString(),
                                Text = a.Symbol
                            }
                        ).ToList();

                if ((selectedindex == null) || (selectedindex <= 0))
                {
                    selectedindex = _context.StockMaster.Where(a => a.Symbol == "^NSEI").FirstOrDefault().StockMasterID;
                    indexList.FirstOrDefault(a => a.Text.Equals("^NSEI")).Selected = true;
                }
                indexList.FirstOrDefault(a => a.Value.Equals(selectedindex.ToString())).Selected = true;

                if (onlyHistory != null)
                {
                    OnlyHistory = onlyHistory;
                }
                if (string.IsNullOrEmpty(buyDate) == false)
                {
                    BuyDate = System.Convert.ToDateTime(buyDate).Date;
                }
                else
                {
                    BuyDate = DateTime.MinValue;
                }

                if (string.IsNullOrEmpty(sellDate) == false)
                {
                    SellDate = System.Convert.ToDateTime(sellDate).Date;
                }
                else
                {
                    SellDate = DateTime.MinValue;
                }
                if (string.IsNullOrEmpty(fromDate) == false)
                {
                    FromDate = System.Convert.ToDateTime(fromDate).Date;
                }
                else
                {
                    FromDate = DateTime.MinValue;
                }
                ChartContent = "Stock history - ";
                var selectedRecord = _context.StockMaster.AsSplitQuery().Where(x => (x.StockMasterID == CurrentID)).FirstOrDefault();
                if (selectedRecord != null)
                {
                    InvestmentType = selectedRecord.INVESTMENT_TYPE;
                }
                if (onlyHistory == 1)
                {
                    listBuyDates = _context.BEARISH_ENGULFING
                        //.Include(a => a.StockMaster)
                        .AsSplitQuery().Where(a => a.StockMasterID == CurrentID)
                        .Select(a => new DateTime(a.BUY_CANDLE_DATE.Year, a.BUY_CANDLE_DATE.Month, a.BUY_CANDLE_DATE.Day)).ToList();
                    listSellDates = _context.BEARISH_ENGULFING
                        //.Include(a => a.StockMaster)
                        .AsSplitQuery().Where(a => a.StockMasterID == CurrentID)
                        .Select(a => new DateTime(a.SELL_CANDLE_DATE.Year, a.SELL_CANDLE_DATE.Month, a.SELL_CANDLE_DATE.Day)).ToList();
                    ChartContent = "Bearish Engulfing Strategy: ";
                }
                else if (onlyHistory == 2)
                {
                    listBuyDates = _context.BULLISH_ENGULFING_STRATEGY
                        //.Include(a => a.StockMaster)
                        .AsSplitQuery().Where(a => a.StockMasterID == CurrentID)
                        .Select(a => new DateTime(a.BUY_CANDLE_DATE.Year, a.BUY_CANDLE_DATE.Month, a.BUY_CANDLE_DATE.Day)).ToList();
                    listSellDates = _context.BULLISH_ENGULFING_STRATEGY
                        //.Include(a => a.StockMaster)
                        .AsSplitQuery().Where(a => a.StockMasterID == CurrentID)
                        .Select(a => new DateTime(a.SELL_CANDLE_DATE.Year, a.SELL_CANDLE_DATE.Month, a.SELL_CANDLE_DATE.Day)).ToList();
                    ChartContent = "Bullish Engulfing Strategy: ";
                }
                else if (onlyHistory == 3)
                {
                    listBuyDates = _context.V20_CANDLE_STRATEGY
                        //.Include(a => a.StockMaster)
                        .AsSplitQuery().Where(a => a.StockMasterID == CurrentID)
                        .Select(a => new DateTime(a.FROM_DATE.Year, a.FROM_DATE.Month, a.FROM_DATE.Day)).ToList();
                    listSellDates = _context.V20_CANDLE_STRATEGY
                        //.Include(a => a.StockMaster)
                        .AsSplitQuery().Where(a => a.StockMasterID == CurrentID)
                        .Select(a => new DateTime(a.TO_DATE.Year, a.TO_DATE.Month, a.TO_DATE.Day)).ToList();
                    ChartContent = "V20 Strategy: ";
                }

                if (onlyHistory == 0)
                {
                    if (FromDate != DateTime.MinValue)
                    {
                        listHistory = _context.StockPriceHistory
                            //.Include(a => a.StockMaster)
                            .AsSplitQuery().Where(a => (a.StockMasterID == CurrentID) && (a.PriceDate.Date.CompareTo(FromDate.Date) >= 0)).ToList();

                        if ((selectedindex != null) && (selectedindex != -1))
                        {
                            listIndexHistory = _context.StockPriceHistory
                            //.Include(a => a.StockMaster)
                            .AsSplitQuery().Where(a => (a.StockMasterID == selectedindex) && (a.PriceDate.Date.CompareTo(FromDate.Date) >= 0)).ToList();
                        }
                    }
                    else
                    {
                        listHistory = _context.StockPriceHistory
                            //.Include(a => a.StockMaster)
                            .AsSplitQuery().Where(a => a.StockMasterID == CurrentID).ToList();
                        if ((selectedindex != null) && (selectedindex != -1))
                        {
                            listIndexHistory = _context.StockPriceHistory
                            //.Include(a => a.StockMaster)
                            .AsSplitQuery().Where(a => a.StockMasterID == selectedindex).ToList();
                        }
                    }
                }
                else
                {
                    if (listBuyDates.Min().Date.CompareTo(listSellDates.Min()) < 0)
                    {
                        listHistory = _context.StockPriceHistory
                            //.Include(a => a.StockMaster)
                            .AsSplitQuery().Where(a => (a.StockMasterID == CurrentID) && (a.PriceDate.Date.CompareTo(listBuyDates.Min().Date.AddDays(-10)) >= 0)).ToList();
                    }
                    else
                    {
                        listHistory = _context.StockPriceHistory
                            //.Include(a => a.StockMaster)
                            .AsSplitQuery().Where(a => (a.StockMasterID == CurrentID) && (a.PriceDate.Date.CompareTo(listSellDates.Min().Date.AddDays(-10)) >= 0)).ToList();
                    }

                }
            }
        }
    }
}
