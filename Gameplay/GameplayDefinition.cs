using UnityEngine;

public static class GameplayDefinition
{
    #region Members

    // Board.
    public static readonly PropPoolType TilesHolderPoolType = PropPoolType.TilesHolder;
    public static readonly float MoveTileHolderTransformsDelayTime = 0.25f;
    public static readonly float MoveNextTileHolderTransformDelayTime = 0.1f;
    public static readonly float TilesOverlappedThreshold = 1.0f;
    public static readonly float SetTilesVisualizationDelayTime = 0.1f;

    // Tiles Holder.
    public static readonly float TilesHolderMovingTime = 0.3f;

    // Tile.
    public static readonly Vector3 TileNormalScale = Vector3.one;
    public static readonly Vector3 TileHighlightedScale = TileNormalScale * 1.1f;
    public static readonly float TileDisappearedScaleTime = 0.15f;
    public static readonly float TileMoveToStackTime = 0.3f;
    public static readonly float TileMoveToBoardTime = 0.3f;
    public static readonly float TileMoveSlowInsideStackTime = 0.2f;
    public static readonly float TileMoveQuickInsideStackTime = 0.1f;
    public static readonly float TileFadeColorTime = 0.3f;

    // Tile Stack.
    public static readonly Vector3[] InStackLocalTilePositions = GetInStackLocalTilePositions();
    public static readonly float CameraOffsetCenter = 0.0f;
    public static readonly float TileStackCenterOffset = GameDefinition.MaxLevelHeight * TileDefinition.TileHalfSize + CameraOffsetCenter;
    public static readonly float DestroyTilesComboDelayTime = TileMoveToStackTime + 0.05f;
    public static readonly float RearrangeTileStackDelayTime = TileDisappearedScaleTime - 0.05f;
    public static readonly float AfterRearrangeTileStackDelayTime = 2.0f;
    public static readonly float FadeTileStackDelayTime = TileMoveToStackTime + 0.25f;
    public static readonly float MoveAfterIntroGameTime = 0.35f;
    public static readonly int ReviveNumberOfTiles = 3;

    // Others.
    public static readonly float DelayTimeForNextWatchAds = 10.0f * 60;
    public static readonly int TutorialTileStackSortingOrder = 1000;

    public static float PlayIntroGameAnimationDelayTime
    {
        get
        {
            int numberOfLayers = GameManager.Instance.CurrentLevel.numberOfLayers;
            float returnDelayTime = MoveTileHolderTransformsDelayTime
                                  + MoveNextTileHolderTransformDelayTime * numberOfLayers
                                  + SetTilesVisualizationDelayTime
                                  + 0.25f;
            return returnDelayTime;
        }
    }

    public static float StartPlayGameDelayTime
    {
        get
        {
            float returnDelayTime = PlayIntroGameAnimationDelayTime + 0.5f;
            return returnDelayTime;
        }
    }

    #endregion Members

    #region Class Methods

    private static Vector3[] GetInStackLocalTilePositions()
    {
        Vector3[] positions = new Vector3[GameDefinition.TileStackSize];
        Vector3 nextPosition = new Vector3(-(GameDefinition.TileStackSize - 1) * TileDefinition.TileHalfSize, 0.1f, 0);

        for (int i = 0; i < GameDefinition.TileStackSize; i++)
        {
            positions[i] = nextPosition;
            nextPosition += Vector3.right * TileDefinition.TileSize;
        }

        return positions;
    }

    #endregion Class Methods
}