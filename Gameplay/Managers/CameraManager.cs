using UnityEngine;

public class CameraManager : MonoBehaviour
{
    #region Members

    [SerializeField]
    private float _defaultOrthographicSize = 11.0f;
    [SerializeField]
    private float _desiredAspectRatio = 1080.0f / 1920.0f;
    private Camera _camera;

    #endregion Members

    #region API Methods

    private void Awake()
    {
        _camera = gameObject.GetComponent<Camera>();
        EventManager.AddListener<Level>(GameEventType.InitLevel, OnInitLevel);
    }

    private void Start()
    {
        if (_camera.aspect < _desiredAspectRatio)
            _camera.orthographicSize = _defaultOrthographicSize * _desiredAspectRatio / _camera.aspect;
        else
            _camera.orthographicSize = _defaultOrthographicSize;
    }

    #endregion API Methods

    #region Class Methods

    private void OnInitLevel(Level level)
    {
        float positionX = level.width * TileDefinition.TileHalfSize - TileDefinition.TileHalfSize;
        float positionY = level.height * TileDefinition.TileHalfSize - TileDefinition.TileHalfSize
                        - TileDefinition.TileSize
                        + GameplayDefinition.CameraOffsetCenter;
        transform.position = new Vector3(positionX, positionY, transform.position.z);
    }

    #endregion Class Methods
}