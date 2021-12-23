using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Playables;

public class HowToPlayPopUp : PopUp
{
    #region Members

    [SerializeField]
    private List<PlayableDirector> _playableDirectors;

    #endregion Members

    #region Class Methods

    public override void Init(BaseScene parentScene)
    {
        base.Init(parentScene);
        _playableDirectors.ForEach((playableDirector) => playableDirector.Play());
    }

    public override void Close()
    {
        _playableDirectors.ForEach((playableDirector) => playableDirector.Stop());
        base.Close();
    }

    #endregion Class Methods
}