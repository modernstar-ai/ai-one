using System.Security.Claims;
using Agile.Chat.Application.Assistants.Services;
using Agile.Chat.Application.Audits.Services;
using Agile.Chat.Application.ChatThreads.Services;
using Agile.Chat.Application.Files.Utils;
using Agile.Chat.Domain.Assistants.ValueObjects;
using Agile.Chat.Domain.Audits.Aggregates;
using Agile.Chat.Domain.ChatThreads.Aggregates;
using Agile.Chat.Domain.ChatThreads.Entities;
using Agile.Chat.Domain.ChatThreads.ValueObjects;
using Agile.Framework.AzureDocumentIntelligence;
using Agile.Framework.BlobStorage.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Agile.Chat.Application.ChatThreads.Commands;

public static class UploadFileToThreadById
{
    public record Command(Guid Id, IFormFile File) : IRequest<IResult>;

    public class Handler(ILogger<Handler> logger, 
        IAuditService<ChatThread> chatThreadAuditService, 
        IHttpContextAccessor contextAccessor, 
        IChatThreadService chatThreadService,
        IAssistantService assistantService,
        IChatThreadFileService chatThreadFileService,
        IBlobStorage blobStorage,
        IDocumentIntelligence documentIntelligence) : IRequestHandler<Command, IResult>
    {
        public async Task<IResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var username = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;
            logger.LogInformation("Fetched user: {Username}", username);
            if(string.IsNullOrWhiteSpace(username)) return Results.Forbid();
            
            logger.LogInformation("Getting Chat Thrread Id: {Id}", request.Id);
            var chatThread = await chatThreadService.GetItemByIdAsync(request.Id.ToString(), ChatType.Thread.ToString());
            if(chatThread is null) return Results.NotFound();
            if (!chatThread.UserId.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                return Results.Forbid();

            if (!string.IsNullOrWhiteSpace(chatThread.AssistantId))
            {
                var assistant = await assistantService.GetItemByIdAsync(chatThread.AssistantId);
                if (assistant != null && !assistant.FilterOptions.AllowInThreadFileUploads)
                    return Results.BadRequest("In thread file uploads are disabled for this assistant");
            }
            
            logger.LogInformation("Getting ChatThread files");
            var files = await chatThreadFileService.GetAllAsync(request.Id.ToString());
            if(files.Any(x => x.Name.Equals(request.File.FileName, StringComparison.InvariantCultureIgnoreCase)))
                return Results.BadRequest("File already exists");
            if(files.Count >= 5)
                return Results.BadRequest("Maximum number of files to upload to a chat thread is 5");

            string content = string.Empty;
            try
            {
                content = await documentIntelligence.CrackDocumentAsync(request.File.OpenReadStream(),
                    FileHelpers.ContainsText(request.File.FileName));
            }
            catch (Exception ex)
            {
                return Results.BadRequest($"File extraction failed: {ex.Message}");
            }
            
            var url = await blobStorage.UploadThreadFileAsync(request.File.OpenReadStream(), request.File.ContentType,
                request.File.FileName, request.Id.ToString());
            var threadFile = ChatThreadFile.Create(request.File.FileName, content, request.File.ContentType, request.File.Length, url, request.Id.ToString());
            //Updating chat thread to signify change in last modified
            await chatThreadFileService.UpdateItemByIdAsync(threadFile.Id, threadFile, ChatType.File.ToString());
            await Task.WhenAll(
                chatThreadAuditService.UpdateItemByPayloadIdAsync(chatThread), 
                chatThreadService.UpdateItemByIdAsync(chatThread.Id, chatThread, ChatType.Thread.ToString()));
            logger.LogInformation("Updated Chat thread Successfully with new file added: {@Thread}", chatThread);
            
            return Results.Ok();
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(ILogger<Validator> logger, IHttpContextAccessor contextAccessor)
        {
            
        }
    }
}