using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile.Framework.AzureAiSearch.Models
{

    public class IndexReport
    {
        public string? Name { get; set; }
        public long? DocumentCount { get; set; }
        public string? StorageSize { get; set; }
        public string? VectorIndexSize { get; set; }
        public int? ReplicasCount { get; set; }
        public string? LastRefreshTime { get; set; }
        public string? Status { get; set; }
        public List<IndexerDetail>? Indexers { get; set; }
        public List<DataSourceDetail>? DataSources { get; set; }
    }

    public class IndexerDetail
    {
        public string? Name { get; set; }
        public string? TargetIndex { get; set; }
        public string? DataSource { get; set; }
        public string? Schedule { get; set; }
        public string? LastRunTime { get; set; }
        public string? NextRunTime { get; set; }
        public string? DocumentsProcessed { get; set; }
        public string? Status { get; set; }
    }

    public class DataSourceDetail
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Container { get; set; }
        public string? ConnectionStatus { get; set; }
    }
}
