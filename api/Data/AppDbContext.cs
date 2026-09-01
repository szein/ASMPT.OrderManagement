using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    // private readonly ICurrentUserService _currentUserService; // Service to get logged-in user
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderBoard> OrderBoards { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<BoardComponent> BoardComponents { get; set; }
    public DbSet<Component> Components { get; set; }
    public DbSet<ComponentType> ComponentTypes { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OrderBoard>()
            .HasKey(ob => new { ob.OrderId, ob.BoardId });

        modelBuilder.Entity<OrderBoard>()
            .HasOne(ob => ob.Order)
            .WithMany(o => o.OrderBoards)
            .HasForeignKey(ob => ob.OrderId);

        modelBuilder.Entity<OrderBoard>()
            .HasOne(ob => ob.Board)
            .WithMany(b => b.OrderBoards)
            .HasForeignKey(ob => ob.BoardId);

        modelBuilder.Entity<BoardComponent>()
            .HasKey(bc => new { bc.BoardId, bc.ComponentId });

        modelBuilder.Entity<BoardComponent>()
            .HasOne(bc => bc.Board)
            .WithMany(b => b.BoardComponents)
            .HasForeignKey(bc => bc.BoardId);

        modelBuilder.Entity<BoardComponent>()
            .HasOne(bc => bc.Component)
            .WithMany(c => c.BoardComponents)
            .HasForeignKey(bc => bc.ComponentId);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Auditable>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                //entry.Entity.CreatedBy = _currentUserService.UserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModifiedAt = DateTime.UtcNow;
                //entry.Entity.LastModifiedBy = _currentUserService.UserId;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}