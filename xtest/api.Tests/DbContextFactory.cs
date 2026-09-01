using Microsoft.EntityFrameworkCore;

public class DbContextFactory
{
    public DbContextFactory()
    {
    }

    public AppDbContext CreateFakeDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
         .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
         .Options;

        var context = new AppDbContext(options);

        // Forces EF Core to initialize and build the Identity model
        context.Database.EnsureCreated();

        return context;
    }
}