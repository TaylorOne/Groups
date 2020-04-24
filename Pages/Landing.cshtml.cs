using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Groups.Areas.Identity.Data;
using Microsoft.Extensions.Configuration;
using Groups.Data;
using System.Web;
using System.Diagnostics;

namespace Groups.Pages
{
    [Authorize]
    public class LandingModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string _targetFilePath;

        public LandingModel(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _targetFilePath = config.GetValue<string>("StoredImagesPath");
        }

        [BindProperty]
        public Group Group { get; set; }

        [BindProperty]
        public ICollection<Post> Posts { get; set; }

        public ICollection<ApplicationUser> Members { get; set; }

        public ICollection<ApplicationUser> UsersToJoin { get; set; }

        [BindProperty]
        public Post Post { get; set; }
        public ApplicationUser AppUser { get; set; }

        public UsersToJoin UserToJoin = new UsersToJoin(); 

        public bool UserInGroup { get; set; }

        public bool UserWantsToJoin { get; set; }

        public bool UserIsAdmin { get; set; }


    public async Task<IActionResult> OnGetAsync(int? id)
        {
            if  (id == null)
            {
                return NotFound();
            }

            Group = await _context.Groups
                .Include(m => m.BannerImg)
                .FirstOrDefaultAsync(m => m.GroupId == id);
            Posts = await _context.Posts
                .Where(m => m.GroupId == id)
                .Include(m => m.User)
                .OrderByDescending(m => m.Id)
                .AsNoTracking()
                .ToListAsync();
            Members = await _context.UserGroups
                .Where(m => m.Group == Group)
                .Select(m => m.User)
                .ToListAsync();
            UsersToJoin = await _context.UsersToJoin
                .Where(m => m.Group == Group)
                .Select(m => m.User)
                .ToListAsync();

            // BannerImg = Path.Combine("http://localhost/images/", bImage.SafeFileName);
            // BannerImg = BannerImg.Replace("\\", "/");

            AppUser = await _userManager.GetUserAsync(User);

            UserInGroup = Members.Contains(AppUser);

            UserWantsToJoin = UsersToJoin.Contains(AppUser);

            if (Group == null)
            {
                return NotFound();
            }

            UserIsAdmin = Group.Admin == AppUser ? true : false;

            return Page();
        }

        public async Task<JsonResult> OnPostAsync(string url, string postString)
        {
            // get Group ID from url
            var urlIndex = url.IndexOf('=') + 1;
            int id = Int32.Parse(url.Substring(urlIndex));
            
            Post.User = await _userManager.GetUserAsync(User);
            Post.Group = await _context.Groups.FirstOrDefaultAsync(m => m.GroupId == id);
            Post.Content = HttpUtility.UrlDecode(postString);

            _context.Posts.Add(Post);
            await _context.SaveChangesAsync();

            var post = new Dictionary<string, string>()
            {
                { "User", Post.User.FullName },
                { "Content", Post.Content }
            };

            return new JsonResult(post);
        }

        public async Task<JsonResult> OnPostJoinGroupAsync(string groupID)
        {
            
            UserToJoin.GroupId = Int32.Parse(groupID);
            UserToJoin.UserId = _userManager.GetUserId(User);

            _context.UsersToJoin.Add(UserToJoin);
            await _context.SaveChangesAsync();

            return new JsonResult(groupID);
        }

        public string AddFilePath(string fileName)
        {
            return "http://localhost/images/" + fileName;
        }

        // http://localhost/images/
    }
}