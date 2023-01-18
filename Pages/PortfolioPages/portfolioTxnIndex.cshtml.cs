using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MarketAnalytics.Data;
using MarketAnalytics.Models;
using System.Data;
using System.Net;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.Text;

namespace MarketAnalytics.Pages.PortfolioPages
{
    public class PortfolioTxnIndex : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public List<SelectListItem> symbolList { get; set; }
        public List<SelectListItem> menuList { get; set; }

        public PortfolioTxnIndex(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            symbolList = new List<SelectListItem>();
            menuList = new List<SelectListItem>();
        }

        public string DateSort { get; set; }
        public string ExchangeSort { get; set; }
        public string SymbolSort { get; set; }
        public string CompNameSort { get; set; }
        [BindProperty]
        public string CurrentFilter { get; set; }
        [BindProperty]
        public string CurrentSort { get; set; }
        [BindProperty]
        public int? CurrentPageIndex { get; set; }
        [BindProperty]
        public int? CurrentClosedPageIndex { get; set; }

        public bool RefreshAllStocks { get; set; } = false;
        public PaginatedList<PORTFOLIOTXN> portfolioOpenTxn { get; set; } = default!;
        public PaginatedList<PORTFOLIOTXN> portfolioClosedTxn { get; set; } = default!;

        public string portfolioMasterName { get; set; } = string.Empty;
        public double portfolioTotalCost { get; set; } = default(double);
        public double portfolioTotalGain { get; set; } = default(double);
        public double portfolioTotalValue { get; set; } = default(double);

