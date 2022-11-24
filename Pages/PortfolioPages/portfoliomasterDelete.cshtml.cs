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

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.PORTFOLIO_MASTER == null)
            {
                return NotFound();
            }

            var selectedrecord = await _context.PORTFOLIO_MASTER.FirstOrDefaultAsync(m => m.PORTFOLIO_MASTER_ID == id);

            if (selectedrecord == null)
            {
                return NotFound();
            }
            else 
            {
                portfolioMaster = selectedrecord;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.PORTFOLIO_MASTER == null)
            {
                return NotFound();
            }
            var selectedrecord = await _context.PORTFOLIO_MASTER.FindAsync(id);

            if (selectedrecord != null)
            {
                portfolioMaster = selectedrecord;
                _context.PORTFOLIO_MASTER.Remove(portfolioMaster);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./portfoliomasterIndex");
        }
    }
}
