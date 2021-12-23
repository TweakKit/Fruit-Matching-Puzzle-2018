using UnityEngine;

public class PausePopUp : PopUp
{
    #region Members

    [SerializeField]
    private AnimatedButton _bgmSoundButton;
    [SerializeField]
    private AnimatedButton _sfxSoundButton;

    #endregion Members

    #region Class Methods

    public override void Init(BaseScene parentScene)
    {
        base.Init(parentScene);
        _bgmSoundButton.GetComponentInChildren<SpriteSwapper>().SetEnabled(PlayerPrefsManager.Instance.SavedData.SoundBGMOn);
        _sfxSoundButton.GetComponentInChildren<SpriteSwapper>().SetEnabled(PlayerPrefsManager.Instance.SavedData.SoundSFXOn);
    }

    public void ChangeBGMSoundState()
    {
        SoundManager.Instance.SetBgm(!PlayerPrefsManager.Instance.SavedData.SoundBGMOn);
    }

    public void ChangeSFXSoundState()
    {
        SoundManager.Instance.SetSfx(!PlayerPrefsManager.Instance.SavedData.SoundSFXOn);
    }

    #endregion Class Methods
}