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

        public PortfolioTxnCreateModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
            symbolList = new List<SelectListItem>();
        }

        public IActionResult OnGet(int? masterId)
        {
            symbolList.Clear();
            symbolList = _context.StockMaster.Select(a =>
                                              new SelectListItem
                                              {
                                                  Value = a.StockMasterID.ToString(),
                                                  Text = a.Symbol
                                              }).ToList();

            if (masterId != null)
            {
                portfolioTxn = new PORTFOLIOTXN();
                portfolioTxn.PORTFOLIO_MASTER_ID = (int)masterId;
                //var masterRec = _context.PORTFOLIO_MASTER.Select(m => (m.PORTFOLIO_MASTER_ID == masterId));
                var masterRec = _context.PORTFOLIO_MASTER.FirstOrDefault(m => (m.PORTFOLIO_MASTER_ID == masterId));
                if (masterRec != null)
                {
                    portfolioMasterName = masterRec.PORTFOLIO_NAME.ToString();
                }
            }
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

            _context.PORTFOLIOTXN.Add(portfolioTxn);
            await _context.SaveChangesAsync();

            return RedirectToPage("./portfolioTxnIndex", new { masterId = portfolioTxn.PORTFOLIO_MASTER_ID });
        }
    }
}