        [BindProperty]
        public int MasterId { get; set; }
        [BindProperty]
        public int TransId { get; set; }
        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex, int? pageIndexClosed,  int? masterid,
            bool? refreshAll, bool? getQuote, int? stockid, bool? updateBuySell, bool? lifetimeHighLow)
        {
            //DateTime[] quoteDate = null;
            //double[] open, high, low, close, volume, change, changepercent, prevclose = null;

            if ((_context.PORTFOLIOTXN != null) && (masterid != null))
            {
                MasterId = (int)masterid;

                menuList.Clear();
                int listCntr = -1;
                SelectListItem menuItem = new SelectListItem("-- Select Action --", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("Sell Transaction", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("Edit Transaction", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("Delete Transaction", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("Details", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("Update (Quote, Strategies, High/Low)", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("History Table", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("Chart-History", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("Chart-SMA/RSI/STOCH", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("SMA V40 Strategy", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("V20 Strategy", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("Bullish Engulfing Strategy", listCntr.ToString());
                menuList.Add(menuItem);

                listCntr++;
                menuItem = new SelectListItem("Bearish Engulfing STrategy", listCntr.ToString());
                menuList.Add(menuItem);

                var masterRec = await _context.PORTFOLIO_MASTER.FirstOrDefaultAsync(m => (m.PORTFOLIO_MASTER_ID == masterid));
                portfolioMasterName = masterRec.PORTFOLIO_NAME;

                //_context.PORTFOLIOTXN.Include(x => x.PORTFOLIO_MASTER_ID);
                IQueryable<PORTFOLIOTXN> txnClosedIQ = _context.PORTFOLIOTXN.Where(x => (x.PORTFOLIO_MASTER_ID == masterid) && (x.TXN_TYPE.Equals("S")));

                IQueryable<PORTFOLIOTXN> txnOpenIQ = _context.PORTFOLIOTXN.Where(x => (x.PORTFOLIO_MASTER_ID == masterid) && (x.TXN_TYPE.Equals("B")));
                IQueryable<PORTFOLIOTXN> distinctOpenIQ = txnOpenIQ.GroupBy(a => a.stockMaster.Symbol)
                                                            .Select(x => x.FirstOrDefault());
                symbolList.Clear();

                //symbolList = distinctIQ.Select(a =>
                //                                new SelectListItem
                //                                {
                //                                    Value = a.StockMasterID.ToString(),
                //                                    Text = a.stockMaster.Symbol
                //                                }).AsEnumerable().ToList();

                symbolList = _context.PORTFOLIOTXN.Where(x => (x.PORTFOLIO_MASTER_ID == MasterId) && (x.TXN_TYPE.Equals("B")))
                                                      .OrderBy(a => a.stockMaster.Symbol)
                                                      .Select(a =>
                                                          new SelectListItem
                                                          {
                                                              Value = a.StockMasterID.ToString(),
                                                              Text = a.stockMaster.Symbol
                                                          }
                                                      ).Distinct().ToList();

                CurrentSort = sortOrder;
                DateSort = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
                SymbolSort = sortOrder == "Symbol" ? "symbol_desc" : "Symbol";
                ExchangeSort = sortOrder == "Exchange" ? "exchange_desc" : "Exchange";
                CompNameSort = sortOrder == "CompName" ? "compname_desc" : "CompName";
                if (searchString != null)
                {
                    pageIndex = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                if (stockid != null)
                {
                    //var selectedRecord = await _context.PORTFOLIOTXN.FirstOrDefaultAsync(m => ((m.PORTFOLIO_MASTER_ID == masterid) && (m.StockMasterID == stockid)));
                    var selectedRecord = txnOpenIQ.FirstOrDefault(m => (m.StockMasterID == stockid));
                    if (selectedRecord != null)
                    {
                        DbInitializer.GetQuoteAndUpdateAllPortfolioTxn(_context, selectedRecord);
                        //await GetQuoteAndUpdate(stockid, masterid, refreshAll, getQuote, updateBuySell, lifetimeHighLow);
                        searchString = selectedRecord.stockMaster.Symbol;
                    }
                }

                if ((refreshAll != null) && (refreshAll == true))
                {
                    foreach (var item in distinctOpenIQ)
                    {
                        DbInitializer.GetQuoteAndUpdateAllPortfolioTxn(_context, item);
                    }
                }

                //portfolioTotalCost = _context.PORTFOLIOTXN.Where(x => x.PORTFOLIO_MASTER_ID == MasterId)
                //                                            .Sum(a => a.TOTAL_COST);
                portfolioTotalCost = txnOpenIQ.Sum(a => a.TOTAL_COST);
                //portfolioTotalGain = (double)_context.PORTFOLIOTXN.Where(x => x.PORTFOLIO_MASTER_ID == MasterId)
                //                                            .Sum(a => a.GAIN_AMT);
                portfolioTotalGain = (double)txnOpenIQ.Sum(a => a.GAIN_AMT);
                //portfolioTotalValue = (double)_context.PORTFOLIOTXN.Where(x => x.PORTFOLIO_MASTER_ID == MasterId)
                //                                            .Sum(a => a.VALUE);
                portfolioTotalValue = (double)txnOpenIQ.Sum(a => a.VALUE);

                CurrentFilter = searchString;

                //IQueryable<PortfolioTxn> portfolioIQ = from s in _context.PORTFOLIOTxn select s;

                if (!String.IsNullOrEmpty(searchString))
                {
                    txnOpenIQ = txnOpenIQ.Where(s => s.stockMaster.Symbol.ToUpper().Contains(searchString.ToUpper())
                                                || s.stockMaster.CompName.ToUpper().Contains(searchString.ToUpper()));
                    var searchRecord = txnOpenIQ.FirstOrDefault();
                    if (searchRecord == null)
                    {
                        txnOpenIQ = _context.PORTFOLIOTXN.Where(x => (x.PORTFOLIO_MASTER_ID == masterid) && (x.TXN_TYPE.Equals("B")));
                    }
                    else if (symbolList.Exists(a => (a.Value.Equals(searchRecord.StockMasterID.ToString()) == true)))
                    {
                        symbolList.FirstOrDefault(a => a.Value.Equals(searchRecord.StockMasterID.ToString())).Selected = true;
                    }

                }
                switch (sortOrder)
                {
                    case "date_desc":
                        txnOpenIQ = txnOpenIQ.OrderByDescending(s => s.TXN_BUY_DATE);
                        break;
                    case "Symbol":
                        txnOpenIQ = txnOpenIQ.OrderBy(s => s.stockMaster.Symbol);
                        break;
                    case "symbol_desc":
                        txnOpenIQ = txnOpenIQ.OrderByDescending(s => s.stockMaster.Symbol);
                        break;
                    case "Exchange":
                        txnOpenIQ = txnOpenIQ.OrderBy(s => s.stockMaster.Exchange);
                        break;
                    case "exchange_desc":
                        txnOpenIQ = txnOpenIQ.OrderByDescending(s => s.stockMaster.Exchange);
                        break;
                    case "CompName":
                        txnOpenIQ = txnOpenIQ.OrderBy(s => s.stockMaster.CompName);
                        break;
                    case "compname_desc":
                        txnOpenIQ = txnOpenIQ.OrderByDescending(s => s.stockMaster.CompName);
                        break;
                    default:
                        txnOpenIQ = txnOpenIQ.OrderBy(s => s.TXN_BUY_DATE);
                        break;
                }
                var pageSize = Configuration.GetValue("PageSize", 10);
                
                CurrentPageIndex = pageIndex;
                portfolioOpenTxn = await PaginatedList<PORTFOLIOTXN>.CreateAsync(txnOpenIQ.AsNoTracking(), pageIndex ?? 1, pageSize);

                CurrentClosedPageIndex = pageIndexClosed;                
                portfolioClosedTxn = await PaginatedList<PORTFOLIOTXN>.CreateAsync(txnClosedIQ.AsNoTracking(), pageIndexClosed ?? 1, pageSize);
            }
        }

        public IActionResult OnPostTransactionAction(string menuitemsel, int?masterid, int? txnid, int? stockid, int? pageIndex, int? pageClosedIndex, string sortOrder, string currentFilter)
        {
            if ((menuitemsel.Equals("-1") == false) && (txnid != null) && (masterid != null) && (stockid != null))
            {
                //PORTFOLIOTXN currentTxn = _context.PORTFOLIOTXN.FirstOrDefault(t => t.PORTFOLIOTXN_ID== txnid);
                switch (menuitemsel)
                {
                    case "0"://case of sell txn
                        return RedirectToPage("./portfolioTxnSell", new { masterid = masterid, txnid = txnid, stockid = stockid, sortOrder = sortOrder, pageIndex = pageIndex, pageClosedIndex = pageClosedIndex, currentFilter = currentFilter, getQuote = "false", refreshAll = "false", lifetimeHighLow = "false" });

                    case "1"://case of edit txn
                        return RedirectToPage("./portfolioTxnEdit", new { masterid = masterid, txnid = txnid, stockid = stockid, sortOrder = sortOrder, pageIndex = pageIndex, pageClosedIndex = pageClosedIndex, currentFilter = currentFilter, getQuote = "false", refreshAll = "false", lifetimeHighLow = "false" });
                    case "2"://case of delete txn
                        return RedirectToPage("./portfolioTxnDelete", new { masterid = masterid, txnid = txnid, stockid = stockid, sortOrder = sortOrder, pageIndex = pageIndex, pageClosedIndex = pageClosedIndex, currentFilter = currentFilter, getQuote = "false", refreshAll = "false", lifetimeHighLow = "false" });
                    case "3"://case of txn details
                        return RedirectToPage("./portfolioTxnDetails", new { masterid = masterid, txnid = txnid, stockid = stockid, sortOrder = sortOrder, pageIndex = pageIndex, pageClosedIndex = pageClosedIndex, currentFilter = currentFilter, getQuote = "false", refreshAll = "false", lifetimeHighLow = "false" });
                    case "4"://case of update get quote, buy sell, high low
                        return RedirectToPage("./portfolioTxnIndex", new { masterid = masterid, txnid = txnid, stockid = stockid, sortOrder = sortOrder, pageIndex = pageIndex, pageClosedIndex = pageClosedIndex, currentFilter = currentFilter, getQuote = "true", refreshAll = "false", lifetimeHighLow = "false" });
                    case "5"://case of show history table
                        return RedirectToPage("/History/Index", new { id = stockid});
                    case "6"://case of show history chart
                        return RedirectToPage("/StandardIndicators/chartHistory", new { stockid = stockid, onlyhistory = 0, history = true });
                    case "7"://case of show smarsistoch chart
                        return RedirectToPage("/StandardIndicators/chartSMARSISTOCH", new { id = stockid});
                    case "8": //case of strategy SMA
                        return RedirectToPage("/BuySell/smav40", new { symbolToUpdate = stockid});
                    case "9": //case of strategy V20
                        return RedirectToPage("/BuySell/v20BuySell", new { symbolToUpdate = stockid });
                    case "10": //case of strategy Bullinsh engulfing
                        return RedirectToPage("/BuySell/BullishEngulfing", new { symbolToUpdate = stockid});
                    case "11": //case of strategy Bearish Engulfing
                        return RedirectToPage("/BuySell/BearishEngulfing", new { symbolToUpdate = stockid });

                    default:
                        return RedirectToPage("./portfolioTxnIndex", new { masterid = masterid, sortOrder = sortOrder, pageIndex = pageIndex, pageClosedIndex = pageClosedIndex, currentFilter = currentFilter, getQuote = "false", refreshAll = "false", lifetimeHighLow = "false" });
                }
            }
            return RedirectToPage("./portfolioTxnIndex", new { masterid = masterid, txnid = txnid, stockid = stockid, sortOrder = sortOrder, pageIndex = pageIndex, pageClosedIndex = pageClosedIndex, currentFilter = currentFilter, getQuote = "true", refreshAll = "false", lifetimeHighLow = "false" });
        }
        public void SelectedRow()
        {
            TransId = 0;
        }
        public async Task GetQuoteAndUpdate(int? stockid, int? masterid, bool? refreshAll, bool? getQuote,
                                    bool? updateBuySell, bool? lifetimeHighLow)
        {
            DateTime[] quoteDate = null;
            double[] open, high, low, close, volume, change, changepercent, prevclose = null;
            double lifetimehigh = 0, lifetimelow = 0;

            var selectedRecord = await _context.PORTFOLIOTXN.FirstOrDefaultAsync(m => ((m.PORTFOLIO_MASTER_ID == masterid) && (m.StockMasterID == stockid)));
            if (selectedRecord != null)
            {
                if (((refreshAll != null) && (refreshAll == true)) || ((getQuote != null) && (getQuote == true)))
                {
                    DbInitializer.GetQuote(selectedRecord.stockMaster.Symbol + (selectedRecord.stockMaster.Exchange.Length == 0 ? "" : ("." + selectedRecord.stockMaster.Exchange)), out quoteDate, out open,
                    out high, out low, out close,
                    out volume, out change, out changepercent, out prevclose);
                    if (quoteDate != null)
                    {
                        selectedRecord.stockMaster.Close = close[0];
                        selectedRecord.stockMaster.Open = open[0];
                        selectedRecord.stockMaster.High = high[0];
                        selectedRecord.stockMaster.Low = low[0];
                        selectedRecord.stockMaster.Volume = volume[0];
                        selectedRecord.stockMaster.Change = change[0];
                        selectedRecord.stockMaster.ChangePercent = changepercent[0];
                        selectedRecord.stockMaster.PrevClose = prevclose[0];
                        _context.StockMaster.Update(selectedRecord.stockMaster);

                        selectedRecord.CMP = close[0];
                        selectedRecord.VALUE = close[0] * selectedRecord.QUANTITY;
                        selectedRecord.GAIN_AMT = selectedRecord.VALUE - selectedRecord.TOTAL_COST;
                        selectedRecord.GAIN_PCT = (selectedRecord.GAIN_AMT / selectedRecord.VALUE) * 100;
                        _context.PORTFOLIOTXN.Update(selectedRecord);

                        //now find txn for same symbol in this portfolio
                        IQueryable<PORTFOLIOTXN> duplicateIQ = _context.PORTFOLIOTXN.Where(a => (a.PORTFOLIO_MASTER_ID == selectedRecord.PORTFOLIO_MASTER_ID)
                            && (a.stockMaster.Symbol == selectedRecord.stockMaster.Symbol)
                            && (a.PORTFOLIOTXN_ID != selectedRecord.PORTFOLIOTXN_ID));
                        foreach (var duplicateitem in duplicateIQ)
                        {
                            duplicateitem.CMP = close[0];
                            duplicateitem.VALUE = duplicateitem.QUANTITY * close[0];
                            duplicateitem.GAIN_AMT = duplicateitem.VALUE - duplicateitem.TOTAL_COST;
                            duplicateitem.GAIN_PCT = (duplicateitem.GAIN_AMT / duplicateitem.VALUE) * 100;

                            _context.PORTFOLIOTXN.Update(duplicateitem);
                        }
                        _context.SaveChanges();
                    }
                }
                if (((refreshAll != null) && (refreshAll == true)) || ((updateBuySell != null) && (updateBuySell == true)))
                {
                    DbInitializer.GetSMA_BUYSELL(_context, selectedRecord.stockMaster, 20, 50, 200);
                }
                if (((refreshAll != null) && (refreshAll == true)) || ((lifetimeHighLow != null) && (lifetimeHighLow == true)))
                {
                    DbInitializer.GetLifetimeHighLow(_context, selectedRecord.stockMaster);
                }
            }
        }
    }
}
