using DishesApi.DbContexts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DishesDbContext>(
    o => o.UseSqlite(builder.Configuration["ConnectionStrings:DishesDBConnectionString"])
);

var app = builder.Build();

app.Urls.Add("https://localhost:5000");

app.MapGet("/", () => "Hello World!");

app.MapGet(
    "dishes",
    async (DishesDbContext db) =>
    {
        return await db.Dishes.ToListAsync();
    }
);

using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()!.CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<DishesDbContext>();
    context.Database.EnsureDeleted();
    context.Database.Migrate();
}

app.Run();
