using Agile.Framework.AzureAiSearch.Models;
using Azure.Search.Documents.Indexes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agile.Chat.Application.AiSearch.Dtos
{
    public class IndexReportDto
    {
        public SearchIndexStatistics? SearchIndexStatistics { get; set; }
        public List<IndexerDetail>? Indexers { get; set; }
        public List<DataSourceDetail>? DataSources { get; set; }
    }
 
}
