﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MarketAnalytics.Data;
using MarketAnalytics.Models;
using System.Data;
using System.Net;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Scripting;

namespace MarketAnalytics.Pages.Master
{
    public class IndexModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public List<SelectListItem> menuList { get; set; }
        public List<SelectListItem> groupList { get; set; }


        public IndexModel(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            menuList = new List<SelectListItem>();
            groupList = new List<SelectListItem>();
        }

        public string ExchangeSort { get; set; }
        public string SymbolSort { get; set; }
        public string CompNameSort { get; set; }
        public string TypeSort { get; set; }
        public string V40Sort { get; set; }
        public string V40NSort { get; set; }
        public string V200Sort { get; set; }
        [BindProperty]
        public string CurrentFilter { get; set; }
        [BindProperty]
        public string CurrentSort { get; set; }

        public bool RefreshAllStocks { get; set; } = false;
        [BindProperty]
        public int? CurrentGroup { get; set; }
        [BindProperty]
        public int CurrentSel { get; set; }
        [BindProperty]
        public int? CurrentPageIndex { get; set; }
        [BindProperty]
        public string CurrentGroupSelection { get; set; }
        public PaginatedList<StockMaster> StockMaster { get; set; } = default!;

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex, int? id,
                    bool? refreshAll, bool? history, bool? getQuote, bool? lifetimeHighLow, int? groupsel, bool? updateStrategy,
                    string filterCategory)
        {
            if (_context.StockMaster != null)
            {
                groupList.Clear();
                SelectListItem selectAll = new SelectListItem("-- Show All --", "-95", true);
                groupList.Insert(0, selectAll);

                selectAll = new SelectListItem("-- Show: V40, V40N, V200 --", "-99");
                groupList.Add(selectAll);

                selectAll = new SelectListItem("Show V40", "-98");
                groupList.Add(selectAll);

                selectAll = new SelectListItem("Show V40N", "-97");
                groupList.Add(selectAll);

                selectAll = new SelectListItem("Show V200", "-96");
                groupList.Add(selectAll);

                menuList.Clear();
                SelectListItem menuItem = new SelectListItem("-- Select Action --", "-1");
                menuList.Add(menuItem);

                menuItem = new SelectListItem("Edit Category", "0");
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Show Details", "1");
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Get Quote", "2");
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Update (HighLow/Strategy)", "3");
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Show History", "4");
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Chart: History", "5");
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Chart: SMA-RSI-STOCH", "6");
                menuList.Add(menuItem);
                menuItem = new SelectListItem("SMA V40 Strategy", "7");
                menuList.Add(menuItem);
                menuItem = new SelectListItem("V20 Strategy", "8");
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Bullish Engulfing Strategy", "9");
                menuList.Add(menuItem);
                menuItem = new SelectListItem("Bearish Engulfing STrategy", "10");
                menuList.Add(menuItem);


                //StockMaster = await _context.StockMaster.ToListAsync();
                //Commented above line and Added following for sorting, searching, paging
                CurrentSort = sortOrder;
                SymbolSort = String.IsNullOrEmpty(sortOrder) ? "symbol_desc" : "";
                ExchangeSort = sortOrder == "Exchange" ? "exchange_desc" : "Exchange";
                CompNameSort = sortOrder == "CompName" ? "compname_desc" : "CompName";
                TypeSort = sortOrder == "TypeSort" ? "typesort_desc" : "TypeSort";

                V40Sort = sortOrder == "V40" ? "v40_desc" : "V40";
                V40NSort = sortOrder == "V40N" ? "v40n_desc" : "V40N";
                V200Sort = sortOrder == "V200" ? "v200_desc" : "V200";

                if (searchString != null)
                {
                    pageIndex = 1;
                    DbInitializer.SearchOnlineInsertInDB(_context, searchString);
                }
                else
                {
                    searchString = currentFilter;
                }

                if (refreshAll == true)
                {
                    string fetchedData = await DbInitializer.FetchMasterData();
                    DbInitializer.Initialize(_context, fetchedData);

                    //RefreshAllStockMaster();
                    RefreshAllStocks = false;
                }

                IQueryable<StockMaster> stockmasterIQ = null;

                CurrentGroup = groupsel;
                if ((string.IsNullOrEmpty(filterCategory) == false) && filterCategory.Equals("Show & Update Category"))
                {
                    updateStrategy = true;
                }
                if (CurrentGroup != null)
                {
                    if ((updateStrategy != null) && (updateStrategy == true))
                    {
                        DbInitializer.UpdateQuoteStrategy(_context, (int)CurrentGroup);
                    }
                }
                if ((CurrentGroup != null) && (CurrentGroup == -99))
                {
                    stockmasterIQ = _context.StockMaster.Where(s => ((s.V40 == true) || (s.V40N == true) || (s.V200 == true)));
                    groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else if ((CurrentGroup != null) && (CurrentGroup == -98))
                {
                    stockmasterIQ = _context.StockMaster.Where(s => (s.V40 == true));
                    groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else if ((CurrentGroup != null) && (CurrentGroup == -97))
                {
                    stockmasterIQ = _context.StockMaster.Where(s => (s.V40N == true));
                    groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else if ((CurrentGroup != null) && (CurrentGroup == -96))
                {
                    stockmasterIQ = _context.StockMaster.Where(s => (s.V200 == true));
                    groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                }
                else
                {
                    stockmasterIQ = from s in _context.StockMaster select s;
                }

                if ((id != null) && (id > 0))
                {
                    var selectedRecord = await _context.StockMaster.FirstOrDefaultAsync(m => m.StockMasterID == id);
                    if (selectedRecord != null)
                    {
                        if (getQuote == true)
                        {
                            DbInitializer.UpdateStockQuote(_context, selectedRecord);
                        }
                        if ((updateStrategy != null) && (updateStrategy == true))
                        {
                            DbInitializer.UpdateQuoteStrategy(_context, (int)id);
                        }
                    }
                }


                CurrentFilter = searchString;


                if (!String.IsNullOrEmpty(searchString))
                {
                    stockmasterIQ = stockmasterIQ.Where(s => s.Symbol.ToUpper().Contains(searchString.ToUpper())
                                                            || s.CompName.ToUpper().Contains(searchString.ToUpper()));
                    if (stockmasterIQ.Count() <= 0)
                    {
                        CurrentGroup = groupsel = -95;
                        groupList.FirstOrDefault(a => a.Value.Equals(CurrentGroup.ToString())).Selected = true;
                        stockmasterIQ = from s in _context.StockMaster select s;
                    }
                }
                switch (sortOrder)
                {
                    case "symbol_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.Symbol);
                        break;
                    case "Exchange":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.Exchange);
                        break;
                    case "exchange_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.Exchange);
                        break;
                    case "CompName":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.CompName);
                        break;
                    case "compname_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.CompName);
                        break;
                    case "TypeSort":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.INVESTMENT_TYPE);
                        break;
                    case "typesort_desc":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.INVESTMENT_TYPE);
                        break;
                    case "V40":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.V40);
                        break;
                    case "v40_desc":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.V40);
                        break;
                    case "V40N":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.V40N);
                        break;
                    case "v40n_desc":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.V40N);
                        break;
                    case "V200":
                        stockmasterIQ = stockmasterIQ.OrderByDescending(s => s.V200);
                        break;
                    case "v200_desc":
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.V200);
                        break;

                    default:
                        stockmasterIQ = stockmasterIQ.OrderBy(s => s.Symbol);
                        break;
                }
                var pageSize = Configuration.GetValue("PageSize", 10);
                CurrentPageIndex = pageIndex;
                StockMaster = await PaginatedList<StockMaster>.CreateAsync(stockmasterIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
            }
        }

        public IActionResult OnPostStockAction(int? id, string menuitemsel, string sortOrder, int pageIndex, string currentFilter, int? groupsel)
        {
            if ((id != null) && (menuitemsel.Equals("-1") == false))
            {
                StockMaster stockMaster = _context.StockMaster.FirstOrDefault(m => m.StockMasterID == id);

                switch (menuitemsel)
                {
                    case "0"://case of edit category
                        return RedirectToPage("./Edit", new { id = id, groupsel = groupsel, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter });
                    case "1"://case of show details
                        return RedirectToPage("./Details", new { id = id, groupsel = groupsel, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter });
                    case "2"://case of Get Quote
                             //DbInitializer.UpdateStockQuote(_context, stockMaster);
                        return RedirectToPage("./Index", new { id = id, groupsel = groupsel, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter, getQuote = true, updateStrategy = false });
                    case "3": //case of update high low & strategy
                              //DbInitializer.GetLifetimeHighLow(_context, stockMaster);
                        return RedirectToPage("./Index", new { id = id, groupsel = groupsel, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter, getQuote = false, updateStrategy = true });

                    case "4": //case of history
                        return RedirectToPage("/History/Index", new { id = id });

                    case "5": //case of history chart
                        return RedirectToPage("/StandardIndicators/chartHistory", new { stockid = id, onlyhistory = 0, history = true });
                    case "6": //case of chart SMA RSI STOCH
                        return RedirectToPage("/StandardIndicators/chartSMARSISTOCH", new { id = id });
                    case "7": //case of strategy SMA
                        return RedirectToPage("/BuySell/smav40", new { symbolToUpdate = id });
                    case "8": //case of strategy V20
                        return RedirectToPage("/BuySell/v20BuySell", new { symbolToUpdate = id });
                    case "9": //case of strategy Bullinsh engulfing
                        return RedirectToPage("/BuySell/BullishEngulfing", new { symbolToUpdate = id });
                    case "10": //case of strategy Bearish Engulfing
                        return RedirectToPage("/BuySell/BearishEngulfing", new { symbolToUpdate = id });
                    default:
                        return RedirectToPage("./Index", new { id = id, groupsel = groupsel, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter });
                }
            }
            return RedirectToPage("./Index", new { id = id, groupsel = groupsel, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter });
        }
    }
}
