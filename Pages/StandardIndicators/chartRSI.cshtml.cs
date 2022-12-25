using MarketAnalytics.Data;
using MarketAnalytics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MarketAnalytics.Pages.StandardIndicators
{
    public class chartRSIModel : PageModel
    {
        public List<StockPriceHistory> listRSI = new List<StockPriceHistory>();
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public int? CurrentID { get; set; }
        public DateTime ToDate { get; set; } = DateTime.Today.Date;
        public DateTime FromDate { get; set; } = DateTime.Today.Date.AddYears(-1).Date;

        public StockMaster StockMasterRec { get; set; }

        public chartRSIModel(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
        }

        public async Task OnGetAsync(int? id)
        {
            listRSI.Clear();
            if (_context.StockPriceHistory != null)
            {
                StockMasterRec = null;
                if (id != null)
                {
                    CurrentID = id;
                    //var selectedRecord = await _context.StockMaster.FirstOrDefaultAsync(m => m.StockMasterID == id);
                    StockMasterRec = await _context.StockMaster.FirstOrDefaultAsync(m => m.StockMasterID == id);
                }
                else
                {
                    CurrentID = _context.StockPriceHistory.FirstOrDefault().StockMasterID;
                    StockMasterRec = await _context.StockMaster.FirstOrDefaultAsync(m => m.StockMasterID == CurrentID);
                }

                DbInitializer.getRSIDataTableFromDaily(_context, StockMasterRec, StockMasterRec.Symbol, StockMasterRec.Exchange, CurrentID, 
                                                        StockMasterRec.CompName, DateTime.Today.Date);
                listRSI = ChartData(CurrentID);
            }
        }
        private List<StockPriceHistory> ChartData(int? id)
        {
            //IQueryable<StockPriceHistory> stockpriceIQ = from s in _context.StockPriceHistory select s;
            //List<StockPriceHistory> chartDataList = (stockpriceIQ.Where(s => (s.StockMasterID == CurrentID))).ToList();
            List<StockPriceHistory> chartDataList = (_context.StockPriceHistory.Where(s => (s.StockMasterID == CurrentID))).ToList();
            //chartDataList = _context.StockPriceHistory.ToList();
            return chartDataList;
        }

    }
}
