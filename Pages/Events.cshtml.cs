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
using System.Globalization;

namespace Groups.Pages
{
    [Authorize]
    public class EventsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EventsModel(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [BindProperty]
        public Group Group { get; set; }

        [BindProperty]
        public ICollection<ApplicationUser> Members { get; set; }

        public Event Event = new Event();
        public bool UserInGroup { get; set; }
        public bool UserIsAdmin { get; set; }

        public string dateToday = DateTime.Now.ToString("yyyy-MM-dd");

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            Group = await _context.Groups
                .Include(m => m.BannerImg)
                .Include(m => m.Events)
                .FirstOrDefaultAsync(m => m.GroupId == id);
            Members = await _context.UserGroups
                .Where(m => m.Group == Group)
                .Select(m => m.User)
                .ToListAsync();

            if (Group == null)
            {
                return NotFound();
            }

            UserInGroup = Members.Contains(user);

            UserIsAdmin = Group.Admin == user ? true : false;

            return Page();
        }

        public async Task<JsonResult> OnPostAsync(
            string name, string description, string location, string dateStart,
            string timeStart, string dateEnd, string timeEnd, string url)
        {

            //get GroupId from page url
            var urlIndex = url.IndexOf('=') + 1;
            Event.GroupId = Int32.Parse(url.Substring(urlIndex));

            Event.Name = name;
            Event.Description = description;
            Event.Location = location;
            Event.Start = DateTime.Parse(dateStart + "T" + timeStart);
            Event.End = DateTime.Parse(dateEnd + "T" + timeEnd);

            _context.Events.Add(Event);
            await _context.SaveChangesAsync();

            DateTimeFormatInfo dtfi = CultureInfo.GetCultureInfo("en-US").DateTimeFormat;

            var eventInfo = new Dictionary<string, string>()
            {
                { "name", Event.Name },
                { "desc", Event.Description },
                { "loc", Event.Location },
                { "month", dtfi.GetMonthName(Event.Start.Month) },
                { "day", Event.Start.Day.ToString() },
                { "startT", Event.Start.ToShortTimeString() },
                { "endT", Event.End.ToShortTimeString() }
            };

            return new JsonResult(eventInfo);
        }

        public string AddFilePath(string fileName)
        {
            return "http://localhost/images/" + fileName;
        }

        public string GetMonthName(int MoNum)
        {
            DateTimeFormatInfo dtfi = CultureInfo.GetCultureInfo("en-US").DateTimeFormat;
            return dtfi.GetMonthName(MoNum);
        }

        public string GetTime(DateTime date)
        {
            return date.ToString("t",
                  CultureInfo.CreateSpecificCulture("en-us"));
        }
    }
}