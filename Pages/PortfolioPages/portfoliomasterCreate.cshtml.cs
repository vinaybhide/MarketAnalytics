using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MarketAnalytics.Data;
using MarketAnalytics.Models;
using System.Security.Policy;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MarketAnalytics.Pages.PortfolioPages
{

    [Authorize(Roles = "Registered, Administrator")]
    public class PortfolioMasterCreateModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        [BindProperty]
        public Portfolio_Master portfolioMaster { get; set; }
        [BindProperty]
        public bool FirstTimeMaster { get; set; }
        [BindProperty]
        public string UserName { get; set; }

        [BindProperty]
        public string UserId { get; set; }

        public bool bNameUnique = true;
        public string errorMessage = "Portfolio name exists! Please use unique portfolio name.";
        public PortfolioMasterCreateModel(MarketAnalytics.Data.DBContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            UserId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            UserName = httpContextAccessor.HttpContext.User.Identity.Name;
        }

        public IActionResult OnGet(bool? firsttimemaster)
        {
            portfolioMaster = new Portfolio_Master();
            FirstTimeMaster = (firsttimemaster == null)? false: (bool)firsttimemaster;

            return Page();
        }



        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (_context.PORTFOLIO_MASTER.FirstOrDefault(x => (x.PORTFOLIO_NAME.ToUpper() == portfolioMaster.PORTFOLIO_NAME.ToUpper())) == null)
            {

                portfolioMaster.Id = UserId;
                _context.PORTFOLIO_MASTER.Add(portfolioMaster);
                await _context.SaveChangesAsync();

                return RedirectToPage("./portfoliomasterIndex", new {masterid = portfolioMaster.PORTFOLIO_MASTER_ID, firsttimemaster=FirstTimeMaster});
            }
            else
            {
                bNameUnique = false;
                return Page();
            }
        }
    }
}
