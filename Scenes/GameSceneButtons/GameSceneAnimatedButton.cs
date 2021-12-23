using UnityEngine;

/// <summary>
/// This kind of button is used in the main game scene.
/// It is only allowed to be interacted when no popup shown in the main game scene.
/// </summary>
public class GameSceneAnimatedButton : AnimatedButton
{
    #region Members

    [SerializeField]
    private GameScene _gameScene;

    #endregion Members

    #region Class Methods

    protected override bool Press()
    {
        if (_gameScene.NoPopUpOpened())
            return base.Press();

        return false;
    }

    #endregion Class Methods
}