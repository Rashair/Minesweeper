using static Minesweeper.IoManager;

namespace Minesweeper;

public class MinesweeperGame
{
    public void Play()
    {
        Print("Welcome to Minesweeper!\n");

        var game = new GameManager();
        var result = game.Play();
        DisplayResult(result);
    }

    private void DisplayResult(GameResult result)
    {
        PrintLine(GetEndGameMessage(result.GameState));
        PrintStatistics(result);
    }

    private string GetEndGameMessage(EndGameState state)
    {
        return state switch
        {
            EndGameState.Won => "You won the game!",
            EndGameState.Lost => "Bomb! You lost the game :(",
            EndGameState.Cancelled => "Game was cancelled",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }

    private void PrintStatistics(GameResult result)
    {
        PrintLine($"Uncovered {result.UncoveredNoFields} out of {result.TotalNoFields}");

        var fieldsToUncover = result.TotalNoFields - result.UncoveredNoFields - result.BombFields;
        if (fieldsToUncover > 0)
        {
            PrintLine($"Fields without bombs to uncover: {fieldsToUncover}");
        }

        PrintLine($"Total bombs {result.BombFields}");
    }
}