using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace crayoncloud_marketdata
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Controllers
            builder.Services.AddControllers();

            // SQLite config
            const string dbPath = "marketdata.db";
            builder.Services.AddDbContext<MarketDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Tools and services
            builder.Services.AddSingleton<CoinspotScraper>();

            // FIX: Register CoinStrengthAnalyzer as scoped (depends on MarketDbContext)
            builder.Services.AddScoped<CoinStrengthAnalyzer>();

            builder.Services.AddHostedService<CoinSnapshotCollector>();

            // Swagger (dev)
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "MarketData API", Version = "v1" });
            });

            var app = builder.Build();

            // Enable WAL mode
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "PRAGMA journal_mode=WAL;";
                command.ExecuteNonQuery();
            }

            // Ensure DB exists
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MarketDbContext>();
                db.Database.EnsureCreated();
            }

            // Middleware
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MarketData API v1");
                });
            }

            // Redirect root and /api to Swagger UI
            app.MapGet("/", context =>
            {
                context.Response.Redirect("/swagger");
                return Task.CompletedTask;
            });

            app.MapGet("/api", context =>
            {
                context.Response.Redirect("/swagger");
                return Task.CompletedTask;
            });

            app.Run();
        }
    }
}
