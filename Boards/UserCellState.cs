namespace Minesweeper.Boards;

internal enum UserCellState
{
    Covered = 0,
    Uncovered,
    Flagged,
}

internal static class UserCellStateExtensions
{
    internal static string ToPrintSymbol(this UserCellState cellState, int cellValue)
    {
        return cellState switch
        {
            UserCellState.Covered => "?",
            UserCellState.Uncovered => GetCellValueSymbol(cellValue),
            UserCellState.Flagged => "X",
            _ => throw new ArgumentOutOfRangeException(nameof(cellState), cellState, null)
        };
    }

    private static string GetCellValueSymbol(int cellValue)
    {
        if (cellValue == BoardConstants.BombValue)
        {
            return "\ud83d\udca3";
        }

        if (cellValue == BoardConstants.EmptyValue)
        {
            return " ";
        }

        return cellValue.ToString();
    }
}