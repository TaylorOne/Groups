using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Groups.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using Groups.Utilities;
using Groups.Data;

namespace Groups.Pages
{
    [Authorize]
    public class PhotosModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions = { ".jpg", ".jpeg", ".png" };
        private readonly string _targetFilePath;

        public PhotosModel(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration config)
        {
            _context = context;
            _userManager = userManager;
            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");
            _targetFilePath = config.GetValue<string>("StoredImagesPath");

        }

        [BindProperty]
        public Group Group { get; set; }

        [BindProperty]
        public ICollection<ApplicationUser> Members { get; set; }

        public Image Image = new Image();

        public bool UserInGroup { get; set; }

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
                .Include(m => m.Images)
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

        public async Task<JsonResult> OnPostAsync(IFormCollection FormC)
        {
            var data = new Dictionary<string, string>();

            if (FormC == null)
            {
                
            }
            else
            {
                // get Group ID from url
                var urlIndex = FormC["url"].ToString().IndexOf('=') + 1; ;
                int GroupId = Int32.Parse(FormC["url"].ToString().Substring(urlIndex));

                IFormFile fileUpload = FormC.Files.FirstOrDefault();
                Debug.WriteLine(fileUpload.FileName);

                var formFileContent =
                await FileHelpers.ProcessFormFile(
                    fileUpload, ModelState, _permittedExtensions,
                    _fileSizeLimit);

                Debug.WriteLine(formFileContent);

                if (formFileContent.Length == 0)
                {
                    // the file user attempted to upload doesn't check out, so do nothing
                    data.Add("empty", "empty");
                }
                else
                {
                    // For the file name of the uploaded file stored
                    // server-side, use Path.GetRandomFileName to generate a safe
                    // random file name, but change extension back to that of the original image, so it's usable
                    var trustedFileNameForFileStorage = Path.GetRandomFileName();
                    string extension = Path.GetExtension(fileUpload.FileName);
                    trustedFileNameForFileStorage = Path.ChangeExtension(trustedFileNameForFileStorage, extension);
                    var filePath = Path.Combine(
                        _targetFilePath, trustedFileNameForFileStorage);

                    // store image in separate location
                    using (var fileStream = System.IO.File.Create(filePath))
                    {
                        await fileStream.WriteAsync(formFileContent);
                    }

                    Image.SafeFileName = trustedFileNameForFileStorage;
                    Image.GroupId = GroupId;
                    Image.UntrustedFileName = fileUpload.FileName;

                    _context.Images.Add(Image);
                    await _context.SaveChangesAsync();

                    data.Add("imgSafeName", Image.SafeFileName);
                    data.Add("imgUnsafeName", Image.UntrustedFileName);
                }

            }

            return new JsonResult(data);
        }

        public string AddFilePath(string fileName)
        {
            return "../GroupAppImages/" + fileName;
        }

        // http://localhost/images/
    }
}