using core.Models;
using Xunit;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

public class BoardRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_Returns_List_Of_Boards()
    {
        var fakeLogger = A.Fake<ILogger<BoardRepository>>();
        var fakeAppContext = A.Fake<AppDbContext>();
        var expectedBoards = new List<Board>
        {
            new Board { Id = 1, Name = "Board 1", Description = "Description 1", Length = 10.5, Width = 8.5 },
            new Board { Id = 2, Name = "Board 2", Description = "Description 2", Length = 12.0, Width = 9.0 }
        };

        A.CallTo(() => fakeAppContext.Boards.ToListAsync<Board>()).Returns(Task.FromResult(expectedBoards));

        var boardRepository = new BoardRepository(fakeAppContext, fakeLogger);

        var result = await boardRepository.GetAllAsync();

        Assert.NotNull(result);
        Assert.IsType<List<Board>>(result);
        Assert.Equal(expectedBoards.Count, result.Count);
        foreach (var board in expectedBoards)
        {
            Assert.Contains(result, b => b.Id == board.Id && b.Name == board.Name && b.Description == board.Description && b.Length == board.Length && b.Width == board.Width);
        }
    }
}
