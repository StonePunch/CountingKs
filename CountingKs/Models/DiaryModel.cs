using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CountingKs.Models
{
    public class DiaryModel
    {
        public ICollection<LinkModel> Links { get; set; }
        public DateTime CurrentDate { get; set; }
        public IEnumerable<DiaryEntryModel> Entries { get; set; }
    }
}