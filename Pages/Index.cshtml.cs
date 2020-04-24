using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Groups.Areas.Identity.Data;
using Groups.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Groups.Pages
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CityState { get; set; }
        public DateTime DOB { get; set; }

        public ApplicationUser AppUser { get; set; }
        public string UserId { get; set; }

        public List<Group> Groups = new List<Group>();
        public ICollection<Group> NewGroups { get; set; }

        public List<Group> GroupsUserWantsToJoin = new List<Group>();

        public UsersToJoin UsersToJoin = new UsersToJoin();

        public async Task<IActionResult> OnGetAsync()
        {

            if (_signInManager.IsSignedIn(User))
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

                } else
                {
                    UserId = _userManager.GetUserId(User);
                }

                // Get groups user is a member or admin of
                var userGroups = _context.UserGroups;
                IQueryable<Group> query = from u in userGroups
                                        where u.UserId.Contains(UserId)
                                        select u.Group;

                foreach (var item in query)
                {
                    Groups.Add(item);
                }

                // Get all groups from db
                NewGroups = await _context.Groups
                    .Include(m => m.BannerImg)
                    .ToListAsync();

                // Filter out groups user is already in 
                NewGroups = NewGroups
                    .Except(Groups)
                    .ToList();

                // Get groups user wants to join
                var usersToJoin = _context.UsersToJoin;
                query = from u in usersToJoin
                        where u.UserId.Contains(UserId)
                        select u.Group;

                foreach (var item in query)
                {
                    GroupsUserWantsToJoin.Add(item);
                }

                return Page();

            } else
            {
                return Page();
            }
        }

        public async Task<JsonResult> OnPostAsync(string gId)
        {
            UsersToJoin.GroupId = Int32.Parse(gId);
            UsersToJoin.UserId = _userManager.GetUserId(User);

            _context.UsersToJoin.Add(UsersToJoin);
            await _context.SaveChangesAsync();

            return new JsonResult(gId);
        }

        public string AddFilePath(string fileName)
        {
            return "http://localhost/images/" + fileName;
        }
    }
}
