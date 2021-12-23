using UnityEngine;

using TMPro;

public class AlertPopUp : PopUp
{
    #region Members

    [SerializeField]
    public TextMeshProUGUI _titleText;
    [SerializeField]
    public TextMeshProUGUI _descriptionText;

    #endregion Members

    #region Class Methods

    public void Init(string title, string description)
    {
        _titleText.text = title;
        _descriptionText.text = description;
    }

    #endregion Class Methods
}