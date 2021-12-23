using UnityEngine;

using TMPro;
using UniRx;

public class GameScene : BaseScene
{
    #region Members

    [Header("Pop-ups")]

    [SerializeField]
    private GameObject _pausePopUp;
    [SerializeField]
    private GameObject _watchAdsPopUp;
    [SerializeField]
    private GameObject _freeItemsPopUp;
    [SerializeField]
    private GameObject _giftWatchAdsPopUp;
    [SerializeField]
    private GameObject _gameOverPopUp;
    [SerializeField]
    private GameObject _victoryPopUp;

    [Header("Others")]

    [SerializeField]
    private Animator _introPlayGameAnimator;
    [SerializeField]
    private TextMeshProUGUI _levelTitleText;
    [SerializeField]
    private GameObject _adsIcon;
    [SerializeField]
    private TextMeshProUGUI _adsDelayTimeText;
    [SerializeField]
    private NormalButton _rewindButton;
    [SerializeField]
    private NormalButton _hintButton;
    [SerializeField]
    private NormalButton _reloadButton;
    [SerializeField]
    private TextMeshProUGUI _rewindTurnsText;
    [SerializeField]
    private TextMeshProUGUI _hintTurnsText;
    [SerializeField]
    private TextMeshProUGUI _reloadTurnsText;

    private float _currentAdsDelayTime;
    private bool _hasWatchedAds;

    #endregion Members

    #region API Methods

    private void Awake()
    {
        EventManager.AddListener<Level>(GameEventType.InitLevel, OnInitLevel);
        EventManager.AddListener(GameEventType.RewindATileSucessfully, OnRewindATileSucessfully);
        EventManager.AddListener(GameEventType.UseAHintSucessfully, OnUseAHintSucessfully);
        EventManager.AddListener(GameEventType.ReloadLevelSucessfully, OnReloadLevelSucessfully);
        EventManager.AddListener(GameEventType.LoseLevel, OnLoseLevel);
        EventManager.AddListener<int>(GameEventType.WinLevel, OnWinLevel);
        EventManager.AddListener(GameEventType.StartWatchingRewardedAds, OnStartWatchingRewardedAds);
        EventManager.AddListener(GameEventType.HasWifiTurnedOn, OnHasWifiTurnedOn);

        /// BAD CODE --- Rewrite later ///
        if (!GameManager.Instance.FinishedAllTutorials)
        {
            EventManager.AddListener(TutorialEventType.FinishTutorial1, () => SetUpHelpButtons(1, 1));
            EventManager.AddListener(TutorialEventType.FinishTutorial2, () => SetUpHelpButtons(1, 2));
            EventManager.AddListener(TutorialEventType.FinishTutorial3, () => SetUpHelpButtons(1, 3));
            EventManager.AddListener(TutorialEventType.FinishTutorial4, () => SetUpHelpButtons(1, 4));
        }
        //////////////////////////////////

        PlayerPrefsManager.Instance.SavedData.ObserveEveryValueChanged((savedData) => savedData.RewindTurns)
                                             .TakeUntilDestroy(this)
                                             .Subscribe(OnRewindTurnsChanged);

        PlayerPrefsManager.Instance.SavedData.ObserveEveryValueChanged((savedData) => savedData.HintTurns)
                                             .TakeUntilDestroy(this)
                                             .Subscribe(OnHintTurnsChanged);

        PlayerPrefsManager.Instance.SavedData.ObserveEveryValueChanged((savedData) => savedData.ReloadTurns)
                                             .TakeUntilDestroy(this)
                                             .Subscribe(OnReloadTurnsChanged);
    }

    protected override void Start()
    {
        base.Start();
        AdmobController.Instance.RequestAndShowBannerAd();
    }

    private void OnHasWifiTurnedOn() => AdmobController.Instance.RequestAndShowBannerAd();

    private void Update()
    {
        if (GameplayManager.Instance.IsPlaying && _hasWatchedAds)
        {
            _currentAdsDelayTime -= Time.deltaTime;
            _adsDelayTimeText.text = GetTimeFormat(_currentAdsDelayTime);

            if (_currentAdsDelayTime <= 0.0f)
            {
                _currentAdsDelayTime = GameplayDefinition.DelayTimeForNextWatchAds;
                _hasWatchedAds = false;
                _adsIcon.SetActive(true);
                _adsDelayTimeText.gameObject.SetActive(false);
            }
        }
    }

    #endregion API Methods

    #region Class Methods

    public void OpenPausePopUp() => OpenPopup<PausePopUp>(true, true, false, _pausePopUp);
    public void OpenGiftWatchAdsPopUp() => OpenPopup<GiftWatchAdsPopUp>(true, true, true, _giftWatchAdsPopUp);
    private void OnLoseLevel() => OpenPopup<GameOverPopUp>(true, false, false, _gameOverPopUp);

