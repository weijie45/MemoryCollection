using Dapper.Contrib.Extensions;
using System;

namespace MemoriesCollection.Models
{
    [Table("ErrorLog")]
    public class ErrorLog
    {
        [Key]
        public int LogNo { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public DateTime LogDate { get; set; }
    }
}