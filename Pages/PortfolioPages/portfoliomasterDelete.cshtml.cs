using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Models;

namespace MarketAnalytics.Pages.PortfolioPages
{
    public class PortfolioMasterDeleteModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;

        public PortfolioMasterDeleteModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
        }

        [BindProperty]
      public Portfolio_Master portfolioMaster { get; set; }
        [BindProperty]
        public bool FirstTimeMaster { get; set; }
        public async Task<IActionResult> OnGetAsync(int? masterid, bool? firsttimemaster)
        {
            if (masterid == null || _context.PORTFOLIO_MASTER == null)
            {
                return NotFound();
            }

            var selectedrecord = await _context.PORTFOLIO_MASTER.FirstOrDefaultAsync(m => m.PORTFOLIO_MASTER_ID == masterid);

            if (selectedrecord == null)
            {
                return NotFound();
            }
            else 
            {
                portfolioMaster = selectedrecord;
            }
            FirstTimeMaster = (bool)firsttimemaster;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? masterid)
        {
            if (masterid == null || _context.PORTFOLIO_MASTER == null)
            {
                return NotFound();
            }
            var selectedrecord = await _context.PORTFOLIO_MASTER.FindAsync(masterid);

            if (selectedrecord != null)
            {
                portfolioMaster = selectedrecord;
                _context.PORTFOLIO_MASTER.Remove(portfolioMaster);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./portfoliomasterIndex", new { firsttimemaster = FirstTimeMaster });
        }
    }
}
