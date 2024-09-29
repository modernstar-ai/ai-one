using Azure.AI.OpenAI;
using Azure;
using Azure.Identity;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using DotNetEnv;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IToolService, ToolService>();

// Ensure the environment variables are loaded for OpenAI Endpoint access
DotNetEnv.Env.Load();

// Define the CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "https://agc0928-apiapp.azurewebsites.net") // Allow specific origins
                  .AllowAnyMethod() // Allow all HTTP methods (GET, POST, etc.)
                  .AllowAnyHeader() // Allow all headers (Authorization, Content-Type, etc.)
                  .AllowCredentials(); // If needed, allow credentials (cookies, authorization headers)
        });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Apply the CORS Policy
app.UseCors("AllowSpecificOrigins"); // Apply the CORS policy globally to all routes

// Register the API endpoints from ToolEndpoints
app.MapToolEndpoints(); //.WithOpenApi();
app.MapChatCompletionsEndpoint();

app.UseHttpsRedirection();

app.Run();
