using MarketAnalytics.Data;
using MarketAnalytics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MarketAnalytics.Pages.PortfolioPages
{
    public class portfolioTxnClosedCreateModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;

        [BindProperty]
        public PORTFOLIOTXN portfolioTxn { get; set; }
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

        public portfolioTxnClosedCreateModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int? masterid, int? txnid, int? stockid, int pageIndex, int pageClosedIndex,
                    string sortOrder, string currentFilter, string searchString)
        {
            if (txnid == null || _context.PORTFOLIOTXN == null)
            {
                return NotFound();
            }

            var selectedrecord = await _context.PORTFOLIOTXN.AsSplitQuery().FirstOrDefaultAsync(m => m.PORTFOLIOTXN_ID == txnid);
            if (selectedrecord == null)
            {
                return NotFound();
            }
            portfolioTxn = selectedrecord;
            portfolioTxn.TXN_SELL_DATE = DateTime.Today.Date;
            txnDate = selectedrecord.TXN_BUY_DATE;
            parentPageIndex = pageIndex;
            parentClosedPageIndex = pageClosedIndex;
            parentSortOrder = sortOrder;
            parentFilter = currentFilter;
            parentSearchString = searchString;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            portfolioTxn.TOTAL_SELL_AMT = portfolioTxn.SELL_QUANTITY * portfolioTxn.SELL_AMT_PER_UNIT;
            portfolioTxn.SELL_GAIN_AMT = portfolioTxn.TOTAL_COST - portfolioTxn.TOTAL_SELL_AMT;
            portfolioTxn.SELL_GAIN_PCT = (portfolioTxn.SELL_GAIN_AMT / (portfolioTxn.SELL_QUANTITY * portfolioTxn.COST_PER_UNIT)) * 100;
            portfolioTxn.SOLD_AFTER = (int)((portfolioTxn.TXN_BUY_DATE - portfolioTxn.TXN_SELL_DATE).TotalDays);

            //now update the original quantity and related parameters
            portfolioTxn.PURCHASE_QUANTITY = portfolioTxn.PURCHASE_QUANTITY - portfolioTxn.SELL_QUANTITY;

            if (portfolioTxn.PURCHASE_QUANTITY > 0)
            {
                DateTime[] quoteDate = null;
                double[] open, high, low, close, volume, change, changepercent, prevclose = null;

                portfolioTxn.stockMaster = (StockMaster)_context.StockMaster.Find(portfolioTxn.StockMasterID);
                DbInitializer.GetQuote(portfolioTxn.stockMaster.Symbol + (portfolioTxn.stockMaster.Exchange.Length == 0 ? "" : ("." + portfolioTxn.stockMaster.Exchange)), out quoteDate, out open, out high, out low, out close,
                            out volume, out change, out changepercent, out prevclose);
                if (quoteDate != null)
                {
                    portfolioTxn.CMP = close[0];
                    portfolioTxn.VALUE = portfolioTxn.PURCHASE_QUANTITY * close[0];

                    portfolioTxn.TOTAL_COST = portfolioTxn.PURCHASE_QUANTITY * portfolioTxn.COST_PER_UNIT;
                    portfolioTxn.GAIN_AMT = portfolioTxn.VALUE - portfolioTxn.TOTAL_COST;
                    portfolioTxn.GAIN_PCT = (portfolioTxn.GAIN_AMT / portfolioTxn.VALUE) * 100;
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
            return RedirectToPage("./portfolioTxnIndex", new { masterid = portfolioTxn.PORTFOLIO_MASTER_ID, pageIndex = parentPageIndex, pageClosedIndex = parentClosedPageIndex, sortOrder = parentSortOrder, currentFilter = parentFilter, searchString = parentSearchString });

        }
        private bool PORTFOLIOTxnExists(int txnid)
        {
            return _context.PORTFOLIOTXN.Any(e => e.PORTFOLIOTXN_ID == txnid);
        }

    }
}
