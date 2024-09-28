var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IToolService, ToolService>();


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

app.UseHttpsRedirection();

// Apply the CORS Policy
app.UseCors("AllowSpecificOrigins"); // Apply the CORS policy globally to all routes


// Register the API endpoints from ToolEndpoints
app.MapToolEndpoints(); //.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
