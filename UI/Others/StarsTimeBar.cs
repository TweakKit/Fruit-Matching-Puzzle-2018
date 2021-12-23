using UnityEngine;
using UnityEngine.UI;

using UniRx;

public class StarsTimeBar : MonoBehaviour
{
    #region Members

    private static readonly float reachFirstStarThresHoldValue = 0.2f;
    private static readonly float reachSecondStarThresHoldValue = 0.5f;
    private static readonly float reachThirdStarThresHoldValue = 0.8f;
    private static readonly float limitedTimeThresholdValue = 180.0f;
    private static readonly float decreasedTimePerWinTilesCombo = 5.0f;

    [SerializeField]
    private Image _timeBarSliderImage;
    [SerializeField]
    private Transform[] _starPanels;

    private float _timeBarValue;
    private float _gameTime;

    #endregion Members

    #region API Methods

    private void Awake()
    {
        EventManager.AddListener<Level>(GameEventType.InitLevel, OnInitLevel);
        EventManager.AddListener(GameEventType.WinTilesCombo, OnWinTilesCombo);
        this.ObserveEveryValueChanged((t) => t._timeBarValue)
            .TakeUntilDestroy(this)
            .Subscribe(OnTimeBarValueChanged);
    }

    private void Update()
    {
        if (GameplayManager.Instance.IsPlaying && GameplayManager.Instance.HasPickedATile)
        {
            _gameTime += Time.deltaTime;
            _gameTime = Mathf.Clamp(_gameTime, 0.0f, limitedTimeThresholdValue);
            _timeBarValue = 1 - _gameTime / limitedTimeThresholdValue;
        }
    }

    #endregion API Methods

    #region Class Methods

    private void OnInitLevel(Level level)
    {
        _timeBarValue = 1.0f;
        _gameTime = 0.0f;
    }

    private void OnWinTilesCombo()
    {
        _gameTime -= decreasedTimePerWinTilesCombo;
    }

    private void OnTimeBarValueChanged(float timeBarValue)
    {
        int achievedStars;
        _timeBarSliderImage.fillAmount = timeBarValue;

        if (timeBarValue >= reachThirdStarThresHoldValue)
        {
            achievedStars = 3;

            for (int i = 0; i < 3; i++)
            {
                if (!_starPanels[i].GetChild(0).gameObject.activeSelf)
                    _starPanels[i].GetChild(0).gameObject.SetActive(true);
            }
        }
        else if (timeBarValue >= reachSecondStarThresHoldValue)
        {
            achievedStars = 2;

            for (int i = 0; i < 2; i++)
            {
                if (!_starPanels[i].GetChild(0).gameObject.activeSelf)
                    _starPanels[i].GetChild(0).gameObject.SetActive(true);
            }

            for (int i = 2; i < 3; i++)
            {
                if (_starPanels[i].GetChild(0).gameObject.activeSelf)
                    _starPanels[i].GetChild(0).gameObject.SetActive(false);
            }
        }
        else if (timeBarValue >= reachFirstStarThresHoldValue)
        {
            achievedStars = 1;

            for (int i = 0; i < 1; i++)
            {
                if (!_starPanels[i].GetChild(0).gameObject.activeSelf)
                    _starPanels[i].GetChild(0).gameObject.SetActive(true);
            }

            for (int i = 1; i < 3; i++)
            {
                if (_starPanels[i].GetChild(0).gameObject.activeSelf)
                    _starPanels[i].GetChild(0).gameObject.SetActive(false);
            }
        }
        else
        {
            achievedStars = 0;

            for (int i = 0; i < 3; i++)
            {
                if (_starPanels[i].GetChild(0).gameObject.activeSelf)
                    _starPanels[i].GetChild(0).gameObject.SetActive(false);
            }
        }

        GameplayManager.Instance.AchievedStars = achievedStars;
    }

    #endregion Class Methods
}