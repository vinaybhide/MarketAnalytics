using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using MarketAnalytics.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MarketAnalytics.Pages.PortfolioPages
{
    [Authorize]
    public class PortfolioMasterIndex : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public List<SelectListItem> masterList { get; set; }
        public List<SelectListItem> menuList { get; set; }

        public List<double> portfolioCost { get; set; }
        public List<double> portfolioValue { get; set; }
        public List<double> portfolioGain { get; set; }
        public List<double> portfolioGainPct { get; set; }

        public PortfolioMasterIndex(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            masterList = new List<SelectListItem>();
            portfolioCost = new List<double>();
            portfolioValue = new List<double>();
            portfolioGain = new List<double>();
            portfolioGainPct = new List<double>();
            menuList = new List<SelectListItem>();
        }

        public string nameSort { get; set; }
        [BindProperty]
        public string CurrentFilter { get; set; }
        [BindProperty]
        public string CurrentSort { get; set; }

        [BindProperty]
        public int CurrentID { get; set; }
        [BindProperty]
        public int? CurrentPageIndex { get; set; }

        public PaginatedList<Portfolio_Master> portfolioMaster { get; set; } = default!;

        public async Task OnGetAsync(string sortOrder, bool? firsttimemaster, string currentFilter, string searchString, 
                                        int? pageIndex, int? masterid, bool? updateBuySell)
        {
            masterList.Clear();
            masterList = _context.PORTFOLIO_MASTER.Select(a =>
                                                    new SelectListItem
                                                    {
                                                        Value = a.PORTFOLIO_MASTER_ID.ToString(),
                                                        Text = a.PORTFOLIO_NAME.ToString()
                                                    }).ToList();
            portfolioCost.Clear();
            portfolioValue.Clear();
            portfolioGain.Clear();
            portfolioGainPct.Clear();

            menuList.Clear();
            SelectListItem menuItem = new SelectListItem("-- Select Action --", "-1");
            menuList.Add(menuItem);

            menuItem = new SelectListItem("Edit Portfolio", "0");
            menuList.Add(menuItem);
            menuItem = new SelectListItem("Delete Portfolio", "1");
            menuList.Add(menuItem);
            menuItem = new SelectListItem("Show Transactions", "2");
            menuList.Add(menuItem);

            if (_context.PORTFOLIO_MASTER != null)
            {
                CurrentSort = sortOrder;
                nameSort = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                if (searchString != null)
                {
                    pageIndex = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                CurrentFilter = searchString;

                //IQueryable<Portfolio_Master> portfolioIQ = from s in _context.PORTFOLIO_MASTER select s;
                IQueryable<Portfolio_Master> portfolioIQ = _context.PORTFOLIO_MASTER.AsNoTracking();
                
                Portfolio_Master searchRecord = null;

                if (masterid != null)
                {
                    //var currentrecord = portfolioIQ.FirstOrDefault(a => a.PORTFOLIO_MASTER_ID == masterid);
                    portfolioIQ = portfolioIQ.Where(a => a.PORTFOLIO_MASTER_ID == masterid); ;
                    searchRecord = portfolioIQ.First();//portfolioIQ.FirstOrDefault(a => a.PORTFOLIO_MASTER_ID == masterid);
                    //if (currentrecord != null)
                    if(searchRecord != null)
                    {
                        searchString = searchRecord.PORTFOLIO_NAME;
                    }
                    //CurrentID = masterid;
                }
                if (!String.IsNullOrEmpty(searchString))
                {
                    if(searchRecord == null)
                    {
                        portfolioIQ = portfolioIQ.Where(s => s.PORTFOLIO_NAME.ToUpper().Contains(searchString.ToUpper()));

                        searchRecord = portfolioIQ.First();
                    }
                    if (searchRecord != null)
                    {
                        if (masterList.Exists(a => (a.Value.Equals(searchRecord.PORTFOLIO_MASTER_ID.ToString()) == true)))
                        {
                            masterList.FirstOrDefault(a => a.Value.Equals(searchRecord.PORTFOLIO_MASTER_ID.ToString())).Selected = true;
                        }
                    }
                    CurrentFilter = searchString;
                }

                switch (sortOrder)
                {
                    case "name_desc":
                        portfolioIQ = portfolioIQ.OrderByDescending(s => s.PORTFOLIO_NAME);
                        break;
                    default:
                        portfolioIQ = portfolioIQ.OrderBy(s => s.PORTFOLIO_NAME);
                        break;
                }
                var pageSize = Configuration.GetValue("PageSize", 10);
                CurrentPageIndex = pageIndex;
                portfolioMaster = await PaginatedList<Portfolio_Master>.CreateAsync(portfolioIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
                //if (sortOrder == null && currentFilter == null && searchString == null && pageIndex == null &&
                //    masterid == null)
                if ((firsttimemaster == null) || (firsttimemaster == true))
                {
                    DbInitializer.GetQuoteAndUpdateAllPortfolioTxn(_context, null);
                }
                foreach (var item in portfolioMaster)
                {
                    portfolioCost.Add(_context.PORTFOLIOTXN.Where(x => (x.PORTFOLIO_MASTER_ID == item.PORTFOLIO_MASTER_ID) && (x.TXN_TYPE.Equals("B")))
                                            .Sum(a => a.TOTAL_COST));
                    portfolioValue.Add((double)_context.PORTFOLIOTXN.Where(x => (x.PORTFOLIO_MASTER_ID == item.PORTFOLIO_MASTER_ID) && (x.TXN_TYPE.Equals("B")))
                                                            .Sum(a => a.VALUE));
                    portfolioGain.Add((double)_context.PORTFOLIOTXN.Where(x => (x.PORTFOLIO_MASTER_ID == item.PORTFOLIO_MASTER_ID) && (x.TXN_TYPE.Equals("B")))
                                                            .Sum(a => a.GAIN_AMT));
                    if (portfolioValue.Last() == 0)
                    {
                        portfolioGainPct.Add(0);
                    }
                    else
                    {
                        portfolioGainPct.Add((portfolioValue.Last() - portfolioCost.Last()) / portfolioCost.Last() * 100);
                    }
                }
            }
        }
        public IActionResult OnPostPortfolioAction(string menuitemsel, string sortOrder, bool? firsttimemaster, string currentFilter, string searchString,
                                        int? pageIndex, int? masterid, bool? updateBuySell)
        {
            if ((masterid != null) && (menuitemsel.Equals("-1") == false))
            {
                Portfolio_Master currentrecord = _context.PORTFOLIO_MASTER.FirstOrDefault(a => a.PORTFOLIO_MASTER_ID == masterid);
                switch (menuitemsel)
                {
                    case "0": //case of edit portfolio
                        return RedirectToPage("./portfoliomasterEdit", new { masterid = masterid, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter, firsttimemaster = firsttimemaster, searchString = searchString });
                    case "1": //case of delete portfolio
                        return RedirectToPage("./portfoliomasterDelete", new { masterid = masterid, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter, firsttimemaster = firsttimemaster, searchString = searchString });
                    case "2": //case of delete portfolio
                        return RedirectToPage("./portfolioTxnIndex", new { masterid = masterid, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter, firsttimemaster = firsttimemaster, searchString = searchString });
                    default:
                        return RedirectToPage("./portfoliomasterindex", new { masterid = masterid, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter, firsttimemaster = firsttimemaster, searchString = searchString });
                }
            }
            return RedirectToPage("./portfoliomasterindex", new { masterid = masterid, sortOrder = sortOrder, pageIndex = pageIndex, currentFilter = currentFilter, firsttimemaster = firsttimemaster, searchString = searchString });
        }
    }
}