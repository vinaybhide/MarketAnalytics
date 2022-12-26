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
using System.Diagnostics;

namespace MarketAnalytics.Pages.History
{
    public class IndexModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;

        public List<SelectListItem> symbolList { get; set; }
        public List<SelectListItem> groupList { get; set; }

        public string PriceDateSort { get; set; }
        public string SymbolSort { get; set; }
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
            groupList = new List<SelectListItem>();
        }

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex,
                                    int? id, bool? refreshAll)
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

                SelectListItem selectAll = new SelectListItem("-- Show All --", "-1");
                symbolList.Insert(0, selectAll);

                groupList.Clear();

                selectAll = new SelectListItem("-- Select All --", "-99");
                groupList.Add(selectAll);

                selectAll = new SelectListItem("Refresh V40", "-98");
                groupList.Add(selectAll);

                selectAll = new SelectListItem("Refresh V40N", "-97");
                groupList.Add(selectAll);

                selectAll = new SelectListItem("Refresh V200", "-96");
                groupList.Add(selectAll);

                IQueryable<StockPriceHistory> stockpriceIQ = null;

                //CurrentID = id;
                CurrentSort = sortOrder;
                SymbolSort = String.IsNullOrEmpty(sortOrder) ? "symbol_desc" : "";
                PriceDateSort = sortOrder == "PriceDate" ? "pricedate_desc" : "PriceDate";

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
                if(id == null)
                {
                    id = -1;
                }
                CurrentID = id;
                if ((id != null) && (id > 0))
                {
                    //CurrentID = id;
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

                            DbInitializer.GetSMA_EMA_MACD_BBANDS_Table(_context, StockMasterRec, StockMasterRec.Symbol, StockMasterRec.Exchange, CurrentID,
                                StockMasterRec.CompName, System.Convert.ToDateTime(lastPriceDate));

                            DbInitializer.getRSIDataTableFromDaily(_context, StockMasterRec, StockMasterRec.Symbol, StockMasterRec.Exchange, CurrentID,
                                                                    StockMasterRec.CompName, System.Convert.ToDateTime(lastPriceDate), period: "14");

                            DbInitializer.V20CandlesticPatternFinder(_context, StockMasterRec);

                            DbInitializer.GetSMA_BUYSELL(_context, StockMasterRec, StockMasterRec.Symbol, StockMasterRec.Exchange,
                                StockMasterRec.StockMasterID, StockMasterRec.CompName, 20, 50, 200);

                            DbInitializer.GetBullishEngulfingBuySellList(_context, StockMasterRec, DateTime.Today.AddDays(-180), 30, 10);
                            DbInitializer.GetBearishEngulfingBuySellList(_context, StockMasterRec, DateTime.Today.AddDays(-180), 30, 10);
                            DbInitializer.GetLifetimeHighLow(_context, StockMasterRec);

                            RefreshAllStocks = false;
                        }
                    }


                    CurrentFilter = searchString;

                    stockpriceIQ = _context.StockPriceHistory.Where(s => (s.StockMasterID == CurrentID));
                    if (!String.IsNullOrEmpty(searchString))
                    {
                        //stockpriceIQ = stockpriceIQ.Where(s => s.PriceDate.Date.Equals(Convert.ToDateTime(searchString).Date)
                        //                                        && (s.StockMasterID == CurrentID));
                        stockpriceIQ = stockpriceIQ.Where(s => (s.PriceDate.Date >= (Convert.ToDateTime(searchString).Date))
                                                                && (s.StockMasterID == CurrentID));
                    }
                }
                else if((id != null) && (id < -1))
                {
                    RefreshHistoryForGroup((int)id);
                    if (id == -99)
                    {
                        stockpriceIQ = _context.StockPriceHistory.Where(s => ((s.StockMaster.V200 == true)
                                                                  || (s.StockMaster.V40 == true) || (s.StockMaster.V40N == true)));
                        CompanyName = "V40+V40N+V200";

                    }
                    else if (id == -98)
                    {
                        stockpriceIQ = _context.StockPriceHistory.Where(s => (s.StockMaster.V40 == true));
                        CompanyName = "V40";
                    }
                    else if (id == -97)
                    {
                        stockpriceIQ = _context.StockPriceHistory.Where(s => (s.StockMaster.V40N == true));
                        CompanyName = "V40N";
                    }
                    else if (id == -96)
                    {
                        stockpriceIQ = _context.StockPriceHistory.Where(s => (s.StockMaster.V200 == true));
                        CompanyName = "V200";
                    }
                }
                else
                {
                    //show all
                    CompanyName = "All";
                    stockpriceIQ = _context.StockPriceHistory;
                }
                //IQueryable<StockPriceHistory> stockpriceIQ = from s in _context.StockPriceHistory select s;


                if (id != null)
                {
                    switch (sortOrder)
                    {
                        case "symbol_desc":
                            stockpriceIQ = stockpriceIQ.OrderByDescending(s => s.StockMaster.Symbol);
                            break;
                        case "PriceDate":
                            stockpriceIQ = stockpriceIQ.OrderBy(s => s.PriceDate);
                            break;
                        case "pricedate_desc":
                            stockpriceIQ = stockpriceIQ.OrderByDescending(s => s.PriceDate);
                            break;
                        default:
                            stockpriceIQ = stockpriceIQ.OrderBy(s => s.StockMaster.Symbol);
                            break;
                    }
                    var pageSize = Configuration.GetValue("PageSize", 10);
                    StockPriceHistory = await PaginatedList<StockPriceHistory>.CreateAsync(stockpriceIQ.AsNoTracking(), pageIndex ?? 1, pageSize, CurrentID);
                }
            }
        }

        public void RefreshHistoryForGroup(int groupId)
        {
            IQueryable<StockMaster> stockmasterIQ = null;
            string lastPriceDate = string.Empty;
            try
            {
                if (groupId == -99)
                {
                    stockmasterIQ = _context.StockMaster.Where(s => ((s.V200 == true) || (s.V40 == true) || (s.V40N == true)));
                }
                else if (groupId == -98)
                {
                    stockmasterIQ = _context.StockMaster.Where(s => (s.V40 == true));
                }
                else if (groupId == -97)
                {
                    stockmasterIQ = _context.StockMaster.Where(s => (s.V40N == true));
                }
                else if (groupId == -96)
                {
                    stockmasterIQ = _context.StockMaster.Where(s => (s.V200 == true));
                }
                foreach (var item in stockmasterIQ)
                {
                    lastPriceDate = DbInitializer.IsHistoryUpdated(_context, item, item.StockMasterID);
                    if (string.IsNullOrEmpty(lastPriceDate) == false)
                    {
                        DbInitializer.InitializeHistory(_context, item, item.Symbol, item.CompName, item.Exchange, lastPriceDate);

                        DbInitializer.GetSMA_EMA_MACD_BBANDS_Table(_context, item, item.Symbol, item.Exchange, item.StockMasterID,
                            item.CompName, System.Convert.ToDateTime(lastPriceDate));

                        DbInitializer.getRSIDataTableFromDaily(_context, item, item.Symbol, item.Exchange, CurrentID,
                                                                item.CompName, System.Convert.ToDateTime(lastPriceDate), period: "14");

                        DbInitializer.V20CandlesticPatternFinder(_context, item);

                        DbInitializer.GetSMA_BUYSELL(_context, item, item.Symbol, item.Exchange,
                            item.StockMasterID, item.CompName, 20, 50, 200);

                        DbInitializer.GetBullishEngulfingBuySellList(_context, item, DateTime.Today.AddDays(-180), 30, 10);
                        DbInitializer.GetBearishEngulfingBuySellList(_context, item, DateTime.Today.AddDays(-180), 30, 10);
                        DbInitializer.GetLifetimeHighLow(_context, item);
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
