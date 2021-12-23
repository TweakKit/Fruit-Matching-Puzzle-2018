using System.Collections;

using UnityEngine;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    #region Members

    [SerializeField]
    private float _loadLoadingBarDelayTime;
    [SerializeField]
    private float _minLoadLoadingBarTime = 0.5f;
    [SerializeField]
    private float _maxLoadLoadingBarTime = 1.5f;
    [SerializeField]
    private Image _loadingBarSliderImage;
    [SerializeField]
    private SceneTransition _sceneTransition;

    #endregion Members

    #region API Methods

    private IEnumerator Start()
    {
        _loadingBarSliderImage.fillAmount = 0.0f;
        yield return new WaitForSeconds(_loadLoadingBarDelayTime);
        float randomLoadLoadingBarTime = Random.Range(_minLoadLoadingBarTime, _maxLoadLoadingBarTime);
        float time = 0.0f;

        while (time < randomLoadLoadingBarTime)
        {
            time += Time.deltaTime;
            _loadingBarSliderImage.fillAmount = time / randomLoadLoadingBarTime;
            yield return null;
        }

        _loadingBarSliderImage.fillAmount = 1.0f;
        _sceneTransition.PerformTransition();
    }

    #endregion API Methods
}