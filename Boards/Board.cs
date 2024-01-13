namespace Minesweeper.Boards;
using static IoManager;

public class Board
{
    private readonly int _gridSize;
    private readonly int _bombsNumber;
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
        for (int i = 0; i < _gridSize; ++i)
        {
            for (int j = 0; j < _gridSize; ++j)
            {
                Print(_userBoard[i, j].ToPrintSymbol(_systemBoard[i, j]));
            }
            PrintLine();
        }
    }

    public void PrintSystem()
    {
        for (int i = 0; i < _gridSize; ++i)
        {
            for (int j = 0; j < _gridSize; ++j)
            {
                Print(_userBoard[i, j].ToPrintSymbol(_systemBoard[i, j]));
            }
            PrintLine();
        }
    }

    public UncoverResult Uncover(int row, int col)
    {
        if (IsOutOfRangeIndex(row) || IsOutOfRangeIndex(col))
        {
            PrintLine($"Input is outside of the board. '{row}' and '{col}' must be in [0, {_gridSize - 1} range]");
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

        _userBoard[row, col] = UserCellState.Uncovered;
        ++_uncoveredNoFields;

        var result =  _systemBoard[row, col] switch
        {
            BoardConstants.BombValue => UncoverResult.Bomb,
            BoardConstants.EmptyValue => UncoverResult.Blank,
            _ => UncoverResult.Number,
        };

        if (result == UncoverResult.Blank)
        {
            // TODO: Uncover all
        }

        return result;
    }

    public FlagResult Flag(int row, int col)
    {
        if (IsOutOfRangeIndex(row) || IsOutOfRangeIndex(col))
        {
            PrintLine($"Input is outside of the board. '{row}' and '{col}' must be in [0, {_gridSize - 1} range]");
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