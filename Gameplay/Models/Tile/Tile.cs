using UnityEngine;

public enum TileType
{
    Tile1,
    Tile2,
    Tile3,
    Tile4,
    Tile5,
    Tile6,
    Tile7,
    Tile8,
    Tile9,
    Tile10,
    Tile11,
    Tile12,
    Tile13,
    Tile14,
    Tile15,
    Tile16,
    Tile17,
    Tile18,
    Tile19,
    Tile20,
}

public class Tile
{
    #region Members

    protected readonly int layer;
    protected readonly Vector3 originalPosition;
    protected Vector3 _currentPosition;

    public int Layer => this.layer;
    public Vector3 OriginalPosition => this.originalPosition;
    public Vector3 CurrentPosition => _currentPosition;
    public TileType TileType { get; set; }

    #endregion Members

    #region Class Methods

    public Tile(int layer, Vector3 position)
    {
        this.layer = layer;
        this.originalPosition = position;
        _currentPosition = position;
    }

    public void MoveToStack(Vector3 inStackPosition)
    {
        _currentPosition = inStackPosition;
    }

    public void MoveToBoard()
    {
        _currentPosition = this.originalPosition;
    }

    public TutorialTile ToTutorialTile()
    {
        var tutorialTile = new TutorialTile(this.layer, this.originalPosition);
        tutorialTile.TileType = TileType;
        return tutorialTile;
    }

    #endregion Class Methods
}