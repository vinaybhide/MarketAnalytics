using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MarketAnalytics.Data;
using MarketAnalytics.Models;

namespace MarketAnalytics.Pages.PortfolioPages
{

    public class CreateModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;

        public List<SelectListItem> exchangeList { get; set; }
        public List<SelectListItem> symbolList { get; set; }
        public int SelectedTag { get; set; }

        public CreateModel(MarketAnalytics.Data.DBContext context)
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
                portfolio = new PORTFOLIO();
                //portfolio.PORTFOLIO_MASTER_ID = (int)masterId;
            }
            return Page();
        }

        [BindProperty]
        public PORTFOLIO portfolio { get; set; }


        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            DateTime[] quoteDate = null;
            double[] open, high, low, close, volume, change, changepercent, prevclose = null;

            portfolio.StockMaster = (StockMaster)_context.StockMaster.Find(portfolio.StockMasterID);

            DbInitializer.GetQuote(portfolio.StockMaster.Symbol + ".NS", out quoteDate, out open, out high, out low, out close,
                        out volume, out change, out changepercent, out prevclose);
            if (quoteDate != null)
            {
                portfolio.CMP = close[0];
                portfolio.VALUE = portfolio.QUANTITY * close[0];
            }

            portfolio.TOTAL_COST = portfolio.QUANTITY * portfolio.COST_PER_SHARE;

            _context.PORTFOLIO.Add(portfolio);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
