using UnityEngine;

public enum TilesHolderOrigin
{
    Top,
    Bottom,
    Left,
    Right
}

public class TilesHolderGUI : MonoBehaviour
{
    #region Members

    private TilesHolderOrigin _tilesHolderStartOrigin;

    #endregion Members

    #region Class Methods

    public void SetOrigin(TilesHolderOrigin tilesHolderStartOrigin)
    {
        _tilesHolderStartOrigin = tilesHolderStartOrigin;
        transform.position = _tilesHolderStartOrigin.GetTilesHolderOriginPosition();
    }

    public void MoveOrigin()
    {
        LeanTween.move(gameObject, Vector3.zero, GameplayDefinition.TilesHolderMovingTime)
                 .setEase(LeanTweenType.easeOutCubic);
    }

    public void Destroy()
    {
        PoolManager.ReturnObjectToPool(gameObject);
    }

    #endregion Class Methods
}