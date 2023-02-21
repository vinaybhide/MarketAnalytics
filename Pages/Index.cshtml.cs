using MarketAnalytics.Data;
using MarketAnalytics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MarketAnalytics.Pages
{
    public class IndexModel : PageModel
    {
        public List<StockPriceHistory> niftyHistory = new List<StockPriceHistory>();
        public List<StockPriceHistory> sensexHistory = new List<StockPriceHistory>();
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public DateTime ToDate { get; set; } = DateTime.Today.Date;
        public DateTime FromDate { get; set; } = DateTime.Today.Date.AddYears(-1).Date;

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger, MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            Configuration = configuration;
        }

        public void OnGet()
        {
            niftyHistory.Clear();
            sensexHistory.Clear();

            niftyHistory = ChartData("^NSEI", "NSI");
            sensexHistory = ChartData("^BSESN", "BSE");

        }

        /// <summary>
        /// ^BSESN
        /// ^NSEI
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="exchange"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        private List<StockPriceHistory> ChartData(string symbol, string exchange, string fromDate = null, string toDate = null)
        {
            List<StockPriceHistory> chartDataList = null;
            if (string.IsNullOrEmpty(fromDate))
            {
                FromDate = DateTime.Today.Date.AddYears(-1).Date;
            }
            else
            {
                FromDate = Convert.ToDateTime(fromDate);
            }
            if (string.IsNullOrEmpty(toDate))
            {
                ToDate = DateTime.Today.Date;
            }
            else
            {
                ToDate = Convert.ToDateTime(toDate);
            }
            //StockMaster stockMaster = _context.StockMaster.First(x => (x.Symbol.Equals(symbol) && x.Exchange.Equals(exchange)));
            //var selectedRecord = _context.StockMaster.AsSplitQuery().FirstOrDefault(x => (x.Symbol.Equals(symbol) && x.Exchange.Equals(exchange)));
            var selectedRecord = _context.StockMaster.AsSplitQuery().Where(x => (x.Symbol.Equals(symbol) && x.Exchange.Equals(exchange))).FirstOrDefault();

            string lastPriceDate = DateTime.Today.ToString("yyyy-MM-dd");
            if(selectedRecord == null)
            {
                DbInitializer.SearchOnlineInsertInDB(_context, symbol);
                selectedRecord = _context.StockMaster.AsSplitQuery().FirstOrDefault(x => (x.Symbol.Equals(symbol) && x.Exchange.Equals(exchange)));
            }
            if (selectedRecord != null)
            {
                //lastPriceDate = DbInitializer.IsHistoryUpdated(_context, selectedRecord);
                //if (string.IsNullOrEmpty(lastPriceDate) == false)
                //{
                    DbInitializer.InitializeHistory(_context, selectedRecord, lastPriceDate);
                //}
                //IQueryable<StockPriceHistory> stockpriceIQ = from s in _context.StockPriceHistory select s;
                //List<StockPriceHistory> chartDataList = (stockpriceIQ.Where(s => (s.StockMasterID == CurrentID))).ToList();
                chartDataList = selectedRecord.collectionStockPriceHistory
                                                            .Where(s => (s.PriceDate.Date.CompareTo(FromDate) >= 0) && (s.PriceDate.Date.CompareTo(ToDate) <= 0))
                                                            .OrderBy(s => s.PriceDate)
                                                            .ToList();
            }            
            return chartDataList;
        }

    }
}