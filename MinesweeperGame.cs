namespace Minesweeper;

public class MinesweeperGame
{
    public void Play()
    {
        IoManager.Print("Welcome to Minesweeper!\n");

        var game = new GameManager();
        game.Initialise();
        game.Play();
    }
}