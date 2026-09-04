using core.Models;
using Xunit;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public class OrderRepositoryTests
{   
    private DbContextFactory _dbContextFactory = new DbContextFactory();

    [Fact]
    public async Task SaveAsync_Books_Component_Quantity_And_Marks_Order_Created()
    {
        await using var context = _dbContextFactory.CreateFakeDbContext();
        var componentType = new ComponentType { Id = 1, Name = "Type 1" };
        var component = new Component { Id = 1, ComponentTypeId = 1, ComponentType = componentType, Quantity = 4 };
        var board = new Board { Id = 1, Name = "Board" };
        context.ComponentTypes.Add(componentType);
        context.Components.Add(component);
        context.Boards.Add(board);
        await context.SaveChangesAsync();
        
        var expectedOrder = new Order { Id = Guid.NewGuid(), Name = "Order 1", Description = "Description 1", OrderDate = new DateTime(2026, 9, 1) };
        var request = new CreateOrderRequest(
            Name: expectedOrder.Name,
            Description: expectedOrder.Description,
            OrderDate: expectedOrder.OrderDate,
            Boards: new List<CreateOrderBoardRequest>
            {
                new CreateOrderBoardRequest(
                    board.Id,
                    Components: new List<CreateOrderBoardComponentRequest>
                    {
                        new CreateOrderBoardComponentRequest(ComponentId: component.Id, Quantity: 4)
                    }
                )
            });

        var repository = new OrderRepository(context, A.Fake<ILogger<OrderRepository>>());
        var result = await repository.AddAsync(expectedOrder);

        Assert.Equal(OrderStatus.Created, result!.Status);
        Assert.Equal(4, (await context.Components.FindAsync(component.Id))!.Quantity);
    }

    [Fact]
    public async Task GetAllAsync_Returns_List_Of_Orders()
    {
        // Arrange
        using var context = _dbContextFactory.CreateFakeDbContext();
        var fakeLogger = A.Fake<ILogger<OrderRepository>>();
        var expectedOrders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), Name = "Order 1", Description = "Description 1", OrderDate = new DateTime(2026, 9, 1) },
            new Order { Id = Guid.NewGuid(), Name = "Order 2", Description = "Description 2", OrderDate = new DateTime(2026, 9, 2) }
        };        
        context.Orders.AddRange(expectedOrders);
        await context.SaveChangesAsync();
        var orderRepository = new OrderRepository(context, fakeLogger);

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