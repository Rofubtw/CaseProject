using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays selected entity information on the right panel (MVC View).
/// Shows building/unit details and unit production buttons when applicable.
/// </summary>
public class InfoPanelView : MonoBehaviour
{
    [Header("Entity Info")]
    [SerializeField] private GameObject infoContainer;
    [SerializeField] private Image entityImage;
    [SerializeField] private TextMeshProUGUI entityNameText;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Unit Production")]
    // [SerializeField] private GameObject productionContainer;
    [SerializeField] private Transform productionListParent;
    [SerializeField] private UnitProductionItemUI unitProductionItemPrefab;

    private Building selectedBuilding;
    private System.Action<UnitData> onUnitProductionRequested;

    /// <summary>
    /// Shows building info. If building can produce units, lists them as buttons.
    /// </summary>
    public void ShowBuildingInfo(Building building, System.Action<UnitData> onUnitProductionRequested)
    {
        selectedBuilding = building;
        this.onUnitProductionRequested = onUnitProductionRequested;

        infoContainer.SetActive(true);

        if (entityImage != null && building.Data.sprite != null)
            entityImage.sprite = building.Data.sprite;

        if (entityNameText != null)
            entityNameText.text = building.Data.entityName;

        if (healthText != null)
            healthText.text = $"HP: {building.CurrentHealth}/{building.MaxHealth}";

        // Show production list only if building can produce units
        bool canProduce = building.Data.CanProduceUnits;
        productionListParent.gameObject.SetActive(canProduce);

        ClearProductionList();

        if (canProduce)
        {
            foreach (var unitData in building.Data.producibleUnits)
            {
                UnitProductionItemUI item = Instantiate(unitProductionItemPrefab, productionListParent);
                item.Setup(unitData, OnUnitProductionClicked);
            }
        }
    }

    /// <summary>
    /// Shows unit (soldier) info. No production buttons.
    /// </summary>
    public void ShowUnitInfo(Unit unit)
    {
        selectedBuilding = null;

        infoContainer.SetActive(true);
        productionListParent.gameObject.SetActive(false);

        if (entityImage != null && unit.Data.sprite != null)
            entityImage.sprite = unit.Data.sprite;

        if (entityNameText != null)
            entityNameText.text = unit.Data.entityName;

        if (healthText != null)
            healthText.text = $"HP: {unit.CurrentHealth}/{unit.MaxHealth}\nDMG: {unit.Data.attackDamage}";

        ClearProductionList();
    }

    public void Hide()
    {
        infoContainer.SetActive(false);
        productionListParent.gameObject.SetActive(false);
        selectedBuilding = null;
        ClearProductionList();
    }

    /// <summary>
    /// Refreshes HP text for the currently displayed entity.
    /// </summary>
    public void UpdateHealth(int current, int max)
    {
        if (healthText != null)
            healthText.text = $"HP: {current}/{max}";
    }

    private void OnUnitProductionClicked(UnitData data)
    {
        onUnitProductionRequested?.Invoke(data);
    }

    private void ClearProductionList()
    {
        for (int i = productionListParent.childCount - 1; i >= 0; i--)
            Destroy(productionListParent.GetChild(i).gameObject);
    }
}
