using core.Models;
using Xunit;
using FakeItEasy;
using Microsoft.Extensions.Logging;

public class ComponentTypeServiceTests
{
    [Fact (Skip = "WIP")]
    public async Task Add_Predefined_Component_Types_Adds_Default_Types_Once()
    {
        // var fakeLogger = A.Fake<ILogger<ComponentTypeService>>();
        // var fakeRepository = A.Fake<IComponentTypeRepository>();
        // var expectedTypes = new List<ComponentType>
        // {
        //     new() { Id = 1, Name = "BGA", Description = "A type of surface-mount packaging." },
        //     new() { Id = 2, Name = "QFN", Description = "A leadless quad flat package." }
        // };

        // A.CallTo(() => fakeRepository.GetAllAsync()).Returns(Task.FromResult(new List<ComponentType>()));
        // A.CallTo(() => fakeRepository.AddAsync(A<ComponentType>.Ignored)).ReturnsLazily((ComponentType componentType) => componentType);

        // var service = new ComponentTypeService(fakeRepository, fakeLogger);

        // var result = await service.AddPredefinedComponentTypesAsync();

        // Assert.NotNull(result);
        // Assert.Contains(result, ct => ct.Name == "BGA" && ct.Description.Contains("surface-mount", StringComparison.OrdinalIgnoreCase));
        // Assert.Contains(result, ct => ct.Name == "QFN");
        // Assert.True(result.Count >= 2);
        // Assert.Equal(expectedTypes.Count, result.Take(expectedTypes.Count).Count());
    }
}
