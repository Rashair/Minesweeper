namespace Minesweeper.Boards;

using static IoManager;

public class Board
{
    private readonly int _bombsNumber;
    private readonly int _gridSize;
    private readonly int[,] _systemBoard;
    private readonly UserCellState[,] _userBoard;

    private int _uncoveredNoFields;

    public Board(int gridSize, int bombsNumber)
    {
        if (gridSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(gridSize), gridSize, "Must be greater than 0.");
        }

        if (bombsNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(bombsNumber), bombsNumber, "Must be greater than 0.");
        }

        if (bombsNumber >= gridSize * gridSize)
        {
            throw new ArgumentOutOfRangeException(nameof(bombsNumber), bombsNumber, "Must be less than the number of cells.");
        }

        _gridSize = gridSize;
        _bombsNumber = bombsNumber;
        _systemBoard = new int[gridSize, gridSize];
        _userBoard = new UserCellState[gridSize, gridSize];
        PlaceBombs();
    }

    private void PlaceBombs()
    {
        for (int i = 0; i < _bombsNumber; ++i)
        {
            var (row, col) = GetEmptyRandomRowCol();
            _systemBoard[row, col] = BoardConstants.BombValue;
            IncrementNeighbouringFields(row, col);
        }
    }

    private (int row, int col) GetEmptyRandomRowCol()
    {
        const int maxAttempts = 1_000;
        for (int i = 0; i < maxAttempts; ++i)
        {
            var row = Random.Shared.Next(0, _gridSize);
            var col = Random.Shared.Next(0, _gridSize);
            if (_systemBoard[row, col] == BoardConstants.BombValue)
            {
                continue;
            }

            return (row, col);
        }

        throw new InvalidOperationException("Could not find an empty cell to place a bomb. Please provide different board settings.");
    }

    // ReSharper disable once CognitiveComplexity - simple enough
    private void IncrementNeighbouringFields(int row, int col)
    {
        for (int i = row - 1; i <= row + 1; ++i)
        {
            if (IsOutOfRangeIndex(i))
            {
                continue;
            }

            for (int j = col - 1; j <= col + 1; ++j)
            {
                if (IsOutOfRangeIndex(j) || (i == row && j == col))
                {
                    continue;
                }

                if (_systemBoard[i, j] != BoardConstants.BombValue)
                {
                    _systemBoard[i, j] += 1;
                }
            }
        }
    }

    private bool IsOutOfRangeIndex(int k)
    {
        return k < 0 || k >= _gridSize;
    }

    public void PrintUser()
    {
        PrintInternal((i, j) => _userBoard[i, j].ToPrintSymbol(_systemBoard[i, j]));
    }

    public void PrintSystem()
    {
        PrintInternal((i, j) => CellStateExtensions.GetCellValueSymbol(_systemBoard[i, j]));
    }

    private void PrintInternal(Func<int, int, string> getPrintSymbol)
    {
        PrintColumnNumbers();
        PrintUpDownBorder();

        for (int i = 0; i < _gridSize; ++i)
        {
            Print($"{i}");
            PrintLeftRightBorder();
            for (int j = 0; j < _gridSize; ++j)
            {
                Print(getPrintSymbol(i, j));
                PrintLeftRightBorder();
            }

            PrintLine();
        }

        PrintUpDownBorder();
    }

    private void PrintColumnNumbers()
    {
        // Print column numbers
        Print(" "); // Skip row numbers
        PrintLeftRightBorder();
        for (int j = 0; j < _gridSize; ++j)
        {
            Print(j.ToString());
            PrintLeftRightBorder();
        }

        PrintLine();
    }

    private void PrintUpDownBorder()
    {
        Print(" "); // skip border for row numbers
        // (symbol + border) for each symbol + (1 for row numbers)
        var borderLength = _gridSize * 2 + 1;
        for (int j = 0; j < borderLength; ++j)
        {
            Print("-");
        }

        PrintLine();
    }

    private void PrintLeftRightBorder()
    {
        Print("|");
    }

    public UncoverResult Uncover(int row, int col)
    {
        if (!ValidateIndexes(row, col))
        {
            return UncoverResult.Failure;
        }

        var userBoardValue = _userBoard[row, col];
        if (userBoardValue == UserCellState.Flagged)
        {
            return UncoverResult.Flagged;
        }

        if (userBoardValue == UserCellState.Uncovered)
        {
            PrintLine($"[{row},{col}] already uncovered");
            return UncoverResult.Failure;
        }

        if (_systemBoard[row, col] == BoardConstants.BlankValue)
        {
            UncoverBlanks(row, col);
        }
        else
        {
            UncoverInternal(row, col);
        }

        return _systemBoard[row, col] switch
        {
            BoardConstants.BombValue => UncoverResult.Bomb,
            BoardConstants.BlankValue => UncoverResult.Blank,
            _ => UncoverResult.Number,
        };
        ;
    }

    private void UncoverBlanks(int row, int col)
    {
#if DEBUG
        if (!ValidateIndexes(row, col))
        {
            return;
        }
#endif

        UncoverWalk(row, col);
    }

    private void UncoverWalk(int row, int col)
    {
        if (IsOutOfRangeIndex(row) || IsOutOfRangeIndex(col))
        {
            return;
        }

        if (_userBoard[row, col] == UserCellState.Uncovered)
        {
            return;
        }

        UncoverInternal(row, col);
        if (_systemBoard[row, col] != BoardConstants.BlankValue)
        {
            return;
        }

        UncoverWalk(row - 1, col - 1);
        UncoverWalk(row - 1, col);
        UncoverWalk(row - 1, col + 1);
        UncoverWalk(row, col - 1);
        UncoverWalk(row, col + 1);
        UncoverWalk(row + 1, col - 1);
        UncoverWalk(row + 1, col);
        UncoverWalk(row + 1, col + 1);
    }

    private void UncoverInternal(int row, int col)
    {
        _uncoveredNoFields += _userBoard[row, col] == UserCellState.Uncovered ? 0 : 1;
        _userBoard[row, col] = UserCellState.Uncovered;
    }

    public FlagResult Flag(int row, int col)
    {
        if (!ValidateIndexes(row, col))
        {
            return FlagResult.Failure;
        }

        var userBoardValue = _userBoard[row, col];
        if (userBoardValue == UserCellState.Uncovered)
        {
            return FlagResult.AlreadyUncovered;
        }

        if (userBoardValue == UserCellState.Flagged)
        {
            _userBoard[row, col] = UserCellState.Covered;
        }
        else
        {
            _userBoard[row, col] = UserCellState.Flagged;
        }

        return FlagResult.Success;
    }

    private bool ValidateIndexes(int row, int col)
    {
        if (IsOutOfRangeIndex(row) || IsOutOfRangeIndex(col))
        {
            PrintLine($"Input is outside of the board. '{row}' and '{col}' must be in [0, {_gridSize - 1} range]");
            return false;
        }

        return true;
    }

    public int GetUncoveredFieldsNumber()
    {
        return _uncoveredNoFields;
    }

    public int GetTotalFieldsNumber()
    {
        return _gridSize * _gridSize;
    }

    public int GetBombsNumber()
    {
        return _bombsNumber;
    }
}

public enum UncoverResult
{
    Failure = 0,
    Blank,
    Number,
    Bomb,
    Flagged,
}

public enum FlagResult
{
    Failure = 0,
    Success,
    AlreadyUncovered,
}