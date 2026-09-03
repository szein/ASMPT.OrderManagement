using core.Models;
using Xunit;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

public class OrderRepositoryTests
{   
    private DbContextFactory _dbContextFactory = new DbContextFactory();

    [Fact]
    public async Task SaveAsync_Books_Component_Quantity_And_Marks_Order_Created()
    {
        await using var context = _dbContextFactory.CreateFakeDbContext();
        var componentType = new ComponentType { Id = 1, Name = "Type 1" };
        var component = new Component { Id = 1, ComponentTypeId = 1, ComponentType = componentType, Quantity = 10 };
        var board = new Board { Id = 1, Name = "Board" };
        var order = new Order { Id = Guid.NewGuid(), Name = "Order", Status = OrderStatus.Pending };
        context.ComponentTypes.Add(componentType);
        context.Components.Add(component);
        context.Boards.Add(board);
        context.Orders.Add(order);
        context.OrderBoards.Add(new OrderBoard { OrderId = order.Id, BoardId = board.Id });
        context.OrderComponents.Add(new OrderComponent { OrderId = order.Id, ComponentId = component.Id, Quantity = 6 });
        await context.SaveChangesAsync();

        var repository = new OrderRepository(context, A.Fake<ILogger<OrderRepository>>());
        var result = await repository.SaveAsync(order.Id);

        Assert.Equal(OrderStatus.Created, result!.Status);
        Assert.Equal(4, (await context.Components.FindAsync(component.Id))!.Quantity);
    }

    [Fact]
    public async Task SaveAsync_Rejects_Insufficient_Stock_Without_Changing_Order()
    {
        await using var context = _dbContextFactory.CreateFakeDbContext();
        var component = new Component { Id = 1, ComponentTypeId = 1, Quantity = 2 };
        var board = new Board { Id = 1, Name = "Board" };
        var order = new Order { Id = Guid.NewGuid(), Name = "Order", Status = OrderStatus.Pending };
        context.Components.Add(component);
        context.Boards.Add(board);
        context.Orders.Add(order);
        context.OrderBoards.Add(new OrderBoard { OrderId = order.Id, BoardId = board.Id });
        context.OrderComponents.Add(new OrderComponent { OrderId = order.Id, ComponentId = component.Id, Quantity = 3 });
        await context.SaveChangesAsync();

        var repository = new OrderRepository(context, A.Fake<ILogger<OrderRepository>>());
        await Assert.ThrowsAsync<ArgumentException>(() => repository.SaveAsync(order.Id));

        Assert.Equal(OrderStatus.Pending, (await context.Orders.FindAsync(order.Id))!.Status);
        Assert.Equal(2, (await context.Components.FindAsync(component.Id))!.Quantity);
    }

    [Fact]
    public async Task DeleteAsync_Deletes_Pending_Order_And_Associations()
    {
        await using var context = _dbContextFactory.CreateFakeDbContext();
        var board = new Board { Id = 1, Name = "Board" };
        var component = new Component { Id = 1, ComponentTypeId = 1, Quantity = 10 };
        var order = new Order { Id = Guid.NewGuid(), Name = "Order", Status = OrderStatus.Pending };
        context.Boards.Add(board);
        context.Components.Add(component);
        context.Orders.Add(order);
        context.OrderBoards.Add(new OrderBoard { OrderId = order.Id, BoardId = board.Id });
        context.OrderComponents.Add(new OrderComponent { OrderId = order.Id, ComponentId = component.Id, Quantity = 1 });
        await context.SaveChangesAsync();

        var repository = new OrderRepository(context, A.Fake<ILogger<OrderRepository>>());
        Assert.True(await repository.DeleteAsync(order.Id));

        Assert.Empty(await context.OrderBoards.Where(assignment => assignment.OrderId == order.Id).ToListAsync());
        Assert.Empty(await context.OrderComponents.Where(assignment => assignment.OrderId == order.Id).ToListAsync());
    }

    [Fact]
    public async Task GetAllAsync_Returns_List_Of_Orders()
    {
        // Arrange
        using var fakeAppContext = _dbContextFactory.CreateFakeDbContext();
        var fakeLogger = A.Fake<ILogger<OrderRepository>>();
        var expectedOrders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), Name = "Order 1", Description = "Description 1", OrderDate = new DateTime(2026, 9, 1) },
            new Order { Id = Guid.NewGuid(), Name = "Order 2", Description = "Description 2", OrderDate = new DateTime(2026, 9, 2) }
        };        
        fakeAppContext.Orders.AddRange(expectedOrders);
        await fakeAppContext.SaveChangesAsync();
        var orderRepository = new OrderRepository(fakeAppContext, fakeLogger);

        // Act
        var result = await orderRepository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<Order>>(result);
        Assert.Equal(expectedOrders.Count, result.Count);
        foreach (var order in expectedOrders)
        {
            Assert.Contains(result, o => o.Id == order.Id && o.Name == order.Name && o.Description == order.Description && o.OrderDate == order.OrderDate);
        }
    }
}