using System;

using UnityEngine;

using GoogleMobileAds.Api;

public class AdmobController : PersistentSingleton<AdmobController>
{
    #region Members

    private BannerView _bannerView;
    private InterstitialAd _interstitialAd;
    private RewardedAd _rewardedAd;
    private Action _callBackAsFinishedWatchingRewardedAd;
    private Action _callBackAsStoppedWatchingRewardedAd;
    private bool _hasWatchedRewardedAd;
    private bool _isShowingAds;
    private float _currentInternetCheckingTime;
    private bool _hasInternetConnection;

    #endregion Members

    #region API Methods

    private void Start()
    {
        _currentInternetCheckingTime = 0.0f;
        _hasInternetConnection = false;
        CheckInternetConnection();
    }

    private void Update()
    {
        _currentInternetCheckingTime += Time.deltaTime;
        if (_currentInternetCheckingTime >= GameDefinition.InternetCheckingDelayTime)
        {
            _currentInternetCheckingTime = 0.0f;
            CheckInternetConnection();
        }
    }

    #endregion API Methods

    #region Class Methods

    public bool CanShowRewardedVideoAd() => _hasInternetConnection && _rewardedAd.IsLoaded();

    public void ShowInterstialAd()
    {
        if (_interstitialAd.IsLoaded())
            _interstitialAd.Show();
    }

    public void ShowRewardVideoAd(Action callBackAsFinishedWatchingRewardedAd, Action callBackAsStoppedWatchingRewardedAd)
    {
        _callBackAsFinishedWatchingRewardedAd = callBackAsFinishedWatchingRewardedAd;
        _callBackAsStoppedWatchingRewardedAd = callBackAsStoppedWatchingRewardedAd;
        _rewardedAd.Show();
    }

    public void DestroyBanner()
    {
        if (_bannerView != null)
        {
            _bannerView.Hide();
            _bannerView.Destroy();
        }
    }

    public void RequestAndShowBannerAd()
    {
        _bannerView = new BannerView(UnitAdIDs.Banner_AD_ID, AdSize.Banner, AdPosition.Bottom);

        // Called when an ad request has successfully loaded.
        _bannerView.OnAdLoaded += HandleOnAdLoadedBannerAd;
        // Called when an ad request failed to load.
        _bannerView.OnAdFailedToLoad += HandleOnAdFailedToLoadBannerAd;
        // Called when an ad is clicked.
        _bannerView.OnAdOpening += HandleOnAdOpenedBannerAd;
        // Called when the user returned from the app after an ad click.
        _bannerView.OnAdClosed += HandleOnAdClosedBannerAd;
        // Called when the ad click caused the user to leave the application.
        _bannerView.OnAdLeavingApplication += HandleOnAdLeavingApplicationBannerAd;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().AddTestDevice(AdRequest.TestDeviceSimulator).Build();

        // Load the banner with the request.
        _bannerView.LoadAd(request);
    }

    private void Initialize()
    {
        MobileAds.Initialize(_ => { });
        _isShowingAds = false;
        RequestInterstitialAd();
        CreateAndLoadRewardedAd();
        EventManager.Invoke(GameEventType.HasWifiTurnedOn);
    }

    private void RequestInterstitialAd()
    {
        // Initialize an InterstitialAd.
        _interstitialAd = new InterstitialAd(UnitAdIDs.Interstitial_AD_ID);

        // Called when an ad request has successfully loaded.
        _interstitialAd.OnAdLoaded += HandleOnAdLoadedInterstitialAd;
        // Called when an ad request failed to load.
        _interstitialAd.OnAdFailedToLoad += HandleOnAdFailedToLoadInterstitialAd;
        // Called when an ad is shown.
        _interstitialAd.OnAdOpening += HandleOnAdOpenedInterstitialAd;
        // Called when the ad is closed.
        _interstitialAd.OnAdClosed += HandleOnAdClosedInterstitialAd;
        // Called when the ad click caused the user to leave the application.
        _interstitialAd.OnAdLeavingApplication += HandleOnAdLeavingApplicationInterstitialAd;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().AddTestDevice(AdRequest.TestDeviceSimulator).Build();

        // Load the interstitial with the request.
        _interstitialAd.LoadAd(request);
    }

