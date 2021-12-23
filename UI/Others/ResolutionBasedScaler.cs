using UnityEngine;

public class ResolutionBasedScaler : MonoBehaviour
{
    #region Members

    private const float designedResolution = 1080.0f / 1920.0f;
    private const float defaultScaledSize = 1.0f;
    private static readonly float scaledOffsetSize = 0.1f;

    [SerializeField]
    private bool _initAtStart = false;

    #endregion Members

    #region API Methods

    private void Start()
    {
        if (_initAtStart)
            Scale();
    }

    #endregion API Methods

    #region Class Methods

    public void Scale()
    {
        if (((float)Screen.width / Screen.height) < designedResolution)
            transform.localScale = Vector3.one * (((float)Screen.width / Screen.height)
                                 / (defaultScaledSize * designedResolution)
                                 + scaledOffsetSize);
        else
            transform.localScale = Vector3.one * defaultScaledSize;
    }

    #endregion Class Methods
}