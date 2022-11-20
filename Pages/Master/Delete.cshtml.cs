using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Data;
using MarketAnalytics.Models;

namespace MarketAnalytics.Pages.Master
{
    public class DeleteModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;

        public DeleteModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
        }

        [BindProperty]
      public StockMaster StockMaster { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.StockMaster == null)
            {
                return NotFound();
            }

            var stockmaster = await _context.StockMaster.FirstOrDefaultAsync(m => m.StockMasterID == id);

            if (stockmaster == null)
            {
                return NotFound();
            }
            else 
            {
                StockMaster = stockmaster;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null || _context.StockMaster == null)
            {
                return NotFound();
            }
            var stockmaster = await _context.StockMaster.FindAsync(id);

            if (stockmaster != null)
            {
                StockMaster = stockmaster;
                _context.StockMaster.Remove(StockMaster);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
