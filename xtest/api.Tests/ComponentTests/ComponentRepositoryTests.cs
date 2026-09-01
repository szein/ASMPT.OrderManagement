using Xunit;
using core.Models;
using Microsoft.Extensions.Logging;
using FakeItEasy;

public class ComponentRepositoryTests
{
    private DbContextFactory _dbContextFactory = new DbContextFactory();

    [Fact]
    public async Task GetAllAsync_Returns_List_Of_Components()
    {
        //Arrenge
        using var fakeAppContext = _dbContextFactory.CreateFakeDbContext();
        var fakeLogger = A.Fake<ILogger<ComponentRepository>>();
        var expectedComponentTypes = new List<ComponentType>
        {
            new ComponentType { Id = 1, Name = "Component Type 1", Description = "Description 1" },
            new ComponentType { Id = 2, Name = "Component Type 2", Description = "Description 2" }
        };
        var expectedComponents = new List<Component>
        {
            new Component { Id = 1, ComponentTypeId = expectedComponentTypes[0].Id, Quantity = 100, ComponentType = expectedComponentTypes[0]},
            new Component { Id = 2, ComponentTypeId = expectedComponentTypes[1].Id, Quantity = 200, ComponentType = expectedComponentTypes[1]}
        };

        fakeAppContext.Components.AddRange(expectedComponents);
        await fakeAppContext.SaveChangesAsync();

        var componentRepository = new ComponentRepository(fakeAppContext, fakeLogger);

        //Act
        var result = await componentRepository.GetAllAsync();

        Assert.NotNull(result);
        Assert.IsType<List<Board>>(result);
        Assert.Equal(expectedComponents.Count, result.Count);
    }
}