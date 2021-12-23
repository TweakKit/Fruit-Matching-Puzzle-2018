using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class TileStackGUI : MonoBehaviour
{
    #region Members

    private TileGUI[] _tileGUIs;
    private List<TileGUI> _rewindTileGUIs;

    public List<TileType> TileTypes
    {
        get
        {
            List<TileType> tileTypes = new List<TileType>();

            for (int i = 0; i < _tileGUIs.Length; i++)
                if (_tileGUIs[i])
                    tileTypes.Add(_tileGUIs[i].OwnerTile.TileType);
                else
                    break;

            return tileTypes;
        }
    }

    #endregion Members

    #region API Methods

    private void Start()
    {
        _tileGUIs = new TileGUI[GameDefinition.TileStackSize];
        _rewindTileGUIs = new List<TileGUI>();
    }

    #endregion API Methods

    #region Class Methods

    public void Reset()
    {
        for (int i = 0; i < _tileGUIs.Length; i++)
            if (_tileGUIs[i])
                _tileGUIs[i].Destroy();

        _tileGUIs = new TileGUI[GameDefinition.TileStackSize];
        _rewindTileGUIs = new List<TileGUI>();
    }

    public TileStackData GetNextTileStackData(TileGUI tileGUI)
    {
        for (int i = 0; i < _tileGUIs.Length; i++)
        {
            if (_tileGUIs[i])
            {
                if (_tileGUIs[i].OwnerTile.TileType == tileGUI.OwnerTile.TileType)
                {
                    for (int j = i + 1; j < _tileGUIs.Length; j++)
                    {
                        if (!_tileGUIs[j])
                        {
                            return new TileStackData(j, transform.TransformPoint(GameplayDefinition.InStackLocalTilePositions[j]));
                        }
                        else if (_tileGUIs[j].OwnerTile.TileType != tileGUI.OwnerTile.TileType)
                        {
                            for (int h = _tileGUIs.Length - 1; h > j; h--)
                            {
                                if (_tileGUIs[h - 1])
                                {
                                    _tileGUIs[h] = _tileGUIs[h - 1];
                                    _tileGUIs[h].MoveSlowInsideStack(transform.TransformPoint(GameplayDefinition.InStackLocalTilePositions[h]));
                                }
                            }

                            return new TileStackData(j, transform.TransformPoint(GameplayDefinition.InStackLocalTilePositions[j]));
                        }
                    }
                }
            }
            else return new TileStackData(i, transform.TransformPoint(GameplayDefinition.InStackLocalTilePositions[i]));
        }

        return TileStackData.Default;
    }

    public void InsertATileGUI(TileGUI tileGUI, int insertedIndex)
    {
        tileGUI.transform.SetParent(this.transform);
        _tileGUIs[insertedIndex] = tileGUI;
        _rewindTileGUIs.Add(tileGUI);
        SetTileOrderLayers();

        List<TileGUI> comboTileGUIs;
        List<ComboMovedTile> comboMovedTiles;

        if (HasATilesCombo(out comboTileGUIs, out comboMovedTiles))
        {
            var sequence = LeanTween.sequence();
            sequence.append(GameplayDefinition.DestroyTilesComboDelayTime);
            sequence.append(() => DestroyTilesCombo(comboTileGUIs));
            sequence.append(GameplayDefinition.RearrangeTileStackDelayTime);
            sequence.append(() => RearrangeTileStack(comboMovedTiles));
        }
        else
        {
            if (IsFull())
            {
                GameplayManager.Instance.SetGameState(GameState.NotPlaying);
                SoundManager.Instance.PlaySound(SoundManager.Instance.loseLevelSFXClip);
                LeanTween.delayedCall(GameplayDefinition.FadeTileStackDelayTime, FadeInTiles);
            }
        }
    }

    public TileGUI RewindATile(Action onPlacedOnBoard)
    {
        if (_rewindTileGUIs.Count > 0)
        {
            TileGUI rewindTileGUI = _rewindTileGUIs[_rewindTileGUIs.Count - 1];

            for (int i = 0; i < _tileGUIs.Length; i++)
            {
                if (rewindTileGUI == _tileGUIs[i])
                {
                    _tileGUIs[i] = null;
                    break;
                }
            }

            bool stop = false;
            for (int i = 0; i < _tileGUIs.Length - 1 && !stop; i++)
            {
                if (!_tileGUIs[i] && _tileGUIs[i + 1])
                {
                    for (int j = i + 1; j < _tileGUIs.Length; j++)
                    {
                        if (_tileGUIs[j])
                        {
                            _tileGUIs[j - 1] = _tileGUIs[j];
                            _tileGUIs[j] = null;

                            Vector3 newInStackPosition = transform.TransformPoint(GameplayDefinition.InStackLocalTilePositions[j - 1]);
                            _tileGUIs[j - 1].MoveSlowInsideStack(newInStackPosition);
                        }
                        else
                        {
                            stop = true;
                            break;
                        }
                    }
                }
            }

            rewindTileGUI.MoveToBoard(onPlacedOnBoard);
            _rewindTileGUIs.RemoveAt(_rewindTileGUIs.Count - 1);

            return rewindTileGUI;
        }

        return null;
    }

    public void ReviveTiles()
    {
        for (int i = 0; i < _tileGUIs.Length; i++)
            if (_tileGUIs[i])
                _tileGUIs[i].FadeOut();
    }

    private void SetTileOrderLayers()
    {
        List<int> sortingOrders = new List<int>();

        for (int i = 0; i < _tileGUIs.Length; i++)
            if (_tileGUIs[i])
                sortingOrders.Add(_tileGUIs[i].CurrentSortingOrder + GameManager.Instance.CurrentLevel.numberOfTiles);

        sortingOrders.Sort();

        for (int i = 0; i < sortingOrders.Count; i++)
            _tileGUIs[i].SetSortingOrder(sortingOrders[i]);
    }

    private bool HasATilesCombo(out List<TileGUI> comboTileGUIs, out List<ComboMovedTile> comboMovedTiles)
    {
        comboTileGUIs = new List<TileGUI>();
        comboMovedTiles = new List<ComboMovedTile>();

        for (int i = 0; i < _tileGUIs.Length; i++)
        {
            if (_tileGUIs[i])
            {
                int tempIndex = i + 1;
                int countTilesCombo = 1;

                while (tempIndex < _tileGUIs.Length && tempIndex < i + GameDefinition.TilesComboSum)
                {
                    if (_tileGUIs[tempIndex] && _tileGUIs[tempIndex].OwnerTile.TileType == _tileGUIs[i].OwnerTile.TileType)
                        countTilesCombo++;
                    else
                        break;

                    tempIndex++;
                }

                if (countTilesCombo == GameDefinition.TilesComboSum)
                {
                    for (int j = i; j < i + GameDefinition.TilesComboSum; j++)
                    {
                        comboTileGUIs.Add(_tileGUIs[j]);
                        _rewindTileGUIs.Remove(_tileGUIs[j]);
                        _tileGUIs[j] = null;
                    }

                    for (int j = i + GameDefinition.TilesComboSum; j < _tileGUIs.Length; j++)
                    {
                        if (_tileGUIs[j])
                        {
                            int newIndex = j - GameDefinition.TilesComboSum;
                            _tileGUIs[newIndex] = _tileGUIs[j];
                            _tileGUIs[j] = null;

                            Vector3 newComboMovedTilePosition = transform.TransformPoint(GameplayDefinition.InStackLocalTilePositions[newIndex]);
                            comboMovedTiles.Add(new ComboMovedTile(_tileGUIs[newIndex], newComboMovedTilePosition));
                        }
                    }

                    return true;
                }
            }
            else return false;
        }

        return false;
    }

    private void DestroyTilesCombo(List<TileGUI> comboTileGUIs)
    {
        comboTileGUIs.ForEach((comboTileGUI) => comboTileGUI.Disappear());
        EventManager.Invoke(GameEventType.WinTilesCombo);
        SoundManager.Instance.PlaySound(SoundManager.Instance.winComboSFXClip);
    }

    private void RearrangeTileStack(List<ComboMovedTile> comboMovedTiles)
    {
        comboMovedTiles.ForEach((comboMovedTile) =>
        {
            if (comboMovedTile.movedTileGUI.OwnerTile != null)
                comboMovedTile.movedTileGUI.MoveQuickInsideStack(comboMovedTile.moveToPosition);
        });
    }

    private void FadeInTiles()
    {
        for (int i = 0; i < _tileGUIs.Length; i++)
            _tileGUIs[i].FadeIn();

        LeanTween.delayedCall(GameplayDefinition.FadeTileStackDelayTime, () => EventManager.Invoke(GameEventType.LoseLevel));
    }

    private bool IsFull() => _tileGUIs[GameDefinition.TileStackSize - 1] != null;
    public bool IsEmpty() => _tileGUIs[0] == null;

    #endregion Class Methods
}

public struct TileStackData
{
    #region Members

    public static TileStackData Default = new TileStackData();
    public readonly int inStackTilePositionIndex;
    public readonly Vector3 inStackTilePosition;

    #endregion Members

    #region Struct Methods

    public TileStackData(int inStackTilePositionIndex, Vector3 inStackTilePosition)
    {
        this.inStackTilePositionIndex = inStackTilePositionIndex;
        this.inStackTilePosition = inStackTilePosition;
    }

    #endregion Struct Methods
}

public struct ComboMovedTile
{
    #region Members

    public readonly TileGUI movedTileGUI;
    public readonly Vector3 moveToPosition;

    #endregion Members

    #region Struct Methods

    public ComboMovedTile(TileGUI movedTileGUI, Vector3 moveToPosition)
    {
        this.movedTileGUI = movedTileGUI;
        this.moveToPosition = moveToPosition;
    }

    #endregion Struct Methods
}