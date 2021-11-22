using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class MazeGenerator : MonoBehaviour
{
    #region Fields
    // Inputted dimensions of the maze
    [SerializeField]
    private int Width;
    [SerializeField]
    private int Height;

    // Boolean determining whether the maze generation is instant or not
    [SerializeField]
    private bool isInstant;
    
    [SerializeField]
    private MazeGenerationData MazeGenerationData;

    // How many cells is the algorithm allowed to process before moving on and drawing out the calculated cells
    [SerializeField]
    private int TargetSteps;

    // A 2-dimensional array containing reference to all the cells
    [SerializeField]
    private MazeCell[,] Maze;

    // A stack representing the travel history the algorithm walks through during its calculations
    [SerializeField]
    private Stack<MazeCell> VisitedCells;

    // How many cells have been visited
    [SerializeField]
    private int VisitedCount;

    // Random class
    private Random rnd;

    // The cell the algorithm is currently on
    private MazeCell currentCell;

    // The amount of time the coroutine needs to wait before moving onto the next cell
    [SerializeField]
    private float delay;
    #endregion

    #region Delegates and Events
    // Delegate and event for when a tile needs to be rendered
    public delegate void RenderTile(TileType tile, MazeCell currentCell, NeighborDirection? direction);
    public static event RenderTile OnRenderTile;
    #endregion

    void Start()
    {
        // Instantiate the random class
        rnd = new Random();

        // Subscribe to events
        GenerationUIHandler.OnGenerateMaze += CreateMaze;
        MazeUIHandler.OnClearMaze += ClearMaze;
        CellRenderer.OnMazeCleared += RegenerateMaze;
    }

    public void CreateMaze(MazeGenerationData data)
    {
        // Save the MazeGenerationData for potential regeneration
        MazeGenerationData = data;

        // Set the incoming width and height of the maze
        this.Width = data.Width;
        this.Height = data.Height;
        this.isInstant = data.isInstant;

        // Create the maze
        Maze = new MazeCell[Width, Height];

        // Create the maze stack
        VisitedCells = new Stack<MazeCell>();

        // Set the VisitedCount to 0
        VisitedCount = 0;

        /* 
         * If the requested generation isn't instant, set the target steps and delay for step-wise generation or custom target steps and delay. 
         * Otherwise, set the target steps high enough to instantly generate the maze
        */
        if (!isInstant)
        {
            TargetSteps = data.Steps != null ? data.Steps.Value : 1;
            delay = data.Delay != null ? data.Delay.Value : 0.01f;
        }
        else
        {
            TargetSteps = (Width*Height)*3;
            delay = 0;
        }

        // Populate the maze array
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Maze[x, y] = new MazeCell()
                {
                    X = x,
                    Y = y
                };
            }
        }

        // Set all the neighbors of the present cells
        foreach (MazeCell cell in Maze)
        {
            FindNeighbors(cell);
        }

        // Start generating the maze
        GenerateMaze();
    }

    private void GenerateMaze()
    {
        // Create the starting cell
        MazeCell startingCell = Maze[0, 0];

        // Push onto the history stack, increase the count of visited tiles and set the starting tile to Visited
        VisitedCells.Push(startingCell);
        VisitedCount++;
        startingCell.Visited = true;

        // Set the current cell as starting cell
        currentCell = startingCell;

        // Start the algorithm
        StartCoroutine(Generate());
    }

    IEnumerator Generate()
    {
        // Set the amount of steps to 0
        int steps = 0;
        // As long as not all cells haven't been visited during backtracking yet, keep calculating
        while (VisitedCells.Count > 0)
        {
            // If the current step count exceeds the requested steps, reset the steps and wait for the given delay
            if (steps >= TargetSteps)
            {
                steps = 0;
                yield return new WaitForSeconds(delay);
            }
            // Continue as long as the current cell is set
            if (currentCell != null)
            {
                // If there are available neighbors to move to
                if (currentCell.Neighbors.Length > 0 && currentCell != null)
                {
                    // Put all the present neighbors in a temporary array
                    MazeCell[] neighbors = currentCell.Neighbors.Where(n => n != null && !n.Visited).ToArray();
                    // As long as this list is not null
                    if (neighbors.Length > 0)
                    {
                        // Call the event to render the tile
                        OnRenderTile(TileType.GENERATED, currentCell, null);

                        // Randomly select a new neighbor to move to
                        currentCell = neighbors[rnd.Next(0, neighbors.Length)];

                        // Push the newly selected cell to the history stack, increase the visited count and mark the cell as visited
                        VisitedCells.Push(Maze[currentCell.X, currentCell.Y]);
                        VisitedCount++;
                        currentCell.Visited = true;
                    }
                    // Start backtracking when no non-visited neighbors are available
                    else
                    {
                        // Render the tile for the current cell
                        OnRenderTile(TileType.VISITED, currentCell, null);

                        // Save the previous cell and remove it from the stack
                        MazeCell previousCell = new MazeCell();
                        previousCell = VisitedCells.Pop();

                        // As long as the end has not been reached yet, set the current cell to the next position in the stack
                        if (VisitedCells.Count != 0)
                            currentCell = VisitedCells.Peek();

                        // Decide the direction the algorithm is moving
                        NeighborDirection direction = (NeighborDirection)Array.IndexOf(previousCell.Neighbors, currentCell);

                        // Render the new cell as visited
                        OnRenderTile(TileType.VISITED, currentCell, direction);
                    }
                }
            }
            steps++;
        }
    }

    // Stop the calculation and set all values back to their defaults
    private void ClearMaze()
    {
        StopAllCoroutines();

        Width = 0;
        Height = 0;

        TargetSteps = 0;

        Maze = null;
        VisitedCells = null;

        VisitedCount = 0;

        currentCell = null;

        delay = 0;
    }

    // Run the algorithm again using the same data as set before
    private void RegenerateMaze()
    {
        ClearMaze();
        CreateMaze(MazeGenerationData);
    }

    private void FindNeighbors(MazeCell cell)
    {
        // Condition to check whether a cell is within bounds
        Func<int, int, bool> checkCoordsWithinBounds = (int x, int y) =>
        {
            if ((x >= 0 && x < Width) && (y >= 0 && y < Height))
                return true;
            else
                return false;
        };

        cell.Neighbors = new MazeCell[4];
        
        // Set the neighbor to the north as long as it is within bounds
        if (checkCoordsWithinBounds(cell.X, cell.Y - 1))
        {
            MazeCell north = Maze[cell.X, cell.Y - 1];
            if (!north.Visited) cell.Neighbors[(int)NeighborDirection.NORTH] = north;
        }

        // Set the neighbor to the south as long as it is within bounds
        if (checkCoordsWithinBounds(cell.X, cell.Y + 1))
        {
            MazeCell south = Maze[cell.X, cell.Y + 1];
            if (!south.Visited) cell.Neighbors[(int)NeighborDirection.SOUTH] = south;
        }

        // Set the neighbor to the east as long as it is within bounds
        if (checkCoordsWithinBounds(cell.X + 1, cell.Y))
        {
            MazeCell east = Maze[cell.X + 1, cell.Y];
            if (!east.Visited) cell.Neighbors[(int)NeighborDirection.EAST] = east;
        }

        // Set the neighbor to the west as long as it is within bounds
        if (checkCoordsWithinBounds(cell.X - 1, cell.Y))
        {
            MazeCell west = Maze[cell.X - 1, cell.Y];
            if (!west.Visited) cell.Neighbors[(int)NeighborDirection.WEST] = west;
        }
    }
}

// The types of tiles
public enum TileType
{
    GENERATED,
    VISITED
}
