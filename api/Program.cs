using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// Add services to the container.

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

builder.Services.AddOpenApi();

//DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=ASMPTOrderManagement.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

//Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
// Use this to authenticate on API level
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     // .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
//     .AddJwtBearer(options =>
//     {
        
//         options.Authority = $"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}/v2.0";
//         options.TokenValidationParameters.ValidAudience = builder.Configuration["AzureAd:ClientId"]; // or "api://<client-id>"
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateAudience = true,
//             ValidAudience = builder.Configuration["AzureAd:ClientId"], // e.g. "your-app-client-id" or "api://your-app-client-id"
//             ValidIssuers = new[]
//             {
//                 $"https://sts.windows.net/{builder.Configuration["AzureAd:TenantId"]}/",
//                 $"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}/v2.0"
//             }
//         };
//     });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ReaderApiScope", policy =>
        policy.RequireClaim("http://schemas.microsoft.com/identity/claims/scope", "user_access"));
    options.AddPolicy("WriterApiScope", policy =>
        policy.RequireClaim("http://schemas.microsoft.com/identity/claims/scope", "admin_access"));
});


// builder.Services.AddIdentityApiEndpoints<IdentityUser>()
//     .AddEntityFrameworkStores<AppDbContext>();
// builder.Services.AddAuthorization();

builder.Services.AddScoped<IUserContext, UserContext>();

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

    if (!await dbContext.Boards.AnyAsync() && !await dbContext.ComponentTypes.AnyAsync())
    {
        await SeedDatabaseAsync(dbContext);
        await dbContext.SaveChangesAsync();
    }
}

async Task SeedDatabaseAsync(AppDbContext dbContext)
{
    var order1Id = Guid.NewGuid();
    var order1BoardId = Guid.NewGuid();
    var order2Id = Guid.NewGuid();
    var order2BoardId = Guid.NewGuid();

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
        new OrderBoard { Id = order1BoardId, OrderId = order1Id, BoardId = 1 },
        new OrderBoard { Id = order2BoardId, OrderId = order2Id, BoardId = 2 }
    );
    dbContext.OrderBoardComponents.AddRange(
        new OrderBoardComponent { OrderBoardId = order1BoardId, ComponentId = 1, Quantity = 2 },
        new OrderBoardComponent { OrderBoardId = order1BoardId, ComponentId = 2, Quantity = 1 },
        new OrderBoardComponent { OrderBoardId = order2BoardId, ComponentId = 2, Quantity = 4 },
        new OrderBoardComponent { OrderBoardId = order2BoardId, ComponentId = 3, Quantity = 3 }
    );
}

app.UseSerilogRequestLogging();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    Console.WriteLine("Development");
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<UserContextMiddleware>();

app.MapControllers();

app.Run();
