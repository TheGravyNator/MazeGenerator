using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MazePositioner : MonoBehaviour
{
    #region Fields
    // The positions of the screen and maze corners for later scaling calculations
    private Vector3 ScreenBottomLeft;
    private Vector3 MazeTopRight;
    private Vector3 MazeZero;

    // The dimensions of the maze and the screen in world space
    private float MazeHeight;
    private float MazeWidth;
    private float ScreenHeight;
    private float ScreenWidth;

    // A value to adjust how much space the UI takes in on the maze generation screen
    [SerializeField]
    private float UISpace;

    // References to the Grid and Tilemap components of the maze
    [SerializeField]
    private Grid Maze;
    [SerializeField]
    private Tilemap TileMap;

    // Padding between the edges of the screen and the edges of the maze
    [SerializeField]
    private float Padding;
    #endregion

    void Start()
    {
        // Subscribe to the Generate and Clear events for the maze
        GenerationUIHandler.OnGenerateMaze += ScaleMazeToScreen;
        MazeUIHandler.OnClearMaze += ResetScale;
    }

    private void ScaleMazeToScreen(MazeGenerationData data)
    {
        // Get the maze and screen vectors for later calculations
        ScreenBottomLeft = Camera.main.ScreenToWorldPoint(Vector3.zero);
        MazeZero = TileMap.CellToWorld(Vector3Int.zero);
        MazeTopRight = TileMap.CellToWorld(new Vector3Int(data.Width, data.Height, 0));

        // Position the bottom left point of the maze in the bottom left of the screen
        Maze.transform.position = new Vector3(ScreenBottomLeft.x, ScreenBottomLeft.y) + new Vector3(Padding, Padding, 0);

        // Get the screen dimensions in world space and account for padding 
        ScreenHeight = (Camera.main.orthographicSize) - Padding;
        ScreenWidth = ((Camera.main.orthographicSize) * Camera.main.aspect) - Padding;

        // Calculate the dimensions of the maze in world space
        MazeHeight = MazeTopRight.y - MazeZero.y;
        MazeWidth = MazeTopRight.x - MazeZero.x;

        // Calculate the scale factors to match the maze dimensions with the screen dimensions
        float heightscale =  ScreenHeight / MazeHeight;
        float widthscale =  ScreenWidth*UISpace / MazeWidth;

        // Check which scale factor is smaller, to make certain the maze always fits the screen size
        float scale = heightscale > widthscale ? widthscale : heightscale;

        // Set the maze's scale to the resulting scale factor
        Maze.transform.localScale = new Vector3(scale, scale);
    }

    private void ResetScale()
    {
        // Set the scale of the maze back to default
        Maze.transform.localScale = Vector3.one;
    }
}
