using UnityEngine;

public static class TileDefinition
{
    #region Members

    public const string LayerMaskName = "Tile";
    public const float TileHalfSize = 0.6f;
    public const float TileSize = TileHalfSize * 2;
    public const int InitialSortingLayer = 10;

    public static readonly Color DisabledColor = new Color(0.5f, 0.5f, 0.5f);
    public static readonly Color EnabledColor = new Color(1.0f, 1.0f, 1.0f);

    #endregion Members
}