using Xunit;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

public class OrderRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_Returns_List_Of_Orders()
    {
        // Arrange
        var fakeLogger = A.Fake<ILogger<OrderRepository>>();
        var fakeAppContext = A.Fake<AppDbContext>();
        var expectedOrders = new List<Order>
        {
            new Order { Id = 1, Name = "Order 1", Description = "Description 1", OrderDate = new DateTime(2026, 9, 1) },
            new Order { Id = 2, Name = "Order 2", Description = "Description 2", OrderDate = new DateTime(2026, 9, 2) }
        };
        A.CallTo(() => fakeAppContext.Orders.ToListAsync<Order>()).Returns(Task.FromResult(expectedOrders));
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