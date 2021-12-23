using System.Collections.Generic;

using UnityEngine;

public class TilesPickerManager : MonoBehaviour
{
    #region Members

    private TileGUI _previousPickedTileGUI;
    private TileGUI _currentPickedTileGUI;

    #endregion Members

    #region API Methods

    private void Update()
    {
        if (GameplayManager.Instance.IsPlaying)
        {
            if (Input.GetMouseButton(0))
            {
                CheckHit();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                CheckHit();

                if (_currentPickedTileGUI)
                {
                    _currentPickedTileGUI.Pick();
                    _currentPickedTileGUI = null;
                    _previousPickedTileGUI = null;
                }
            }
        }
    }

    #endregion API Methods

    #region Class Methods

    private void CheckHit()
    {
        Ray2D ray = new Ray2D(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward);
        RaycastHit2D[] hits = Physics2D.RaycastAll(ray.origin, ray.direction, Mathf.Infinity, 1 << LayerMask.NameToLayer(TileDefinition.LayerMaskName));
        if (hits.Length > 0)
        {
            List<TileGUI> hitTileGUIs = new List<TileGUI>();
            foreach (var hit in hits)
                hitTileGUIs.Add(hit.transform.gameObject.GetComponent<TileGUI>());

            _currentPickedTileGUI = hitTileGUIs.Find((hitTileGUI) => hitTileGUI.IsPickable && hitTileGUI.IsInBoard);
            if (_currentPickedTileGUI)
            {
                _currentPickedTileGUI.Highlight();
                if (_currentPickedTileGUI != _previousPickedTileGUI)
                {
                    _previousPickedTileGUI?.Unhighlight();
                    _previousPickedTileGUI = _currentPickedTileGUI;
                }
            }
            else _previousPickedTileGUI?.Unhighlight();
        }
        else
        {
            _currentPickedTileGUI?.Unhighlight();
            _previousPickedTileGUI?.Unhighlight();
            _currentPickedTileGUI = null;
            _previousPickedTileGUI = null;
        }
    }

    #endregion Class Methods
}