    public void OpenWatchAdsPopUp()
    {
        if (!_hasWatchedAds)
        {
            OpenPopup<WatchAdsPopUp>(true, true, false, _watchAdsPopUp);
        }
    }

    public void OpenFreeItemsPopUp(FreeItemType freeItemType)
    {
        OpenPopup<FreeItemsPopUp>(true, true, false, _freeItemsPopUp, popup => popup.Init(freeItemType));
    }

    private void OnWinLevel(int achievedStars)
    {
        OpenPopup<VictoryPopUp>(true, false, false, _victoryPopUp, popup => popup.Init(achievedStars));
    }

    private void OnStartWatchingRewardedAds()
    {
        _hasWatchedAds = true;
        _adsDelayTimeText.text = GetTimeFormat(_currentAdsDelayTime);
        _adsIcon.SetActive(false);
        _adsDelayTimeText.gameObject.SetActive(true);
    }

    private void OnInitLevel(Level level)
    {
        _levelTitleText.text = "Level "+ ((level.mapID - 1) * 20 + level.levelID);
        _introPlayGameAnimator.Rebind();
        _hasWatchedAds = false;
        _currentAdsDelayTime = GameplayDefinition.DelayTimeForNextWatchAds;
        _adsDelayTimeText.text = GetTimeFormat(_currentAdsDelayTime);
        _adsIcon.SetActive(true);
        _adsDelayTimeText.gameObject.SetActive(false);
        SetUpHelpButtons(level.mapID, level.levelID);
        LeanTween.delayedCall(GameplayDefinition.PlayIntroGameAnimationDelayTime, PlayIntroPlayGameAnimation);
    }

    /// BAD CODE --- Rewrite later ///
    private void SetUpHelpButtons(int mapID, int levelID)
    {
        _rewindButton.onClick.RemoveAllListeners();
        _hintButton.onClick.RemoveAllListeners();
        _reloadButton.onClick.RemoveAllListeners();

        if (mapID == 1 && levelID == 1)
        {
            _rewindButton.Disable();
            _hintButton.Disable();
            _reloadButton.Disable();

            _rewindButton.transform.GetChild(0).gameObject.SetActive(false);
            _hintButton.transform.GetChild(0).gameObject.SetActive(false);
            _reloadButton.transform.GetChild(0).gameObject.SetActive(false);

            _rewindButton.transform.GetChild(1).gameObject.SetActive(true);
            _hintButton.transform.GetChild(1).gameObject.SetActive(true);
            _reloadButton.transform.GetChild(1).gameObject.SetActive(true);
        }
        else if (mapID == 1 && levelID == 2)
        {
            if (PlayerPrefsManager.Instance.SavedData.HasCompletedTutorial2)
            {
                _rewindButton.onClick.AddListener(RewindATile);

                _rewindButton.Enable();
                _hintButton.Disable();
                _reloadButton.Disable();

                _rewindButton.transform.GetChild(0).gameObject.SetActive(true);
                _hintButton.transform.GetChild(0).gameObject.SetActive(false);
                _reloadButton.transform.GetChild(0).gameObject.SetActive(false);

                _rewindButton.transform.GetChild(1).gameObject.SetActive(false);
                _hintButton.transform.GetChild(1).gameObject.SetActive(true);
                _reloadButton.transform.GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                _rewindButton.Disable();
                _hintButton.Disable();
                _reloadButton.Disable();

                _rewindButton.transform.GetChild(0).gameObject.SetActive(false);
                _hintButton.transform.GetChild(0).gameObject.SetActive(false);
                _reloadButton.transform.GetChild(0).gameObject.SetActive(false);

                _rewindButton.transform.GetChild(1).gameObject.SetActive(true);
                _hintButton.transform.GetChild(1).gameObject.SetActive(true);
                _reloadButton.transform.GetChild(1).gameObject.SetActive(true);
            }
        }
        else if (mapID == 1 && levelID == 3)
        {
            if (PlayerPrefsManager.Instance.SavedData.HasCompletedTutorial3)
            {
                _rewindButton.onClick.AddListener(RewindATile);
                _hintButton.onClick.AddListener(UseAHint);

                _rewindButton.Enable();
                _hintButton.Enable();
                _reloadButton.Disable();

                _rewindButton.transform.GetChild(0).gameObject.SetActive(true);
                _hintButton.transform.GetChild(0).gameObject.SetActive(true);
                _reloadButton.transform.GetChild(0).gameObject.SetActive(false);

                _rewindButton.transform.GetChild(1).gameObject.SetActive(false);
                _hintButton.transform.GetChild(1).gameObject.SetActive(false);
                _reloadButton.transform.GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                _rewindButton.onClick.AddListener(RewindATile);

                _rewindButton.Enable();
                _hintButton.Disable();
                _reloadButton.Disable();

                _rewindButton.transform.GetChild(0).gameObject.SetActive(true);
                _hintButton.transform.GetChild(0).gameObject.SetActive(false);
                _reloadButton.transform.GetChild(0).gameObject.SetActive(false);

                _rewindButton.transform.GetChild(1).gameObject.SetActive(false);
                _hintButton.transform.GetChild(1).gameObject.SetActive(true);
                _reloadButton.transform.GetChild(1).gameObject.SetActive(true);
            }
        }
        else if (mapID == 1 && levelID == 4)
        {
            if (PlayerPrefsManager.Instance.SavedData.HasCompletedTutorial4)
            {
                _rewindButton.onClick.AddListener(RewindATile);
                _hintButton.onClick.AddListener(UseAHint);
                _reloadButton.onClick.AddListener(ReloadLevel);

                _rewindButton.Enable();
                _hintButton.Enable();
                _reloadButton.Enable();

                _rewindButton.transform.GetChild(0).gameObject.SetActive(true);
                _hintButton.transform.GetChild(0).gameObject.SetActive(true);
                _reloadButton.transform.GetChild(0).gameObject.SetActive(true);

                _rewindButton.transform.GetChild(1).gameObject.SetActive(false);
                _hintButton.transform.GetChild(1).gameObject.SetActive(false);
                _reloadButton.transform.GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                _rewindButton.onClick.AddListener(RewindATile);
                _hintButton.onClick.AddListener(UseAHint);

                _rewindButton.Enable();
                _hintButton.Enable();
                _reloadButton.Disable();

                _rewindButton.transform.GetChild(0).gameObject.SetActive(true);
                _hintButton.transform.GetChild(0).gameObject.SetActive(true);
                _reloadButton.transform.GetChild(0).gameObject.SetActive(false);

                _rewindButton.transform.GetChild(1).gameObject.SetActive(false);
                _hintButton.transform.GetChild(1).gameObject.SetActive(false);
                _reloadButton.transform.GetChild(1).gameObject.SetActive(true);
            }
        }
        else
        {
            _rewindButton.onClick.AddListener(RewindATile);
            _hintButton.onClick.AddListener(UseAHint);
            _reloadButton.onClick.AddListener(ReloadLevel);

            _rewindButton.Enable();
            _hintButton.Enable();
            _reloadButton.Enable();

            _rewindButton.transform.GetChild(0).gameObject.SetActive(true);
            _hintButton.transform.GetChild(0).gameObject.SetActive(true);
            _reloadButton.transform.GetChild(0).gameObject.SetActive(true);

            _rewindButton.transform.GetChild(1).gameObject.SetActive(false);
            _hintButton.transform.GetChild(1).gameObject.SetActive(false);
            _reloadButton.transform.GetChild(1).gameObject.SetActive(false);
        }
    }
    //////////////////////////////////

