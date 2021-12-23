using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class BoardGUI : MonoBehaviour
{
    #region Members

    private List<TileGUI> _tileGUIs;
    private List<TilesHolderGUI> _tileHolderGUIs;

    #endregion Members

    #region Class Methods

    public void Init(Level level, TutorialType tutorialType)
    {
        CreateTilesHolders(level.numberOfLayers);
        CreatTiles(level.GetTiles(tutorialType), tutorialType);
        SetTilesPickablity();
        SetTileSortingLayers();
        SetTileHolderTransformsOrigin();
        MoveTileHolderTransformsOrigin();
    }

    public void Reset()
    {
        _tileGUIs.ForEach((tileGUI) => tileGUI.Destroy());
        _tileGUIs.Clear();
        _tileHolderGUIs.ForEach((tileHolderGUI) => tileHolderGUI.Destroy());
        _tileHolderGUIs.Clear();
    }

    public void Remove(TileGUI tileGUI)
    {
        _tileGUIs.Remove(tileGUI);

        if (_tileGUIs.Count == 0)
            LeanTween.delayedCall(1, () => EventManager.Invoke(GameEventType.CheckGameStatus));
        else
            UpdateTileGUIStatuses();
    }

    public void Insert(TileGUI tileGUI)
    {
        tileGUI.transform.SetParent(_tileHolderGUIs[tileGUI.Layer - 1].transform);
        _tileGUIs.Add(tileGUI);
    }

    public void UpdateTileGUIStatuses()
    {
        SetTilesPickablity();
        SetEnabledTileSortingLayers();
        SetTilesVisualization();
    }

    public bool CheckUseHints(List<TileType> inStackTileTypes)
    {
        Dictionary<TileType, List<TileGUI>> tileTypeTileGUIsDictionary = new Dictionary<TileType, List<TileGUI>>();

        for (int layer = _tileHolderGUIs.Count - 1; layer >= 0; layer--)
        {
            List<TileGUI> layeredTileGUIs = _tileGUIs.FindAll((tileGUI) => tileGUI.Layer == layer + 1);
            foreach (var layeredTileGUI in layeredTileGUIs)
            {
                if (!tileTypeTileGUIsDictionary.ContainsKey(layeredTileGUI.OwnerTile.TileType))
                {
                    List<TileGUI> tileGUIs = new List<TileGUI>().AddAndReturn(layeredTileGUI);
                    tileTypeTileGUIsDictionary.Add(layeredTileGUI.OwnerTile.TileType, tileGUIs);
                }
                else tileTypeTileGUIsDictionary[layeredTileGUI.OwnerTile.TileType].Add(layeredTileGUI);
            }

            if (inStackTileTypes.Count == 0)
            {
                if (tileTypeTileGUIsDictionary.Any((entry) => entry.Value.Count >= GameDefinition.TilesComboSum))
                {
                    KeyValuePair<TileType, List<TileGUI>> selectedEntry = tileTypeTileGUIsDictionary.First((entry) => entry.Value.Count >= GameDefinition.TilesComboSum);
                    List<TileGUI> moveHintTileGUIs = new List<TileGUI>();

                    for (int i = 0; i < GameDefinition.TilesComboSum; i++)
                        moveHintTileGUIs.Add(selectedEntry.Value[i]);

                    EventManager.Invoke<List<TileGUI>>(GameEventType.MoveHintTiles, GetOrderedMoveHintTileGUIs(moveHintTileGUIs));

                    return true;
                }
            }

            List<List<TileType>> allTileTypesList = new List<List<TileType>>();
            foreach (var tileType in inStackTileTypes.Distinct())
                allTileTypesList.Add(inStackTileTypes.FindAll((inStackTileType) => inStackTileType == tileType));

            while (allTileTypesList.Count != 0)
            {
                List<TileType> prioritizedCheckedTileTypes = allTileTypesList.OrderByDescending(l => l.Count).FirstOrDefault();
                if (prioritizedCheckedTileTypes.Count > 0)
                {
                    TileType prioritizedCheckedTileType = prioritizedCheckedTileTypes[0];

                    if (tileTypeTileGUIsDictionary.Any((entry) => entry.Key == prioritizedCheckedTileType
                                                               && prioritizedCheckedTileTypes.Count + entry.Value.Count
                                                               >= GameDefinition.TilesComboSum))
                    {
                        var selectedEntry = tileTypeTileGUIsDictionary.First((entry) => entry.Key == prioritizedCheckedTileType
                                                                                     && prioritizedCheckedTileTypes.Count + entry.Value.Count
                                                                                     >= GameDefinition.TilesComboSum);

                        if (GameDefinition.TilesComboSum - prioritizedCheckedTileTypes.Count + inStackTileTypes.Count <= GameDefinition.TileStackSize)
                        {
                            List<TileGUI> moveHintTileGUIs = new List<TileGUI>();

                            for (int i = 0; i < GameDefinition.TilesComboSum - prioritizedCheckedTileTypes.Count; i++)
                                moveHintTileGUIs.Add(selectedEntry.Value[i]);

                            EventManager.Invoke<List<TileGUI>>(GameEventType.MoveHintTiles, GetOrderedMoveHintTileGUIs(moveHintTileGUIs));

                            return true;
                        }
                    }
                }
                allTileTypesList.Remove(prioritizedCheckedTileTypes);
            }
        }

        return false;
    }

    public void Reload()
    {
        List<TileType> tileTypes = new List<TileType>();
        _tileGUIs.ForEach((tileGUI) => tileTypes.Add(tileGUI.OwnerTile.TileType));
        tileTypes.Shuffle();

        for (int i = 0; i < _tileGUIs.Count; i++)
        {
            _tileGUIs[i].OwnerTile.TileType = tileTypes[i];
            _tileGUIs[i].UpdateSprite();
        }

        SetTileHolderTransformsOrigin();
        MoveTileHolderTransformsOrigin();
    }

    private void CreateTilesHolders(int numberOfLayers)
    {
        _tileHolderGUIs = new List<TilesHolderGUI>();
        for (int i = 0; i < numberOfLayers; i++)
        {
            GameObject tilesHolderGameObject = PoolManager.GetObjectFromPool(GameplayDefinition.TilesHolderPoolType);
            tilesHolderGameObject.transform.position = Vector3.zero;
            tilesHolderGameObject.transform.SetParent(transform);
            _tileHolderGUIs.Add(tilesHolderGameObject.GetComponent<TilesHolderGUI>());
        }
    }

    private void CreatTiles(List<Tile> tiles, TutorialType tutorialType)
    {
        _tileGUIs = new List<TileGUI>();

        if (tutorialType == TutorialType.None)
        {
            foreach (var tile in tiles)
            {
                GameObject tileGameObject = PoolManager.GetObjectFromPool(TilePoolType.Tile);
                tileGameObject.transform.SetParent(_tileHolderGUIs[tile.Layer - 1].transform);
                tileGameObject.AddComponent<TileGUI>().Init(tile);
                _tileGUIs.Add(tileGameObject.GetComponent<TileGUI>());
            }
        }
        else
        {
            foreach (var tile in tiles)
            {
                GameObject tileGameObject = PoolManager.GetObjectFromPool(TilePoolType.Tile);
                tileGameObject.transform.SetParent(_tileHolderGUIs[tile.Layer - 1].transform);
                tileGameObject.AddComponent<TutorialTileGUI>().Init(tile);
                _tileGUIs.Add(tileGameObject.GetComponent<TutorialTileGUI>());
            }
        }
    }

    private void SetTilesPickablity()
    {
        foreach (var tileGUI in _tileGUIs)
        {
            int checkedLayer = tileGUI.Layer + 1;
            bool foundTileOverlap = false;

            while (checkedLayer <= _tileHolderGUIs.Count && !foundTileOverlap)
            {
                List<TileGUI> higherLayerTileGUIs = _tileGUIs.Where((checkedTileGUI) => checkedTileGUI.Layer == checkedLayer).ToList();
                foreach (var higherLayerTileGUI in higherLayerTileGUIs)
                {
                    float distance = Vector2.Distance(tileGUI.CurrentPosition, higherLayerTileGUI.CurrentPosition);
                    if (distance < GameplayDefinition.TilesOverlappedThreshold)
                    {
                        tileGUI.IsPickable = false;
                        foundTileOverlap = true;
                        break;
                    }
                }

                checkedLayer++;
            }

            if (!foundTileOverlap)
                tileGUI.IsPickable = true;
        }
    }

    private void SetTileSortingLayers()
    {
        List<TileGUI> orderedTileGUIs = new List<TileGUI>();
        List<TileGUI> checkedTileGUIs = new List<TileGUI>(_tileGUIs);

        while (checkedTileGUIs.Count != 0)
        {
            int lowestLayer = checkedTileGUIs.Min((tileGUI) => tileGUI.Layer);
            List<TileGUI> filteredTileGUIs = checkedTileGUIs.Where((tileGUI) => tileGUI.Layer == lowestLayer).ToList();
            float lowestPositionX = filteredTileGUIs.Min((tileGUI) => tileGUI.CurrentPosition.x);
            filteredTileGUIs = filteredTileGUIs.Where((tileGUI) => tileGUI.CurrentPosition.x == lowestPositionX).ToList();
            float highestPositionY = filteredTileGUIs.Max((tileGUI) => tileGUI.CurrentPosition.y);
            TileGUI filteredTileGUI = filteredTileGUIs.Find((tileGUI) => tileGUI.CurrentPosition.y == highestPositionY);

            orderedTileGUIs.Add(filteredTileGUI);
            checkedTileGUIs.Remove(filteredTileGUI);
        }

        int nextSortingLayer = TileDefinition.InitialSortingLayer;
        orderedTileGUIs.ForEach((orderedTileGUI) =>
        {
            orderedTileGUI.SetOriginalSortingOrder(nextSortingLayer);
            nextSortingLayer += 1;
        });

        GameplayManager.Instance.TileGUISortingOrder = nextSortingLayer;
    }

    private void SetTileHolderTransformsOrigin()
    {
        List<int> tileHolderOriginCodes = new List<int>();

        Action AddTileHolderOriginCodesToList = () =>
        {
            foreach (var tileHolderOrigin in Enum.GetValues(typeof(TilesHolderOrigin)))
                tileHolderOriginCodes.Add((int)tileHolderOrigin);
        };

        AddTileHolderOriginCodesToList();

        for (int i = 0; i < _tileHolderGUIs.Count; i++)
        {
            int randomTileHolderOriginCode = tileHolderOriginCodes.GetRandomFromList();
            tileHolderOriginCodes.Remove(randomTileHolderOriginCode);
            if (tileHolderOriginCodes.Count == 0)
                AddTileHolderOriginCodesToList();

            _tileHolderGUIs[i].SetOrigin((TilesHolderOrigin)randomTileHolderOriginCode);
        }
    }

    private void MoveTileHolderTransformsOrigin()
    {
        StartCoroutine(StartMovingTileHolderTransformsOrigin());
    }

    private IEnumerator StartMovingTileHolderTransformsOrigin()
    {
        yield return new WaitForSeconds(GameplayDefinition.MoveTileHolderTransformsDelayTime);

        int countIndex = 0;
        while (countIndex < _tileHolderGUIs.Count)
        {
            yield return new WaitForSeconds(GameplayDefinition.MoveNextTileHolderTransformDelayTime);
            _tileHolderGUIs[countIndex].MoveOrigin();
            countIndex++;
        }

        yield return new WaitForSeconds(GameplayDefinition.SetTilesVisualizationDelayTime);

        SetEnabledTileSortingLayers();
        SetTilesVisualization();

        yield break;
    }

    private void SetTilesVisualization()
    {
        _tileGUIs.ForEach((tileGUI) => tileGUI.SetVisualization());
    }

    private void SetEnabledTileSortingLayers()
    {
        List<TileGUI> enabledOrderedTileGUIs = new List<TileGUI>();
        List<TileGUI> checkedTileGUIs = new List<TileGUI>(_tileGUIs.FindAll((tileGUI) => tileGUI.IsPickable));

        while (checkedTileGUIs.Count != 0)
        {
            float lowestPositionX = checkedTileGUIs.Min((tileGUI) => tileGUI.CurrentPosition.x);
            List<TileGUI> filteredTileGUIs = checkedTileGUIs.Where((tileGUI) => tileGUI.CurrentPosition.x == lowestPositionX).ToList();
            float highestPositionY = filteredTileGUIs.Max((tileGUI) => tileGUI.CurrentPosition.y);
            TileGUI filteredTileGUI = filteredTileGUIs.Find((tileGUI) => tileGUI.CurrentPosition.y == highestPositionY);

            enabledOrderedTileGUIs.Add(filteredTileGUI);
            checkedTileGUIs.Remove(filteredTileGUI);
        }

        int nextSortingLayer = GameplayManager.Instance.TileGUISortingOrder;
        enabledOrderedTileGUIs.ForEach((enabledOrderedTileGUI) =>
        {
            enabledOrderedTileGUI.SetOriginalSortingOrder(nextSortingLayer);
            nextSortingLayer += 1;
        });

        GameplayManager.Instance.TileGUISortingOrder = nextSortingLayer;
    }

    private static List<TileGUI> GetOrderedMoveHintTileGUIs(List<TileGUI> moveHintTileGUIs)
    {
        List<TileGUI> orderedMoveHintTileGUIs = new List<TileGUI>();
        List<TileGUI> checkedTileGUIs = new List<TileGUI>(moveHintTileGUIs);

        while (checkedTileGUIs.Count != 0)
        {
            float lowestPositionX = checkedTileGUIs.Min((tileGUI) => tileGUI.CurrentPosition.x);
            List<TileGUI> filteredTileGUIs = checkedTileGUIs.Where((tileGUI) => tileGUI.CurrentPosition.x == lowestPositionX).ToList();
            float highestPositionY = filteredTileGUIs.Max((tileGUI) => tileGUI.CurrentPosition.y);
            TileGUI filteredTileGUI = filteredTileGUIs.Find((tileGUI) => tileGUI.CurrentPosition.y == highestPositionY);

            orderedMoveHintTileGUIs.Add(filteredTileGUI);
            checkedTileGUIs.Remove(filteredTileGUI);
        }

        return orderedMoveHintTileGUIs;
    }

    #endregion Class Methods
}