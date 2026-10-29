using EshopAPI.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();


var databaseUrl = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing database connection string.");

var uri = new Uri(databaseUrl);
var credentials = uri.UserInfo.Split(':', 2);

var connectionString = new NpgsqlConnectionStringBuilder
{
    Host = uri.Host,
    Port = uri.Port == -1 ? 5432 : uri.Port,
    Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
    Username = Uri.UnescapeDataString(credentials[0]),
    Password = Uri.UnescapeDataString(credentials[1]),
    SslMode = SslMode.Require
}.ConnectionString;

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalFrontend", policy =>
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("LocalFrontend");
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseHttpsRedirection();
app.Run();
