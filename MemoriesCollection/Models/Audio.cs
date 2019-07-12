using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MemoriesCollection.Models
{
    [Table("Audio")]
    public class Audio
    {
        [Key]
        public int AudioNo { get; set; }

        public string FileName { get; set; }
        public string FileExt { get; set; }
        public int Size { get; set; }
        public DateTime OrgCreateDateTime { get; set; }
        public DateTime OrgModifyDateTime { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime ModifyDateTime { get; set; }
    }
}