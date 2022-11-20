using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Data;
using MarketAnalytics.Models;

namespace MarketAnalytics.Pages.History
{
    public class EditModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;

        public EditModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public StockPriceHistory StockPriceHistory { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.StockPriceHistory == null)
            {
                return NotFound();
            }

            var stockpricehistory =  await _context.StockPriceHistory.FirstOrDefaultAsync(m => m.StockPriceHistoryID == id);
            if (stockpricehistory == null)
            {
                return NotFound();
            }
            StockPriceHistory = stockpricehistory;
           ViewData["StockMasterID"] = new SelectList(_context.StockMaster, "StockMasterID", "CompName");
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

            _context.Attach(StockPriceHistory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockPriceHistoryExists(StockPriceHistory.StockPriceHistoryID))
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

        private bool StockPriceHistoryExists(int id)
        {
          return _context.StockPriceHistory.Any(e => e.StockPriceHistoryID == id);
        }
    }
}
