using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace MemoriesCollection.Models
{
    [Table("Album")]
    public class Album
    {
        [Key]
        public int AlbumNo { get; set; }

        public string AlbumName { get; set; }

        public string AlbumDesc { get; set; }

        public string PassWord { get; set; }
        public string ImgNo { get; set; }
        //public string VideoNo { get; set; }
        public string BgImg { get; set; }
        public DateTime CreateDateTime { get; set; }

        public DateTime ModifyDateTime { get; set; }
    }
}