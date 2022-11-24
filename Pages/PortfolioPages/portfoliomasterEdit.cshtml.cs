using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Models;
using MarketAnalytics.Data;

namespace MarketAnalytics.Pages.PortfolioPages
{
    public class PortfolioMasterEditModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;

        public PortfolioMasterEditModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
        }
        [BindProperty]
        public Portfolio_Master portfolioMaster { get; set; }
        
        public bool bNameUnique = true;
        public string errorMessage = "Portfolio name exists! Please use unique portfolio name.";
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.PORTFOLIO_MASTER == null)
            {
                return NotFound();
            }

            var selectedrecord =  await _context.PORTFOLIO_MASTER.FirstOrDefaultAsync(m => m.PORTFOLIO_MASTER_ID == id);
            if (selectedrecord == null)
            {
                return NotFound();
            }
            portfolioMaster = selectedrecord;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (_context.PORTFOLIO_MASTER.FirstOrDefault(x => (x.PORTFOLIO_NAME.ToUpper() == portfolioMaster.PORTFOLIO_NAME.ToUpper())) == null)
            {
                _context.Attach(portfolioMaster).State = EntityState.Modified;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PORTFOLIOExists(portfolioMaster.PORTFOLIO_MASTER_ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToPage("./portfoliomasterIndex");
            }
            else
            {
                bNameUnique = false;
                return Page();
            }

        }

        private bool PORTFOLIOExists(int id)
        {
          return _context.PORTFOLIO_MASTER.Any(e => e.PORTFOLIO_MASTER_ID == id);
        }
    }
}
