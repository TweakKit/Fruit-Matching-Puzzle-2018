using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

public class TileMasterEditorWindow : EditorWindow
{
    #region Members

    private static readonly string[] tabTitles = new[] { "Level Editor" };
    private readonly List<EditorTab> editorTabs = new List<EditorTab>();

    private int _currentlySelectedTabIndex = -1;
    private int _previousSelectedTabIndex = -1;

    #endregion Members

    #region API Methods

    private void OnEnable()
    {
        editorTabs.Add(new LevelDesignEditorTab(this));
        _currentlySelectedTabIndex = 0;
    }

    private void OnGUI()
    {
        _currentlySelectedTabIndex = GUILayout.Toolbar(_currentlySelectedTabIndex, tabTitles);
        if (_currentlySelectedTabIndex >= 0 && _currentlySelectedTabIndex < editorTabs.Count)
        {
            var selectedEditor = editorTabs[_currentlySelectedTabIndex];
            if (_currentlySelectedTabIndex != _previousSelectedTabIndex)
            {
                selectedEditor.Select();
                GUI.FocusControl(null);
            }
            selectedEditor.Draw();
            _previousSelectedTabIndex = _currentlySelectedTabIndex;
        }
    }

    #endregion API Methods

    #region Class Methods

    [MenuItem("Window/Tile Master/Editor", false, 0)]
    private static void Init()
    {
        var window = GetWindow(typeof(TileMasterEditorWindow));
        window.titleContent = new GUIContent("Tile Master Editor Window");
    }

    #endregion Class Methods
}