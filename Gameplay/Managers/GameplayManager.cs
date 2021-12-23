using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public enum GameState
{
    Playing,
    NotPlaying,
}

public class GameplayManager : Singleton<GameplayManager>
{
    #region Members

    [SerializeField]
    private BoardGUI _boardGUI;
    [SerializeField]
    private TileStackGUI _tileStackGUI;

    private Level _level;
    private GameState _gameState;
    private bool _hasStartedGame;
    private float _currentGameTime;
    private bool _hasPickedATile;

    public bool HasPickedATile => _hasPickedATile;
    public bool IsPlaying => _gameState == GameState.Playing;
    public bool IsNotPlaying => _gameState == GameState.NotPlaying;
    public int TileGUISortingOrder { get; set; }
    public int AchievedStars { get; set; }

    #endregion Members

    #region API Methods

    protected override void Awake()
    {
        base.Awake();
        EventManager.AddListener<TileGUI>(GameEventType.PickATile, OnPickATile);
        EventManager.AddListener<List<TileGUI>>(GameEventType.MoveHintTiles, OnMoveHintTiles);
        EventManager.AddListener(GameEventType.RewindATile, OnRewindATile);
        EventManager.AddListener(GameEventType.UseAHint, OnUseAHint);
        EventManager.AddListener(GameEventType.ReloadLevel, OnReloadLevel);
        EventManager.AddListener(GameEventType.CheckGameStatus, OnCheckGameStatus);
        EventManager.AddListener(GameEventType.ReviveLevel, OnReviveLevel);
        EventManager.AddListener(GameEventType.RestartLevel, OnLoadLevel);
        EventManager.AddListener(GameEventType.PlayNextLevel, OnLoadLevel);
        EventManager.AddListener(GameEventType.OpenPopUp, OnOpenPopUp);
        EventManager.AddListener<int>(GameEventType.ClosePopUp, OnClosePopUp);
    }

    private void Start() => LoadLevel();

    private void Update()
    {
        _currentGameTime += Time.deltaTime;
        if (!_hasStartedGame && _currentGameTime >= GameplayDefinition.StartPlayGameDelayTime)
        {
            SetGameState(GameState.Playing);
            _hasStartedGame = true;
        }
    }

    #endregion API Methods

    #region Class Methods

    public void SetGameState(GameState gameState)
    {
        if (_gameState != gameState)
            _gameState = gameState;
    }

    private void LoadLevel()
    {
        _level = GameManager.Instance.LoadGameLevel();
        _hasStartedGame = false;
        _currentGameTime = 0.0f;
        _hasPickedATile = false;
        AchievedStars = 0;
        SetGameState(GameState.NotPlaying);
        EventManager.Invoke<Level>(GameEventType.InitLevel, _level);

        if (GameManager.Instance.CanShowTutorial1)
        {
            _boardGUI.Init(_level, TutorialType.Special);
            EventManager.Invoke(TutorialEventType.StartTutorial1);
        }
        else if (GameManager.Instance.CanShowTutorial2)
        {
            _boardGUI.Init(_level, TutorialType.Normal);
            EventManager.Invoke(TutorialEventType.StartTutorial2);
        }
        else if (GameManager.Instance.CanShowTutorial3)
        {
            _boardGUI.Init(_level, TutorialType.Normal);
            EventManager.Invoke(TutorialEventType.StartTutorial3);
        }
        else if (GameManager.Instance.CanShowTutorial4)
        {
            _boardGUI.Init(_level, TutorialType.Normal);
            EventManager.Invoke(TutorialEventType.StartTutorial4);
        }
        else _boardGUI.Init(_level, TutorialType.None);
    }

    private void OnPickATile(TileGUI tileGUI)
    {
        if (!_hasPickedATile)
            _hasPickedATile = true;

        _boardGUI.Remove(tileGUI);
        TileStackData nextTileStackData = _tileStackGUI.GetNextTileStackData(tileGUI);
        tileGUI.MoveToStack(nextTileStackData.inStackTilePosition);
        _tileStackGUI.InsertATileGUI(tileGUI, nextTileStackData.inStackTilePositionIndex);
    }

    private void OnMoveHintTiles(List<TileGUI> moveHintTileGUIs)
    {
        foreach (var moveHintTileGUI in moveHintTileGUIs)
            _boardGUI.Remove(moveHintTileGUI);

        foreach (var moveHintTileGUI in moveHintTileGUIs)
        {
            TileStackData nextTileStackData = _tileStackGUI.GetNextTileStackData(moveHintTileGUI);
            moveHintTileGUI.MoveToStackByHints(nextTileStackData.inStackTilePosition);
            _tileStackGUI.InsertATileGUI(moveHintTileGUI, nextTileStackData.inStackTilePositionIndex);
        }
    }

    private void OnRewindATile()
    {
        TileGUI rewindTileGUI = _tileStackGUI.RewindATile(_boardGUI.UpdateTileGUIStatuses);
        if (rewindTileGUI)
        {
            _boardGUI.Insert(rewindTileGUI);
            EventManager.Invoke(GameEventType.RewindATileSucessfully);
        }
    }

    private void OnUseAHint()
    {
        if (_boardGUI.CheckUseHints(_tileStackGUI.TileTypes))
            EventManager.Invoke(GameEventType.UseAHintSucessfully);
    }

    private void OnReloadLevel()
    {
        _boardGUI.Reload();
        EventManager.Invoke(GameEventType.ReloadLevelSucessfully);
    }

    private void OnCheckGameStatus()
    {
        if (_tileStackGUI.IsEmpty())
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.winLevelSFXClip);
            GameManager.Instance.CompleteLevel(AchievedStars);
            EventManager.Invoke<int>(GameEventType.WinLevel, AchievedStars);
        }
        else
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.loseLevelSFXClip);
            EventManager.Invoke(GameEventType.LoseLevel);
        }
    }

    private void OnReviveLevel()
    {
        for (int i = 0; i < GameplayDefinition.ReviveNumberOfTiles; i++)
        {
            TileGUI rewindTileGUI = _tileStackGUI.RewindATile(_boardGUI.UpdateTileGUIStatuses);
            _boardGUI.Insert(rewindTileGUI);
        }
        _tileStackGUI.ReviveTiles();
    }

    private void OnLoadLevel()
    {
        StartCoroutine(LoadLevelSync());
    }

    private void OnOpenPopUp()
    {
        SetGameState(GameState.NotPlaying);
    }

    private void OnClosePopUp(int numberOfShownPopUps)
    {
        if (numberOfShownPopUps == 1)
            SetGameState(GameState.Playing);
    }

    private IEnumerator LoadLevelSync()
    {
        _boardGUI.Reset();
        _tileStackGUI.Reset();

        yield return null;

        LoadLevel();
    }

    #endregion Class Methods
}