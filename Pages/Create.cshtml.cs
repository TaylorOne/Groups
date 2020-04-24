using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Groups.Areas.Identity.Data;
using Microsoft.Extensions.Configuration;
using Groups.Data;
using Microsoft.AspNetCore.Http;
using Groups.Utilities;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace Groups.Pages
{
    [Authorize]
    public class createModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly long _fileSizeLimit;
        private readonly string[] _permittedExtensions = { ".jpg", ".jpeg", ".png" };
        private readonly string _targetFilePath;

        public createModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration config)
        {   
            _context = context;
            _userManager = userManager;
            _fileSizeLimit = config.GetValue<long>("FileSizeLimit");
            _targetFilePath = config.GetValue<string>("StoredImagesPath");
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Group Group { get; set; }

        public UserGroup UserGroup = new UserGroup();

        [BindProperty]
        public IFormFile FileUpload { get; set; }

        private Image Image = new Image();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // first create group without banner image
            Group.Created = DateTime.Now;
            Group.Admin = await _userManager.GetUserAsync(User);

            _context.Groups.Add(Group);
            await _context.SaveChangesAsync();

            // after group saved, retrieve database-generated GroupId and add entry to UserGroup entity
            var groupID = (from g in _context.Groups
                           where g.Name == Group.Name && g.Admin == Group.Admin
                           select g.GroupId).First();

            UserGroup.GroupId = groupID;
            UserGroup.UserId = Group.AdminId;

            _context.UserGroups.Add(UserGroup);
            await _context.SaveChangesAsync();

            if (FileUpload == null)
            {
                // if user doesn't provide an image at time of group creation, add a default one
                Image.SafeFileName = "fovryhvh.jpg";
                Image.GroupId = groupID;

            } else
            {
                var formFileContent =
                await FileHelpers.ProcessFormFile(
                    FileUpload, ModelState, _permittedExtensions,
                    _fileSizeLimit);

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
                Image.GroupId = groupID;

                // store image in separate location
                using (var fileStream = System.IO.File.Create(filePath))
                {
                    await fileStream.WriteAsync(formFileContent);
                }
            }
            
            _context.Images.Add(Image);
            await _context.SaveChangesAsync();

            // after storing image data, get the image ID from db and store as group BannerImg
            Image.Id = (from g in _context.Images
                        where g.GroupId == groupID && g.SafeFileName == Image.SafeFileName
                        select g.Id).First();

            Group.BannerImg = Image;
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}