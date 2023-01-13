using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MarketAnalytics.Data;
using MarketAnalytics.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketAnalytics.Pages.PortfolioPages
{

    public class PortfolioTxnCreateModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;

        public List<SelectListItem> exchangeList { get; set; }
        public List<SelectListItem> symbolList { get; set; }
        public int SelectedTag { get; set; }
        public string portfolioMasterName { get; set; } = string.Empty;

        [BindProperty]
        public PORTFOLIOTXN portfolioTxn { get; set; }

        [BindProperty]
        public int parentpageIndex { get; set; }
        public PortfolioTxnCreateModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
            symbolList = new List<SelectListItem>();
        }

        public IActionResult OnGet(int? masterid, int? pageIndex)
        {
            symbolList.Clear();
            symbolList = _context.StockMaster.OrderBy(a => a.Symbol).Select(a =>
                                              new SelectListItem
                                              {
                                                  Value = a.StockMasterID.ToString(),
                                                  Text = a.Symbol
                                              }).ToList();
            if (masterid != null)
            {
                portfolioTxn = new PORTFOLIOTXN();
                portfolioTxn.PORTFOLIO_MASTER_ID = (int)masterid;
                //var masterRec = _context.PORTFOLIO_MASTER.Select(m => (m.PORTFOLIO_MASTER_ID == masterId));
                var masterRec = _context.PORTFOLIO_MASTER.FirstOrDefault(m => (m.PORTFOLIO_MASTER_ID == masterid));
                if (masterRec != null)
                {
                    portfolioMasterName = masterRec.PORTFOLIO_NAME.ToString();
                    portfolioTxn.portfolioMaster = masterRec;
                    portfolioTxn.TXN_DATE= DateTime.Now;
                }
            }
            parentpageIndex = (int)pageIndex;
            return Page();
        }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            DateTime[] quoteDate = null;
            double[] open, high, low, close, volume, change, changepercent, prevclose = null;

            portfolioTxn.stockMaster = (StockMaster)_context.StockMaster.Find(portfolioTxn.StockMasterID);

            DbInitializer.GetQuote(portfolioTxn.stockMaster.Symbol + ".NS", out quoteDate, out open, out high, out low, out close,
                        out volume, out change, out changepercent, out prevclose);
            if (quoteDate != null)
            {
                portfolioTxn.CMP = close[0];
                portfolioTxn.VALUE = portfolioTxn.QUANTITY * close[0];
            }

            portfolioTxn.TOTAL_COST = portfolioTxn.QUANTITY * portfolioTxn.COST_PER_SHARE;
            portfolioTxn.GAIN_AMT = portfolioTxn.VALUE - portfolioTxn.TOTAL_COST;
            portfolioTxn.GAIN_PCT = (portfolioTxn.GAIN_AMT / portfolioTxn.VALUE) * 100;

            _context.PORTFOLIOTXN.Add(portfolioTxn);
            await _context.SaveChangesAsync();

            //return RedirectToPage("./portfolioTxnIndex", new { masterid = portfolioTxn.PORTFOLIO_MASTER_ID, pageIndex = parentpageIndex });
            return RedirectToPage("./portfolioTxnIndex", new { masterid = portfolioTxn.PORTFOLIO_MASTER_ID});
        }
    }
}
