using System;

using UnityEngine;

public class TileGUI : MonoBehaviour
{
    #region Members

    protected SpriteRenderer _spriteRender;
    protected int _originalSortingOrder;
    protected bool _isHighlighted;
    protected bool _isUnhighlighted;

    public int Layer => OwnerTile.Layer;
    public int CurrentSortingOrder => _spriteRender.sortingOrder;
    public Vector3 CurrentPosition => OwnerTile.CurrentPosition;
    public Tile OwnerTile { get; protected set; }
    public bool IsInBoard { get; protected set; }
    public Sprite Sprite { get; protected set; }
    public bool IsPickable { get; set; }

    #endregion Members

    #region API Methods

    protected virtual void Awake()
    {
        _spriteRender = gameObject.GetComponent<SpriteRenderer>();
    }

    #endregion API Methods

    #region Class Methods

    public virtual void Init(Tile ownerTile)
    {
        OwnerTile = ownerTile;
        IsInBoard = true;
        transform.position = ownerTile.OriginalPosition;
        transform.localScale = GameplayDefinition.TileNormalScale;
        _spriteRender.color = TileDefinition.EnabledColor;
        _spriteRender.sprite = TileSpriteChanger.Instance.GetTileSprite(ownerTile.TileType);
        _isHighlighted = false;
        _isUnhighlighted = false;
    }

    public virtual void SetOriginalSortingOrder(int sortingOrder)
    {
        _originalSortingOrder = sortingOrder;
        SetSortingOrder(sortingOrder);
    }

    public virtual void SetSortingOrder(int sortingOrder)
    {
        _spriteRender.sortingOrder = sortingOrder;
    }

    public virtual void Highlight()
    {
        if (!_isHighlighted)
        {
            _isHighlighted = true;
            _isUnhighlighted = false;
            transform.localScale = GameplayDefinition.TileHighlightedScale;
            SetSortingOrder(GameplayManager.Instance.TileGUISortingOrder);
        }
    }

    public virtual void Unhighlight()
    {
        if (!_isUnhighlighted)
        {
            _isUnhighlighted = true;
            _isHighlighted = false;
            transform.localScale = GameplayDefinition.TileNormalScale;
            SetSortingOrder(_originalSortingOrder);
        }
    }

    public virtual void Pick()
    {
        IsInBoard = false;
        transform.localScale = GameplayDefinition.TileNormalScale;
        LeanTween.cancel(gameObject);
        SoundManager.Instance.PlaySound(SoundManager.Instance.pickTileSFXClip);
        EventManager.Invoke<TileGUI>(GameEventType.PickATile, this);
    }

    public virtual void SetVisualization()
    {
        _spriteRender.color = IsPickable ? TileDefinition.EnabledColor : TileDefinition.DisabledColor;
    }

    public virtual void MoveToStack(Vector3 inStackPosition)
    {
        SetSortingOrder(GameplayManager.Instance.TileGUISortingOrder);
        OwnerTile.MoveToStack(inStackPosition);
        LeanTween.move(gameObject, inStackPosition, GameplayDefinition.TileMoveToStackTime);
    }

    public virtual void MoveToStackByHints(Vector3 inStackPosition)
    {
        IsInBoard = false;
        IsPickable = true;
        SetVisualization();
        GameplayManager.Instance.TileGUISortingOrder += 1;
        MoveToStack(inStackPosition);
    }

    public virtual void MoveSlowInsideStack(Vector3 inStackPosition)
    {
        LeanTween.cancel(gameObject);
        OwnerTile.MoveToStack(inStackPosition);
        LeanTween.move(gameObject, inStackPosition, GameplayDefinition.TileMoveSlowInsideStackTime);
    }

    public virtual void MoveQuickInsideStack(Vector3 inStackPosition)
    {
        LeanTween.cancel(gameObject);
        OwnerTile.MoveToStack(inStackPosition);
        LeanTween.move(gameObject, inStackPosition, GameplayDefinition.TileMoveQuickInsideStackTime);
    }

    public virtual void MoveToBoard(Action onPlacedOnBoard)
    {
        _isHighlighted = false;
        _isUnhighlighted = false;
        IsInBoard = true;
        SetSortingOrder(GameplayManager.Instance.TileGUISortingOrder);
        OwnerTile.MoveToBoard();
        LeanTween.move(gameObject, OwnerTile.OriginalPosition, GameplayDefinition.TileMoveToBoardTime)
                 .setOnComplete(() => onPlacedOnBoard());
    }

    public virtual void UpdateSprite()
    {
        _spriteRender.color = TileDefinition.EnabledColor;
        _spriteRender.sprite = TileSpriteChanger.Instance.GetTileSprite(OwnerTile.TileType);
    }

    public virtual void Disappear()
    {
        LeanTween.scale(gameObject, Vector2.zero, GameplayDefinition.TileDisappearedScaleTime)
                 .setOnComplete(Destroy);
    }

    public virtual void Destroy()
    {
        OwnerTile = null;
        PoolManager.ReturnObjectToPool(gameObject);
        Destroy(this);
    }

    public virtual void FadeIn()
    {
        LeanTween.color(gameObject, TileDefinition.DisabledColor, GameplayDefinition.TileFadeColorTime);
    }

    public virtual void FadeOut()
    {
        LeanTween.color(gameObject, TileDefinition.EnabledColor, GameplayDefinition.TileFadeColorTime);
    }

    #endregion Class Methods
}