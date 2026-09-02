using core.Models;
using Xunit;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

public class OrderRepositoryTests
{   
    private DbContextFactory _dbContextFactory = new DbContextFactory();

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