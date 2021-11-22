using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCell
{
    // X and Y coordinates
    public int X { get; set; }
    public int Y { get; set; }

    // Boolean indicating whether the cell has been visited 
    public bool Visited { get; set; }

    // Array including all this cell's neighboring cells
    public MazeCell[] Neighbors { get; set; }
}

// Enum representing the direction of the neighbors of a cell
public enum NeighborDirection
{
    NORTH,
    EAST,
    SOUTH,
    WEST
}
