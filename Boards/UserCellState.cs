namespace Minesweeper.Boards;

internal enum UserCellState
{
    Covered = 0,
    Uncovered,
    Flagged,
}

internal static class CellStateExtensions
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

    internal static string GetCellValueSymbol(int cellValue)
    {
        if (cellValue == BoardConstants.BombValue)
        {
            return "*";
        }

        if (cellValue == BoardConstants.BlankValue)
        {
            return " ";
        }

        return cellValue.ToString();
    }
}