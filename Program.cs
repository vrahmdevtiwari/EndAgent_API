using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TEST_WebApiOsDetails.data;
using TEST_WebApiOsDetails.Migrations;
using EndAgent_API.MongoDB;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// ================== SQL DbContext ==================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ================== Form Options ==================
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = int.MaxValue; // unlimited file size
});

builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = int.MaxValue);

// ================== Authentication ==================
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["ApiEndPoints:SSOLogin"];
        options.TokenValidationParameters = new TokenValidationParameters { ValidateAudience = false };
        options.BackchannelHttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    });

// ================== Controllers ==================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ================== Rate Limiting ==================
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();

// ================== HttpClient Factory ==================
builder.Services.AddHttpClient();

 //================== MongoDAL & IMongoDatabase ==================
// manually create MongoDAL and MongoDatabase for DI
builder.Services.AddSingleton<MongoDAL>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    string mongoConn = config.GetValue<string>("ConnectionStrings:mongoDBConnection");
    string mongoDbName = config.GetValue<string>("ConnectionStrings:mongoDBDatabase");
    return new MongoDAL(mongoConn, mongoDbName);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    string mongoConn = config.GetValue<string>("ConnectionStrings:mongoDBConnection");
    string mongoDbName = config.GetValue<string>("ConnectionStrings:mongoDBDatabase");
    var client = new MongoClient(mongoConn);
    return client.GetDatabase(mongoDbName);
});

// ================== Hosted Service ==================
builder.Services.AddScoped<PatchSyncService>();

var app = builder.Build();

// ================== Swagger ==================
app.UseSwagger();
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}

// ================== Middleware ==================
app.UseIpRateLimiting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ================== Logger ==================
var loggerFactory = app.Services.GetService<ILoggerFactory>();
loggerFactory.AddFile($@"{Directory.GetCurrentDirectory()}\Logs\log.txt");

app.Run();