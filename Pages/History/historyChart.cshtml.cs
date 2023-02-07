using MarketAnalytics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MarketAnalytics.Pages.History
{
    public class historyChartModel : PageModel
    {
        public List<StockPriceHistory> stockPriceHistories = new List<StockPriceHistory>();
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public int? CurrentID { get; set; }
        public DateTime ToDate { get; set; } = DateTime.Today.Date;
        public DateTime FromDate { get; set; } = DateTime.Today.Date.AddYears(-1).Date;
        public historyChartModel(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
        }

        public async Task OnGetAsync(int? id, string fromDate, string toDate)
        {
            stockPriceHistories.Clear();
            if(id != null)
            {
                CurrentID = id;
            }

            stockPriceHistories = ChartData(id, fromDate, toDate);
        }

        private List<StockPriceHistory> ChartData(int? id, string fromDate, string toDate)
        {
            if(string.IsNullOrEmpty(fromDate))
            {
                FromDate = DateTime.Today.Date.AddYears(-1).Date;
            }
            else
            {
                FromDate = Convert.ToDateTime(fromDate);
            }
            if(string.IsNullOrEmpty(toDate))
            {
                ToDate = DateTime.Today.Date;
            }
            else
            {
                ToDate = Convert.ToDateTime(toDate);
            }

            //IQueryable<StockPriceHistory> stockpriceIQ = from s in _context.StockPriceHistory select s;
            //List<StockPriceHistory> chartDataList = (stockpriceIQ.Where(s => (s.StockMasterID == CurrentID))).ToList();
            List<StockPriceHistory> chartDataList = _context.StockPriceHistory
                .AsSplitQuery()
                .Where(s => (s.StockMasterID == CurrentID) &&
                        (s.PriceDate.Date.CompareTo(FromDate) >= 0) && (s.PriceDate.Date.CompareTo(ToDate) <= 0)
                ).ToList();
            //chartDataList = _context.StockPriceHistory.ToList();
            return chartDataList;
        }
    }
}
