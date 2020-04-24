using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Groups.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Groups.Data;
using Microsoft.AspNetCore.Authorization;

namespace Groups.Pages
{
    [Authorize]
    public class AboutModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AboutModel(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Group Group { get; set; }

        public bool UserIsAdmin { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            Group = await _context.Groups
                .Include(m => m.BannerImg)
                .FirstOrDefaultAsync(m => m.GroupId == id);

            if (Group == null)
            {
                return NotFound();
            }

            UserIsAdmin = Group.Admin == user ? true : false;

            return Page();
        }

        public string AddFilePath(string fileName)
        {
            return "http://localhost/images/" + fileName;
        }
    }
}