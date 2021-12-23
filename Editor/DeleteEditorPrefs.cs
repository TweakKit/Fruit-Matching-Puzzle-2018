using UnityEngine;

using UnityEditor;

public class DeleteEditorPrefs
{
    #region Class Methods

    [MenuItem("Window/Tile Master/Delete EditorPrefs", false, 2)]
    public static void DeleteAllEditorPrefs()
    {
        EditorPrefs.DeleteAll();
        Debug.Log("Delete all editor prefs");
    }

    #endregion Class Methods
}