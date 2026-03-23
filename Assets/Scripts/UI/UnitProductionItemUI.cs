using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Button for producing a unit from the Information Panel.
/// </summary>
public class UnitProductionItemUI : MonoBehaviour
{
    [SerializeField] private Image unitImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button button;

    private UnitData data;
    private Action<UnitData> onClicked;

    public void Setup(UnitData data, Action<UnitData> onClicked)
    {
        this.data = data;
        this.onClicked = onClicked;

        if (unitImage != null && data.sprite != null)
            unitImage.sprite = data.sprite;

        if (nameText != null)
            nameText.text = data.entityName;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        onClicked?.Invoke(this.data);
    }
}
