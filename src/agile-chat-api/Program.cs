using agile_chat_api.Endpoints;
using agile_chat_api.Utils;
using agile_chat_api.Extensions;
using agile_chat_api.Services;
using Azure.AI.OpenAI;
using Azure.Identity;
using DotNetEnv;
using Microsoft.Azure.Cosmos;

// Load environment variables for OpenAI Endpoint and Cosmos DB access
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Cosmos DB configuration
string cosmosDbUri = Env.GetString("AZURE_COSMOSDB_URI");
string cosmosDbKey = Env.GetString("AZURE_COSMOSDB_KEY");

if (string.IsNullOrEmpty(cosmosDbUri) || string.IsNullOrEmpty(cosmosDbKey))
{
    throw new InvalidOperationException("Cosmos DB configuration is missing. Ensure that AZURE_COSMOSDB_URI and AZURE_COSMOSDB_KEY are set in the environment variables.");
}

builder.Services.AddAzureAdAuth();
// Register CosmosClient as a singleton
builder.Services.AddSingleton(s => new CosmosClient(cosmosDbUri, cosmosDbKey));

// Add services to the container
builder.Services.AddSingleton<IToolService, ToolService>();
builder.Services.AddSingleton<IAssistantService, AssistantService>();
builder.Services.AddSingleton<IPersonaService, PersonaService>();
builder.Services.AddSingleton<IAzureAiSearchService, AzureAiSearchService>();
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();

// Register ConsoleLogger
builder.Services.AddSingleton<ILogger>(s => new ConsoleLogger("ConsoleLogger", LogLevel.Debug));

// Define the CORS policy with allowed origins
// Load allowed origins from environment variables or use default values
string[] allowedOrigins = Env.GetString("ALLOWED_ORIGINS")?.Split(';') ?? new[] { "http://localhost:3000", "http://localhost:5000", "https://agilechat-dev-webapp.azurewebsites.net" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply the CORS policy globally
app.UseCors("AllowSpecificOrigins");

// Set Referrer-Policy header to strict-origin-when-cross-origin
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

// Register API endpoints
app.MapToolEndpoints();
app.MapAssistantEndpoints();
app.MapChatCompletionsEndpoint();
app.MapPersonaEndpoints();
app.MapFileEndpoints();

app.Run();
