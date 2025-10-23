using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TabTogetherApi.Configuration;
using TabTogetherApi.DbContexts;
using TabTogetherApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Get connection string safely
var conn = builder.Configuration.GetConnectionString("TabTogtherConnectionString")
           ?? throw new InvalidOperationException("Connection string 'TabTogtherConnectionString' not found.");
builder.Services.AddDbContext<TabTogetherContext>(options =>
    options.UseSqlServer(conn.Replace(@"\\", @"\")));

// Ensure Azure settings section exists in appsettings.json:
// "AzureDocumentIntelligence": { "Endpoint": "https://...", "ApiKey": "..." }

// Register settings so DI can inject IOptions<AzureDocumentIntelligenceSettings>
builder.Services.Configure<AzureDocumentIntelligenceSettings>(builder.Configuration.GetSection("AzureDocumentIntelligence"));

builder.Services.Configure<BlobStorageSettings>(builder.Configuration.GetSection(BlobStorageSettings.ConfigurationSection));

// Register the repository directly � DI will inject IOptions<AzureDocumentIntelligenceSettings>
builder.Services.AddScoped<IDocumentItelligenceRepository, DocumentItelligenceRepository>();
builder.Services.AddScoped<IStorageAccountBlobRepository, StorageAccountBlobRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
        policy.WithOrigins(
                "http://localhost:5173",   // frontend dev server (http)
                "https://localhost:5173",  // frontend dev server (https)
                "https://localhost:7122",  // swagger / API origin
                "http://localhost:7122"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("X-Pagination")
            //.AllowCredentials() // enable only if your client sends credentials
    );
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
app.UseCors("DefaultCors");
app.MapControllers();
app.Run();