using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Groups.Data
{
    [Table("File")]
    public class FileUpload
    {
        public int Id { get; set; }
        public string UntrustedFileName { get; set; }
        public string SafeFileName { get; set; }
        public DateTime DateAdded { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }

    }
}
