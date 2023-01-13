using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Models;

namespace MarketAnalytics.Pages.PortfolioPages
{
    public class DeleteModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;

        public DeleteModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
        }

        [BindProperty]
      public PORTFOLIO portfolio { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.PORTFOLIO == null)
            {
                return NotFound();
            }

            var selectedrecord = await _context.PORTFOLIO.FirstOrDefaultAsync(m => m.PORTFOLIO_ID == id);

            if (selectedrecord == null)
            {
                return NotFound();
            }
            else 
            {
                portfolio = selectedrecord;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.PORTFOLIO == null)
            {
                return NotFound();
            }
            var selectedrecord = await _context.PORTFOLIO.FindAsync(id);

            if (selectedrecord != null)
            {
                portfolio = selectedrecord;
                _context.PORTFOLIO.Remove(portfolio);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
