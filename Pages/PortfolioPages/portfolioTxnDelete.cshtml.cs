using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Models;

namespace MarketAnalytics.Pages.PortfolioPages
{
    public class PortfolioTxnDeleteModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;

        public PortfolioTxnDeleteModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PORTFOLIOTXN portfolioTxn { get; set; }

        [BindProperty]
        public int parentPageSummaryIndex { get; set; }
        [BindProperty]
        public int parentPageIndex { get; set; }
        [BindProperty]
        public int parentClosedPageIndex { get; set; }
        [BindProperty]
        public string parentSortOrder { get; set; }
        [BindProperty]
        public string parentFilter { get; set; }
        [BindProperty]
        public string parentSearchString { get; set; }
        public async Task<IActionResult> OnGetAsync(int? txnid, int pageSummaryIndex, int pageIndex,
            int pageClosedIndex, string sortOrder, string currentFilter, string searchString)
        {
            if (txnid == null || _context.PORTFOLIOTXN == null)
            {
                return NotFound();
            }

            var selectedrecord = await _context.PORTFOLIOTXN.Include(a => a.stockMaster).AsSplitQuery().FirstOrDefaultAsync(m => m.PORTFOLIOTXN_ID == txnid);

            if (selectedrecord == null)
            {
                return NotFound();
            }
            portfolioTxn = selectedrecord;
            parentPageSummaryIndex = pageSummaryIndex;
            parentPageIndex = pageIndex;
            parentClosedPageIndex = pageClosedIndex;
            parentSortOrder = sortOrder;
            parentFilter = currentFilter;
            parentSearchString= searchString;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? txnid)
        {
            if (txnid == null || _context.PORTFOLIOTXN == null)
            {
                return NotFound();
            }
            var selectedrecord = await _context.PORTFOLIOTXN.FindAsync(txnid);

            if (selectedrecord != null)
            {
                portfolioTxn = selectedrecord;
                _context.PORTFOLIOTXN.Remove(portfolioTxn);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./portfolioTxnIndex", new {masterid = portfolioTxn.PORTFOLIO_MASTER_ID, 
                stockid = portfolioTxn.StockMasterID, pageSummaryIndex = parentPageSummaryIndex, pageIndex = parentPageIndex, 
                pageClosedIndex = parentClosedPageIndex, sortOrder = parentSortOrder, currentFilter = parentFilter, 
                searchString = parentSearchString});
        }
    }
}
