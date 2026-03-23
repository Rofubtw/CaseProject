using UnityEngine;

/// <summary>
/// Handles entity selection via mouse click and raycasting.
/// Left click on building/unit = select, left click on empty = deselect.
/// </summary>
public class SelectionController : MonoBehaviour
{
    [SerializeField] private LayerMask selectableLayer;

    private Camera mainCamera;
    private DamageableEntity currentSelection;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            TrySelect();
        }
    }

    private void TrySelect()
    {
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = selectableLayer == 0
            ? Physics2D.Raycast(mouseWorldPos, Vector2.zero)
            : Physics2D.Raycast(mouseWorldPos, Vector2.zero, Mathf.Infinity, selectableLayer);

        Debug.Log($"[Selection] Raycast at {mouseWorldPos}, hit: {(hit.collider != null ? hit.collider.name : "nothing")}");

        if (hit.collider != null)
        {
            DamageableEntity entity = hit.collider.GetComponentInParent<DamageableEntity>();
            if (entity != null && entity != currentSelection)
            {
                currentSelection = entity;
                Debug.Log($"[Selection] Selected: {entity.name}");
                EventBus<EntitySelectedEvent>.Raise(new EntitySelectedEvent(entity));
                return;
            }
        }

        // Clicked on empty space
        if (currentSelection != null)
        {
            currentSelection = null;
            EventBus<SelectionClearedEvent>.Raise(new SelectionClearedEvent());
        }
    }

    private bool IsPointerOverUI()
    {
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
}
