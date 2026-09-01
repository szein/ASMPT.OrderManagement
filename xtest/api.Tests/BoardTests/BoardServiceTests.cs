using core.Models;
using Xunit;
using FakeItEasy;
using Microsoft.Extensions.Logging;

public class BoardServiceTests
{
    [Fact]
    public async Task Get_All_Boards_Returns_List_Of_Boards()
    {
        var fakeLogger = A.Fake<ILogger<BoardService>>();
        var fakeRepository = A.Fake<IBoardRepository>();
        var expectedBoards = new List<Board>
        {
            new Board { Id = 1, Name = "Board 1", Description = "Description 1", Length = 10.5, Width = 8.5 },
            new Board { Id = 2, Name = "Board 2", Description = "Description 2", Length = 12.0, Width = 9.0 }
        };

        A.CallTo(() => fakeRepository.GetAllAsync()).Returns(Task.FromResult(expectedBoards));

        var boardService = new BoardService(fakeRepository, fakeLogger);

        var result = await boardService.GetAllAsync();

        Assert.NotNull(result);
        Assert.IsType<List<Board>>(result);
        Assert.Equal(expectedBoards.Count, result.Count);
        foreach (var board in expectedBoards)
        {
            Assert.Contains(result, b => b.Id == board.Id && b.Name == board.Name && b.Description == board.Description && b.Length == board.Length && b.Width == board.Width);
        }
    }

    [Fact]
    public async Task UpdateComponentAssignmentsAsync_Updates_Only_Board_Component_Associations()
    {
        var fakeLogger = A.Fake<ILogger<BoardService>>();
        var fakeRepository = A.Fake<IBoardRepository>();
        var requestedAssignments = new[]
        {
            new BoardComponentAssignment(10, 2),
            new BoardComponentAssignment(20, 3)
        };

        A.CallTo(() => fakeRepository.GetByIdAsync(1)).Returns(Task.FromResult<Board?>(new Board
        {
            Id = 1,
            Name = "Board 1",
            Description = "Description 1",
            Length = 10.5,
            Width = 8.5
        }));

        A.CallTo(() => fakeRepository.UpdateComponentAssignmentsAsync(1, A<IEnumerable<BoardComponentAssignment>>.That.Matches(ids => ids.SequenceEqual(requestedAssignments))))
            .Returns(Task.FromResult<Board?>(new Board
            {
                Id = 1,
                Name = "Board 1",
                Description = "Description 1",
                Length = 10.5,
                Width = 8.5
            }));

        var boardService = new BoardService(fakeRepository, fakeLogger);

        var result = await boardService.UpdateComponentAssignmentsAsync(1, requestedAssignments);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        A.CallTo(() => fakeRepository.GetByIdAsync(1)).MustHaveHappened();
        A.CallTo(() => fakeRepository.UpdateComponentAssignmentsAsync(1, A<IEnumerable<BoardComponentAssignment>>.That.Matches(ids => ids.SequenceEqual(requestedAssignments)))).MustHaveHappened();
    }

    [Fact]
    public async Task UpdateComponentAssignmentsAsync_Returns_Error_If_Component_Quantity_Exceeded()
    {
        var dbContext = new DbContextFactory().CreateFakeDbContext();
        int exptectedComponentTypeId = 1;
        dbContext.ComponentTypes.Add(new ComponentType { Id = exptectedComponentTypeId, Name = "Component Type 1", Description = "Description 1" });
        dbContext.Components.Add(new Component { Id = 10, ComponentTypeId = exptectedComponentTypeId, Quantity = 5 });
        await dbContext.SaveChangesAsync();        
        var repository = new BoardRepository(dbContext, A.Fake<ILogger<BoardRepository>>());

        var boardService = new BoardService(repository, A.Fake<ILogger<BoardService>>());
        var requestedAssignments = new[]
        {
            new BoardComponentAssignment(10, 10) // Exceeds available quantity
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await boardService.UpdateComponentAssignmentsAsync(1, requestedAssignments)
        );

    }

}
