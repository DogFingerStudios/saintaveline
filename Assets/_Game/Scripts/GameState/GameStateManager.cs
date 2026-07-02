public enum GameStartMode
{
    NewGame,
    LoadGame    // Also "Continue Game"
}

public static class GameStateManager
{
    public static GameStartMode StartMode = GameStartMode.NewGame;
    public static string SaveFileName { get; set; } = string.Empty;
}
