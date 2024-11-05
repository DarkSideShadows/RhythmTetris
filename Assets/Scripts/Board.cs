using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

/* contains the tilemap, maintains the state of the game, render and set tiles */
public class Board : MonoBehaviour
{
    public MenuController menuController { get; private set; }
    public SoundEffectController SFXcontroller { get; private set; }

    public Tilemap tilemap { get; private set; }    // reference to tilemap
    public Piece activePiece { get; private set; }  // reference to tetris piece
    public TetrominoData[] tetrominos;  // array of tetromino data to customize tetrominos in unity editor
    public Vector3Int spawnPosition;    // modified in Unity editor, spawn piece at position
    public Vector2Int boardSize = new Vector2Int(10, 20); // defining bounds for valid piece position

    public RectInt Bounds // use built-in RectInt function
    {
        get
        {
        /* the original position is in the center, need to offset by half the board size to put us in the bottom left corner */
            Vector2Int position = new Vector2Int(-this.boardSize.x/2, -this.boardSize.y/2);
            return new RectInt(position, this.boardSize); // position and size
        }
    }

    private AudioSource music;
    private bool musicHasEnded = false;
    public AudioClip getLuckyClip;
    public AudioClip bakaMitaiClip;
    public AudioClip galdinQuayClip;

/* initialize values and populate tetromino data */
    public void Awake() // called automatically when component is initialized
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();
        this.menuController = GetComponentInChildren<MenuController>();
        this.SFXcontroller = GetComponentInChildren<SoundEffectController>();
        /* populate tetrominos array with tetrominodata */
        for (int i = 0; i < this.tetrominos.Length; i++)
            this.tetrominos[i].Initialize();
        if (music == null)
            music = gameObject.AddComponent<AudioSource>();
    }

/* when our game starts, we can spawn a piece AND choose which song to play */
    public void Start()
    {
        SpawnPiece();
        PlayMusic();
    }

    public void PlayMusic()
    {
        // get song name from playerprefs
        string selectedSong = PlayerPrefs.GetString("SelectedSong", "");
        float bpm = PlayerPrefs.GetFloat("SelectedBPM", 116f);

        // select AudioClip based on song name saved in player prefs
        switch (selectedSong)
        {
            case "Get Lucky":
                music.clip = getLuckyClip;
                break;
            case "Baka Mitai":
                music.clip = bakaMitaiClip;
                break;
            case "Galdin Quay":
                music.clip = galdinQuayClip;
                break;
            default:
                Debug.LogError("No valid song found in PlayerPrefs.");
                return;
        }

        if (music.clip != null)
            music.Play();
    }

    public void Update()
    {
        // check if music ended -> go to game over screen
        if (!musicHasEnded && music.time >= music.clip.length)
        {
            musicHasEnded = true;
            GameOver();
        }
    }

/* pick an element from our array to spawn */
    public void SpawnPiece()
    {
        /* pick some random data for tetromino */
        int random = Random.Range(0, this.tetrominos.Length);
        TetrominoData data = this.tetrominos[random];
        /* initialize piece with data */
        this.activePiece.Initialize(this, this.spawnPosition, data);
        /* set piece on tile map if valid */
        if (IsValidPosition(this.activePiece, this.spawnPosition))
            Set(this.activePiece);
        else // when spawn piece is in invalid position, game is over!
            GameOver();
    }

    private void GameOver()
    {
        this.tilemap.ClearAllTiles(); // clear the entire board
        SFXcontroller.PlayGameOverSound();
        menuController.GameOver();    // go to game over screen
    }

/* set piece in tetris board */
    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

/* clear piece in tetris board */
    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

/* on the game board, check if the piece position can exist */
    public bool IsValidPosition(Piece piece, Vector3Int newPosition)
    {
        RectInt bounds = this.Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + newPosition; // get tile position
            /* tile is occupying existing tile space */
            if (this.tilemap.HasTile(tilePosition))
                return false;
            /* out of bounds? */
            if (!bounds.Contains((Vector2Int) tilePosition))
                return false;
        }

        return true;
    } 

/* clear full line from board */
    public void ClearLines()
    {
        /* loop through every row in our tile map, determine if every column is full
         * if every column is full, the row is full. */
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;  // bottom row to top row

        while (row < bounds.yMax) // check for lines in each row
        {
            if (IsLineFull(row))    // is this row full?
            {
                LineClear(row);     // clear the line
                SFXcontroller.PlaylineBreakSound();
            }
            else
                row++;              // move on to next row
        }
    }

    public bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++) // iterating from left to right
        {
            Vector3Int position = new Vector3Int(col, row, 0); // grab position of column (x, y, z)
            if (!this.tilemap.HasTile(position)) // tile is empty, line is not full
                return false;
        }

        return true;
    }

    private void LineClear(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++) // iterating from left to right
        {
            Vector3Int position = new Vector3Int(col, row, 0); // grab position of column (x, y, z)
            this.tilemap.SetTile(position, null); // set tile at that position to null
        }

        while (row < bounds.yMax) // have every row above fall down
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++) // check each column in each row
            {
                Vector3Int position = new Vector3Int(col, row+1, 0); // set position to row above
                TileBase tileAbove = this.tilemap.GetTile(position); // get the tile above

                position = new Vector3Int(col, row, 0); // change position to current row
                this.tilemap.SetTile(position, tileAbove); // change the tile
            }

            row++;
        }
    }

}