using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MarketAnalytics.Data;
using MarketAnalytics.Models;

namespace MarketAnalytics.Pages.History
{
    public class CreateModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;

        public CreateModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["StockMasterID"] = new SelectList(_context.StockMaster, "StockMasterID", "CompName");
            return Page();
        }

        [BindProperty]
        public StockPriceHistory StockPriceHistory { get; set; }
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.StockPriceHistory.Add(StockPriceHistory);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
