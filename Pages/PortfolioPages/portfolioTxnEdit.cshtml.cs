﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MarketAnalytics.Models;
using MarketAnalytics.Data;
using Microsoft.CodeAnalysis.Text;

namespace MarketAnalytics.Pages.PortfolioPages
{
    public class PortfolioTxnEditModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;

        public PortfolioTxnEditModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
        }
        [BindProperty]
        public PORTFOLIOTXN portfolioTxn { get; set; }
        [BindProperty]
        public int parentPageSummaryIndex { get; set; }
        [BindProperty]
        public int parentPageIndex { get; set; }
        [BindProperty]
        public int parentClosedPageIndex { get; set; }

        [BindProperty]
        public string parentSortOrder { get; set; }
        [BindProperty]
        public string parentFilter { get; set; }
        [BindProperty]
        public string parentSearchString { get; set; }

        public DateTime txnDate { get; set; }
        public async Task<IActionResult> OnGetAsync(int? masterid, int? txnid, int pageSummaryIndex, int pageIndex, 
            int pageClosedIndex, string sortOrder, string currentFilter, string searchString)
        {
            if (txnid == null || _context.PORTFOLIOTXN == null)
            {
                return NotFound();
            }

            var selectedrecord =  await _context.PORTFOLIOTXN.Include(a => a.stockMaster).AsSplitQuery().FirstOrDefaultAsync(m => m.PORTFOLIOTXN_ID == txnid);
            if (selectedrecord == null)
            {
                return NotFound();
            }
            portfolioTxn = selectedrecord;
            txnDate = selectedrecord.TXN_BUY_DATE;
            parentPageSummaryIndex = pageSummaryIndex;
            parentPageIndex = pageIndex;
            parentClosedPageIndex = pageClosedIndex;
            parentSortOrder = sortOrder;
            parentFilter = currentFilter;
            parentSearchString = searchString;

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

            portfolioTxn.TOTAL_COST = portfolioTxn.PURCHASE_QUANTITY * portfolioTxn.COST_PER_UNIT;

            DateTime[] quoteDate = null;
            double[] open, high, low, close, volume, change, changepercent, prevclose = null;

            portfolioTxn.stockMaster = (StockMaster)_context.StockMaster.Find(portfolioTxn.StockMasterID);
            DbInitializer.GetQuote(portfolioTxn.stockMaster.Symbol + (portfolioTxn.stockMaster.Exchange.Length == 0 ? "" : ("." + portfolioTxn.stockMaster.Exchange)), out quoteDate, out open, out high, out low, out close,
                        out volume, out change, out changepercent, out prevclose);
            if (quoteDate != null)
            {
                portfolioTxn.CMP = close[0];
                portfolioTxn.VALUE = portfolioTxn.PURCHASE_QUANTITY * close[0];
                portfolioTxn.GAIN_AMT = portfolioTxn.VALUE - portfolioTxn.TOTAL_COST;
                if (portfolioTxn.TOTAL_COST > 0)
                {
                    portfolioTxn.GAIN_PCT = (portfolioTxn.GAIN_AMT / portfolioTxn.VALUE) * 100;
                }
                else
                {
                    portfolioTxn.GAIN_PCT = 100;
                }
            }

            _context.Attach(portfolioTxn).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PORTFOLIOTxnExists(portfolioTxn.PORTFOLIOTXN_ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            //return RedirectToPage("./portfolioTxnIndex");
            return RedirectToPage("./portfolioTxnIndex", new { masterid = portfolioTxn.PORTFOLIO_MASTER_ID, stockid = portfolioTxn.StockMasterID, pageSummaryIndex = parentPageSummaryIndex, pageIndex = parentPageIndex, pageClosedIndex = parentClosedPageIndex, sortOrder = parentSortOrder, currentFilter = parentFilter, searchString = parentSearchString });

        }

        private bool PORTFOLIOTxnExists(int txnid)
        {
          return _context.PORTFOLIOTXN.Any(e => e.PORTFOLIOTXN_ID == txnid);
        }
    }
}
