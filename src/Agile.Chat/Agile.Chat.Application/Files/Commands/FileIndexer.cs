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
using Agile.Framework.AzureDocumentIntelligence.Converters;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Agile.Chat.Application.Files.Commands;

public static class FileIndexer
{
    public record Command(string FileName, string IndexName, string FolderName, EventGridHelpers.FileMetadata FileMetadata, EventGridHelpers.Type EventType) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, 
        IAppKernel appKernel,
        IFileService fileService, 
        IIndexService indexService, 
        IAzureAiSearch azureAiSearch, 
        IMediator mediator, 
        IDocumentIntelligence documentIntelligence) : IRequestHandler<Command, IResult>
    {
        private AsyncRetryPolicy _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(6, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); // Exponential backoff 
        private bool _indexExists;
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            await HandleIndexSyncing(request.IndexName, request.EventType);
            _indexExists = await azureAiSearch.IndexExistsAsync(request.IndexName);

            var file = await HandleFileSyncing(request.EventType, request.FileName, request.IndexName,
                request.FolderName, request.FileMetadata);
            if (file is null) return Results.Ok();
            
            try
            {
                if (request.EventType == EventGridHelpers.Type.BlobDeleted)
                    return await HandleFileDeletionAsync(file);

                return await HandleFileUpdatingAsync(file);
            }
            catch (Exception)
            {
                file.Update(FileStatus.Failed, file.ContentType, file.Size);
                await fileService.UpdateItemByIdAsync(file.Id, file);
                throw;
            }
        }

        private async Task<IResult> HandleFileDeletionAsync(CosmosFile file)
        {
            if(_indexExists) await azureAiSearch.DeleteFileContentsByIdAsync(file.Id, file.IndexName);
            await fileService.DeleteItemByIdAsync(file.Id);
            return Results.Ok();
        }

        private async Task<IResult> HandleFileUpdatingAsync(CosmosFile file)
        {
            var command = new DownloadFileByUrl.Command(file.Url);
            var result = await mediator.Send(command);

            if (result is not FileStreamHttpResult okResult)
                return result;

            var document = FileHelpers.HasCustomConverter(file.Url, out var converter)
                ? await converter.ExtractDocumentAsync(okResult.FileStream)
                : await documentIntelligence.CrackDocumentAsync(okResult.FileStream,
                    FileHelpers.TextFormats.Contains(okResult.ContentType));
            var chunks = documentIntelligence.ChunkDocumentWithOverlap(document).Where(chunk => !string.IsNullOrWhiteSpace(chunk)).ToList();
            
            var embeddings = await _retryPolicy.ExecuteAsync(async _ => await appKernel.GenerateEmbeddingsAsync([..chunks, file.Name]), new CancellationToken());
            var nameEmbedding = embeddings.Last();
            var documents = chunks
                .Select((chunk, index) => AzureSearchDocument.Create(file.Id, chunk, file.Name, file.Url, embeddings[index], nameEmbedding))
                .ToList();
            
            if(_indexExists) await azureAiSearch.DeleteFileContentsByIdAsync(file.Id, file.IndexName);
            if(_indexExists) await azureAiSearch.IndexDocumentsAsync(documents, file.IndexName);
            file.Update(FileStatus.Indexed, okResult.ContentType, okResult.FileStream.Position);
            await fileService.UpdateItemByIdAsync(file.Id, file);
            return Results.Ok();
        }
        
        private async Task<CosmosFile?> HandleFileSyncing(EventGridHelpers.Type eventType, string fileName, string indexName, string folderName, EventGridHelpers.FileMetadata fileMetadata)
        {
            //In the case of blob creating, ensure it's created in cosmos
            if (eventType == EventGridHelpers.Type.BlobCreated && !await fileService.ExistsAsync(fileName, indexName, folderName))
            {
                var file = CosmosFile.Create(fileName, 
                    fileMetadata.BlobUrl, 
                    fileMetadata.ContentType,
                    fileMetadata.ContentLength, 
                    indexName, 
                    folderName);
                file.Update(FileStatus.Indexing, file.ContentType, file.Size);
                
                await fileService.AddItemAsync(file);
                return file;
            }
            else
            {
                var file = await fileService.GetFileByFolderAsync(fileName, indexName, folderName);
                if (file != null)
                {
                    file.Update(eventType == EventGridHelpers.Type.BlobDeleted ? FileStatus.Deleting : FileStatus.Indexing, file.ContentType, file.Size);
                    await fileService.UpdateItemByIdAsync(file.Id, file);
                }
                return file;
            }
        }

        private async Task HandleIndexSyncing(string indexName, EventGridHelpers.Type eventType)
        {
            if (!indexService.Exists(indexName) && eventType != EventGridHelpers.Type.BlobDeleted)
            {
                if(!await azureAiSearch.IndexExistsAsync(indexName))
                    await azureAiSearch.CreateIndexIfNotExistsAsync(indexName);
                
                var index = CosmosIndex.Create(indexName, indexName, null);
                await indexService.AddItemAsync(index);
            }
        }
    }
}