using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Models;
using MarketAnalytics.Data;

namespace MarketAnalytics.Pages.PortfolioPages
{
    public class EditModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;

        public EditModel(MarketAnalytics.Data.DBContext context)
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

            var selectedrecord =  await _context.PORTFOLIO.FirstOrDefaultAsync(m => m.PORTFOLIO_ID == id);
            if (selectedrecord == null)
            {
                return NotFound();
            }
            portfolio = selectedrecord;
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

            portfolio.TOTAL_COST = portfolio.QUANTITY * portfolio.COST_PER_SHARE;

            DateTime[] quoteDate = null;
            double[] open, high, low, close, volume, change, changepercent, prevclose = null;

            portfolio.StockMaster = (StockMaster)_context.StockMaster.Find(portfolio.StockMasterID);

            DbInitializer.GetQuote(portfolio.StockMaster.Symbol + ".NS", out quoteDate, out open, out high, out low, out close,
                        out volume, out change, out changepercent, out prevclose);
            if (quoteDate != null)
            {
                portfolio.CMP = close[0];
                portfolio.VALUE = portfolio.QUANTITY * close[0];
            }

            _context.Attach(portfolio).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PORTFOLIOExists(portfolio.PORTFOLIO_ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool PORTFOLIOExists(int id)
        {
          return _context.PORTFOLIO.Any(e => e.PORTFOLIO_ID == id);
        }
    }
}
