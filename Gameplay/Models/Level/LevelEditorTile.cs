public class LevelEditorTile
{
    #region Members

    public int indexX;
    public int indexY;
    public int layer;

    #endregion Members

    #region Class Methods

    public LevelEditorTile(int indexX, int indexY, int layer)
    {
        this.indexX = indexX;
        this.indexY = indexY;
        this.layer = layer;
    }

    #endregion Class Methods
}