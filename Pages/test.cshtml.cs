using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Groups.Pages
{
    public class testModel : PageModel
    {

        [BindProperty]
        public IFormFile FileUpload { get; set; }

        public void OnGet()
        {
        }

        public async Task<JsonResult> OnPostAsync(IFormFile file)
        {
            if (file == null)
            {
                Debug.WriteLine("WELL, THIS SUCKS!!");
            }

            /*
            byte[] newFile;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);

                newFile = memoryStream.ToArray(); 
            }

            // store image in separate location
            using (var fileStream = System.IO.File.Create("c:\\Users\\jordo\\source\\repos\\GroupAppFiles"))
            {
                await fileStream.WriteAsync(newFile);
            }
            */

            foreach (var item in file.Headers)
            {
                Debug.WriteLine(item.Key + ": " + item.Value);
            }

            Debug.WriteLine(file.Headers["Content-Disposition"]);
            Debug.WriteLine(file.FileName);

            var post = new Dictionary<string, string>()
            {
                { "url", "splah"},
                { "fileData", file.FileName }
            };

            return new JsonResult(post);
        }
    }
}