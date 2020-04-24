using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Groups.Data;


namespace Groups.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {

        [PersonalData]
        public string FirstName { get; set; }
        [PersonalData]
        public string LastName { get; set; }

        public string FullName
        {
            get => FirstName + " " + LastName;
        }

        [PersonalData]
        public DateTime DOB { get; set; }
        [PersonalData]
        public string CityState { get; set; }
        public ICollection<UserGroup> UserGroups { get; set; }
        public ICollection<Post> Posts { get; set; }

    }

}
