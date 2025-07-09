using Agile.Framework.AzureAiSearch.Models;
using Agile.Framework.Common.EnvironmentVariables;

namespace Agile.Framework.AzureAiSearch;

public class SearchFilterBuilder
{
    private readonly List<string> _folderFilters = new();
    private readonly List<string> _tagFilters = new();
    private readonly string _containerName;
    private readonly string _indexName;

    public SearchFilterBuilder(string indexName)
    {
        _containerName = Constants.BlobIndexContainerName;
        _indexName = indexName;
    }

    public SearchFilterBuilder AddFolders(IEnumerable<string> folders)
    {
        if (folders == null) return this;

        foreach (var folder in folders.Where(f => !string.IsNullOrWhiteSpace(f)))
        {
            var cleanedFolder = CleanFolderPath(folder);
            var folderPattern = $"\"/{_containerName}/{_indexName}/{cleanedFolder}\"~";
            var folderFilter = $"search.ismatch('{folderPattern}', '{nameof(AzureSearchDocument.Url)}', null, 'any')";
            
            if (!_folderFilters.Contains(folderFilter))
            {
                _folderFilters.Add(folderFilter);
            }
        }
        
        return this;
    }

    public SearchFilterBuilder AddTags(IEnumerable<string> tags)
    {
        if (tags == null) return this;
        
        foreach (var tag in tags.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            var tagFilter = $"{nameof(AzureSearchDocument.Tags)}/any(t: t eq '{tag}')";
            
            if (!_tagFilters.Contains(tagFilter))
            {
                _tagFilters.Add(tagFilter);
            }
        }
        
        return this;
    }

    public string Build()
    {
        var hasFolderFilters = _folderFilters.Count > 0;
        var hasTagFilters = _tagFilters.Count > 0;
        
        if (!hasFolderFilters && !hasTagFilters)
        {
            return string.Empty;
        }
        
        var folderFilterGroup = hasFolderFilters ? $"({string.Join(" or ", _folderFilters)})" : string.Empty;
        var tagFilterGroup = hasTagFilters ? $"({string.Join(" or ", _tagFilters)})" : string.Empty;
        
        if (hasFolderFilters && hasTagFilters)
        {
            return $"{folderFilterGroup} and {tagFilterGroup}";
        }
        
        return folderFilterGroup + tagFilterGroup;
    }

    private static string CleanFolderPath(string folder)
    {
        folder = folder.Trim();
        
        if (folder.StartsWith("/"))
            folder = folder.TrimStart('/');
        if (!folder.EndsWith('/'))
            folder += '/';
        
        return folder;
    }
}