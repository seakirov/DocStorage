using System.Collections.Generic;

namespace DocStorage.Models
{
    public class DocumentsWithPages
    {
        public IEnumerable<Document> Documents { get; set; }
        public PageInfo PageInfo { get; set; }
    }
}