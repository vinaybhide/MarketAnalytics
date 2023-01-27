using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Data;
using MarketAnalytics.Models;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MarketAnalytics.Pages.PortfolioPages
{
    public class portfolioTxnSummaryIndexModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public List<SelectListItem> symbolList { get; set; }
        public List<SelectListItem> menuList { get; set; }

        public string DateSort { get; set; }
        public string ExchangeSort { get; set; }
        public string SymbolSort { get; set; }
        public string CompNameSort { get; set; }
        [BindProperty]
        public string CurrentFilter { get; set; }
        [BindProperty]
        public string CurrentSort { get; set; }
        [BindProperty]
        public int? CurrentPageIndex { get; set; }
        [BindProperty]
        public int? CurrentClosedPageIndex { get; set; }

        public bool RefreshAllStocks { get; set; } = false;
        public PaginatedList<PORTFOLIOTXN> portfolioOpenTxn { get; set; } = default!;
        public PaginatedList<PORTFOLIOTXN> portfolioClosedTxn { get; set; } = default!;

        public string portfolioMasterName { get; set; } = string.Empty;
        public double portfolioTotalCost { get; set; } = default(double);
        public double portfolioTotalGain { get; set; } = default(double);
        public double portfolioTotalValue { get; set; } = default(double);

        [BindProperty]
        public int MasterId { get; set; }
        [BindProperty]
        public int TransId { get; set; }

        public portfolioTxnSummaryIndexModel(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            symbolList = new List<SelectListItem>();
            menuList = new List<SelectListItem>();
        }

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex, int? pageIndexClosed, int? masterid,
            bool? refreshAll, bool? getQuote, int? stockid, bool? updateBuySell, bool? lifetimeHighLow)
        {
            if ((_context.PORTFOLIOTXN != null) && (masterid != null))
            {
                MasterId = (int)masterid;
            }
        }
    }
}
