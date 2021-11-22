using UnityEngine;

public class MazeUIHandler : MonoBehaviour
{
    #region Fields
    // Reference to the MazeGeneration UI object
    [SerializeField]
    private GameObject MazeControls;
    #endregion

    #region Delegates and Events
    // Delegate and event for when the tilemap and data of the maze needs to be cleared
    public delegate void ClearMaze();
    public static event ClearMaze OnClearMaze;

    // Delegate and event for when the maze is regenerated from the Maze Generation menu
    public delegate void RegenerateMaze();
    public static event RegenerateMaze OnRegenerateMaze;
    #endregion

    void Start()
    {
        // Subscribe to the game state machine from the GameManager
        GameManager.OnGameStateChanged += OpenMazeGenerationControls;
    }

    public void OnBack()
    {
        // Set the game state to Generation Menu
        GameManager.Instance.ChangeGameState(GameStates.GENERATION_MENU);
    }

    public void OnRegenerate()
    {
        // Fire off the maze regeneration event
        OnRegenerateMaze();
    }

    private void OpenMazeGenerationControls(GameStates currentGameState, GameStates newGameState)
    {

        // Set the Maze Generation Menu UI on or off depending on the game state
        if (newGameState == GameStates.MAZE_GENERATION)
            MazeControls.gameObject.SetActive(true);
        else if (newGameState == GameStates.GENERATION_MENU)
        {
            // Fire off the Clear Maze event
            OnClearMaze();
            MazeControls.gameObject.SetActive(false);
        }
    }
}
