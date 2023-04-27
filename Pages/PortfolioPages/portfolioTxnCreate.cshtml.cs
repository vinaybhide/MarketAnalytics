using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MarketAnalytics.Data;
using MarketAnalytics.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Newtonsoft.Json.Linq;

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
        public string TxnType { get; set; } = string.Empty;

        [BindProperty]
        public int MasterId { get; set; }

        [BindProperty]
        public int? TxnId { get; set; }

        [BindProperty]
        public string portfolioName { get; set; } = string.Empty;

        [BindProperty]
        public PORTFOLIOTXN portfolioTxn { get; set; }

        [BindProperty]
        public int parentPageIndex { get; set; }
        [BindProperty]
        public int parentClosedPageIndex { get; set; }


        [BindProperty]
        public string TypeSelected { get; set; }
        [BindProperty]
        public string ExchangeSelected { get; set; }

        [BindProperty]
        public string parentSortOrder { get; set; }
        [BindProperty]
        public string parentFilter { get; set; }

        public PortfolioTxnCreateModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
            exchangeList = new List<SelectListItem>();
            symbolList = new List<SelectListItem>();
            typeList = new List<SelectListItem>();
        }

        public IActionResult OnGet(string txntype, int masterid, int? txnid, int pageIndex, int pageClosedIndex,
            string sortOrder, string currentFilter)
        {
            if (string.IsNullOrEmpty(txntype) == false)
            {
                MasterId = masterid;
                TxnType = txntype;
                TxnId = txnid;
                parentPageIndex = pageIndex;
                parentClosedPageIndex = pageClosedIndex;
                parentSortOrder = sortOrder;
                parentFilter = currentFilter;

                var masterRec = _context.PORTFOLIO_MASTER.AsSplitQuery().FirstOrDefault(m => (m.PORTFOLIO_MASTER_ID == masterid));
                portfolioTxn = new PORTFOLIOTXN();
                portfolioTxn.PORTFOLIO_MASTER_ID = (int)masterid;
                portfolioName = masterRec.PORTFOLIO_NAME.ToString();
                portfolioTxn.portfolioMaster = masterRec;
                portfolioTxn.TXN_TYPE = txntype;

                if (txntype.Equals("B"))
                {
                    typeList.Clear();
                    typeList = _context.StockMaster.AsSplitQuery().Select(a => a.INVESTMENT_TYPE).Select(a =>
                                                            new SelectListItem
                                                            {
                                                                Value = a.ToString(),
                                                                Text = a.ToString()
                                                            }).Distinct().ToList();
                    exchangeList.Clear();
                    exchangeList = _context.StockMaster.AsSplitQuery().Select(a => a.Exchange).Select(a =>
                                                            new SelectListItem
                                                            {
                                                                Value = a.ToString(),
                                                                Text = a.ToString()
                                                            }).Distinct().ToList();
                    portfolioTxn.TXN_BUY_DATE = DateTime.Now;
                }
                else if (txntype.Equals("S"))
                {
                    var existingTxn = _context.PORTFOLIOTXN.Find(TxnId);

                    //set existing field values
                    portfolioTxn.TXN_BUY_DATE = existingTxn.TXN_BUY_DATE;
                    portfolioTxn.PURCHASE_QUANTITY = existingTxn.PURCHASE_QUANTITY;
                    portfolioTxn.COST_PER_UNIT = existingTxn.COST_PER_UNIT;
                    portfolioTxn.TOTAL_COST = existingTxn.TOTAL_COST;
                    portfolioTxn.TXN_SELL_DATE = DateTime.Now;
                    portfolioTxn.SELL_QUANTITY = existingTxn.PURCHASE_QUANTITY;
                    portfolioTxn.SELL_AMT_PER_UNIT = 0;
                    portfolioTxn.TOTAL_SELL_AMT = 0;
                    portfolioTxn.StockMasterID = existingTxn.StockMasterID;
                    portfolioTxn.stockMaster = existingTxn.GetStockMaster(_context);//existingTxn.stockMaster;


                    symbolList.Clear();
                    symbolList = _context.StockMaster.AsSplitQuery().Where(a => a.StockMasterID == existingTxn.StockMasterID).Select(a =>
                                                            new SelectListItem
                                                            {
                                                                Value = a.StockMasterID.ToString(),
                                                                Text = a.CompName + "(" + a.Symbol + "." + a.Exchange + ")"
                                                            }).Distinct().ToList();
                    symbolList.FirstOrDefault(a => a.Value.Equals(existingTxn.StockMasterID.ToString())).Selected = true;
                }
                //var masterRec = _context.PORTFOLIO_MASTER.Select(m => (m.PORTFOLIO_MASTER_ID == masterId));

            }
            return Page();
        }

        public IActionResult OnPostSearchLocalOnline(string searchString, string searchWhere, string typesel, string exchangesel, int? masterid,
            int pageIndex, int pageClosedIndex, string portfolioName)
        {
            symbolList.Clear();

            if (searchString != null)
            {
                if (string.IsNullOrEmpty(searchWhere) == false)
                {
                    if (searchWhere.Equals("Search Online"))
                    {
                        DbInitializer.SearchOnlineInsertInDB(_context, searchString);
                    }
                }
                else
                {
                    DbInitializer.SearchOnlineInsertInDB(_context, searchString);
                }

                symbolList = _context.StockMaster.AsSplitQuery().Where(a => a.Symbol.ToUpper().Contains(searchString.ToUpper()) || a.CompName.ToUpper().Contains(searchString.ToUpper())).OrderBy(a => a.CompName).Select(a =>
                                                  new SelectListItem
                                                  {
                                                      Value = a.StockMasterID.ToString(),
                                                      Text = a.CompName + " (" + a.Symbol + "." + a.Exchange + ")"
                                                  }).ToList();

            }
            TypeSelected = typesel;

            typeList.Clear();
            typeList = _context.StockMaster.AsSplitQuery().Select(a => a.INVESTMENT_TYPE).Select(a =>
                                                    new SelectListItem
                                                    {
                                                        Value = a.ToString(),
                                                        Text = a.ToString()
                                                    }).Distinct().ToList();

            if (string.IsNullOrEmpty(TypeSelected) == false)
            {
                typeList.FirstOrDefault(a => a.Value.Equals(TypeSelected)).Selected = true;
            }

            ExchangeSelected = exchangesel;
            exchangeList.Clear();
            exchangeList = _context.StockMaster.AsSplitQuery().Select(a => a.Exchange).Select(a =>
                                                    new SelectListItem
                                                    {
                                                        Value = a.ToString(),
                                                        Text = a.ToString()
                                                    }).Distinct().ToList();

            if (string.IsNullOrEmpty(ExchangeSelected) == false)
            {
                exchangeList.FirstOrDefault(a => a.Value.Equals(ExchangeSelected)).Selected = true;
            }

            if (masterid != null)
            {
                MasterId = (int)masterid;
                portfolioTxn = new PORTFOLIOTXN();
                portfolioTxn.PORTFOLIO_MASTER_ID = (int)masterid;
                //var masterRec = _context.PORTFOLIO_MASTER.Select(m => (m.PORTFOLIO_MASTER_ID == masterId));
                var masterRec = _context.PORTFOLIO_MASTER.AsSplitQuery().FirstOrDefault(m => (m.PORTFOLIO_MASTER_ID == masterid));
                if (masterRec != null)
                {
                    portfolioName = masterRec.PORTFOLIO_NAME.ToString();
                    portfolioTxn.portfolioMaster = masterRec;
                    portfolioTxn.TXN_BUY_DATE = DateTime.Now;
                }
            }

            parentPageIndex = pageIndex;
            parentClosedPageIndex = pageClosedIndex;
            return Page();
        }
        public IActionResult OnPostFilterSelected(string typesel, string exchangesel, int? masterid, int pageIndex, int pageClosedIndex, string portfolioName)
        {
            symbolList.Clear();
            if ((string.IsNullOrEmpty(typesel) == false) && (string.IsNullOrEmpty(exchangesel) == false))
            {
                symbolList = _context.StockMaster.AsSplitQuery().Where(a => a.INVESTMENT_TYPE.Equals(typesel) && a.Exchange.Equals(exchangesel)).OrderBy(a => a.CompName).Select(a =>
                                                  new SelectListItem
                                                  {
                                                      Value = a.StockMasterID.ToString(),
                                                      Text = a.CompName + " (" + a.Symbol + "." + a.Exchange + ")"
                                                  }).ToList();
            }
            else if (string.IsNullOrEmpty(typesel) == false)
            {
                symbolList = _context.StockMaster.AsSplitQuery().Where(a => a.INVESTMENT_TYPE.Equals(typesel)).OrderBy(a => a.CompName).Select(a =>
                                                  new SelectListItem
                                                  {
                                                      Value = a.StockMasterID.ToString(),
                                                      Text = a.CompName + " (" + a.Symbol + "." + a.Exchange + ")"
                                                  }).ToList();
            }
            else if (string.IsNullOrEmpty(exchangesel) == false)
            {
                symbolList = _context.StockMaster.AsSplitQuery().Where(a => a.Exchange.Equals(exchangesel)).OrderBy(a => a.CompName).Select(a =>
                                                  new SelectListItem
                                                  {
                                                      Value = a.StockMasterID.ToString(),
                                                      Text = a.CompName + " (" + a.Symbol + "." + a.Exchange + ")"
                                                  }).ToList();
            }
            else
            {
                symbolList.Clear();
            }
            TypeSelected = typesel;

            typeList.Clear();
            typeList = _context.StockMaster.AsSplitQuery().Select(a => a.INVESTMENT_TYPE).Select(a =>
                                                    new SelectListItem
                                                    {
                                                        Value = a.ToString(),
                                                        Text = a.ToString()
                                                    }).Distinct().ToList();

            if (string.IsNullOrEmpty(TypeSelected) == false)
            {
                typeList.FirstOrDefault(a => a.Value.Equals(TypeSelected)).Selected = true;
            }

            ExchangeSelected = exchangesel;
            exchangeList.Clear();
            exchangeList = _context.StockMaster.AsSplitQuery().Select(a => a.Exchange).Select(a =>
                                                    new SelectListItem
                                                    {
                                                        Value = a.ToString(),
                                                        Text = a.ToString()
                                                    }).Distinct().ToList();

            if (string.IsNullOrEmpty(ExchangeSelected) == false)
            {
                exchangeList.FirstOrDefault(a => a.Value.Equals(ExchangeSelected)).Selected = true;
            }

            if (masterid != null)
            {
                MasterId = (int)masterid;
                portfolioTxn = new PORTFOLIOTXN();
                portfolioTxn.PORTFOLIO_MASTER_ID = (int)masterid;
                //var masterRec = _context.PORTFOLIO_MASTER.Select(m => (m.PORTFOLIO_MASTER_ID == masterId));
                var masterRec = _context.PORTFOLIO_MASTER.AsSplitQuery().FirstOrDefault(m => (m.PORTFOLIO_MASTER_ID == masterid));
                if (masterRec != null)
                {
                    portfolioName = masterRec.PORTFOLIO_NAME.ToString();
                    portfolioTxn.portfolioMaster = masterRec;
                    portfolioTxn.TXN_BUY_DATE = DateTime.Now;
                }
            }
            parentPageIndex = pageIndex;
            parentClosedPageIndex = pageClosedIndex;
            return Page();
        }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync(int txnid, string txntype, int masterid)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            DateTime[] quoteDate = null;
            double[] open, high, low, close, volume, change, changepercent, prevclose = null;
            PORTFOLIOTXN existingTxn = null;
            portfolioTxn.stockMaster = (StockMaster)_context.StockMaster.Find(portfolioTxn.StockMasterID);

            if (portfolioTxn.stockMaster.Exchange.Equals("NA") == true)
            {
                quoteDate = new DateTime[1] { DateTime.Today.Date };
                open = new double[1] { 1 };
                high = new double[1] { 1 };
                low = new double[1] { 1 };
                close = new double[1] { 1 };
                volume = new double[1] { 0 };
                change = new double[1] { 0 };
                changepercent = new double[1] { 0 };
                prevclose = new double[1] { 1 };

            }
            else
            {
                DbInitializer.GetQuote(portfolioTxn.stockMaster.Symbol + (portfolioTxn.stockMaster.Exchange.Length == 0 ? "" : ("." + portfolioTxn.stockMaster.Exchange)), out quoteDate, out open, out high, out low, out close,
                        out volume, out change, out changepercent, out prevclose);
            }

            //if (DbInitializer.GetQuote(portfolioTxn.stockMaster.Symbol + (portfolioTxn.stockMaster.Exchange.Length == 0 ? "" : ("." + portfolioTxn.stockMaster.Exchange)), out quoteDate, out open, out high, out low, out close,
            //            out volume, out change, out changepercent, out prevclose))
            //{
                if ((quoteDate != null) && (close != null))
                {
                    portfolioTxn.CMP = close[0];
                    //portfolioTxn.VALUE = portfolioTxn.PURCHASE_QUANTITY * close[0];

                    if (TxnType.Equals("B"))
                    {
                        portfolioTxn.TOTAL_COST = portfolioTxn.PURCHASE_QUANTITY * portfolioTxn.COST_PER_UNIT;
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
                    else if (TxnType.Equals("S"))
                    {
                        existingTxn = _context.PORTFOLIOTXN.Find(TxnId);

                        portfolioTxn.TXN_TYPE = TxnType;
                        portfolioTxn.TOTAL_SELL_AMT = portfolioTxn.SELL_QUANTITY * portfolioTxn.SELL_AMT_PER_UNIT;
                        //sell value - buy value for sold number of stocks
                        portfolioTxn.SELL_GAIN_AMT = portfolioTxn.TOTAL_SELL_AMT - (portfolioTxn.SELL_QUANTITY * existingTxn.COST_PER_UNIT);
                        if (existingTxn.COST_PER_UNIT > 0)
                        {
                            portfolioTxn.SELL_GAIN_PCT = (portfolioTxn.SELL_GAIN_AMT / (portfolioTxn.SELL_QUANTITY * existingTxn.COST_PER_UNIT)) * 100;
                        }
                        else
                        {
                            portfolioTxn.SELL_GAIN_PCT = 100;
                        }
                        portfolioTxn.SOLD_AFTER = portfolioTxn.TXN_SELL_DATE.Date.Subtract(existingTxn.TXN_BUY_DATE.Date).Days;

                        portfolioTxn.TXN_BUY_DATE = existingTxn.TXN_BUY_DATE;
                        portfolioTxn.COST_PER_UNIT = existingTxn.COST_PER_UNIT;

                        portfolioTxn.PURCHASE_QUANTITY = portfolioTxn.SELL_QUANTITY;
                        portfolioTxn.TOTAL_COST = portfolioTxn.PURCHASE_QUANTITY * portfolioTxn.COST_PER_UNIT;

                        portfolioTxn.CMP = existingTxn.CMP = close[0];

                        existingTxn.PURCHASE_QUANTITY = existingTxn.PURCHASE_QUANTITY - portfolioTxn.SELL_QUANTITY;

                        portfolioTxn.VALUE = portfolioTxn.PURCHASE_QUANTITY * close[0];
                        portfolioTxn.GAIN_AMT = portfolioTxn.VALUE - portfolioTxn.TOTAL_COST;
                        portfolioTxn.GAIN_PCT = (portfolioTxn.GAIN_AMT / portfolioTxn.TOTAL_COST) * 100;

                        if (existingTxn.PURCHASE_QUANTITY > 0)
                        {
                            existingTxn.TOTAL_COST = existingTxn.PURCHASE_QUANTITY * existingTxn.COST_PER_UNIT;
                            existingTxn.VALUE = existingTxn.PURCHASE_QUANTITY * close[0];
                            existingTxn.GAIN_AMT = existingTxn.VALUE - existingTxn.TOTAL_COST;
                            existingTxn.GAIN_PCT = (existingTxn.GAIN_AMT / existingTxn.TOTAL_COST) * 100;
                            _context.PORTFOLIOTXN.Update(existingTxn);
                        }
                        else
                        {
                            _context.PORTFOLIOTXN.Remove(existingTxn);
                        }
                    }

                    _context.PORTFOLIOTXN.Add(portfolioTxn);
                    await _context.SaveChangesAsync(true);
                }
                //return RedirectToPage("./portfolioTxnIndex", new { masterid = portfolioTxn.PORTFOLIO_MASTER_ID, pageIndex = parentpageIndex });
                return RedirectToPage("./portfolioTxnIndex", new { masterid = portfolioTxn.PORTFOLIO_MASTER_ID, searchString = portfolioTxn.stockMaster.Symbol });
            //}
            return Page();
        }
    }
}
