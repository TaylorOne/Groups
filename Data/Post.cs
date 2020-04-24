using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Groups.Areas.Identity.Data;

namespace Groups.Data
{
    public class Post
    {
        public int Id { get; set; }
        public string Content { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }
    }
}
