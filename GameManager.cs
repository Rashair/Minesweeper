using Minesweeper.Boards;
using static Minesweeper.IoManager;

namespace Minesweeper;

public class GameManager
{
    private readonly Board _board;

    public GameManager()
    {
        var gridSize = GetNonNegativeIntegerInput("Enter grid size: ");
        var bombsNumber = GetNonNegativeIntegerInput("Enter bombs number: ");
        _board = new(gridSize, bombsNumber);
    }

    public void Play()
    {
        PrintInitialInstructions();
        var gameResult = LoopGame();
        DisplayResult(gameResult);
    }

    private EndGameState LoopGame()
    {
        while (true)
        {
            _board.PrintUser();
            PrintLine();

            var operation = GetValidOperation();
            var gameResult = ApplyOperation(operation);
            if (gameResult.HasValue)
            {
                return gameResult.Value;
            }

            if (HasWon())
            {
                return EndGameState.Won;
            }
        }
    }

    private Operation GetValidOperation()
    {
        var operation = GetNonNegativeIntegerInput("Select operation: ");
        if (!Enum.IsDefined(typeof(Operation), operation))
        {
            PrintLine($"Invalid operation: {operation}");
            return GetValidOperation();
        }

        return (Operation) operation;
    }

    private EndGameState? ApplyOperation(Operation operation)
    {
        if (operation == Operation.Cancel)
        {
            return EndGameState.Cancelled;
        }

        if (operation == Operation.Uncover)
        {
            var isBomb = Uncover();
            if (isBomb)
            {
                return EndGameState.Lost;
            }
        }

        if (operation == Operation.Flag)
        {
            Flag();
        }

        return null;
    }

    private bool Uncover()
    {
        var (row, col) = GetRowCol();
        var uncoverResult = _board.Uncover(row, col);
        if (uncoverResult == UncoverResult.Flagged)
        {
            PrintLine("Cannot uncover flagged field");
        }

        return uncoverResult == UncoverResult.Bomb;
    }

    private static (int row, int col) GetRowCol()
    {
        var row = GetNonNegativeIntegerInput("Select row: ");
        var col = GetNonNegativeIntegerInput("Select column: ");
        return (row, col);
    }

    private void Flag()
    {
        var (row, col) = GetRowCol();
        var flagResult = _board.Flag(row, col);
        if (flagResult == FlagResult.AlreadyUncovered)
        {
            PrintLine("Cannot flag uncovered field");
        }
    }

    private void PrintInitialInstructions()
    {
        PrintLine("Board initialised! Let's start the game!");
        PrintLine("Operations: \n 0 = uncover,\n 1 = flag / unflag,\n 2 = cancel game\n");
    }

    private bool HasWon()
    {
        return _board.GetUncoveredFieldsNumber() == _board.GetTotalFieldsNumber() - _board.GetBombsNumber();
    }

    // TODO: Move to MinesweeperGame
    private void DisplayResult(EndGameState state)
    {
        _board!.PrintSystem();
        PrintLine(GetEndGameMessage(state));
        PrintStatistics();
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

    private void PrintStatistics()
    {
        var uncoveredFields = _board.GetUncoveredFieldsNumber();
        var totalFields = _board.GetTotalFieldsNumber();
        var bombs = _board.GetBombsNumber();
        PrintLine($"Uncovered {uncoveredFields} out of {totalFields}");

        var fieldsToUncover = totalFields - uncoveredFields - bombs;
        if (fieldsToUncover > 0)
        {
            PrintLine($"Fields without bombs to uncover: {fieldsToUncover}");
        }

        PrintLine($"Total bombs {bombs}");
    }
}

public enum Operation
{
    Uncover,
    Flag,
    Cancel
}