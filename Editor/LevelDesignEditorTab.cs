using System.IO;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

public class LevelDesignEditorTab : EditorTab
{
    #region Members

    private static readonly int maximumNumberOfLayers = 11;
    private static readonly int maximumNumberOfTiles = GameDefinition.MaxLevelWidth * GameDefinition.MaxLevelHeight * maximumNumberOfLayers;
    private static readonly int maximumNumberOfItemTypes = maximumNumberOfTiles / GameDefinition.TilesComboSum;
    private static readonly float menuButtonWidth = 100.0f;
    private static readonly float menuButtonHeight = 50.0f;
    private static readonly float tileButtonWidth = 30.0f;
    private static readonly float tileButtonHeight = 30.0f;
    private static readonly Dictionary<string, Texture> tileTextures = new Dictionary<string, Texture>();
    private static readonly string drawingLayerEmptyTile = "DrawingLayerEmptyTile";
    private static readonly string drawingLayerOccupiedTile = "DrawingLayerOccupiedTile";
    private static readonly string nonDrawingLayerEmptyTile = "NonDrawingLayerEmptyTile";
    private static readonly string nonDrawingLayerOccupiedTile = "NonDrawingLayerOccupiedTile";

    private Vector2 _scrollPosition;
    private Level _currentLevel;
    private int _drawingLayer;
    private bool _hasStartedDrawingLevel;
    private int _remaingDrawingNumberOfTiles;
    private int _numberOfTilesHasBeenDrawn;

    #endregion Members

    #region Class Methods

    public LevelDesignEditorTab(TileMasterEditorWindow tileMasterEditorWindow) : base(tileMasterEditorWindow)
    {
        _drawingLayer = 0;
        _hasStartedDrawingLevel = false;
        _numberOfTilesHasBeenDrawn = 0;

        var editorImagesPath = new DirectoryInfo(Application.dataPath + "/Game/Editor/Resources");
        var fileInfo = editorImagesPath.GetFiles("*.png", SearchOption.TopDirectoryOnly);
        foreach (var file in fileInfo)
        {
            var filename = Path.GetFileNameWithoutExtension(file.Name);
            tileTextures[filename] = Resources.Load(filename) as Texture;
        }
    }

    public override void Draw()
    {
        Rect rectPosition = new Rect(3, 40, _parentEditorWindow.position.width, _parentEditorWindow.position.height);
        Rect rectView = new Rect(0, 0, _parentEditorWindow.position.width, GameDefinition.MaxLevelHeight * tileButtonHeight + 200);
        _scrollPosition = GUI.BeginScrollView(rectPosition, _scrollPosition, rectView, false, false);

        DrawMenu();

        if (_currentLevel != null)
        {
            DrawGeneralSettings();
            DrawLevelSettings();
            DrawButtonStartDrawingLevel();
            DrawLevelEditor();
        }

        GUI.EndScrollView();
    }

    private void DrawMenu()
    {
        if (GUI.Button(new Rect(3, 0, menuButtonWidth, menuButtonHeight), "New"))
        {
            _currentLevel = new Level();
            _drawingLayer = 0;
            _hasStartedDrawingLevel = false;
            _numberOfTilesHasBeenDrawn = 0;
        }

        if (GUI.Button(new Rect(3 + menuButtonWidth + 3, 0, menuButtonWidth, menuButtonHeight), "Open"))
        {
            var levelFilePath = EditorUtility.OpenFilePanel("Open level", Application.dataPath + "/Game/Resources/Levels", "json");
            if (!string.IsNullOrEmpty(levelFilePath))
                _currentLevel = LoadJsonFile<Level>(levelFilePath);
        }

        if (GUI.Button(new Rect(3 + menuButtonWidth * 2 + 6, 0, menuButtonWidth, menuButtonHeight), "Save"))
        {
            if (_remaingDrawingNumberOfTiles == 0)
                SaveLevel(Application.dataPath + "/Game/Resources/Levels/");
            else
                EditorUtility.DisplayDialog("Attention!", "Not finishing drawing all the tiles!", "Ok", "Cancel");
        }

    }

