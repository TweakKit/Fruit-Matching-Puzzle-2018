using TMPro;
using UnityEngine;

public class HomeScene : BaseScene
{
    #region Members

    [Header("Pop-ups")]

    [SerializeField]
    private GameObject _settingPopUp;
    [SerializeField]
    private GameObject _chooseLevelPopUp;

    [SerializeField]
    private Transform _levelPagesContentContainer;
    [SerializeField]
    private GameObject _levelButtonsContainerPrefab;
    [SerializeField]
    private GameObject _levelButtonPrefab;
    [SerializeField]
    private Transform _pageIndexImageContainer;
    [SerializeField]
    private GameObject _pageIndexImagePrefab;

    #endregion Members

    #region API Methods

    protected override void Start()
    {
        base.Start();
        InitLevels();
        AdmobController.Instance.DestroyBanner();
    }

    #endregion API Methods

    #region Class Methods

    public void OpenEvent()
    {
        OpenAlertPopUp(UIDefinition.AlertTitle, UIDefinition.FeatureNotImplementedDescription);
    }

    public void OpenSettingPopUp() => OpenPopup<SettingPopUp>(true, true, false, _settingPopUp);
    public void OpenStorePopUp() => OpenStorePopUp(FreeItemType.None);
    public void OpenChooseLevelPopUp() => OpenPopup<ChooseLevelPopUp>(false, true, false, _chooseLevelPopUp);

    private void InitLevels()
    {
        int mapID = 1;
        WorldMap worldMap = PlayerPrefsManager.Instance.SavedData.WorldMap;

        while (true)
        {
            Map map = worldMap.GetMap(mapID);

            if (map == null)
                break;

            Instantiate(_pageIndexImagePrefab, _pageIndexImageContainer, false);
            GameObject levelButtonsContainer = Instantiate(_levelButtonsContainerPrefab, _levelPagesContentContainer, false);
            map.mapLevels.ForEach((mapLevel) => CreateALevelButton(mapLevel, mapID, levelButtonsContainer.transform.GetChild(0).transform));
            mapID++;
        }
    }

    private void CreateALevelButton(MapLevel mapLevel, int mapID, Transform levelButtonsContainer)
    {
        GameObject levelButtonGameObject = Instantiate(_levelButtonPrefab, levelButtonsContainer, false);

        if (mapLevel.isUnlocked)
        {
            int mapRealID = mapID;
            int levelRealID = mapLevel.id;
            levelButtonGameObject.GetComponent<NormalButton>().onClick.AddListener(() => OnClickLevelButton(mapRealID, levelRealID));
            levelButtonGameObject.transform.GetChild(0).gameObject.SetActive(true);
            levelButtonGameObject.transform.GetChild(1).gameObject.SetActive(false);
            levelButtonGameObject.transform.GetChild(0).GetChild(2).GetComponent<TextMeshProUGUI>().text = mapLevel.id.ToString();
            for (int i = 0; i < levelButtonGameObject.transform.GetChild(0).GetChild(3).childCount; i++)
            {
                var starsPanel = levelButtonGameObject.transform.GetChild(0).GetChild(3).GetChild(i);
                starsPanel.GetChild(0).gameObject.SetActive((i + 1) > mapLevel.achievedStars);
                starsPanel.GetChild(1).gameObject.SetActive((i + 1) <= mapLevel.achievedStars);
            }

            if (PlayerPrefsManager.Instance.SavedData.CurrentLevelID != mapLevel.id)
            {
                levelButtonGameObject.transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
                levelButtonGameObject.transform.GetChild(0).GetChild(1).gameObject.SetActive(false);
            }
            else
            {
                levelButtonGameObject.transform.GetChild(0).GetChild(1).gameObject.SetActive(true);
                levelButtonGameObject.transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
            }
        }
        else
        {
            levelButtonGameObject.GetComponent<NormalButton>().enabled = false;
            levelButtonGameObject.transform.GetChild(0).gameObject.SetActive(false);
            levelButtonGameObject.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    private void OnClickLevelButton(int mapID, int levelID)
    {
        GameManager.Instance.SetMapID(mapID);
        GameManager.Instance.SetLevelID(levelID);
        LoadingSceneManager.LoadScene(GameDefinition.GameSceneName);
    }

    #endregion Class Methods
}