using MarketAnalytics.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MarketAnalytics.Pages.UserManager
{
    public class UserManagerCreateModel : PageModel
    {
        private readonly MarketAnalytics.Data.DBContext _context;
        [BindProperty]
        public UserMaster userMaster { get; set; }
        [BindProperty]
        public int? requestedUserType { get; set; }

        public bool bNameUnique = true;
        public string errorMessage = "User name exists! Please enter different user name.";
        public UserManagerCreateModel(MarketAnalytics.Data.DBContext context)
        {
            _context = context;
        }
        public IActionResult OnGet(int? requestedby)
        {
            userMaster = new UserMaster();
            requestedUserType = requestedby;
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (_context.UserMasters.FirstOrDefault(x => (x.USER_ID.ToUpper() == userMaster.USER_ID.ToUpper())) == null)
            {

                _context.UserMasters.Add(userMaster);
                await _context.SaveChangesAsync();

                return RedirectToPage("/PortfolioPages/portfoliomasterIndex");
            }
            else
            {
                bNameUnique = false;
                return Page();
            }
        }
    }
}
