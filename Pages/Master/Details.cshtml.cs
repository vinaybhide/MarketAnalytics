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
    public class DetailsModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;

        [BindProperty]
        public int parentPageIndex { get; set; }
        [BindProperty]
        public string CurrentSort { get; set; }

        [BindProperty]
        public string CurrentFilter { get; set; }
        [BindProperty]
        public int? CurrentGroup { get; set; }
        public DetailsModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
        }

        public StockMaster StockMaster { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id, int? groupsel, int? pageIndex, string sortOrder, string currentFilter)
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
            CurrentSort = sortOrder;
            CurrentFilter = currentFilter;
            CurrentGroup = groupsel;
            parentPageIndex = (int)pageIndex;
            StockMaster = stockmaster;
            return Page();
        }
    }
}
