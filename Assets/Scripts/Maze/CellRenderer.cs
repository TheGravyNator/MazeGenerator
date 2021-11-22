using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CellRenderer : MonoBehaviour
{
    #region Fields
    // Reference to the Tilemap
    [SerializeField]
    private Tilemap tilemap;

    // The two possible tiles
    [SerializeField]
    private Tile GeneratedTile;
    [SerializeField]
    private Tile VisitedTile;
    #endregion

    #region Delegates and Events
    // Delegate and event for when the maze is cleared
    public delegate void MazeCleared();
    public static event MazeCleared OnMazeCleared;
    #endregion

    void Start()
    {
        // Subscribe to Render Tile event
        MazeGenerator.OnRenderTile += RenderTile;

        // Subscribe to Clear and Regenerate events
        MazeUIHandler.OnClearMaze += ClearMaze;
        MazeUIHandler.OnRegenerateMaze += RegenerateMaze;
    }

    private void ClearMaze()
    {
        // Clear all the tiles within the tilemap
        tilemap.ClearAllTiles();
    }

    private void RegenerateMaze()
    {
        // Clear all the tiles within the tilemap
        ClearMaze();

        // Fire off the event to indicate the tiles have been cleared
        OnMazeCleared();
    }

    private void RenderTile(TileType type, MazeCell cell, NeighborDirection? direction)
    {
        /*
         * Multiplications with 2 done in these coordinate systems are done because all the walls between cells are also separate tiles on the tilemap.
         */

        // When a cell is first created, set the tile to the Generated tile
        if (type == TileType.GENERATED)
            tilemap.SetTile(new Vector3Int(cell.X * 2, cell.Y * 2, 0), GeneratedTile);
        // When a cell is visited through the backtracker, set the tile to the Visited tile and set the wall cell inbetween the two cells to create a path
        else if (type == TileType.VISITED)
        {
            if (direction == NeighborDirection.NORTH)
                tilemap.SetTile(new Vector3Int((cell.X * 2), (cell.Y * 2) + 1, 0), VisitedTile);
            else if (direction == NeighborDirection.SOUTH)
                tilemap.SetTile(new Vector3Int((cell.X * 2), (cell.Y * 2) - 1, 0), VisitedTile);
            else if (direction == NeighborDirection.EAST)
                tilemap.SetTile(new Vector3Int((cell.X * 2) - 1, (cell.Y * 2), 0), VisitedTile);
            else if (direction == NeighborDirection.WEST)
                tilemap.SetTile(new Vector3Int((cell.X * 2) + 1, (cell.Y * 2), 0), VisitedTile);
            else 
                tilemap.SetTile(new Vector3Int(cell.X * 2, cell.Y * 2, 0), VisitedTile);
        }
    }
}
