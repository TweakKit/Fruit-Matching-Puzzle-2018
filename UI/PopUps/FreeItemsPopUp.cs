using UnityEngine;
using UnityEngine.UI;

public enum FreeItemType
{
    RewindItem,
    HintItem,
    ReloadItem,
    None
}

public class FreeItemsPopUp : PopUp
{
    #region Members

    [SerializeField]
    private Image _freeItemBackgroundImage;
    [SerializeField]
    private Image _freeItemIconImage;
    [SerializeField]
    private Sprite[] _freeItemBackgroundSprites;
    [SerializeField]
    private Sprite[] _freeItemIconSprites;
    [SerializeField]
    private Vector2[] _freeItemIconPositions;
    private FreeItemType _freeItemType;

    #endregion Members

    #region API Methods

    #endregion API Methods

    #region Class Methods

    public void Init(FreeItemType freeItemType)
    {
        _freeItemType = freeItemType;
        _freeItemBackgroundImage.sprite = _freeItemBackgroundSprites[(int)_freeItemType];
        _freeItemIconImage.sprite = _freeItemIconSprites[(int)_freeItemType];
        _freeItemIconImage.rectTransform.anchoredPosition = _freeItemIconPositions[(int)_freeItemType];
    }

    public void WatchAds()
    {
        Close();

        if (AdmobController.Instance.CanShowRewardedVideoAd())
            AdmobController.Instance.ShowRewardVideoAd(OnWatchedRewardedAd, OnStoppedWatchingRewardedAd);
        else
            parentScene.OpenAlertPopUp(UIDefinition.AlertTitle, UIDefinition.AdsCanNotBeLoadedDescription);
    }

    private void OnWatchedRewardedAd()
    {
        switch (_freeItemType)
        {
            case FreeItemType.RewindItem:
                PlayerPrefsManager.Instance.SavedData.RewindTurns += 1;
                break;

            case FreeItemType.HintItem:
                PlayerPrefsManager.Instance.SavedData.HintTurns += 1;
                break;

            case FreeItemType.ReloadItem:
                PlayerPrefsManager.Instance.SavedData.ReloadTurns += 1;
                break;
        }
    }

    private void OnStoppedWatchingRewardedAd()
    {
        parentScene.OpenAlertPopUp(UIDefinition.AlertTitle, UIDefinition.NoRewardReceivedDescription);
    }

    #endregion Class Methods
}