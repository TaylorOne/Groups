using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Groups.Areas.Identity.Data;
using Groups.Data;
using Groups.Utilities;
using System.IO;

namespace Groups.Pages
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions = { ".jpg", ".jpeg", ".png" };
        private readonly string _targetFilePath;

        public EditModel(ApplicationDbContext context,
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
        public IFormFile FileUpload { get; set; }

        public List<string> ImagesOriginalFileName = new List<string>();

        private Image Image = new Image();

        public string Result { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Group = await _context.Groups
                .Include(m => m.Images)
                .FirstOrDefaultAsync(m => m.GroupId == id);

            if (Group == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                Result = "Please correct the form.";

                return Page();
            }

            // load group data from db
            Group = await _context.Groups
                .Include(m => m.Images)
                .FirstOrDefaultAsync(m => m.GroupId == id);

            if (Group == null)
            {
                return NotFound();
            }

            ImagesOriginalFileName = Group.Images.Select(m => m.UntrustedFileName).ToList();

            if (FileUpload == null)
            {

            } else
            {
                var formFileContent =
                await FileHelpers.ProcessFormFile(
                    FileUpload, ModelState, _permittedExtensions,
                    _fileSizeLimit, ImagesOriginalFileName);

                if (formFileContent.Length == 0)
                {
                    // the file user attempted to upload doesn't check out, so do nothing

                } else
                {
                    // For the file name of the uploaded file stored
                    // server-side, use Path.GetRandomFileName to generate a safe
                    // random file name, but change extension back to that of the original image, so it's usable
                    var trustedFileNameForFileStorage = Path.GetRandomFileName();
                    string extension = Path.GetExtension(FileUpload.FileName);
                    trustedFileNameForFileStorage = Path.ChangeExtension(trustedFileNameForFileStorage, extension);
                    var filePath = Path.Combine(
                        _targetFilePath, trustedFileNameForFileStorage);

                    // Get image data and store it in database
                    Image.UntrustedFileName = FileUpload.FileName;
                    Image.SafeFileName = trustedFileNameForFileStorage;
                    Image.GroupId = id;

                    // store image in separate location
                    using (var fileStream = System.IO.File.Create(filePath))
                    {
                        await fileStream.WriteAsync(formFileContent);
                    }

                    _context.Images.Add(Image);
                    await _context.SaveChangesAsync();

                    // after storing image data, get the image ID from db and store as group BannerImg
                    Image.Id = (from g in _context.Images
                                where g.GroupId == id && g.SafeFileName == Image.SafeFileName
                                select g.Id).First();

                    // store new group BannerImg
                    Group.BannerImg = Image;
                }
            }

            // update group data from form
            if (await TryUpdateModelAsync<Group>(
                Group,
                "group",
                m => m.Name, m => m.PrivacyType, m => m.Description, m => m.CityState))
            {
                await _context.SaveChangesAsync();
                return RedirectToPage("Landing",
                    new { id = id });
            }

            return Page();
        }
    }
}