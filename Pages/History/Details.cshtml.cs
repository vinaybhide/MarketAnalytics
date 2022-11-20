using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Data;
using MarketAnalytics.Models;

namespace MarketAnalytics.Pages.History
{
    public class DetailsModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;

        public DetailsModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
        }

      public StockPriceHistory StockPriceHistory { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null || _context.StockPriceHistory == null)
            {
                return NotFound();
            }

            var stockpricehistory = await _context.StockPriceHistory.FirstOrDefaultAsync(m => m.StockPriceHistoryID == id);
            if (stockpricehistory == null)
            {
                return NotFound();
            }
            else 
            {
                StockPriceHistory = stockpricehistory;
            }
            return Page();
        }
    }
}
