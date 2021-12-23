using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ScrollRect))]
public class ScrollSnaper : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    #region Members

    [SerializeField]
    private Transform _pagesIndexPanel;
    [SerializeField]
    private int _startingPageIndex = 0;
    [SerializeField]
    private float _swipeThresholdDistance = 10.0f;
    [SerializeField]
    private float _transitionSpeed = 1.0f;
    [SerializeField]
    private AnimationCurve _transitionCurve;

    private int _swipeThresholdMaxLimit;
    private ScrollRect _pagesScrollRect;
    private RectTransform _pagesScrollContainer;
    private RectTransform _pagesContentContainer;
    private bool _isHorizontal;
    private int _numberOfPages;
    private int _currentPageIndex;
    private bool _isTransitioning;
    private Vector2 _desiredPagesContainPosition;
    private List<Vector2> _pagePositions;
    private bool _isDragging;
    private Vector2 _startDraggingPosition;
    private float _transitionInterpolationValue;

    #endregion Members

    #region API Methods

    private void Awake()
    {
        _pagesScrollRect = gameObject.GetComponent<ScrollRect>();
        _pagesScrollContainer = gameObject.GetComponent<RectTransform>();
        _pagesContentContainer = _pagesScrollRect.content;
    }

    private void Start()
    {
        _numberOfPages = _pagesContentContainer.childCount;
        _isTransitioning = false;
        _transitionInterpolationValue = 0.0f;

        if (_pagesScrollRect.horizontal && !_pagesScrollRect.vertical)
            _isHorizontal = true;
        else if (!_pagesScrollRect.horizontal && _pagesScrollRect.vertical)
            _isHorizontal = false;
        else
            _isHorizontal = true;

        InitPagePositions();
        SetPage(_startingPageIndex);
    }

    private void Update()
    {
        if (_isTransitioning)
        {
            _pagesScrollRect.velocity = Vector2.zero;
            _transitionInterpolationValue += Time.deltaTime * _transitionSpeed;
            _pagesContentContainer.anchoredPosition = Vector2.Lerp(_pagesContentContainer.anchoredPosition,
                                                                   _desiredPagesContainPosition,
                                                                   _transitionCurve.Evaluate(_transitionInterpolationValue));

            if (_transitionInterpolationValue >= 1.0f)
            {
                _pagesContentContainer.anchoredPosition = _desiredPagesContainPosition;
                _transitionInterpolationValue = 0.0f;
                _isTransitioning = false;
            }
        }
    }

    #endregion API Methods

    #region Class Methods

    public void OnBeginDrag(PointerEventData aEventData)
    {
        _isTransitioning = false;
        _isDragging = false;
    }

    public void OnDrag(PointerEventData aEventData)
    {
        if (!_isDragging)
        {
            _isDragging = true;
            _startDraggingPosition = _pagesContentContainer.anchoredPosition;
        }
    }

    public void OnEndDrag(PointerEventData aEventData)
    {
        float difference;
        if (_isHorizontal)
            difference = _startDraggingPosition.x - _pagesContentContainer.anchoredPosition.x;
        else
            difference = _pagesContentContainer.anchoredPosition.y - _startDraggingPosition.y;

        if (Mathf.Abs(difference) > _swipeThresholdDistance
         && Mathf.Abs(difference) < _swipeThresholdMaxLimit)
        {
            if (difference > 0)
                MoveToPage(_currentPageIndex + 1);
            else
                MoveToPage(_currentPageIndex - 1);

        }
        else MoveToPage(GetNearestPage());

        _isDragging = false;
    }

    private void InitPagePositions()
    {
        int width = 0;
        int height = 0;
        int offsetX = 0;
        int offsetY = 0;
        int containerWidth = 0;
        int containerHeight = 0;

        if (_isHorizontal)
        {
            width = (int)_pagesScrollContainer.rect.width;
            offsetX = width / 2;
            containerWidth = width * _numberOfPages;
            _swipeThresholdMaxLimit = width;
        }
        else
        {
            height = (int)_pagesScrollContainer.rect.height;
            offsetY = height / 2;
            containerHeight = height * _numberOfPages;
            _swipeThresholdMaxLimit = height;
        }

        Vector2 newSize = new Vector2(containerWidth, containerHeight);
        _pagesContentContainer.sizeDelta = newSize;
        Vector2 newPosition = new Vector2(containerWidth / 2, containerHeight / 2);
        _pagesContentContainer.anchoredPosition = newPosition;
        _pagePositions = new List<Vector2>();

        for (int i = 0; i < _numberOfPages; i++)
        {
            RectTransform child = _pagesContentContainer.GetChild(i).GetComponent<RectTransform>();
            Vector2 childPosition = Vector2.zero;

            if (_isHorizontal)
                childPosition = new Vector2(i * width - containerWidth / 2 + offsetX, 0f);
            else
                childPosition = new Vector2(0f, -(i * height - containerHeight / 2 + offsetY));

            child.anchoredPosition = childPosition;
            _pagePositions.Add(-childPosition);
        }
    }

    private void SetPage(int pageIndex)
    {
        pageIndex = Mathf.Clamp(pageIndex, 0, _numberOfPages - 1);
        _pagesContentContainer.anchoredPosition = _pagePositions[pageIndex];
        _currentPageIndex = pageIndex;

        for (int i = 0; i < _pagesIndexPanel.childCount; i++)
        {
            _pagesIndexPanel.GetChild(i).GetChild(0).gameObject.SetActive(i != pageIndex);
            _pagesIndexPanel.GetChild(i).GetChild(0).gameObject.SetActive(i == pageIndex);
        }
    }

    private int GetNearestPage()
    {
        float minDistance = float.MaxValue;
        int nearestPageIndex = _currentPageIndex;

        for (int i = 0; i < _pagePositions.Count; i++)
        {
            float distance = Vector2.SqrMagnitude(_pagesContentContainer.anchoredPosition - _pagePositions[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPageIndex = i;
            }
        }

        for (int i = 0; i < _pagesIndexPanel.childCount; i++)
        {
            _pagesIndexPanel.GetChild(i).GetChild(0).gameObject.SetActive(i != nearestPageIndex);
            _pagesIndexPanel.GetChild(i).GetChild(0).gameObject.SetActive(i == nearestPageIndex);
        }

        return nearestPageIndex;
    }

    private void MoveToPage(int pageIndex)
    {
        pageIndex = Mathf.Clamp(pageIndex, 0, _numberOfPages - 1);
        _desiredPagesContainPosition = _pagePositions[pageIndex];
        _transitionInterpolationValue = 0.0f;
        _isTransitioning = true;
        _currentPageIndex = pageIndex;

        for (int i = 0; i < _pagesIndexPanel.childCount; i++)
        {
            _pagesIndexPanel.GetChild(i).GetChild(0).gameObject.SetActive(i != pageIndex);
            _pagesIndexPanel.GetChild(i).GetChild(0).gameObject.SetActive(i == pageIndex);
        }
    }

    #endregion Class Methods
}