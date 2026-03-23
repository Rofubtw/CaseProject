using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Single building item in the Production Menu scroll list.
/// Displays building sprite, name, and size. Fires callback on click.
/// </summary>
public class ProductionItemUI : MonoBehaviour
{
    [SerializeField] private Image buildingImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI sizeText;
    [SerializeField] private Button button;

    private BuildingData data;
    private Action<BuildingData> onClicked;

    public void Setup(BuildingData data, Action<BuildingData> onClicked)
    {
        this.data = data;
        this.onClicked = onClicked;

        if (buildingImage != null && data.sprite != null)
            buildingImage.sprite = data.sprite;

        if (nameText != null)
            nameText.text = data.entityName;

        if (sizeText != null)
            sizeText.text = $"{data.size.x}x{data.size.y}";

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        onClicked?.Invoke(this.data);
    }
}
