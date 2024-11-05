using Unity.VisualScripting;
using UnityEngine;          // import standard data structures
using UnityEngine.Tilemaps; // use tile asset

/* list out different shapes by providing names to those shapes */
public enum Tetromino
{
    I,
    O,
    T,
    J,
    L,
    S,
    Z,
}

/* custom attribute to display tetrominos in the unity editor */
[System.Serializable]
/* custom data structure to store data for each Tetromino (original tetromino configuration) */
public struct TetrominoData
{
    
    public Tetromino tetromino;     /* select which tetromino values we associate this data for */
    public Tile tile;               /* select which tile we want to draw based on selected tetromino */
    public Vector2Int[] cells { get; private set; }         /* coordinate values to actually draw the shape */
    public Vector2Int[,] wallKicks { get; private set; }    /* check which wall kick we need */

    public void Initialize()
    {
        this.cells = Data.Cells[this.tetromino]; /* lookup associated cells with this tetromino and store into cells */
        this.wallKicks = Data.WallKicks[this.tetromino];
    }
}