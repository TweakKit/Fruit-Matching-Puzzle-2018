using UnityEngine;

public class GiftWatchAdsPopUp : PopUp
{
    #region Members

    [SerializeField]
    private StoreProduct _rewardItem;

    #endregion Members

    #region API Methods

    private void Awake()
    {
        _rewardItem.button.onClick.AddListener(OnCollect);
    }

    #endregion API Methods

    #region Class Methods

    private void OnCollect()
    {
        PlayerPrefsManager.Instance.SavedData.RewindTurns += _rewardItem.productInfo.rewindTurns;
        PlayerPrefsManager.Instance.SavedData.HintTurns += _rewardItem.productInfo.hintTurns;
        PlayerPrefsManager.Instance.SavedData.ReloadTurns += _rewardItem.productInfo.reloadTurns;
        _quickClosePopUp = true;
        Close();
    }

    #endregion Class Methods
}