using System.Numerics;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

/* individual piece = controlling, moving and rotating */
public class Piece : MonoBehaviour
{
    public SoundEffectController SFXcontroller;
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }

    public Vector3Int[] cells { get; private set; } // treat cells as tiles
    public Vector3Int position { get; private set; } // tilemaps use vector3int
    public int rotationIndex { get; private set; } // track rotation index (piece configuration)

    public float stepDelay = 1f; // 1 second
    public float lockDelay = 0.5f;

    private float stepTime;
    private float lockTime;

/* use on tetris piece throughout game, reinitialize with correct data communicate to game board to move piece */
    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.data = data;
        this.position = position;
        this.rotationIndex = 0; // default rotation position
        this.stepTime = Time.time + this.stepDelay; // step time is 1 second past current time (when compared with curr time, determines whether we step or not)
        this.lockTime = 0f; // when lockTime exceeds lockDelay, lock piece

        this.SFXcontroller = GetComponentInChildren<SoundEffectController>();

        /* array of cells to modify tetromino position */
        if (this.cells == null)
            this.cells = new Vector3Int[data.cells.Length];
        /* populate cells array with tetromino data */
        for (int i = 0; i < this.cells.Length ; i++)
            this.cells[i] = (Vector3Int) data.cells[i];
    }

/* handle player input, synchronization with the beats */
    private void Update()
    { 
        this.board.Clear(this);

        this.lockTime = Time.deltaTime; // increase lock time (by deltaTime) when moving piece
        /* handle movement */
        if (Input.GetKeyDown(KeyCode.Q))        // rotate left
            Rotate(-1);
        else if (Input.GetKeyDown(KeyCode.E))   // rotate right
            Rotate(1);

        if (Input.GetKeyDown(KeyCode.A))        // move piece left
            Move(Vector2Int.left);
        else if (Input.GetKeyDown(KeyCode.D))   // move piece right
            Move(Vector2Int.right);

        if (Input.GetKeyDown(KeyCode.S))        // move piece down one row
            Move(Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.Space))    // hard drop -> move to bottom row
            HardDrop();

        if (Time.time >= this.stepTime)         // if current time is greater than step time, move the piece
            Step();

        this.board.Set(this);
    }

    private void Step()
    {
        this.stepTime = Time.time + this.stepDelay; // push time into future (adjusting for calculation)
        Move(Vector2Int.down); // move the piece down 1 unit
        if (this.lockTime >= this.lockDelay) // check for a lock, meaning that the piece has stopped (blocked by underlying piece, or reach border)
            Lock();
    }

/* continuously move down until you can't anymore */
    private void HardDrop()
    {
        while(Move(Vector2Int.down))
            continue;

        Lock(); // lock in place
    }

    private void Lock()
    {
        this.SFXcontroller.PlayLockSound();
        this.board.Set(this); // lock the piece into place
        this.board.ClearLines(); // try to clear any lines that can be cleared
        this.board.SpawnPiece(); // create a new piece to play with
    }

/* move X amount in X axis, Y amount in Y axis on the game board */
    private bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = this.position; // calculate new position in order to check if valid
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = this.board.IsValidPosition(this, newPosition);

        if (valid)
        {
            this.position = newPosition;
            this.lockTime = 0f; // reset lock time to avoid locking piece
        }

        return valid;
    }

/* rotate piece in specified direction */
    private void Rotate(int direction)
    {
        int originalRotation= this.rotationIndex; // store current rotation index
        this.rotationIndex += Wrap(this.rotationIndex + direction, 0, 4); // update rotation index

        ApplyRotationMatrix(direction); // apply rotation

        /* wallkick tests, if fail, revert rotation */
        if (!TestWallKicks(this.rotationIndex, direction))
        {
            this.rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction); // reverse rotation direction
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        /* apply each rotation matrix to each cell in our piece */
        for (int i = 0; i < this.cells.Length; i++) 
        {
            UnityEngine.Vector3 cell = this.cells[i];

            int x , y; // new coordinates after rotation

            /* rotation matrix */
            switch (this.data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f; // subtract half a unit because the cell rotates around a different point
                    x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
                
                default:
                    x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
            }

            this.cells[i] = new Vector3Int(x, y, 0);
        }
    }

    /* test if we need wallkick data */
    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        /* test each wallkick case */
        for (int testIndex = 0; testIndex < this.data.wallKicks.GetLength(1); testIndex++)
        {
            Vector2Int translation = this.data.wallKicks[wallKickIndex, testIndex]; // grab translation from dataset

            if (Move(translation))
                return true;
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        /* pattern found on Tetris wiki: rotation index is always * 2, but if we're rotating negative, it is always --, else nothing */
        int wallKickIndex = rotationIndex * 2;
        if (rotationDirection < 0)
            wallKickIndex--;
        // can be out of bounds, need to wrap
        return Wrap(wallKickIndex, 0, this.data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
            return max - (min - input) % (max - min);
        else
            return min + (input - min) % (max - min);
    }
}