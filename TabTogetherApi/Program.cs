using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TabTogetherApi.Configuration;
using TabTogetherApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Ensure Azure settings section exists in appsettings.json:
// "AzureDocumentIntelligence": { "Endpoint": "https://...", "ApiKey": "..." }

// Register settings so DI can inject IOptions<AzureDocumentIntelligenceSettings>
builder.Services.Configure<AzureDocumentIntelligenceSettings>(builder.Configuration.GetSection("AzureDocumentIntelligence"));

// Register the repository directly — DI will inject IOptions<AzureDocumentIntelligenceSettings>
builder.Services.AddScoped<IDocumentItelligenceRepository, DocumentItelligenceRepository>();

// ... rest of your existing registrations unchanged ...

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
        builder.WithOrigins("http://localhost:5173")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .WithExposedHeaders("X-Pagination"));
});

builder.Services.AddControllers()
.AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler =
System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles)
.AddNewtonsoftJson();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseCors();
app.MapControllers();
app.Run();