using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MarketAnalytics.Pages.PortfolioPages
{
    public class PortfolioMasterIndex : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public List<SelectListItem> masterList { get; set; }

        public PortfolioMasterIndex(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            masterList = new List<SelectListItem>();
        }

        public string nameSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }

        public int CurrentID { get; set; }
        public PaginatedList<Portfolio_Master> portfolioMaster { get; set; } = default!;

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex, int? masterid)
        {
            masterList.Clear();
            masterList = _context.PORTFOLIO_MASTER.Select(a =>
                                                    new SelectListItem
                                                    {
                                                        Value = a.PORTFOLIO_MASTER_ID.ToString(),
                                                        Text = a.PORTFOLIO_NAME.ToString()
                                                    }).ToList();

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

                IQueryable<Portfolio_Master> portfolioIQ = from s in _context.PORTFOLIO_MASTER select s;
                if (masterid != null)
                {
                    var currentrecord = portfolioIQ.FirstOrDefault(a => a.PORTFOLIO_MASTER_ID == masterid);
                    if(currentrecord != null)
                    {
                        searchString = currentrecord.PORTFOLIO_NAME;
                    }
                }
                if (!String.IsNullOrEmpty(searchString))
                {
                    portfolioIQ = portfolioIQ.Where(s => s.PORTFOLIO_NAME.ToUpper().Contains(searchString.ToUpper()));
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
                portfolioMaster = await PaginatedList<Portfolio_Master>.CreateAsync(portfolioIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
            }
        }
    }
}