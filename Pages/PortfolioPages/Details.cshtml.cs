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
    public class DetailsModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;

        public DetailsModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
        }

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
    }
}
