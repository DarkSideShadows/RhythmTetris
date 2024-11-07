using System.Numerics;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : MonoBehaviour
{
    public Tile tile; // which tile we render for the ghost piece
    public Board board;
    public Piece trackingPiece;

    public Tilemap tilemap { get; private set; }
    public Vector3Int[] cells { get; private set; } // treat cells as tiles
    public Vector3Int position { get; private set; } // tilemaps use vector3int

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>(); // establish tilemap
        this.cells = new Vector3Int[4];
    }

    // called after all other updates (updated after we update our active piece)
    private void LateUpdate()
    {
        Clear();
        Copy();
        Drop();
        Set();
    }

    // clear the piece
    private void Clear()
    {
        // "this" is the ghost piece
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    // copy the cell data of tracking piece
    private void Copy()
    {
        for (int i = 0; i < this.cells.Length; i++)
        {
            // assign cell data equal to tracking piece data
            this.cells[i] = this.trackingPiece.cells[i];
        }
    }

    // hard drop the ghost piece
    private void Drop()
    {
        Vector3Int position = this.trackingPiece.position;
        // clear tracking piece to prevent conflicting with ghost piece
        this.board.Clear(this.trackingPiece);

        int currentRow = position.y;
        int bottom = -this.board.boardSize.y / 2 - 1; // bottom of the board

        // loop through every row in board (top -> bottom)
        for (int row = currentRow; row >= bottom; row--)
        {
            // find valid position to put ghost piece
            position.y = row;

            if (this.board.IsValidPosition(this.trackingPiece, position))
                // update ghost piece to valid position
                this.position = position;
            else
                // cant go any further, prevent more rows from being tested
                break;
        }

        this.board.Set(this.trackingPiece);
    }

    // set the ghost piece
    private void Set()
    {
        // "this" is the ghost piece
        for (int i = 0; i < this.cells.Length; i++)
        {
            Vector3Int tilePosition = this.cells[i] + this.position;
            this.tilemap.SetTile(tilePosition, this.tile);
        }
    }

}