    private void DrawGeneralSettings()
    {
        EditorGUI.BeginDisabledGroup(_hasStartedDrawingLevel);
        GUI.BeginGroup(new Rect(0, 100, _parentEditorWindow.position.width, 300));
        EditorGUI.LabelField(new Rect(0, 0, 100, 50), "General", EditorStyles.boldLabel);
        EditorGUI.HelpBox(new Rect(0, 40, 300, 50), "The general settings of this level.", MessageType.Info);

        EditorGUI.LabelField(new Rect(0, 80, 80, 50),
                             new GUIContent("Level ID", "The ID of this level."));
        _currentLevel.levelID = EditorGUI.IntField(new Rect(60, 95, 30, 20), _currentLevel.levelID);

        EditorGUI.LabelField(new Rect(0, 105, 80, 50),
                             new GUIContent("Map ID", "The map ID of this level."));
        _currentLevel.mapID = EditorGUI.IntField(new Rect(60, 120, 30, 20), _currentLevel.mapID);
        GUI.EndGroup();
        EditorGUI.EndDisabledGroup();
    }

    private void DrawLevelSettings()
    {
        EditorGUI.BeginDisabledGroup(_hasStartedDrawingLevel);
        GUI.BeginGroup(new Rect(0, 250, _parentEditorWindow.position.width, 300));

        EditorGUI.LabelField(new Rect(0, 0, 100, 50), "Level", EditorStyles.boldLabel);
        EditorGUI.HelpBox(new Rect(0, 40, 300, 50), "The layout settings of this level.", MessageType.Info);

        EditorGUI.LabelField(new Rect(0, 80, 120, 50),
                             new GUIContent("Number of item types", "The number of item types."));
        _currentLevel.numberOfItemTypes = EditorGUI.IntField(new Rect(130, 95, 30, 20), _currentLevel.numberOfItemTypes);
        _currentLevel.numberOfItemTypes = Mathf.Clamp(_currentLevel.numberOfItemTypes, 1, maximumNumberOfItemTypes);

        EditorGUI.LabelField(new Rect(0, 105, 120, 50),
                             new GUIContent("Number of layers", "The number of layers."));
        _currentLevel.numberOfLayers = EditorGUI.IntField(new Rect(130, 120, 30, 20), _currentLevel.numberOfLayers);
        _currentLevel.numberOfLayers = Mathf.Clamp(_currentLevel.numberOfLayers, 1, maximumNumberOfLayers);

        EditorGUI.LabelField(new Rect(0, 130, 120, 50),
                             new GUIContent("Number of tiles", "The number of tiles must be divisible by 3."));
        _currentLevel.numberOfTiles = EditorGUI.IntField(new Rect(130, 145, 30, 20), _currentLevel.numberOfTiles);
        _currentLevel.numberOfTiles = Mathf.Clamp(_currentLevel.numberOfTiles, 1, maximumNumberOfTiles);

        GUI.EndGroup();
        EditorGUI.EndDisabledGroup();
    }

    private void DrawButtonStartDrawingLevel()
    {
        GUI.BeginGroup(new Rect(0, 300, _parentEditorWindow.position.width, 300));
        if (GUI.Button(new Rect(0, 155, 120, menuButtonHeight), _hasStartedDrawingLevel ? "Change settings" : "Start drawing"))
        {
            if (_currentLevel.numberOfTiles % 3 != 0)
            {
                EditorUtility.DisplayDialog("Attention!", "The number of tiles must be divisible by 3!", "Ok", "Cancel");
            }
            else
            {
                _hasStartedDrawingLevel = !_hasStartedDrawingLevel;
                _remaingDrawingNumberOfTiles = _currentLevel.numberOfTiles - _numberOfTilesHasBeenDrawn;
            }
        }
        GUI.EndGroup();
    }

    private void DrawLevelEditor()
    {
        EditorGUI.BeginDisabledGroup(!_hasStartedDrawingLevel);
        GUILayout.BeginArea(new Rect(350, 100, GameDefinition.MaxLevelWidth * (tileButtonWidth + 4), GameDefinition.MaxLevelHeight * (tileButtonHeight + 3)));

        for (int i = 0; i < _currentLevel.numberOfLayers; i++)
            DrawLayer(i + 1);

        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(350 + GameDefinition.MaxLevelWidth * (tileButtonWidth + 3) + 50, 80, 250, (_currentLevel.numberOfLayers + 1) * (menuButtonHeight + 5) + 100));

