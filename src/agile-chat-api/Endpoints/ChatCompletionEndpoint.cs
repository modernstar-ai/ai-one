using Azure.Identity;
using Azure.AI.OpenAI;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenAI.Chat;
using System.ClientModel;
using Azure;
using DotNetEnv;
using static System.Net.WebRequestMethods;

    public static class ChatCompletionsEndpoint
    {
        public static void MapChatCompletionsEndpoint(this IEndpointRouteBuilder app)
        {
            // Map the GET endpoint for SSE-based streaming
            app.MapGet("/api/chatcompletions", async (HttpContext context, string prompt) =>
            {
                if (string.IsNullOrEmpty(prompt))
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Prompt is required.");
                    return;
                }

                // Set up the necessary headers for SSE
                context.Response.Headers.Add("Content-Type", "text/event-stream");
                context.Response.Headers.Add("Cache-Control", "no-cache");
                context.Response.Headers.Add("Connection", "keep-alive");

                // Initialize the AzureOpenAIClient with DefaultAzureCredential
                //AzureOpenAIClient azureClient = new AzureOpenAIClient(
                //    new Uri("https://your-azure-openai-resource.com"),
                //    new DefaultAzureCredential());

                //from doco here 
                //https://learn.microsoft.com/en-us/dotnet/api/overview/azure/ai.openai-readme?view=azure-dotnet-preview#create-client-with-a-microsoft-entra-credential
                var openAiEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
                var openAiApiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
                var openAiDeploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_DEPLOYMENT_NAME");

                if (string.IsNullOrEmpty(openAiEndpoint) || string.IsNullOrEmpty(openAiApiKey))
                {
                    Console.WriteLine("OpenAI endpoint or API key is not set in environment variables. EndPoint: {openAiEndpoint} ");
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("OpenAI endpoint or API key is not set.");
                    return;
                }
                AzureOpenAIClient azureClient = new(
                    new Uri(openAiEndpoint),
                    new AzureKeyCredential(openAiApiKey));
                

                // Get ChatClient using the deployment name
                ChatClient chatClient = azureClient.GetChatClient(openAiDeploymentName);

                // Prepare chat messages based on your prompt and initial context
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage("You are a helpful assistant that talks like a pirate."),
                    new UserChatMessage(prompt) // The user-provided prompt
                };

                // Get streaming completion updates
                CollectionResult<StreamingChatCompletionUpdate> completionUpdates = chatClient.CompleteChatStreaming(messages);

                // Stream responses as Server-Sent Events (SSE)
                foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
                {
                    foreach (ChatMessageContentPart contentPart in completionUpdate.ContentUpdate)
                    {
                        string content = contentPart.Text;
                        if (!string.IsNullOrEmpty(content))
                        {
                            //Console.WriteLine(content);
                            // Send each chunk of the response to the client as an SSE event
                            await context.Response.WriteAsync($"data: {content}\n\n");
                            await context.Response.Body.FlushAsync();
                        }
                    }
                }

                //not required when streaming. it will cause an error
                //return Results.Ok();
                return;
            });
        }
    }

