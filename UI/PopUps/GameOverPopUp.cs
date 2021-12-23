using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;
using GoogleMobileAds.Api;

public class GameOverPopUp : PopUp
{
    #region API Methods

    #endregion API Methods

    #region Class Methods

    public void GoBackHome()
    {
        LoadingSceneManager.LoadScene(GameDefinition.HomeSceneName);
    }

    public void Revive()
    {
        if (AdmobController.Instance.CanShowRewardedVideoAd())
            AdmobController.Instance.ShowRewardVideoAd(OnWatchedRewardedAd, OnStoppedWatchingRewardedAd);
        else
            parentScene.OpenAlertPopUp(UIDefinition.AlertTitle, UIDefinition.AdsCanNotBeLoadedDescription);
    }

    public void Restart()
    {
        _quickClosePopUp = true;
        Close();
        EventManager.Invoke(GameEventType.RestartLevel);
    }

    private void OnWatchedRewardedAd()
    {
        _quickClosePopUp = true;
        Close();
        EventManager.Invoke(GameEventType.ReviveLevel);
    }

    private void OnStoppedWatchingRewardedAd() { }

    #endregion Class Methods
}