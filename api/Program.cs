using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/order-api-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Host.UseSerilog();

builder.Services.AddOpenApi();

//DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=ASMPTOrderManagement.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));


builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IBoardRepository, BoardRepository>();
builder.Services.AddScoped<IBoardService, BoardService>();
builder.Services.AddScoped<IComponentRepository, ComponentRepository>();
builder.Services.AddScoped<IComponentService, ComponentService>();
builder.Services.AddScoped<IComponentTypeRepository, ComponentTypeRepository>();
builder.Services.AddScoped<IComponentTypeService, ComponentTypeService>();

//Services and Controllers
builder.Services.AddControllers();

// Authentication
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy.WithOrigins("http://localhost:5168")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

//Seed
await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await dbContext.Database.MigrateAsync();

    if(!await dbContext.Boards.AnyAsync() && !await dbContext.ComponentTypes.AnyAsync())
    {
        await SeedDatabaseAsync(dbContext);
        await dbContext.SaveChangesAsync();
    }
}

async Task SeedDatabaseAsync(AppDbContext dbContext)
{
    var order1Id = Guid.NewGuid();
    var order2Id = Guid.NewGuid();

    dbContext.Boards.AddRange(
        new Board { Id = 1, Name = "Board 1", Description = "Description for Board 1", Length = 10.5, Width = 8.5 },
        new Board { Id = 2, Name = "Board 2", Description = "Description for Board 2", Length = 12.0, Width = 9.0 }
    );
    dbContext.ComponentTypes.AddRange(
        new ComponentType { Id = 1, Name = "Type A", Description = "Description for Type A" },
        new ComponentType { Id = 2, Name = "Type B", Description = "Description for Type B" },
        new ComponentType { Id = 3, Name = "Type C", Description = "Description for Type C" }
    );
    dbContext.Components.AddRange(
        new Component { Id = 1, ComponentTypeId = 1, Quantity = 5 },
        new Component { Id = 2, ComponentTypeId = 2, Quantity = 10 },
        new Component { Id = 3, ComponentTypeId = 3, Quantity = 0 }
    );
    dbContext.Orders.AddRange(
        new Order { Id = order1Id, Name = "Order 1", OrderDate = DateTime.UtcNow },
        new Order { Id = order2Id, Name = "Order 2", OrderDate = DateTime.UtcNow }
    );
    dbContext.OrderBoards.AddRange(
        new OrderBoard { OrderId = order1Id, BoardId = 1 },
        new OrderBoard { OrderId = order2Id, BoardId = 2 }
    );
    dbContext.BoardComponents.AddRange(
        new BoardComponent { BoardId = 1, ComponentId = 1 },
        new BoardComponent { BoardId = 1, ComponentId = 2 },
        new BoardComponent { BoardId = 2, ComponentId = 2 },
        new BoardComponent { BoardId = 2, ComponentId = 3 }
    );
}

app.UseSerilogRequestLogging();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