    private void RewindATile()
    {
        if (PlayerPrefsManager.Instance.SavedData.RewindTurns > 0)
            EventManager.Invoke(GameEventType.RewindATile);
        else
            OpenStorePopUp(FreeItemType.RewindItem);
    }

    private void UseAHint()
    {
        if (PlayerPrefsManager.Instance.SavedData.HintTurns > 0)
            EventManager.Invoke(GameEventType.UseAHint);
        else
            OpenStorePopUp(FreeItemType.HintItem);
    }

    private void ReloadLevel()
    {
        if (PlayerPrefsManager.Instance.SavedData.ReloadTurns > 0)
            EventManager.Invoke(GameEventType.ReloadLevel);
        else
            OpenStorePopUp(FreeItemType.ReloadItem);
    }

    private void OnRewindATileSucessfully() => PlayerPrefsManager.Instance.SavedData.RewindTurns -= 1;
    private void OnUseAHintSucessfully() => PlayerPrefsManager.Instance.SavedData.HintTurns -= 1;
    private void OnReloadLevelSucessfully() => PlayerPrefsManager.Instance.SavedData.ReloadTurns -= 1;

    private void OnRewindTurnsChanged(int rewindTurns)
    {
        _rewindTurnsText.text = rewindTurns != 0 ? rewindTurns.ToString() : "+";
    }

    private void OnHintTurnsChanged(int hintTurns)
    {
        _hintTurnsText.text = hintTurns != 0 ? hintTurns.ToString() : "+";
    }

    private void OnReloadTurnsChanged(int reloadTurns)
    {
        _reloadTurnsText.text = reloadTurns != 0 ? reloadTurns.ToString() : "+";
    }

    private void PlayIntroPlayGameAnimation()
    {
        _introPlayGameAnimator.SetTrigger("Animate");
    }

    private static string GetTimeFormat(float time)
    {
        string minute = Mathf.Floor(time / 60).ToString("00");
        time -= int.Parse(minute) * 60;
        string second = Mathf.Floor(time).ToString("00");
        return minute + ":" + second + "s";
    }

    #endregion Class Methods
}