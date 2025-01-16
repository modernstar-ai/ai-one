using Azure.Search.Documents.Indexes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile.Framework.AzureAiSearch.Models
{


    public class IndexerDetail
    {
        public string? Name { get; set; }
        public string? TargetIndex { get; set; }
        public string? DataSource { get; set; }
        public string? Schedule { get; set; }
        public DateTime? LastRunTime { get; set; }
        public int? DocumentsProcessed { get; set; }

        public string? Status { get; set; }
    }

    public class DataSourceDetail
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Container { get; set; }
    }
}
