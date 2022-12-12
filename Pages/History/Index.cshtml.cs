using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Data;
using MarketAnalytics.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;

namespace MarketAnalytics.Pages.History
{
    public class IndexModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;

        public List<SelectListItem> symbolList { get; set; }

        public string PriceDateSort { get; set; }
        public int? CurrentID { get; set; }
        public string CurrentSort { get; set; }
        public string CurrentFilter { get; set; }
        public bool RefreshAllStocks { get; set; } = false;

        public string CompanyName { get; set; }
        //public IList<StockPriceHistory> StockPriceHistory { get;set; } = default!;
        public PaginatedList<StockPriceHistory> StockPriceHistory { get; set; } = default!;
        public StockMaster StockMasterRec { get; set; }

        public IndexModel(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            symbolList = new List<SelectListItem>();
        }

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex, 
                                    int? id, bool?refreshAll)
        {
            if (_context.StockPriceHistory != null)
            {
                symbolList.Clear();
                symbolList = _context.StockMaster
                                        .OrderBy(a => a.Symbol)
                                            .Select(a =>
                                                new SelectListItem
                                                {
                                                    Value = a.StockMasterID.ToString(),
                                                    Text = a.Symbol
                                                }
                                            ).ToList();

                //CurrentID = id;
                CurrentSort = sortOrder;
                PriceDateSort = String.IsNullOrEmpty(sortOrder) ? "pricedate_desc" : "";
                if (searchString != null)
                {
                    pageIndex = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                StockMasterRec = null;
                if (id != null)
                {
                    CurrentID = id;
                    //var selectedRecord = await _context.StockMaster.FirstOrDefaultAsync(m => m.StockMasterID == id);
                    StockMasterRec = await _context.StockMaster.FirstOrDefaultAsync(m => m.StockMasterID == id);
                    symbolList.FirstOrDefault(a => a.Value.Equals(CurrentID.ToString())).Selected = true;
                    //}
                    //else
                    //{
                    //    CurrentID = _context.StockPriceHistory.FirstOrDefault().StockMasterID;
                    //    StockMasterRec = await _context.StockMaster.FirstOrDefaultAsync(m => m.StockMasterID == CurrentID);
                    //}

                    CompanyName = StockMasterRec.CompName;

                    //if ((refreshAll == true) && (StockMasterRec != null) && (sortOrder == null) && (currentFilter == null) && (searchString == null) && (pageIndex == null))
                    if ((refreshAll == true) && (StockMasterRec != null) && (sortOrder == null) && (currentFilter == null) && (searchString == null) && (pageIndex == null))
                    {
                        //we have found a matching record from StockMaster, from where we can get id, symbol, company
                        //DbInitializer.InitializeHistory(_context, StockMasterRec, StockMasterRec.Symbol, StockMasterRec.CompName, StockMasterRec.Exchange);
                        //RefreshAllStocks = false;

                        string lastPriceDate = DbInitializer.IsHistoryUpdated(_context, StockMasterRec, CurrentID);
                        if (string.IsNullOrEmpty(lastPriceDate) == false)
                        {
                            DbInitializer.InitializeHistory(_context, StockMasterRec, StockMasterRec.Symbol, StockMasterRec.CompName, StockMasterRec.Exchange, lastPriceDate);
                            RefreshAllStocks = false;
                        }
                    }


                    CurrentFilter = searchString;

                    //IQueryable<StockPriceHistory> stockpriceIQ = from s in _context.StockPriceHistory select s;

                    IQueryable<StockPriceHistory> stockpriceIQ = _context.StockPriceHistory.Where(s => (s.StockMasterID == CurrentID));

                    if (!String.IsNullOrEmpty(searchString))
                    {
                        //stockpriceIQ = stockpriceIQ.Where(s => s.PriceDate.Date.Equals(Convert.ToDateTime(searchString).Date)
                        //                                        && (s.StockMasterID == CurrentID));
                        stockpriceIQ = stockpriceIQ.Where(s => (s.PriceDate.Date >= (Convert.ToDateTime(searchString).Date))
                                                                && (s.StockMasterID == CurrentID));
                    }

                    switch (sortOrder)
                    {
                        case "pricedate_desc":
                            stockpriceIQ = stockpriceIQ.OrderByDescending(s => s.PriceDate);
                            break;
                        default:
                            stockpriceIQ = stockpriceIQ.OrderBy(s => s.PriceDate);
                            break;
                    }
                    var pageSize = Configuration.GetValue("PageSize", 10);
                    StockPriceHistory = await PaginatedList<StockPriceHistory>.CreateAsync(stockpriceIQ.AsNoTracking(), pageIndex ?? 1, pageSize, CurrentID);
                }
            }
        }
    }
}
