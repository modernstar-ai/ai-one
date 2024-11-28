using Agile.Framework.Common.EnvironmentVariables;
using Azure.Search.Documents.Indexes.Models;

namespace Agile.Framework.AzureAiSearch.AiSearchConstants;

public static class DefaultSkillset
{
    public static SearchIndexerSkillset Create(string indexName)
    {
        var skillsetName = SearchConstants.SkillsetName(indexName);
        
        //OCR skill for pdf images
        var ocrSkill = new OcrSkill(new[]
        {
            new InputFieldMappingEntry("image")
            {
                Name = "image",
                Source = "/document/normalized_images/*",
                Inputs = { }
            }
        }, new[]
        {
            new OutputFieldMappingEntry("text")
            {
                TargetName = "text"
            }
        });
        ocrSkill.Name = "#1";
        ocrSkill.Context = "/document/normalized_images/*";
        ocrSkill.DefaultLanguageCode = "en";
        ocrSkill.ShouldDetectOrientation = true;
        ocrSkill.LineEnding = "Space";
        
        //Merge skill for merging the image texts with content texts
        var mergeSkill = new MergeSkill(new[]
        {
            new InputFieldMappingEntry("text")
            {
                Name = "text",
                Source = "/document/content",
                Inputs = { }
            },
            new InputFieldMappingEntry("itemsToInsert")
            {
                Name = "itemsToInsert",
                Source = "/document/normalized_images/*/text",
                Inputs = { }
            },
            new InputFieldMappingEntry("offsets")
            {
                Name = "offsets",
                Source = "/document/normalized_images/*/contentOffset",
                Inputs = { }
            }
        }, new[]
        {
            new OutputFieldMappingEntry("mergedText")
            {
                Name = "mergedText",
                TargetName = "mergedText"
            }
        });
        mergeSkill.Name = "#2";
        mergeSkill.Context = "/document";
        mergeSkill.InsertPreTag = " ";
        mergeSkill.InsertPostTag = " ";
        
        //Split skill for splitting up chunks of documents to embed
        var splitSkill = new SplitSkill(new[]
        {
            new InputFieldMappingEntry("text")
            {
                Name = "text",
                Source = "/document/mergedText",
                Inputs = { }
            }
        }, new[]
        {
            new OutputFieldMappingEntry("textItems")
            {
                Name = "textItems",
                TargetName = "pages"
            }
        });
        splitSkill.Name = "#3";
        splitSkill.Description = "Split skill to chunk documents";
        splitSkill.Context = "/document";
        splitSkill.DefaultLanguageCode = "en";
        splitSkill.TextSplitMode = TextSplitMode.Pages;
        splitSkill.MaximumPageLength = 2000;
        splitSkill.PageOverlapLength = 500;
        splitSkill.Unit = "characters";
        
        //Embedding skill to embed each chunk of text
        var embeddingSkill = new AzureOpenAIEmbeddingSkill(new[]
        {
            new InputFieldMappingEntry("text")
            {
                Name = "text",
                Source = "/document/pages/*",
                Inputs = { }
            }
        }, new[]
        {
            new OutputFieldMappingEntry("embedding")
            {
                Name = "embedding",
                TargetName = "text_vector"
            }
        });
        embeddingSkill.Name = "#4";
        embeddingSkill.Context = "/document/pages/*";
        embeddingSkill.ResourceUri = new Uri(Configs.AzureSearch.Endpoint);
        embeddingSkill.DeploymentName = Configs.AzureOpenAi.EmbeddingsDeploymentName;
        embeddingSkill.ModelName = Configs.AzureOpenAi.EmbeddingsModelName;
        embeddingSkill.ApiKey = Configs.AzureOpenAi.ApiKey;
        
        var skillsets = new List<SearchIndexerSkill>()
        {
            ocrSkill,
            mergeSkill,
            splitSkill,
            embeddingSkill
        };

        var skillset = new SearchIndexerSkillset(skillsetName, skillsets);
        skillset.Description = "Skillset to chunk documents and generate embeddings";
        skillset.CognitiveServicesAccount = new CognitiveServicesAccountKey(Configs.AzureAiServicesKey);
        skillset.IndexProjection = new SearchIndexerIndexProjection(new[]
        {
            new SearchIndexerIndexProjectionSelector(indexName, "parent_id", "/document/pages/*", new[]
            {
                new InputFieldMappingEntry("text_vector")
                {
                    Source = "/document/pages/*/text_vector"
                },
                new InputFieldMappingEntry("chunk")
                {
                    Source = "/document/pages/*"
                },
                new InputFieldMappingEntry("title")
                {
                    Source = "/document/title"
                },
                new InputFieldMappingEntry("metadata_storage_name")
                {
                    Source = "/document/metadata_storage_name"
                },
                new InputFieldMappingEntry("metadata_storage_path")
                {
                    Source = "/document/metadata_storage_path"
                }
            })
        });
        skillset.IndexProjection.Parameters = new SearchIndexerIndexProjectionsParameters
        {
            ProjectionMode = "skipIndexingParentDocuments"
        };

        return skillset;
    }
}