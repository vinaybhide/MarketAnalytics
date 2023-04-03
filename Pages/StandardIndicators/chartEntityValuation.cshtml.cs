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
    public class chartEntityValuation : PageModel
    {
        public List<StockPriceHistory> listHistory;
        public List<StockPriceHistory> listIndexHistory;
        public List<SelectListItem> indexList;
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public int? CurrentID { get; set; }
        public double? QuantityHeld { get; set; }
        public DateTime FromDate { get; set; }
        public string Symbol { get; set; }
        public string CompanyName { get; set; }
        public string InvestmentType { get; set; }
        public string IndexSymbol { get; set; }
        public string ChartContent { get; set; }
        public chartEntityValuation(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            listHistory = new List<StockPriceHistory>();
            listIndexHistory = new List<StockPriceHistory>();
            indexList = new List<SelectListItem>();
        }

        //caller should send fromdate in buydate
        //selldate if not specified will be used as today or null
        public async Task OnGetAsync(int? stockid, string fromDate, int? selectedindex, string quantity)
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
                IndexSymbol = indexList.FirstOrDefault(a => a.Value.Equals(selectedindex.ToString())).Text;

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

                if(string.IsNullOrEmpty(quantity) == false)
                {
                    try
                    {
                        QuantityHeld = double.Parse(quantity);
                    }
                    catch 
                    {
                        QuantityHeld = 0;

                    }
                }
            }
        }
    }
}
