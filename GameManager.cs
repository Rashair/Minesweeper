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

    public GameResult Play()
    {
        PrintInitialInstructions();
        var endGameState = LoopGame();

        PrintLine();
        _board.PrintSystem();

        return new()
        {
            GameState = endGameState,
            TotalNoFields = _board.GetTotalFieldsNumber(),
            UncoveredNoFields = _board.GetUncoveredFieldsNumber(),
            BombFields = _board.GetBombsNumber()
        };
    }

    private EndGameState LoopGame()
    {
        while (true)
        {
            PrintUserBoard();

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

    private void PrintUserBoard()
    {
        _board.PrintUser();
        PrintLine();
#if DEBUG
        _board.PrintSystem();
        PrintLine();
#endif
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
}

public enum Operation
{
    Uncover,
    Flag,
    Cancel
}