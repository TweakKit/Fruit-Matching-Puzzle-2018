using UnityEngine;

public class TutorialTileGUI : TileGUI
{
    #region Members

    private new TutorialTile OwnerTile
    {
        get => base.OwnerTile as TutorialTile;
        set => base.OwnerTile = value;
    }

    #endregion Members

    #region API Methods

    protected override void Awake()
    {
        base.Awake();
        EventManager.AddListener(TutorialEventType.EnableTilePickability, OnEnableTilePickability);
        EventManager.AddListener(TutorialEventType.DisableTilePickability, OnDisableTilePickability);
    }

    #endregion API Methods

    #region Class Methods

    public override void SetOriginalSortingOrder(int sortingOrder)
    {
        if (OwnerTile.TutorialTileLevel == TutorialTileLevel.TutorialLevel1 && OwnerTile.IsPickabilityEnabled)
            sortingOrder += GameplayDefinition.TutorialTileStackSortingOrder;

        base.SetOriginalSortingOrder(sortingOrder);
    }

    public override void Highlight()
    {
        if (!OwnerTile.IsPickabilityEnabled)
            return;

        base.Highlight();

        if (OwnerTile.TutorialTileLevel == TutorialTileLevel.TutorialLevel1)
            SetSortingOrder(GameplayManager.Instance.TileGUISortingOrder + GameplayDefinition.TutorialTileStackSortingOrder);
    }

    public override void Pick()
    {
        if (!OwnerTile.IsPickabilityEnabled)
            return;

        base.Pick();

        if (OwnerTile.TutorialTileLevel == TutorialTileLevel.TutorialLevel1)
            EventManager.Invoke(TutorialEventType.PickATileTutorial1);
        else if (OwnerTile.TutorialTileLevel == TutorialTileLevel.TutorialLevel2)
            EventManager.Invoke(TutorialEventType.PickATileTutorial2);
    }

    public override void MoveToStack(Vector3 inStackPosition)
    {
        base.MoveToStack(inStackPosition);

        if (OwnerTile.TutorialTileLevel == TutorialTileLevel.TutorialLevel1)
            SetSortingOrder(GameplayManager.Instance.TileGUISortingOrder + GameplayDefinition.TutorialTileStackSortingOrder);
    }

    public override void Destroy()
    {
        OwnerTile = null;
        EventManager.RemoveListener(TutorialEventType.EnableTilePickability, OnEnableTilePickability);
        EventManager.RemoveListener(TutorialEventType.DisableTilePickability, OnDisableTilePickability);
        base.Destroy();
    }

    private void OnEnableTilePickability() => OwnerTile.IsPickabilityEnabled = true;
    private void OnDisableTilePickability() => OwnerTile.IsPickabilityEnabled = false;

    #endregion Class Methods
}