using MemoriesCollection.Models;
using System.Collections.Generic;

namespace MemoriesCollection.ViewModels
{
    public class PageTableViewModel
    {
        public bool IsData = false;
        public List<Dictionary<string, string>> Data { get; set; } //列表資料
        public IEnumerable<dynamic> SqlData { get; set; } //列表資料
        public string TagId { get; set; } // Tag PayID   
        public dynamic ViewBag { get; set; } //動態資料               
        public List<Photo> PhotoList { get; set; }
        public List<Album> AlbumList { get; set; }
        public List<VideoInfo> VideoList { get; set; }
        public List<Audio> AudioList { get; set; }
        public Album Album { get; set; }
        public List<ErrorLog> ErrLogList { get; set; }

    }
}