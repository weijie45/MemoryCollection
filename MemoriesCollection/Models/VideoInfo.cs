using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MemoriesCollection.Models
{
    [Table("Video")]
    public class VideoInfo
    {
        [Key]
        public int VideoNo { get; set; }

        public string FileName { get; set; }
        public string FileExt { get; set; }
        public string FileDesc { get; set; }
        public int Size { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Location { get; set; }
        public string Person { get; set; }
        public string IsRotate { get; set; } = "N";
        public DateTime OrgCreateDateTime { get; set; }
        public DateTime OrgModifyDateTime { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime ModifyDateTime { get; set; }
    }
}