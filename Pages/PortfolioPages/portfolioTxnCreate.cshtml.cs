using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MarketAnalytics.Data;
using MarketAnalytics.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.Text;

namespace MarketAnalytics.Pages.PortfolioPages
{

    public class PortfolioTxnCreateModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;


        public List<SelectListItem> exchangeList { get; set; }
        public List<SelectListItem> symbolList { get; set; }
        public List<SelectListItem> typeList { get; set; }
        public int SelectedTag { get; set; }

        [BindProperty]
        public int MasterId { get; set; }

        [BindProperty]
        public string portfolioName { get; set; } = string.Empty;

        [BindProperty]
        public PORTFOLIOTXN portfolioTxn { get; set; }

        [BindProperty]
        public int parentpageIndex { get; set; }
        [BindProperty]
        public string TypeSelected { get; set; }

        public PortfolioTxnCreateModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
            symbolList = new List<SelectListItem>();
            typeList = new List<SelectListItem>();
        }

        public IActionResult OnGet(int? masterid, int? pageIndex)
        {
            typeList.Clear();
            typeList = _context.StockMaster.Select(a => a.INVESTMENT_TYPE).Select(a =>
                                                    new SelectListItem
                                                    {
                                                        Value = a.ToString(),
                                                        Text = a.ToString()
                                                    }).Distinct().ToList();

            //symbolList.Clear();
            //symbolList = _context.StockMaster.OrderBy(a => a.Symbol).Select(a =>
            //                                  new SelectListItem
            //                                  {
            //                                      Value = a.StockMasterID.ToString(),
            //                                      Text = a.Symbol
            //                                  }).ToList();
            if (masterid != null)
            {
                MasterId = (int)masterid;
                portfolioTxn = new PORTFOLIOTXN();
                portfolioTxn.PORTFOLIO_MASTER_ID = (int)masterid;
                //var masterRec = _context.PORTFOLIO_MASTER.Select(m => (m.PORTFOLIO_MASTER_ID == masterId));
                var masterRec = _context.PORTFOLIO_MASTER.FirstOrDefault(m => (m.PORTFOLIO_MASTER_ID == masterid));
                if (masterRec != null)
                {
                    portfolioName = masterRec.PORTFOLIO_NAME.ToString();
                    portfolioTxn.portfolioMaster = masterRec;
                    portfolioTxn.TXN_BUY_DATE= DateTime.Now;
                }
            }
            parentpageIndex = (int)pageIndex;
            return Page();
        }

        public IActionResult OnPostSelectdType(string typesel, int? masterid, int? pageIndex, string portfolioName)
        {
            if (string.IsNullOrEmpty(typesel) == false)
            {
                symbolList.Clear();
                symbolList = _context.StockMaster.Where(a => a.INVESTMENT_TYPE.Equals(typesel)).OrderBy(a => a.CompName).Select(a =>
                                                  new SelectListItem
                                                  {
                                                      Value = a.StockMasterID.ToString(),
                                                      Text = a.CompName+ " (" + a.Symbol + ")"
                                                  }).ToList();
            }
            else
            {
                symbolList.Clear();
            }
            TypeSelected = typesel;

            typeList.Clear();
            typeList = _context.StockMaster.Select(a => a.INVESTMENT_TYPE).Select(a =>
                                                    new SelectListItem
                                                    {
                                                        Value = a.ToString(),
                                                        Text = a.ToString()
                                                    }).Distinct().ToList();

            typeList.FirstOrDefault(a => a.Value.Equals(TypeSelected)).Selected = true;

            if (masterid != null)
            {
                MasterId = (int)masterid;
                portfolioTxn = new PORTFOLIOTXN();
                portfolioTxn.PORTFOLIO_MASTER_ID = (int)masterid;
                //var masterRec = _context.PORTFOLIO_MASTER.Select(m => (m.PORTFOLIO_MASTER_ID == masterId));
                var masterRec = _context.PORTFOLIO_MASTER.FirstOrDefault(m => (m.PORTFOLIO_MASTER_ID == masterid));
                if (masterRec != null)
                {
                    portfolioName = masterRec.PORTFOLIO_NAME.ToString();
                    portfolioTxn.portfolioMaster = masterRec;
                    portfolioTxn.TXN_BUY_DATE = DateTime.Now;
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
            DbInitializer.GetQuote(portfolioTxn.stockMaster.Symbol + (portfolioTxn.stockMaster.Exchange.Length == 0 ? "" : ("." + portfolioTxn.stockMaster.Exchange)), out quoteDate, out open, out high, out low, out close,
                        out volume, out change, out changepercent, out prevclose);
            if (quoteDate != null)
            {
                portfolioTxn.CMP = close[0];
                portfolioTxn.VALUE = portfolioTxn.QUANTITY * close[0];
            }

            portfolioTxn.TOTAL_COST = portfolioTxn.QUANTITY * portfolioTxn.COST_PER_UNIT;
            portfolioTxn.GAIN_AMT = portfolioTxn.VALUE - portfolioTxn.TOTAL_COST;
            portfolioTxn.GAIN_PCT = (portfolioTxn.GAIN_AMT / portfolioTxn.VALUE) * 100;

            _context.PORTFOLIOTXN.Add(portfolioTxn);
            await _context.SaveChangesAsync();

            //return RedirectToPage("./portfolioTxnIndex", new { masterid = portfolioTxn.PORTFOLIO_MASTER_ID, pageIndex = parentpageIndex });
            return RedirectToPage("./portfolioTxnIndex", new { masterid = portfolioTxn.PORTFOLIO_MASTER_ID});
        }
    }
}
