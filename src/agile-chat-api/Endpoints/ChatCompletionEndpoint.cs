using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using Azure;

public static class ChatCompletionsEndpoint
{
    public class JsonChatMessage
    {
        public string text { get; set; }
        public string sender { get; set; }
    }

    public static void MapChatCompletionsEndpoint(this IEndpointRouteBuilder app)
    {
        
        app.MapPost("/chatcompletions", async (HttpContext context) =>
        {

            // Deserialize the incoming JSON payload into a list of ChatMessage objects
            List<JsonChatMessage>? messages = new List<JsonChatMessage>();
            try
            {
                // using var reader = new StreamReader(context.Request.Body);
                // var rawJson = await reader.ReadToEndAsync();
                // Console.WriteLine(rawJson);
                
                messages = await context.Request.ReadFromJsonAsync<List<JsonChatMessage>>();
                Console.WriteLine("messages: " + messages);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            if (messages == null || !messages.Any())
            {
                return Results.BadRequest("No messages provided.");
            }


            // if (string.IsNullOrEmpty(prompt))
            // {
            //     context.Response.StatusCode = 400;
            //     await context.Response.WriteAsync("Prompt is required.");
            //     return;
            // }

            // Set up the necessary headers for SSE
            context.Response.Headers.Append("Content-Type", "text/event-stream");
            context.Response.Headers.Append("Cache-Control", "no-cache");
            context.Response.Headers.Append("Connection", "keep-alive");

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
                return Results.BadRequest("OpenAI endpoint or API key is not set.");;
            }
            AzureOpenAIClient azureClient = new(
                new Uri(openAiEndpoint),
                new AzureKeyCredential(openAiApiKey));


            // Get ChatClient using the deployment name
            ChatClient chatClient = azureClient.GetChatClient(openAiDeploymentName);

            // Prepare chat messages based on your prompt and initial context
            var oaiMessages = new List<ChatMessage>();

            foreach (var chatMessage in messages)
            {
                if (chatMessage.sender == "system")
                {
                    oaiMessages.Add(new SystemChatMessage(chatMessage.text));
                }
                else if (chatMessage.sender == "user")
                {
                    oaiMessages.Add(new UserChatMessage(chatMessage.text));
                }
                else //assistant
                {
                    oaiMessages.Add(new AssistantChatMessage(chatMessage.text));
                }
            }

            // Get streaming completion updates
            CollectionResult<StreamingChatCompletionUpdate> completionUpdates = chatClient.CompleteChatStreaming(oaiMessages);

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
                        await context.Response.WriteAsync(content);
                        await context.Response.Body.FlushAsync();
                    }
                }
            }

            //not required when streaming. it will cause an error
            //return Results.Ok();

            return null;
        }).RequireAuthorization();     


    }


}

