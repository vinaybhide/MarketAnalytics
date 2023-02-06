using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Data;
using MarketAnalytics.Models;

namespace MarketAnalytics.Pages.PortfolioPages
{
    public class PortfolioTxnDetailsModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;

        public PortfolioTxnDetailsModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
        }

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

        public async Task<IActionResult> OnGetAsync(int? masterid, int? txnid, int? stockid, int pageSummaryIndex, int pageIndex, 
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
            parentSearchString = searchString;

            return Page();
        }
    }
}
