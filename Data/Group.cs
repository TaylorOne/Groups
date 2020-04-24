using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Groups.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Groups.Data
{
    public enum Privacy { Public, Private }

    public class Group
    {
        public int GroupId { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }
        public Privacy PrivacyType { get; set; }
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }
        [Display(Name = "Group description")]
        public string Description { get; set; }
        [Display(Name = "Group location (city, state)")]
        public string CityState { get; set; }
        public string AdminId { get; set; }
        public ApplicationUser Admin { get; set; }
        public int? BannerImgId { get; set; }
        [ForeignKey("BannerImgId")]
        public Image BannerImg { get; set; }
        public ICollection<UserGroup> UserGroups { get; set; }
        public ICollection<UsersToJoin> UserToJoinID { get; set; }
        public ICollection<Post> Posts { get; set; }
        public ICollection<Image> Images { get; set; }
        public ICollection<FileUpload> Files { get; set; }
        public ICollection<Event> Events { get; set; }
    }
}
