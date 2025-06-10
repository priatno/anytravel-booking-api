using AnyTravel.BookingApi.Data;
using AnyTravel.BookingApi.Integrations.Notifications;
using AnyTravel.BookingApi.Integrations.TravelXchange;
using Amazon.SimpleNotificationService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Connection string is assembled from the RDS environment variables that
// Elastic Beanstalk injects automatically when an RDS instance is attached
// to the environment. Falls back to appsettings for local development.
string BuildConnectionString()
{
    var host = Environment.GetEnvironmentVariable("RDS_HOSTNAME");
    var port = Environment.GetEnvironmentVariable("RDS_PORT") ?? "1433";
    var db = Environment.GetEnvironmentVariable("RDS_DB_NAME");
    var user = Environment.GetEnvironmentVariable("RDS_USERNAME");
    var password = Environment.GetEnvironmentVariable("RDS_PASSWORD");

    if (string.IsNullOrEmpty(host))
    {
        // local dev fallback
        return builder.Configuration.GetConnectionString("BookingDb")
            ?? throw new InvalidOperationException("No BookingDb connection string configured.");
    }

    return $"Server={host},{port};Database={db};User Id={user};Password={password};TrustServerCertificate=True;";
}

var connectionString = BuildConnectionString();

builder.Services.AddSingleton<SqlDataAccess>(_ => new SqlDataAccess(connectionString));
builder.Services.AddDbContext<BookingDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddHttpClient<TravelXchangeSoapClient>();
builder.Services.Configure<TravelXchangeOptions>(builder.Configuration.GetSection("TravelXchange"));

builder.Services.AddSingleton<IAmazonSimpleNotificationService>(_ => new AmazonSimpleNotificationServiceClient());
builder.Services.Configure<BookingNotificationOptions>(builder.Configuration.GetSection("BookingNotifications"));
builder.Services.AddScoped<BookingNotifier>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Elastic Beanstalk health check target — do not remove, referenced in .ebextensions.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.UseAuthorization();
app.MapControllers();

app.Run();