    private void CreateAndLoadRewardedAd()
    {
        _rewardedAd = new RewardedAd(UnitAdIDs.Video_Rewarded_ID);

        // Called when an ad request has successfully loaded.
        _rewardedAd.OnAdLoaded += HandleRewardedAdLoadedRewardedVideoAd;
        // Called when an ad request failed to load.
        _rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoadRewardedVideoAd;
        // Called when an ad is shown.
        _rewardedAd.OnAdOpening += HandleRewardedAdOpeningRewardedVideoAd;
        // Called when an ad request failed to show.
        _rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShowRewardedVideoAd;
        // Called when the user should be rewarded for interacting with the ad.
        _rewardedAd.OnUserEarnedReward += HandleUserEarnedRewardRewardedVideoAd;
        // Called when the ad is closed.
        _rewardedAd.OnAdClosed += HandleRewardedAdClosedRewardedVideoAd;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().AddTestDevice(AdRequest.TestDeviceSimulator).Build();

        // Load the rewarded ad with the request.
        _rewardedAd.LoadAd(request);
    }

    private void HandleOnAdLoadedBannerAd(object sender, EventArgs args) { }
    private void HandleOnAdOpenedBannerAd(object sender, EventArgs args) { }
    private void HandleOnAdLeavingApplicationBannerAd(object sender, EventArgs args) { }

    private void HandleOnAdFailedToLoadBannerAd(object sender, AdFailedToLoadEventArgs args)
    {
        RequestInterstitialAd();
    }

    private void HandleOnAdClosedBannerAd(object sender, EventArgs args)
    {
        RequestInterstitialAd();
    }

    private void HandleOnAdLoadedInterstitialAd(object sender, EventArgs args) { }

    private void HandleOnAdFailedToLoadInterstitialAd(object sender, AdFailedToLoadEventArgs args)
    {
        RequestInterstitialAd();
    }

    private void HandleOnAdOpenedInterstitialAd(object sender, EventArgs args)
    {
        _isShowingAds = true;
    }

    private void HandleOnAdClosedInterstitialAd(object sender, EventArgs args)
    {
        _isShowingAds = false;
        RequestInterstitialAd();
    }

    private void HandleOnAdLeavingApplicationInterstitialAd(object sender, EventArgs args)
    {
        _isShowingAds = false;
    }

    private void HandleRewardedAdLoadedRewardedVideoAd(object sender, EventArgs args)
    {
        _hasWatchedRewardedAd = false;
    }

    private void HandleRewardedAdFailedToLoadRewardedVideoAd(object sender, AdErrorEventArgs args)
    {
        _hasWatchedRewardedAd = false;
        CreateAndLoadRewardedAd();
    }

    private void HandleRewardedAdOpeningRewardedVideoAd(object sender, EventArgs args)
    {
        _isShowingAds = true;
    }

    private void HandleRewardedAdFailedToShowRewardedVideoAd(object sender, AdErrorEventArgs args)
    {
        _isShowingAds = false;
        CreateAndLoadRewardedAd();
    }

    private void HandleUserEarnedRewardRewardedVideoAd(object sender, Reward args)
    {
        _hasWatchedRewardedAd = true;
        _callBackAsFinishedWatchingRewardedAd?.Invoke();
    }

    private void HandleRewardedAdClosedRewardedVideoAd(object sender, EventArgs args)
    {
        _isShowingAds = false;
        if (!_hasWatchedRewardedAd)
            _callBackAsStoppedWatchingRewardedAd?.Invoke();

        CreateAndLoadRewardedAd();
    }

    // Not highly recommended checking with this method!
    private void CheckInternetConnection()
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            if (!_hasInternetConnection)
            {
                _hasInternetConnection = true;
                Initialize();
            }
        }
        else
        {
            if (_hasInternetConnection)
                _hasInternetConnection = false;
        }
    }

    #endregion Class Methods
}