namespace Minesweeper;

public class GameResult
{
    public required EndGameState GameState { get; init; }
    public required int TotalNoFields { get; init; }
    public required int UncoveredNoFields { get; init; }
    public required int NoBombFields { get; init; }
}

public enum EndGameState
{
    Won,
    Lost,
    Cancelled,
}