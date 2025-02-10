using Agile.Framework.AzureAiSearch.Models;
using Azure.Search.Documents.Indexes.Models;

namespace Agile.Chat.Application.AiSearch.Dtos
{
    public class IndexReportDto
    {
        public SearchIndexStatistics? SearchIndexStatistics { get; set; }
    }
 
}
