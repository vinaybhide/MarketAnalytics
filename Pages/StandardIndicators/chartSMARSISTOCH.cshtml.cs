using MarketAnalytics.Data;
using MarketAnalytics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace MarketAnalytics.Pages.StandardIndicators
{
    public class chartSMARSISTOCHModel : PageModel
    {
        public List<StockPriceHistory> listSMA = new List<StockPriceHistory>();
        public List<StockPriceHistory> listBuy = new List<StockPriceHistory>();
        public List<StockPriceHistory> listSell = new List<StockPriceHistory>();
        public List<SelectListItem> symbolList { get; set; }

        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public int? CurrentID { get; set; }
        public DateTime FromDate { get; set; } = DateTime.Today.Date.AddYears(-1).Date;

        public string SMAFastPeriod { get; set; } = default(string);
        public string SMAMidPeriod { get; set; } = default(string);
        public string SMASlowPeriod { get; set; } = default(string);
        public string RSIPeriod { get; set; } = default(string);
        public string FastK { get; set; } = default(string);
        public string SlowD { get; set; } = default(string);
        public StockMaster StockMasterRec { get; set; }

        public chartSMARSISTOCHModel(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            FromDate = DateTime.Today.AddYears(-10);
            SMAFastPeriod = "20";
            SMAMidPeriod = "50";
            SMASlowPeriod = "200";
            RSIPeriod = "14";
            FastK = "20";
            SlowD = "20";

            symbolList = new List<SelectListItem>();
        }

        public async Task OnGetAsync(int? id, string fromDate, string inputFastPeriod, string inputMidPeriod,
            string inputSlowPeriod, string inputRsiPeriod, string inputFastK, string inputSlowD, int? symbolToUpdate)
        {
            listSMA.Clear();
            symbolList.Clear();
            symbolList = _context.StockMaster.Where(x => ((x.V200 == true) || (x.V40 == true) || (x.V40N == true))).Select(a =>
                                                          new SelectListItem
                                                          {
                                                              Value = a.StockMasterID.ToString(),
                                                              Text = a.Symbol
                                                          }).ToList();

            if (_context.StockPriceHistory != null)
            {
                StockMasterRec = null;
                if ((id != null) || (symbolToUpdate != null))
                {
                    //if ((id == null) && (symbolToUpdate != null))
                    if (symbolToUpdate != null)
                    {
                        id = symbolToUpdate;
                    }

                    //if ((id != null) && (symbolToUpdate == null))
                    //{
                    if (symbolList.Exists(a => (a.Value.Equals(id.ToString()) == true)))
                    {
                        symbolList.FirstOrDefault(a => a.Value.Equals(id.ToString())).Selected = true;
                    }
                    //}

                    CurrentID = id;
                    //var selectedRecord = await _context.StockMaster.FirstOrDefaultAsync(m => m.StockMasterID == id);
                    StockMasterRec = await _context.StockMaster.FirstOrDefaultAsync(m => m.StockMasterID == id);
                    //}
                    //else
                    //{
                    //    CurrentID = _context.StockPriceHistory.FirstOrDefault().StockMasterID;
                    //    StockMasterRec = await _context.StockMaster.FirstOrDefaultAsync(m => m.StockMasterID == CurrentID);
                    //}
                    if (string.IsNullOrEmpty(fromDate))
                    {
                        FromDate = DateTime.Today.AddYears(-10);
                    }
                    else
                    {
                        FromDate = Convert.ToDateTime(fromDate);
                    }
                    if (string.IsNullOrEmpty(inputFastPeriod))
                    {
                        SMAFastPeriod = "20";
                    }
                    else
                    {
                        SMAFastPeriod = inputFastPeriod;
                    }

                    if (string.IsNullOrEmpty(inputMidPeriod))
                    {
                        SMAMidPeriod = "50";
                    }
                    else
                    {
                        SMAMidPeriod = inputMidPeriod;
                    }

                    if (string.IsNullOrEmpty(inputSlowPeriod))
                    {
                        SMASlowPeriod = "200";
                    }
                    else
                    {
                        SMASlowPeriod = inputSlowPeriod;
                    }

                    if (string.IsNullOrEmpty(inputRsiPeriod))
                    {
                        RSIPeriod = "14";
                    }
                    else
                    {
                        RSIPeriod = inputRsiPeriod;
                    }

                    if (string.IsNullOrEmpty(inputFastK))
                    {
                        FastK = "20";
                    }
                    else
                    {
                        FastK = inputFastK;
                    }
                    if (string.IsNullOrEmpty(inputSlowD))
                    {
                        SlowD = "20";
                    }
                    else
                    {
                        SlowD = inputSlowD;
                    }
                    //if ((CurrentID != null) && (string.IsNullOrEmpty(FromDate) == false))
                    //{
                    DbInitializer.GetSMA_EMA_MACD_BBANDS_Table(_context, StockMasterRec, FromDate, small_fast_Period: int.Parse(SMAFastPeriod),
                                    mid_period: int.Parse(SMAMidPeriod), long_slow_Period: int.Parse(SMASlowPeriod), refreshHistory: false);
                    DbInitializer.getRSIDataTableFromDaily(_context, StockMasterRec, FromDate, period: RSIPeriod);
                    DbInitializer.getStochasticDataTableFromDaily(_context, StockMasterRec, FromDate, fastkperiod: FastK, slowdperiod: SlowD);
                    listSMA = ChartData(CurrentID, FromDate);
                    //}
                }
            }
        }
        private List<StockPriceHistory> ChartData(int? id, DateTime fromDate)
        {
            //IQueryable<StockPriceHistory> stockpriceIQ = from s in _context.StockPriceHistory select s;
            //List<StockPriceHistory> chartDataList = (stockpriceIQ.Where(s => (s.StockMasterID == CurrentID))).ToList();
            List<StockPriceHistory> chartDataList = (_context.StockPriceHistory.Where(s => (s.StockMasterID == CurrentID) &&
                        s.PriceDate.Date >= (fromDate.Date))).ToList();
            //chartDataList = _context.StockPriceHistory.ToList();
            return chartDataList;
        }

        private void NewChartData(int? id, string fromDate)
        {
            //IQueryable<StockPriceHistory> stockpriceIQ = from s in _context.StockPriceHistory select s;
            //List<StockPriceHistory> chartDataList = (stockpriceIQ.Where(s => (s.StockMasterID == CurrentID))).ToList();
            listSMA = (_context.StockPriceHistory.Where(s => (s.StockMasterID == CurrentID) &&
                        s.PriceDate.Date >= (Convert.ToDateTime(fromDate).Date))).ToList();
            listBuy = (_context.StockPriceHistory.Where(s => (s.StockMasterID == CurrentID) &&
                        s.PriceDate.Date >= (Convert.ToDateTime(fromDate).Date) &&
                        s.BUY_SMA_STRATEGY != null)).ToList();

            listBuy = (_context.StockPriceHistory.Where(s => (s.StockMasterID == CurrentID) &&
                        s.PriceDate.Date >= (Convert.ToDateTime(fromDate).Date) &&
                        s.SELL_SMA_STRATEGY != null)).ToList();

        }
    }
}
