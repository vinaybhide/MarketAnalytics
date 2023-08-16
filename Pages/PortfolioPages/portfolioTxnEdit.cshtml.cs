using Microsoft.AspNetCore.Mvc;
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
        public int? TxnId { get; set; }
        [BindProperty]
        public int? TxnStockMasterId { get; set; }
        [BindProperty]
        public int? TxnPortfolioMasterId { get; set; }
        [BindProperty]
        public DateTime TxnDate { get; set; }
        [BindProperty]
        public string TxnType { get; set; }
        [BindProperty]
        public double TxnPurchaseQty { get; set; }
        [BindProperty]
        public double TxnCostPerUnit { get; set; }
        [BindProperty]
        public string TxnStockSymbol { get; set; }
        [BindProperty]
        public string TxnStockCompName { get; set; }
        //[BindProperty]
        //public PORTFOLIOTXN portfolioTxn { get; set; }
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

        public async Task<IActionResult> OnGetAsync(int? masterid, int? txnid, int pageSummaryIndex, int pageIndex,
            int pageClosedIndex, string sortOrder, string currentFilter, string searchString)
        {
            if (txnid == null || _context.PORTFOLIOTXN == null)
            {
                return NotFound();
            }

            TxnId = txnid;

            var selectedrecord = await _context.PORTFOLIOTXN.Include(a => a.stockMaster).AsSplitQuery().FirstOrDefaultAsync(m => m.PORTFOLIOTXN_ID == txnid);
            if (selectedrecord == null)
            {
                return NotFound();
            }
            //portfolioTxn = selectedrecord;
            selectedrecord.stockMaster = (StockMaster)_context.StockMaster.Find(selectedrecord.StockMasterID);
            TxnStockSymbol = selectedrecord.stockMaster.Symbol;
            TxnStockCompName = selectedrecord.stockMaster.CompName;
            TxnDate = selectedrecord.TXN_BUY_DATE;
            TxnStockMasterId = selectedrecord.StockMasterID;
            TxnPortfolioMasterId = selectedrecord.PORTFOLIO_MASTER_ID;
            TxnDate = selectedrecord.TXN_BUY_DATE;
            TxnType = selectedrecord.TXN_TYPE;
            TxnPurchaseQty = selectedrecord.PURCHASE_QUANTITY;
            TxnCostPerUnit = selectedrecord.COST_PER_UNIT;

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
            PORTFOLIOTXN existingTxn = _context.PORTFOLIOTXN.Find(TxnId);

            DateTime[] quoteDate = null;
            double[] open, high, low, close, volume, change, changepercent, prevclose = null;

            existingTxn.stockMaster = (StockMaster)_context.StockMaster.Find(existingTxn.StockMasterID);
            DbInitializer.GetQuote(existingTxn.stockMaster.Symbol + (existingTxn.stockMaster.Exchange.Length == 0 ? "" : ("." + existingTxn.stockMaster.Exchange)), out quoteDate, out open, out high, out low, out close,
                        out volume, out change, out changepercent, out prevclose);

            //if ((TxnPurchaseQty > 0) && (TxnCostPerUnit > 0))
            if (TxnPurchaseQty > 0)
            {
                existingTxn.TXN_BUY_DATE = TxnDate;
                existingTxn.PURCHASE_QUANTITY = TxnPurchaseQty;
                existingTxn.COST_PER_UNIT = TxnCostPerUnit;
                existingTxn.TOTAL_COST = TxnPurchaseQty * TxnCostPerUnit;
                existingTxn.DAYS_SINCE = DateTime.Today.Date.Subtract(existingTxn.TXN_BUY_DATE.Date).Days;

                if ((quoteDate != null) && (close != null))
                {
                    existingTxn.VALUE = existingTxn.PURCHASE_QUANTITY * close[0];
                    existingTxn.GAIN_AMT = existingTxn.VALUE - existingTxn.TOTAL_COST;
                    if (existingTxn.TOTAL_COST > 0)
                    {
                        existingTxn.GAIN_PCT = (existingTxn.GAIN_AMT / existingTxn.VALUE) * 100;
                    }
                    else
                    {
                        existingTxn.GAIN_PCT = 100;
                    }
                }
                _context.PORTFOLIOTXN.Update(existingTxn);
            }
            else
            {
                _context.PORTFOLIOTXN.Remove(existingTxn);
            }
            await _context.SaveChangesAsync(true);

            //portfolioTxn.TOTAL_COST = portfolioTxn.PURCHASE_QUANTITY * portfolioTxn.COST_PER_UNIT;

            //DateTime[] quoteDate = null;
            //double[] open, high, low, close, volume, change, changepercent, prevclose = null;

            //portfolioTxn.stockMaster = (StockMaster)_context.StockMaster.Find(portfolioTxn.StockMasterID);
            //DbInitializer.GetQuote(portfolioTxn.stockMaster.Symbol + (portfolioTxn.stockMaster.Exchange.Length == 0 ? "" : ("." + portfolioTxn.stockMaster.Exchange)), out quoteDate, out open, out high, out low, out close,
            //            out volume, out change, out changepercent, out prevclose);
            //if (quoteDate != null)
            //{
            //    portfolioTxn.CMP = close[0];
            //    portfolioTxn.VALUE = portfolioTxn.PURCHASE_QUANTITY * close[0];
            //    portfolioTxn.GAIN_AMT = portfolioTxn.VALUE - portfolioTxn.TOTAL_COST;
            //    if (portfolioTxn.TOTAL_COST > 0)
            //    {
            //        portfolioTxn.GAIN_PCT = (portfolioTxn.GAIN_AMT / portfolioTxn.VALUE) * 100;
            //    }
            //    else
            //    {
            //        portfolioTxn.GAIN_PCT = 100;
            //    }
            //}
            //_context.PORTFOLIOTXN.Update(portfolioTxn);
            //await _context.SaveChangesAsync(true);

            //_context.Attach(portfolioTxn).State = EntityState.Modified;

            //try
            //{
            //    await _context.SaveChangesAsync();
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    if (!PORTFOLIOTxnExists(portfolioTxn.PORTFOLIOTXN_ID))
            //    {
            //        return NotFound();
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}

            //return RedirectToPage("./portfolioTxnIndex");
            return RedirectToPage("./portfolioTxnIndex", new { masterid = TxnPortfolioMasterId, stockid = TxnStockMasterId, pageSummaryIndex = parentPageSummaryIndex, pageIndex = parentPageIndex, pageClosedIndex = parentClosedPageIndex, sortOrder = parentSortOrder, currentFilter = parentFilter, searchString = parentSearchString });
        }

        private bool PORTFOLIOTxnExists(int txnid)
        {
            return _context.PORTFOLIOTXN.Any(e => e.PORTFOLIOTXN_ID == txnid);
        }
    }
}
