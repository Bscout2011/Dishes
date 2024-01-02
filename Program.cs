using AutoMapper;
using DishesApi.DbContexts;
using DishesApi.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DishesDbContext>(
    o => o.UseSqlite(builder.Configuration["ConnectionStrings:DishesDBConnectionString"])
);

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

app.Urls.Add("https://localhost:5000");
app.UseHttpsRedirection();

app.MapGet(
    "dishes",
    async (DishesDbContext db) =>
    {
        return await db.Dishes.ToListAsync();
    }
);

app.MapGet(
    "dishes/{dishId:guid}",
    async (Guid dishId, DishesDbContext db, IMapper mapper) =>
    {
        var dish = await db.Dishes.SingleOrDefaultAsync(d => d.Id == dishId);
        return mapper.Map<DishDto>(dish);
    }
);

app.MapGet(
    "dishes/{dishId:guid}/ingredients",
    async Task<ICollection<IngredientDto>> (Guid dishId, DishesDbContext db, IMapper mapper) =>
    {
        var dish = await db.Dishes.Include(d => d.Ingredients)
            .SingleOrDefaultAsync(d => d.Id == dishId);
        if (dish == null)
            return (ICollection<IngredientDto>)Results.NotFound();
        return mapper.Map<ICollection<IngredientDto>>(dish.Ingredients);
    }
);

app.MapGet(
    "dishes/{dishName}",
    async (string dishName, DishesDbContext db, IMapper mapper) =>
    {
        var dish = await db.Dishes.FirstOrDefaultAsync(d => d.Name == dishName);
        return mapper.Map<DishDto>(dish);
    }
);

// delete database on each launch
using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()!.CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<DishesDbContext>();
    context.Database.EnsureDeleted();
    context.Database.Migrate();
}

app.Run();
