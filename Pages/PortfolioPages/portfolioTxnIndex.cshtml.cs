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

namespace MarketAnalytics.Pages.PortfolioPages
{
    public class PortfolioTxnIndex : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        private readonly IConfiguration Configuration;
        public List<SelectListItem> symbolList { get; set; }

        public PortfolioTxnIndex(MarketAnalytics.Data.DBContext context, IConfiguration configuration)
        {
            _context = context;
            Configuration = configuration;
            symbolList = new List<SelectListItem>();
        }

        public string DateSort { get; set; }
        public string ExchangeSort { get; set; }
        public string SymbolSort { get; set; }
        public string CompNameSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }

        public bool RefreshAllStocks { get; set; } = false;
        public PaginatedList<PORTFOLIOTXN> portfolioTxn { get; set; } = default!;

        public string portfolioMasterName { get; set; } = string.Empty;
        public double portfolioTotalCost { get; set; } = default(double);
        public double portfolioTotalGain { get; set; } = default(double);
        public double portfolioTotalValue { get; set; } = default(double);

        [BindProperty]
        public int MasterId { get; set; }
        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex, int? masterid,
            bool? refreshAll, bool? getQuote, int? stockid, bool? updateBuySell, bool? lifetimeHighLow)
        {
            //DateTime[] quoteDate = null;
            //double[] open, high, low, close, volume, change, changepercent, prevclose = null;

            if ((_context.PORTFOLIOTXN != null) && (masterid != null))
            {
                MasterId = (int)masterid;

                var masterRec = await _context.PORTFOLIO_MASTER.FirstOrDefaultAsync(m => (m.PORTFOLIO_MASTER_ID == masterid));
                portfolioMasterName = masterRec.PORTFOLIO_NAME;

                //_context.PORTFOLIOTXN.Include(x => x.PORTFOLIO_MASTER_ID);

                IQueryable<PORTFOLIOTXN> txnIQ = _context.PORTFOLIOTXN.Where(x => x.PORTFOLIO_MASTER_ID == masterid);
                IQueryable<PORTFOLIOTXN> distinctIQ = txnIQ.GroupBy(a => a.stockMaster.Symbol)
                                                            .Select(x => x.FirstOrDefault());
                symbolList.Clear();

                //symbolList = distinctIQ.Select(a =>
                //                                new SelectListItem
                //                                {
                //                                    Value = a.StockMasterID.ToString(),
                //                                    Text = a.stockMaster.Symbol
                //                                }).AsEnumerable().ToList();

                symbolList = _context.PORTFOLIOTXN.Where(x => x.PORTFOLIO_MASTER_ID == MasterId)
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
                    var selectedRecord = await _context.PORTFOLIOTXN.FirstOrDefaultAsync(m => ((m.PORTFOLIO_MASTER_ID == masterid) && (m.StockMasterID == stockid)));
                    DbInitializer.GetQuoteAndUpdateAllPortfolioTxn(_context, null, selectedRecord, getQuote, updateBuySell, lifetimeHighLow);
                    //await GetQuoteAndUpdate(stockid, masterid, refreshAll, getQuote, updateBuySell, lifetimeHighLow);
                    searchString = selectedRecord.stockMaster.Symbol;
                }
                //else if (string.IsNullOrEmpty(searchString))
                //{
                //    foreach (var item in distinctIQ)
                //    {
                //        DbInitializer.GetQuoteAndUpdateAllPortfolioTxn(_context, null, item, getQuote, updateBuySell, lifetimeHighLow);

                //        //await GetQuoteAndUpdate(item.StockMasterID, masterid, refreshAll, getQuote, updateBuySell, lifetimeHighLow);
                //        //await GetQuoteAndUpdate(item.StockMasterID, masterid, true, true, true, true);
                //    }
                //    txnIQ = _context.PORTFOLIOTXN.Where(x => x.PORTFOLIO_MASTER_ID == masterid);
                //}

                portfolioTotalCost = _context.PORTFOLIOTXN.Where(x => x.PORTFOLIO_MASTER_ID == MasterId)
                                                            .Sum(a => a.TOTAL_COST);
                portfolioTotalGain = (double)_context.PORTFOLIOTXN.Where(x => x.PORTFOLIO_MASTER_ID == MasterId)
                                                            .Sum(a => a.GAIN_AMT);
                portfolioTotalValue = (double)_context.PORTFOLIOTXN.Where(x => x.PORTFOLIO_MASTER_ID == MasterId)
                                                            .Sum(a => a.VALUE);

                CurrentFilter = searchString;

                //IQueryable<PortfolioTxn> portfolioIQ = from s in _context.PORTFOLIOTxn select s;

                if (!String.IsNullOrEmpty(searchString))
                {
                    txnIQ = txnIQ.Where(s => s.stockMaster.Symbol.ToUpper().Contains(searchString.ToUpper())
                                                || s.stockMaster.CompName.ToUpper().Contains(searchString.ToUpper()));
                    var searchRecord = txnIQ.First();
                    if (symbolList.Exists(a => (a.Value.Equals(searchRecord.StockMasterID.ToString()) == true)))
                    {
                        symbolList.FirstOrDefault(a => a.Value.Equals(searchRecord.StockMasterID.ToString())).Selected = true;
                    }

                }
                switch (sortOrder)
                {
                    case "date_desc":
                        txnIQ = txnIQ.OrderByDescending(s => s.PURCHASE_DATE);
                        break;
                    case "Symbol":
                        txnIQ = txnIQ.OrderBy(s => s.stockMaster.Symbol);
                        break;
                    case "symbol_desc":
                        txnIQ = txnIQ.OrderByDescending(s => s.stockMaster.Symbol);
                        break;
                    case "Exchange":
                        txnIQ = txnIQ.OrderBy(s => s.stockMaster.Exchange);
                        break;
                    case "exchange_desc":
                        txnIQ = txnIQ.OrderByDescending(s => s.stockMaster.Exchange);
                        break;
                    case "CompName":
                        txnIQ = txnIQ.OrderBy(s => s.stockMaster.CompName);
                        break;
                    case "compname_desc":
                        txnIQ = txnIQ.OrderByDescending(s => s.stockMaster.CompName);
                        break;
                    default:
                        txnIQ = txnIQ.OrderBy(s => s.PURCHASE_DATE);
                        break;
                }
                var pageSize = Configuration.GetValue("PageSize", 10);
                portfolioTxn = await PaginatedList<PORTFOLIOTXN>.CreateAsync(txnIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
            }
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
                    DbInitializer.GetQuote(selectedRecord.stockMaster.Symbol + "." + selectedRecord.stockMaster.Exchange, out quoteDate, out open,
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
                    DbInitializer.GetSMA_BUYSELL(_context, selectedRecord.stockMaster, selectedRecord.stockMaster.Symbol,
                        selectedRecord.stockMaster.Exchange,
                        selectedRecord.StockMasterID, selectedRecord.stockMaster.CompName, 20, 50, 200);
                }
                if (((refreshAll != null) && (refreshAll == true)) || ((lifetimeHighLow != null) && (lifetimeHighLow == true)))
                {
                    DbInitializer.GetLifetimeHighLow(_context, selectedRecord.stockMaster);
                }
            }
        }
    }
}
