using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Groups.Areas.Identity.Data;
using Groups.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using System.IO;

namespace Groups.Pages
{
    [Authorize]
    public class MembersModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MembersModel(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Group Group { get; set; }

        [BindProperty]
        public ICollection<ApplicationUser> Members { get; set; }

        [BindProperty]
        public ICollection<ApplicationUser> PotentialMembers { get; set; }

        public UsersToJoin UserJoined = new UsersToJoin();

        public UserGroup NewMember = new UserGroup();

        public bool UserIsAdmin { get; set; }

        public bool UserInGroup { get; set; }

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
            Members = await _context.UserGroups
                .Where(m => m.Group == Group)
                .Select(m => m.User)
                .ToListAsync();
            PotentialMembers = await _context.UsersToJoin
                .Where(m => m.Group == Group)
                .Select(m => m.User)
                .ToListAsync();

            // BannerImg = Path.Combine("http://localhost/images/", ImageInfo.SafeFileName);
            // BannerImg = BannerImg.Replace("\\", "/");

            if (Group == null)
            {
                return NotFound();
            }

            UserIsAdmin = Group.Admin == user ? true : false;

            UserInGroup = Members.Contains(user);

            return Page();
        }

        public async Task<JsonResult> OnPostAsync(string UserId, int GroupId)
        {
            var NewMemberId = await _context.UsersToJoin
                .Where(m => m.UserId.Contains(UserId))
                .Select(m => m.UserId)
                .FirstOrDefaultAsync();

            // get new member from temporary group
            UserJoined.GroupId = GroupId;
            UserJoined.UserId = NewMemberId;

            // get new member ready to join group officially
            NewMember.GroupId = GroupId;
            NewMember.UserId = NewMemberId;

            // remove new member from temp group and add her to group
            _context.UsersToJoin.Remove(UserJoined);
            _context.UserGroups.Add(NewMember);
            await _context.SaveChangesAsync();

            return new JsonResult(NewMemberId);
        }

        public string AddFilePath(string fileName)
        {
            return "http://localhost/images/" + fileName;
        }
    }
}