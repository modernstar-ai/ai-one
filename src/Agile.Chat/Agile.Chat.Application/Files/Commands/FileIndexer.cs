using Agile.Chat.Application.Files.Services;
using Agile.Chat.Application.Files.Utils;
using Agile.Chat.Application.Indexes.Services;
using Agile.Chat.Domain.Files.Aggregates;
using Agile.Chat.Domain.Files.ValueObjects;
using Agile.Chat.Domain.Indexes.Aggregates;
using Agile.Framework.Ai;
using Agile.Framework.AzureAiSearch;
using Agile.Framework.AzureAiSearch.Models;
using Agile.Framework.AzureDocumentIntelligence;
using Agile.Framework.Common.EnvironmentVariables;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Polly;

namespace Agile.Chat.Application.Files.Commands;

public static class FileIndexer
{
    public record Command(string FileName, string IndexName, string FolderName, EventGridHelpers.FileMetadata FileMetadata, EventGridHelpers.Type EventType) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger,
        IFileService fileService,
        IIndexService indexService,
        IAzureAiSearch azureAiSearch,
        IAppKernelBuilder appKernelBuilder,
        IMediator mediator,
        IDocumentIntelligence documentIntelligence) : IRequestHandler<Command, IResult>
    {
        private bool _indexExists;
        private IAppKernel _appKernel = null!;

        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            AddSemanticKernel();

            var index = await HandleIndexSyncing(request.IndexName, request.EventType);
            _indexExists = await azureAiSearch.IndexExistsAsync(request.IndexName);

            var file = await HandleFileSyncing(request.EventType, request.FileName, request.IndexName,
                request.FolderName, request.FileMetadata);
            if (file is null) return Results.Ok();

            try
            {
                if (request.EventType == EventGridHelpers.Type.BlobDeleted)
                    return await HandleFileDeletionAsync(file);

                return await HandleFileUpdatingAsync(file, index, request.FileMetadata);
            }
            catch (Exception)
            {
                file.Update(FileStatus.Failed, request.FileMetadata.BlobUrl, file.ContentType, file.Size, file.Tags);
                await fileService.UpdateItemByIdAsync(file.Id, file);
                throw;
            }
        }

        private async Task<IResult> HandleFileDeletionAsync(CosmosFile file)
        {
            if (_indexExists)
            {
                await azureAiSearch.DeleteFileContentsByIdAsync(file.Id, file.IndexName);
                logger.LogInformation("Deleted all chunks from Azure AI Search for file: {FileName} successfuly", file.Name);
            }
            await fileService.DeleteItemByIdAsync(file.Id);
            logger.LogInformation("Deleted file: {FileName} from CosmosDb", file.Name);
            return Results.Ok();
        }

        private async Task<IResult> HandleFileUpdatingAsync(CosmosFile file, CosmosIndex? index, EventGridHelpers.FileMetadata fileMetadata)
        {
            var command = new DownloadFileByUrl.Command(file.Url);
            var result = await mediator.Send(command);

            if (result is not FileStreamHttpResult okResult)
                return result;

            var fileStream = FileHelpers.HasCustomConverter(file.Url, out var converter)
                ? await converter.ConvertDocumentAsync(okResult.FileStream)
                : okResult.FileStream;

            logger.LogInformation("Downloaded file: {FileName} with length {Length}", file.Name, okResult.FileLength);
            var document = FileHelpers.HasCustomExtractor(file.Url, out var extractor)
                ? await extractor.ExtractTextAsync(fileStream)
                : await documentIntelligence.CrackDocumentAsync(fileStream,
                    FileHelpers.ContainsText(file.Name));
            logger.LogInformation("Extracted document contents string length: {Length}", document.Length);
            var chunks = documentIntelligence.ChunkDocumentWithOverlap(document, index?.ChunkSize, index?.ChunkOverlap)
                .Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
            logger.LogInformation("Chunked {Count} documents with Chunk Size {ChunkSize} and overlap {Overlap}", chunks.Count, index?.ChunkSize, index?.ChunkOverlap);

            var embeddings = await GenerateEmbeddingsForChunksAsync([.. chunks, file.Name]);
            var nameEmbedding = embeddings.Last();
            var documents = chunks
                .Select((chunk, index) => AzureSearchDocument.Create(file.Id, chunk, file.Name, file.Url, file.Tags, embeddings[index], nameEmbedding))
                .ToList();

            if (_indexExists)
            {
                await azureAiSearch.DeleteFileContentsByIdAsync(file.Id, file.IndexName);
                logger.LogInformation("Deleted existing chunks in Azure AI Search for file: {FileName} in index {IndexName}", file.Name, file.IndexName);
            }

            if (_indexExists)
            {
                await azureAiSearch.IndexDocumentsAsync(documents, file.IndexName);
                logger.LogInformation("Indexed new chunks in Azure AI Search for file: {FileName} in index {IndexName}", file.Name, file.IndexName);
            }
            file.Update(FileStatus.Indexed, fileMetadata.BlobUrl, okResult.ContentType, okResult.FileStream.Position, file.Tags);
            await fileService.UpdateItemByIdAsync(file.Id, file);
            logger.LogInformation("File status updated. Finished indexing file: {FileName}", file.Name);
            return Results.Ok();
        }

        private async Task<List<ReadOnlyMemory<float>>> GenerateEmbeddingsForChunksAsync(List<string> chunks)
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, _ =>
                    TimeSpan.FromSeconds(60));

            int skip = 0;
            int take = 5;

            var embeddings = new List<ReadOnlyMemory<float>>();

            while (true)
            {
                var batch = chunks.Skip(skip).Take(take).ToList();
                if (batch.Count == 0) break;

                var batchEmbeddings = await retryPolicy.ExecuteAsync(async _ => await _appKernel.GenerateEmbeddingsAsync(batch), new CancellationToken());
                embeddings.AddRange(batchEmbeddings.ToList());
                logger.LogInformation("Embedding status: {Count}/{Total}", embeddings.Count, chunks.Count);
                skip += take;
            }

            logger.LogInformation("Finished embeddings with total embeddings count: {Count}", embeddings.Count);
            return embeddings;
        }

        private async Task<CosmosFile?> HandleFileSyncing(EventGridHelpers.Type eventType, string fileName, string indexName, string folderName, EventGridHelpers.FileMetadata fileMetadata)
        {
            //In the case of blob creating, ensure it's created in cosmos
            if (eventType == EventGridHelpers.Type.BlobCreated && !await fileService.ExistsAsync(fileName, indexName, folderName))
            {
                var file = CosmosFile.Create(fileName,
                    fileMetadata.ContentType,
                    fileMetadata.ContentLength,
                    indexName,
                    folderName,
                    new List<string>());
                file.Update(FileStatus.Indexing, fileMetadata.BlobUrl, file.ContentType, file.Size, file.Tags);

                await fileService.AddItemAsync(file);
                logger.LogInformation("Added new file name: {FileName} in CosmosDb", file.Name);
                return file;
            }
            else
            {
                var file = await fileService.GetFileByFolderAsync(fileName, indexName, folderName);
                if (file != null)
                {
                    file.Update(eventType == EventGridHelpers.Type.BlobDeleted ? FileStatus.Deleting : FileStatus.Indexing, fileMetadata.BlobUrl, file.ContentType, file.Size, file.Tags);
                    await fileService.UpdateItemByIdAsync(file.Id, file);
                    logger.LogInformation("Updated existing file name: {FileName} in CosmosDb", file.Name);
                }
                return file;
            }
        }

        private async Task<CosmosIndex?> HandleIndexSyncing(string indexName, EventGridHelpers.Type eventType)
        {
            var indexExists = indexService.Exists(indexName);
            if (!indexExists && eventType != EventGridHelpers.Type.BlobDeleted)
            {
                if (!await azureAiSearch.IndexExistsAsync(indexName))
                {
                    logger.LogInformation("Creating new index {IndexName} in Azure AI Search", indexName);
                    await azureAiSearch.CreateIndexIfNotExistsAsync(indexName);
                }

                var index = CosmosIndex.Create(indexName, indexName, 2300, 25, null, null);
                await indexService.AddItemAsync(index);
                logger.LogInformation("Added new index {IndexName} in CosmosDb", indexName);
                return index;
            }
            else if (indexExists)
            {
                var index = indexService.GetByName(indexName);
                logger.LogInformation("Fetched existing index name: {IndexName} in CosmosDb", index?.Name);
                return index;
            }
            return null;
        }

        private void AddSemanticKernel()
        {
            var configs = Configs.AzureOpenAi;
            var chatEndpoint = !string.IsNullOrWhiteSpace(configs.Apim.Endpoint) ? configs.Apim.Endpoint : configs.Endpoint;
            var embeddingsEndpoint = !string.IsNullOrWhiteSpace(configs.Apim.EmbeddingsEndpoint) ? configs.Apim.EmbeddingsEndpoint : configs.Endpoint;
            appKernelBuilder.AddAzureOpenAIChatCompletion(configs.DeploymentName);
            appKernelBuilder.AddAzureOpenAITextEmbeddingGeneration(configs.EmbeddingsDeploymentName!);
            _appKernel = appKernelBuilder.Build();
        }
    }
}