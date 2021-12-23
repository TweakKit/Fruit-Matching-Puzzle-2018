using System;
using System.IO;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

using FullSerializer;

public abstract class EditorTab
{
    #region Members

    protected TileMasterEditorWindow _parentEditorWindow;

    #endregion Members

    #region Class Methods

    public EditorTab(TileMasterEditorWindow parentEditorWindow)
    {
        _parentEditorWindow = parentEditorWindow;
    }

    public virtual void Select() { }
    public virtual void Draw() { }

    /// <summary>
    /// Utility method to create and initialize a reorderable list.
    /// </summary>
    /// <param name="headerText">The header text.</param>
    /// <param name="elements">The list of elements contained in the list.</param>
    /// <param name="currentElement">A reference to the currently selected element of the list.</param>
    /// <param name="drawElement">Callback to invoke when an element of the list is drawn.</param>
    /// <param name="selectElement">Callback to invoke when an element of the list is selected.</param>
    /// <param name="createElement">Callback to invoke when an element of the list is created.</param>
    /// <param name="removeElement">Callback to invoke when an element of the list is removed.</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ReorderableList SetUpReorderableList<T>(string headerText,
                                                          List<T> elements,
                                                          ref T currentElement,
                                                          Action<Rect, T> drawElement,
                                                          Action<T> selectElement,
                                                          Action createElement,
                                                          Action<T> removeElement)
    {
        var reorderableList = new ReorderableList(elements, typeof(T), true, true, true, true)
        {
            drawHeaderCallback = (Rect rect) => EditorGUI.LabelField(rect, headerText),
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => drawElement(rect, elements[index])
        };

        reorderableList.onSelectCallback = l => selectElement(elements[reorderableList.index]);
        reorderableList.onAddDropdownCallback = (buttonRect, l) => createElement?.Invoke();
        reorderableList.onRemoveCallback = l =>
        {
            if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this item?", "Yes", "No"))
            {
                var element = elements[l.index];
                removeElement(element);
                ReorderableList.defaultBehaviours.DoRemoveButton(l);
            }
        };

        return reorderableList;
    }

    protected T LoadJsonFile<T>(string path) where T : class
    {
        if (!File.Exists(path))
            return null;

        var streamReader = new StreamReader(path);
        var content = streamReader.ReadToEnd();
        var data = fsJsonParser.Parse(content);
        object deserializedObject = null;
        var fsSerializer = new fsSerializer();
        fsSerializer.TryDeserialize(data, typeof(T), ref deserializedObject).AssertSuccessWithoutWarnings();
        streamReader.Close();
        return deserializedObject as T;
    }

    protected void SaveJsonFile<T>(string path, T data) where T : class
    {
        fsData serializedData;
        var fsSerializer = new fsSerializer();
        fsSerializer.TrySerialize(data, out serializedData).AssertSuccessWithoutWarnings();
        var streamWriter = new StreamWriter(path);
        var content = fsJsonPrinter.PrettyJson(serializedData);
        streamWriter.WriteLine(content);
        streamWriter.Close();
    }

    #endregion Class Methods
}