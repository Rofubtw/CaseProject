using System.Collections;
using UnityEngine;

/// <summary>
/// Visual representation of the grid. Draws grid lines and placement highlights
/// via GL (MVC View layer). Fully resolution-independent.
/// </summary>
public class GridView : MonoBehaviour
{
    [SerializeField] private Color gridLineColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color validPlacementColor = new Color(0f, 1f, 0f, 0.3f);
    [SerializeField] private Color invalidPlacementColor = new Color(1f, 0f, 0f, 0.3f);

    [SerializeField] private RectTransform gameArea;

    private GridModel model;
    private Material lineMaterial;

    // Highlight state — drawn via GL, no GameObjects needed
    private Vector2Int highlightOrigin;
    private Vector2Int highlightSize;
    private Color highlightColor;
    private bool showHighlight;

    public void Initialize(GridModel model)
    {
        this.model = model;
        CreateLineMaterial();
        StartCoroutine(PositionCameraDeferred());
    }

    private IEnumerator PositionCameraDeferred()
    {
        yield return null;
        PositionCamera();
    }

    private void CreateLineMaterial()
    {
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        lineMaterial = new Material(shader);
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        lineMaterial.SetInt("_ZWrite", 0);
        lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
    }

    private void OnRenderObject()
    {
        if (model == null || lineMaterial == null) return;

        float cs = model.CellSize;

        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.MultMatrix(Matrix4x4.identity);

        // --- Draw placement highlights as filled quads ---
        if (showHighlight)
        {
            GL.Begin(GL.QUADS);
            GL.Color(highlightColor);

            for (int x = highlightOrigin.x; x < highlightOrigin.x + highlightSize.x; x++)
            {
                for (int y = highlightOrigin.y; y < highlightOrigin.y + highlightSize.y; y++)
                {
                    if (!model.IsInBounds(x, y)) continue;

                    float x0 = x * cs;
                    float y0 = y * cs;
                    float x1 = (x + 1) * cs;
                    float y1 = (y + 1) * cs;

                    GL.Vertex3(x0, y0, 0f);
                    GL.Vertex3(x1, y0, 0f);
                    GL.Vertex3(x1, y1, 0f);
                    GL.Vertex3(x0, y1, 0f);
                }
            }

            GL.End();
        }

        // --- Draw grid lines ---
        GL.Begin(GL.LINES);
        GL.Color(gridLineColor);

        // Vertical line segments — skip only if BOTH adjacent cells are occupied (inside building)
        for (int x = 0; x <= model.Width; x++)
        {
            for (int y = 0; y < model.Height; y++)
            {
                bool leftOccupied = x > 0 && model.GetCell(x - 1, y).IsOccupied;
                bool rightOccupied = x < model.Width && model.GetCell(x, y).IsOccupied;
                if (leftOccupied && rightOccupied) continue;

                GL.Vertex3(x * cs, y * cs, 0);
                GL.Vertex3(x * cs, (y + 1) * cs, 0);
            }
        }

        // Horizontal line segments — skip only if BOTH adjacent cells are occupied (inside building)
        for (int y = 0; y <= model.Height; y++)
        {
            for (int x = 0; x < model.Width; x++)
            {
                bool belowOccupied = y > 0 && model.GetCell(x, y - 1).IsOccupied;
                bool aboveOccupied = y < model.Height && model.GetCell(x, y).IsOccupied;
                if (belowOccupied && aboveOccupied) continue;

                GL.Vertex3(x * cs, y * cs, 0);
                GL.Vertex3((x + 1) * cs, y * cs, 0);
            }
        }

        GL.End();
        GL.PopMatrix();
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
    /// Green = valid, Red = invalid. Drawn via GL in OnRenderObject.
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
        if (lineMaterial != null)
            DestroyImmediate(lineMaterial);
    }
}
