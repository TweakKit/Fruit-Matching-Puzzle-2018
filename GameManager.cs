using FullSerializer;

public class GameManager : PersistentSingleton<GameManager>
{
    #region Members

    private Level _currentLevel;

    public int CurrentMapID { get; private set; }
    public int CurrentLevelID { get; private set; }
    public string LevelInfo => "Level " + CurrentMapID + " - " + CurrentLevelID;
    public Level CurrentLevel => _currentLevel;
    public bool CanShowTutorial1 => _currentLevel.mapID == 1
                                 && _currentLevel.levelID == 1
                                 && !PlayerPrefsManager.Instance.SavedData.HasCompletedTutorial1;
    public bool CanShowTutorial2 => _currentLevel.mapID == 1
                                 && _currentLevel.levelID == 2
                                 && !PlayerPrefsManager.Instance.SavedData.HasCompletedTutorial2;
    public bool CanShowTutorial3 => _currentLevel.mapID == 1
                                 && _currentLevel.levelID == 3
                                 && !PlayerPrefsManager.Instance.SavedData.HasCompletedTutorial3;
    public bool CanShowTutorial4 => _currentLevel.mapID == 1
                                 && _currentLevel.levelID == 4
                                 && !PlayerPrefsManager.Instance.SavedData.HasCompletedTutorial4;
    public bool FinishedAllTutorials => PlayerPrefsManager.Instance.SavedData.HasCompletedTutorial1
                                     && PlayerPrefsManager.Instance.SavedData.HasCompletedTutorial2
                                     && PlayerPrefsManager.Instance.SavedData.HasCompletedTutorial3
                                     && PlayerPrefsManager.Instance.SavedData.HasCompletedTutorial4;

    #endregion Members

    #region API Methods

    protected override void Awake()
    {
        base.Awake();
        InitializeWorldMapData();
        LoadGameLevel();
    }

    #endregion API Methods

    #region Class Methods

    public Level LoadGameLevel()
    {
        var fsSerializer = new fsSerializer();
        string levelFolderPath = GameDefinition.LevelsResourceFolder + string.Format(GameDefinition.LevelNameFormat, CurrentMapID, CurrentLevelID);
        _currentLevel = FileUtils.LoadJsonFile<Level>(fsSerializer, levelFolderPath);
        return _currentLevel;
    }

    public void CompleteLevel(int achievedStars)
    {
        WorldMap worldMap = PlayerPrefsManager.instance.SavedData.WorldMap;
        Map currentPlayingMap = worldMap.GetMap(CurrentMapID);
        MapLevel mapLevel = currentPlayingMap.GetMapLevel(CurrentLevelID);
        mapLevel.SetAchievedStars(achievedStars);
        PlayerPrefsManager.instance.SavedData.AchievedStarsForChest += achievedStars;

        CurrentLevelID++;
        if (!worldMap.ExistMapLevel(CurrentMapID, CurrentLevelID))
        {
            CurrentMapID++;
            CurrentLevelID = 1;

            if (!worldMap.ExistMapLevel(CurrentMapID, CurrentLevelID))
            {
                CurrentMapID = 1;
                CurrentLevelID = 1;
            }
            else
            {
                Map nextMap = worldMap.GetMap(CurrentMapID);
                nextMap.Unlock();
            }
        }
        else
        {
            MapLevel nextMapLevel = currentPlayingMap.GetMapLevel(CurrentLevelID);
            nextMapLevel.Unlock();
        }

        PlayerPrefsManager.instance.SavedData.CurrentMapID = CurrentMapID;
        PlayerPrefsManager.instance.SavedData.CurrentLevelID = CurrentLevelID;
        PlayerPrefsManager.instance.SavedData.WorldMap = worldMap;
    }

    private void InitializeWorldMapData()
    {
        CurrentMapID = PlayerPrefsManager.instance.SavedData.CurrentMapID;
        CurrentLevelID = PlayerPrefsManager.instance.SavedData.CurrentLevelID;

        if (PlayerPrefsManager.instance.SavedData.WorldMap == null)
        {
            int countMapID = 1;
            int countLevelID = 1;
            WorldMap worldMap = new WorldMap();

            while (true)
            {
                string levelFolderPath = GameDefinition.LevelsResourceFolder
                                       + string.Format(GameDefinition.LevelNameFormat, countMapID, countLevelID);

                if (FileUtils.FileExists(levelFolderPath))
                {
                    worldMap.AddMapLevel(countMapID, countLevelID);
                    countLevelID++;
                }
                else
                {
                    countMapID++;
                    countLevelID = 1;

                    levelFolderPath = GameDefinition.LevelsResourceFolder
                                    + string.Format(GameDefinition.LevelNameFormat, countMapID, countLevelID);

                    if (!FileUtils.FileExists(levelFolderPath))
                        break;
                }
            }

            worldMap.Init();
            PlayerPrefsManager.instance.SavedData.WorldMap = worldMap;
        }
        else
        {
            int countMapID = 1;
            int countLevelID = 1;
            WorldMap worldMap = PlayerPrefsManager.instance.SavedData.WorldMap;

            while (true)
            {
                string levelFolderPath = GameDefinition.LevelsResourceFolder
                                       + string.Format(GameDefinition.LevelNameFormat, countMapID, countLevelID);

                if (FileUtils.FileExists(levelFolderPath))
                {
                    if (worldMap.GetMap(countMapID) == null || worldMap.GetMap(countMapID).GetMapLevel(countLevelID) == null)
                        worldMap.AddMapLevel(countMapID, countLevelID);
                    countLevelID++;
                }
                else
                {
                    countMapID++;
                    countLevelID = 1;

                    levelFolderPath = GameDefinition.LevelsResourceFolder
                                    + string.Format(GameDefinition.LevelNameFormat, countMapID, countLevelID);

                    if (!FileUtils.FileExists(levelFolderPath))
                        break;
                }
            }
        }
    }

    public void SetMapID(int selectedMapID)
    {
        CurrentMapID = selectedMapID;
        PlayerPrefsManager.instance.SavedData.CurrentMapID = selectedMapID;
    }

    public void SetLevelID(int selectedLevelID)
    {
        CurrentLevelID = selectedLevelID;
        PlayerPrefsManager.instance.SavedData.CurrentLevelID = selectedLevelID;
    }

    #endregion Class Methods
}