namespace Minesweeper;

public static class IoManager
{
    public static int GetNonNegativeIntegerInput(string message)
    {
        while (true)
        {
            Print(message);
            var input = Console.ReadLine()?.Trim();
            if (int.TryParse(input, out var row) && row >= 0)
            {
                return row;
            }

            PrintLine($"Invalid number: '{input}'. Must be a non-negative integer.\n");
        }
    }

    public static void Print(string message = "")
    {
        Console.Write(message);
    }

    public static void PrintLine(string message = "")
    {
        Console.WriteLine(message);
    }
}