using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groups.Data
{
    public class Image
    {
        public int Id { get; set; }
        public string UntrustedFileName { get; set; }
        public string SafeFileName { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }
    }
}
