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
        public int parentpageIndex { get; set; }
        public async Task<IActionResult> OnGetAsync(int? masterid, int? txnid, int? stockid, int? pageIndex)
        {
            if (txnid == null || _context.PORTFOLIOTXN == null)
            {
                return NotFound();
            }

            var selectedrecord = await _context.PORTFOLIOTXN.FirstOrDefaultAsync(m => m.PORTFOLIOTXN_ID == txnid);
            if (selectedrecord == null)
            {
                return NotFound();
            }

            portfolioTxn = selectedrecord;
            parentpageIndex = (int)pageIndex;

            return Page();
        }
    }
}
