public class GameManager
{
    // Create a local reference to the Game Manager's instance
    private static GameManager instance;

    // When another scripts asks for access to the GameManager's instance, either give the local instance or create a new one
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = new GameManager();
            return instance;
        }
    }

    // Create enums for the previous and current game state
    public GameStates previousGameState;
    public GameStates GameState;

    // Create the delegate and event for when a game state is changed
    public delegate void GameStateChanged(GameStates previousGameState, GameStates newGameState);
    public static event GameStateChanged OnGameStateChanged;

    void Start()
    {
        // Set the game state the application boots up in, namely the generation menu state
        ChangeGameState(GameStates.GENERATION_MENU);
    }

    public void ChangeGameState(GameStates newGameState)
    {
        // Save the current game state as the previous game state
        previousGameState = GameState;

        // Set the current game state to the inputted new game state
        GameState = newGameState;

        // Fire the event with the previous game state and the new game state
        OnGameStateChanged(previousGameState, GameState);
    }
}

// Enum for the different game states
public enum GameStates
{
    GENERATION_MENU,
    MAZE_GENERATION
}