        if (_hasStartedDrawingLevel)
            EditorGUI.LabelField(new Rect(0, 0, 500, 50), "Remaing drawing number of tiles: " + _remaingDrawingNumberOfTiles);

        if (_drawingLayer != 0)
            EditorGUI.LabelField(new Rect(0, 35, 250, 50), "Drawing layer: " + _drawingLayer);

        float tempPositionY = 100;
        for (int i = 0; i < _currentLevel.numberOfLayers; i++)
        {
            if (GUI.Button(new Rect(0, tempPositionY, menuButtonWidth, menuButtonHeight), "Draw layer " + (i + 1)))
                _drawingLayer = i + 1;

            tempPositionY += menuButtonHeight + 5;
        }
        GUILayout.EndArea();
        EditorGUI.EndDisabledGroup();
    }

    private void DrawLayer(int layer)
    {
        EditorGUI.BeginDisabledGroup(_drawingLayer != layer);
        if (layer % 2 == 0)
        {
            for (var indexX = 0; indexX < GameDefinition.MaxLevelWidth; indexX++)
                for (var indexY = 0; indexY < GameDefinition.MaxLevelHeight; indexY++)
                    CreateButton(indexX, indexY, tileButtonWidth / 2, tileButtonHeight / 2, layer, _drawingLayer == layer);
        }
        else
        {
            for (var x = 0; x < GameDefinition.MaxLevelWidth + 1; x++)
                for (var y = 0; y < GameDefinition.MaxLevelHeight + 1; y++)
                    CreateButton(x, y, 0, 0, layer, _drawingLayer == layer);
        }
        EditorGUI.EndDisabledGroup();
    }

    private void CreateButton(int indexX, int indexY, float offsetX, float offsetY, int layer, bool isDrawingLayer)
    {
        Rect buttonRect = new Rect(new Vector2(indexX * tileButtonWidth + offsetX, indexY * tileButtonHeight + offsetY),
                                   new Vector2(tileButtonWidth, tileButtonHeight));

        string tileImageName = "";
        LevelEditorTile levelEditorTile = _currentLevel.levelEditorTiles.Find((let) => let.indexX == indexX
                                                                 && let.indexY == indexY
                                                                 && let.layer == layer);
        if (isDrawingLayer)
        {
            if (levelEditorTile != null)
                tileImageName = drawingLayerOccupiedTile;
            else
                tileImageName = drawingLayerEmptyTile;
        }
        else
        {
            if (levelEditorTile != null)
                tileImageName = nonDrawingLayerOccupiedTile;
            else
                tileImageName = nonDrawingLayerEmptyTile;
        }

        if (GUI.Button(buttonRect, tileTextures[tileImageName], GUIStyle.none))
            DrawTile(indexX, indexY);
    }

    private void DrawTile(int indexX, int indexY)
    {
        LevelEditorTile levelEditorTile = _currentLevel.levelEditorTiles.Find((let) => let.indexX == indexX
                                                                 && let.indexY == indexY
                                                                 && let.layer == _drawingLayer);
        if (levelEditorTile != null)
        {
            _currentLevel.levelEditorTiles.Remove(levelEditorTile);
            _remaingDrawingNumberOfTiles += 1;
            _numberOfTilesHasBeenDrawn--;
        }
        else
        {
            if (_remaingDrawingNumberOfTiles > 0)
            {
                _currentLevel.levelEditorTiles.Add(new LevelEditorTile(indexX, indexY, _drawingLayer));
                _remaingDrawingNumberOfTiles -= 1;
                _numberOfTilesHasBeenDrawn++;
            }
            else EditorUtility.DisplayDialog("Attention!", "No more tiles can be placed in any layer!", "Ok", "Cancel");
        }
    }

    private void SaveLevel(string path)
    {
        SaveJsonFile(path + string.Format(GameDefinition.LevelNameFormat, _currentLevel.mapID, _currentLevel.levelID), _currentLevel);
        AssetDatabase.Refresh();
    }

    #endregion Class Methods
}