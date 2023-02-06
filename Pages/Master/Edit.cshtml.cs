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
using Microsoft.Data.SqlClient;

namespace MarketAnalytics.Pages.Master
{
    public class EditModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;

        public EditModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
        }
        [BindProperty]
        public StockMaster StockMaster { get; set; } = default!;

        [BindProperty]
        public List<string> StockClassification { get; set; }

        [BindProperty]
        public int parentPageIndex { get; set; }
        [BindProperty]
        public string CurrentSort { get; set; }
        
        [BindProperty]
        public string CurrentFilter { get; set; }

        [BindProperty]
        public int? CurrentGroup { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id, int? groupsel, int? pageIndex, string sortOrder, string currentFilter)
        {
            if (id == null || _context.StockMaster == null)
            {
                return NotFound();
            }
            var stockmaster = await _context.StockMaster.AsSplitQuery().FirstOrDefaultAsync(m => m.StockMasterID == id);
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

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(StockMaster).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockMasterExists(StockMaster.StockMasterID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index", new {pageIndex = parentPageIndex, groupsel = CurrentGroup, sortOrder = CurrentSort, currentFilter = CurrentFilter});
        }

        private bool StockMasterExists(int id)
        {
            return _context.StockMaster.Any(e => e.StockMasterID == id);
        }
    }
}
