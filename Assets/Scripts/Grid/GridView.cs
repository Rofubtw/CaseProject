using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Visual representation of the grid. Draws grid lines and placement highlights
/// via a dynamic Mesh (MVC View layer). Fully resolution-independent.
/// </summary>
public class GridView : MonoBehaviour
{
    [SerializeField] private Color gridLineColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color validPlacementColor = new Color(0f, 1f, 0f, 0.3f);
    [SerializeField] private Color invalidPlacementColor = new Color(1f, 0f, 0f, 0.3f);

    [SerializeField] private RectTransform gameArea;

    private GridModel model;
    private Material gridMaterial;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh gridMesh;
    private GameObject gridObj;

    // Highlight state
    private Vector2Int highlightOrigin;
    private Vector2Int highlightSize;
    private Color highlightColor;
    private bool showHighlight;

    // Resolution tracking for camera re-positioning
    private int lastScreenWidth;
    private int lastScreenHeight;

    // Reusable buffers to avoid GC every frame
    private readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<Color> colors = new List<Color>();
    private readonly List<int> triIndices = new List<int>();

    public void Initialize(GridModel model)
    {
        this.model = model;
        SetupMeshRenderer();
        StartCoroutine(PositionCameraDeferred());
    }

    private IEnumerator PositionCameraDeferred()
    {
        // Wait two frames to ensure canvas layout is fully resolved at any resolution
        yield return null;
        yield return null;
        PositionCamera();
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    private void SetupMeshRenderer()
    {
        // Root-level object to avoid inheriting any parent transform scale/position
        gridObj = new GameObject("GridMesh");
        gridObj.transform.position = Vector3.zero;
        gridObj.transform.rotation = Quaternion.identity;
        gridObj.transform.localScale = Vector3.one;

        meshFilter = gridObj.AddComponent<MeshFilter>();
        meshRenderer = gridObj.AddComponent<MeshRenderer>();

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("UI/Default");
        gridMaterial = new Material(shader);
        gridMaterial.hideFlags = HideFlags.HideAndDontSave;
        gridMaterial.color = Color.white;

        meshRenderer.sharedMaterial = gridMaterial;
        meshRenderer.sortingOrder = -1;

        gridMesh = new Mesh();
        gridMesh.MarkDynamic();
        meshFilter.mesh = gridMesh;
    }

    private void LateUpdate()
    {
        if (model == null) return;

        // Re-position camera when resolution changes
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            StartCoroutine(PositionCameraDeferred());
        }

        RebuildMesh();
    }

    private void RebuildMesh()
    {
        vertices.Clear();
        colors.Clear();
        triIndices.Clear();

        float cs = model.CellSize;
        // Scale thickness relative to cell size so lines are visible at any resolution
        float half = cs * 0.03f;

        // --- Placement highlights as filled quads (triangles) ---
        if (showHighlight)
        {
            for (int x = highlightOrigin.x; x < highlightOrigin.x + highlightSize.x; x++)
            {
                for (int y = highlightOrigin.y; y < highlightOrigin.y + highlightSize.y; y++)
                {
                    if (!model.IsInBounds(x, y)) continue;
                    AddQuad(x * cs, y * cs, (x + 1) * cs, (y + 1) * cs, highlightColor);
                }
            }
        }

        // --- Grid lines as thin quads ---
        // Vertical line segments
        for (int x = 0; x <= model.Width; x++)
        {
            for (int y = 0; y < model.Height; y++)
            {
                bool leftOccupied = x > 0 && model.GetCell(x - 1, y).IsOccupied;
                bool rightOccupied = x < model.Width && model.GetCell(x, y).IsOccupied;
                if (leftOccupied && rightOccupied) continue;

                float lx = x * cs;
                AddQuad(lx - half, y * cs, lx + half, (y + 1) * cs, gridLineColor);
            }
        }

        // Horizontal line segments
        for (int y = 0; y <= model.Height; y++)
        {
            for (int x = 0; x < model.Width; x++)
            {
                bool belowOccupied = y > 0 && model.GetCell(x, y - 1).IsOccupied;
                bool aboveOccupied = y < model.Height && model.GetCell(x, y).IsOccupied;
                if (belowOccupied && aboveOccupied) continue;

                float ly = y * cs;
                AddQuad(x * cs, ly - half, (x + 1) * cs, ly + half, gridLineColor);
            }
        }

        gridMesh.Clear();
        gridMesh.SetVertices(vertices);
        gridMesh.SetColors(colors);
        gridMesh.subMeshCount = 1;
        gridMesh.SetIndices(triIndices, MeshTopology.Triangles, 0);
        // Very large bounds to prevent frustum culling at any resolution
        gridMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
    }

    private void AddQuad(float x0, float y0, float x1, float y1, Color color)
    {
        int i = vertices.Count;
        vertices.Add(new Vector3(x0, y0, 0f));
        vertices.Add(new Vector3(x1, y0, 0f));
        vertices.Add(new Vector3(x1, y1, 0f));
        vertices.Add(new Vector3(x0, y1, 0f));

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

        triIndices.Add(i);
        triIndices.Add(i + 2);
        triIndices.Add(i + 1);
        triIndices.Add(i);
        triIndices.Add(i + 3);
        triIndices.Add(i + 2);
    }

    /// <summary>
    /// Positions and sizes the camera so the grid fills the Middle panel area
    /// at any resolution, without using Camera.rect.
    /// </summary>
    private void PositionCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        // Ensure full-screen viewport
        cam.rect = new Rect(0, 0, 1, 1);

        float screenAspect = (float)Screen.width / Screen.height;

        // Determine what fraction of the screen the game area occupies
        float leftNorm = 0f;
        float rightNorm = 1f;

        if (gameArea != null)
        {
            Vector3[] corners = new Vector3[4];
            gameArea.GetWorldCorners(corners);
            leftNorm = corners[0].x / Screen.width;
            rightNorm = corners[2].x / Screen.width;
        }

        float middleFraction = rightNorm - leftNorm;

        float gridWorldWidth = model.Width * model.CellSize;
        float gridWorldHeight = model.Height * model.CellSize;

        // The middle area's effective aspect ratio
        float middleAspect = middleFraction * screenAspect;
        float gridAspect = gridWorldWidth / gridWorldHeight;

        float halfHeight;
        if (middleAspect >= gridAspect)
            halfHeight = gridWorldHeight / 2f;
        else
            halfHeight = (gridWorldWidth / middleAspect) / 2f;

        cam.orthographicSize = halfHeight;

        float halfWidth = halfHeight * screenAspect;

        // Position camera so grid origin (0,0) maps to the left edge of the game area
        float camX = halfWidth * (1f - 2f * leftNorm);
        float camY = halfHeight;

        cam.transform.position = new Vector3(camX, camY, cam.transform.position.z);
    }

    /// <summary>
    /// Shows colored highlights on the grid for building placement preview.
    /// </summary>
    public void ShowPlacementPreview(Vector2Int origin, Vector2Int size, bool isValid)
    {
        highlightOrigin = origin;
        highlightSize = size;
        highlightColor = isValid ? validPlacementColor : invalidPlacementColor;
        showHighlight = true;
    }

    public void ClearHighlights()
    {
        showHighlight = false;
    }

    private void OnDestroy()
    {
        if (gridObj != null)
            Destroy(gridObj);
        if (gridMaterial != null)
            DestroyImmediate(gridMaterial);
        if (gridMesh != null)
            DestroyImmediate(gridMesh);
    }
}
