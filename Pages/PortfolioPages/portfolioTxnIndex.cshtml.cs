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
            DateTime[] quoteDate = null;
            double[] open, high, low, close, volume, change, changepercent, prevclose = null;

            if ((_context.PORTFOLIOTXN != null) && (masterid != null))
            {
                MasterId = (int)masterid;
                var masterRec = await _context.PORTFOLIO_MASTER.FirstOrDefaultAsync(m => (m.PORTFOLIO_MASTER_ID == masterid));
                portfolioMasterName = masterRec.PORTFOLIO_NAME;
                symbolList.Clear();
                symbolList = _context.PORTFOLIOTXN.Where(x => x.PORTFOLIO_MASTER_ID == MasterId)
                                                        .OrderBy(a => a.stockMaster.Symbol)
                                                        .Distinct()
                                                        .Select(a =>
                                                            new SelectListItem
                                                            {
                                                                Value = a.StockMasterID.ToString(),
                                                                Text = a.stockMaster.Symbol
                                                            }
                                                        ).ToList();
                
                //_context.PORTFOLIOTXN.Include(x => x.PORTFOLIO_MASTER_ID);

                IQueryable<PORTFOLIOTXN> txnIQ = _context.PORTFOLIOTXN.Where(x => x.PORTFOLIO_MASTER_ID == masterid);

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
                    if (selectedRecord != null)
                    {
                        if (getQuote == true)
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
                                selectedRecord.stockMaster.Volume= volume[0];
                                selectedRecord.stockMaster.Change= change[0];
                                selectedRecord.stockMaster.ChangePercent = changepercent[0];
                                selectedRecord.stockMaster.PrevClose= prevclose[0];

                                selectedRecord.CMP = close[0];
                                selectedRecord.VALUE = close[0] * selectedRecord.QUANTITY;

                                selectedRecord.GAIN_AMT = selectedRecord.VALUE - selectedRecord.TOTAL_COST;
                                selectedRecord.GAIN_PCT = (selectedRecord.GAIN_AMT / selectedRecord.VALUE) * 100;

                                _context.StockMaster.Update(selectedRecord.stockMaster);
                                _context.PORTFOLIOTXN.Update(selectedRecord);
                                _context.SaveChanges();
                            }
                        }
                        if(updateBuySell == true)
                        {
                            DbInitializer.GetSMA_BUYSELL(_context, selectedRecord.stockMaster, selectedRecord.stockMaster.Symbol, 
                                selectedRecord.stockMaster.Exchange,
                                selectedRecord.StockMasterID, selectedRecord.stockMaster.CompName, 20, 50, 200);
                        }
                        if((lifetimeHighLow != null) && (lifetimeHighLow == true))
                        {
                            double lifetimehigh, lifetimelow = 0;
                            DbInitializer.GetLifetimeHighLow(_context, selectedRecord.stockMaster, out lifetimehigh, out lifetimelow);
                            selectedRecord.stockMaster.LIFETIME_HIGH = lifetimehigh;
                            selectedRecord.stockMaster.LIFETIME_LOW = lifetimelow;
                            _context.StockMaster.Update(selectedRecord.stockMaster);
                            _context.SaveChanges();
                        }

                        if (((getQuote == null) || (getQuote == false)) && ((updateBuySell == null) || (updateBuySell == false)) 
                            || ((lifetimeHighLow != null) || (lifetimeHighLow == false)))
                        {
                            searchString = selectedRecord.stockMaster.Symbol;
                        }
                    }
                }
                else if(string.IsNullOrEmpty(searchString))
                {
                    //IQueryable<PortfolioTxn> quoteIQ = from s in _context.PORTFOLIOTxn select s;

                    foreach (var item in txnIQ)
                    {
                        quoteDate = null;
                        open = high = low = close = volume = change = changepercent = prevclose = null;
                        DbInitializer.GetQuote(item.stockMaster.Symbol + ".NS", out quoteDate, out open, out high, out low, out close,
                                    out volume, out change, out changepercent, out prevclose);
                        if (quoteDate != null)
                        {
                            item.CMP = close[0];
                            item.VALUE = item.QUANTITY * close[0];
                            item.GAIN_AMT = item.VALUE - item.TOTAL_COST;
                            item.GAIN_PCT = (item.GAIN_AMT / item.VALUE) * 100;
                            _context.PORTFOLIOTXN.Update(item);
                        }

                        if (refreshAll == true)
                        {
                            DbInitializer.GetSMA_BUYSELL(_context, item.stockMaster, item.stockMaster.Symbol, item.stockMaster.Exchange, 
                                                        item.StockMasterID, item.stockMaster.CompName, 20, 50, 200);

                            double lifetimehigh, lifetimelow = 0;
                            DbInitializer.GetLifetimeHighLow(_context, item.stockMaster, out lifetimehigh, out lifetimelow);
                            item.stockMaster.LIFETIME_HIGH = lifetimehigh;
                            item.stockMaster.LIFETIME_LOW = lifetimelow;
                            _context.StockMaster.Update(item.stockMaster);
                        }
                    }
                    _context.SaveChanges();
                }

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

        public void RefreshAllStockMaster()
        {

        }

    }
}