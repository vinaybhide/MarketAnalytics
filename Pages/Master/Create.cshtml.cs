using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MarketAnalytics.Data;
using MarketAnalytics.Models;

namespace MarketAnalytics.Pages.Master
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
            return Page();
        }

        [BindProperty]
        public StockMaster StockMaster { get; set; }
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.StockMaster.Add(StockMaster);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
