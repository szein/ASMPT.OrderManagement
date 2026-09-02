using core.Models;
using Xunit;
using FakeItEasy;
using Microsoft.Extensions.Logging;

public class OrderServiceTests
{
    [Fact]
    public async Task  Get_All_Orders_Returns_List_Of_Orders()
    {
        // Arrange
        var fakeLogger = A.Fake<ILogger<OrderService>>();
        var fakeRepository = A.Fake<IOrderRepository>();
        var expectedOrders = new List<Order>
        {
            new Order { Id = Guid.NewGuid(), Name = "Order 1", Description = "Description 1", OrderDate = new DateTime(2026, 9, 1) },
            new Order { Id = Guid.NewGuid(), Name = "Order 2", Description = "Description 2", OrderDate = new DateTime(2026, 9, 2) }
        };
        A.CallTo(() => fakeRepository.GetAllAsync()).Returns(Task.FromResult(expectedOrders));
        var orderService = new OrderService(fakeRepository, fakeLogger);

        // Act
        var result = await orderService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<List<Order>>(result);
        Assert.Equal(expectedOrders.Count, result.Count);
        foreach (var order in expectedOrders)
        {
            Assert.Contains(result, o => o.Id == order.Id && o.Name == order.Name && o.Description == order.Description && o.OrderDate == order.OrderDate);
        }
    }

    [Fact]
    public async Task UpdateBoardAssignmentsAsync_Updates_Only_Board_Associations()
    {
        // Arrange
        var fakeLogger = A.Fake<ILogger<OrderService>>();
        var fakeRepository = A.Fake<IOrderRepository>();
        var expectedOrder = new Order
        {
            Id = Guid.NewGuid(),
            Name = "Order 1",
            Description = "Description 1",
            OrderDate = new DateTime(2026, 9, 1)
        };

        A.CallTo(() => fakeRepository.GetByIdAsync(expectedOrder.Id)).Returns(Task.FromResult<Order?>(expectedOrder));
        A.CallTo(() => fakeRepository.UpdateBoardAssignmentsAsync(expectedOrder.Id, A<IEnumerable<int>>.That.Matches(ids => ids.SequenceEqual(new[] { 10, 20 }))))
            .Returns(Task.FromResult<Order?>(expectedOrder));

        var orderService = new OrderService(fakeRepository, fakeLogger);

        // Act
        var result = await orderService.UpdateBoardAssignmentsAsync(expectedOrder.Id, new[] { 10, 20 });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedOrder.Id, result!.Id);
    }
}