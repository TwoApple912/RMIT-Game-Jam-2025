using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Infinite tiling background that repeats a SpriteRenderer in all directions
/// and moves with parallax relative to the camera.
/// Works with orthographic cameras.
/// </summary>
[ExecuteAlways]
public class ParallaxTiledBackground : MonoBehaviour
{
    [Header("Required")]
    [Tooltip("The single tile sprite to repeat. Its scale sets tile size.")]
    public SpriteRenderer sourceTile;

    [Tooltip("Camera to follow. Defaults to Camera.main.")]
    public Camera targetCamera;

    [Header("Parallax")]
    [Range(0f, 1.5f)]
    [Tooltip("0 = fixed to world, 1 = locked to camera, >1 exaggerates motion.")]
    public float parallax = 0.5f;

    [Tooltip("Follow camera vertically too. If false, parallax only on X.")]
    public bool followY = true;

    [Header("Tiling")]
    [Tooltip("Extra margin (in world units) beyond camera bounds to avoid pop-in.")]
    public Vector2 margin = new Vector2(1f, 1f);

    [Tooltip("Rebuild the tile grid automatically if screen/aspect/tile size changes.")]
    public bool autoRebuild = true;

    [Tooltip("Optional: set a custom sorting layer/order for all tiles. Leave blank to copy source.")]
    public string overrideSortingLayer = "";
    public int overrideOrderInLayer = int.MinValue;

    // Internal
    Vector2 tileSize;          // world size of one tile
    int cols, rows;            // grid dimensions
    float totalWidth, totalHeight;
    Transform tilesRoot;
    readonly List<Transform> tiles = new List<Transform>();
    Vector3 initialWorldAnchor;
    Vector2 lastKnownTileSize;
    Vector2 lastCamSize;
    float lastParallax;
    bool initialized;

    void OnEnable()
    {
        if (!targetCamera) targetCamera = Camera.main;
        TryInit();
    }

    void OnValidate()
    {
        if (!targetCamera) targetCamera = Camera.main;
        if (autoRebuild && Application.isPlaying) RebuildIfNeeded(force:true);
    }

    void TryInit()
    {
        if (sourceTile == null || targetCamera == null) return;

        initialWorldAnchor = transform.position;
        BuildGrid();
        initialized = true;
    }

    void Update()
    {
        if (!initialized)
        {
            TryInit();
            if (!initialized) return;
        }

        if (autoRebuild) RebuildIfNeeded();

        // 1) Parallax follow
        var camPos = targetCamera.transform.position;
        var targetX = Mathf.Lerp(initialWorldAnchor.x, camPos.x, parallax);
        var targetY = followY ? Mathf.Lerp(initialWorldAnchor.y, camPos.y, parallax) : initialWorldAnchor.y;
        transform.position = new Vector3(targetX, targetY, transform.position.z);

        // 2) Wrap tiles so the grid always covers the camera
        WrapTilesToCoverCamera();
    }

    // ---------- Grid build & maintenance ----------

    void RebuildIfNeeded(bool force = false)
    {
        var camSize = GetCameraExtents();
        var currentTileSize = GetTileSize();
        bool changed = force
            || cols == 0 || rows == 0
            || currentTileSize != lastKnownTileSize
            || camSize != lastCamSize
            || Mathf.Abs(lastParallax - parallax) > 0.0001f;

        if (changed)
        {
            BuildGrid();
        }
    }

    void BuildGrid()
    {
        if (sourceTile == null || targetCamera == null) return;

        // Compute tile size in world units (uses the source tile’s current transform scale)
        tileSize = GetTileSize();
        lastKnownTileSize = tileSize;

        // Camera extents in world units
        var ext = GetCameraExtents();
        lastCamSize = ext;

        // How many tiles needed to cover the view + margin, with a safety band of +2
        cols = Mathf.Max(3, Mathf.CeilToInt((ext.x * 2f + margin.x * 2f) / tileSize.x) + 2);
        rows = Mathf.Max(3, Mathf.CeilToInt((ext.y * 2f + margin.y * 2f) / tileSize.y) + 2);

        totalWidth  = cols * tileSize.x;
        totalHeight = rows * tileSize.y;

        // (Re)create root
        if (tilesRoot == null)
        {
            var root = new GameObject(sourceTile.name + "_Tiles");
            root.hideFlags = Application.isPlaying ? HideFlags.None : HideFlags.DontSaveInEditor;
            tilesRoot = root.transform;
            tilesRoot.SetParent(transform, false);
        }
        else
        {
            // Clear existing children
            for (int i = tilesRoot.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying) Destroy(tilesRoot.GetChild(i).gameObject);
                else DestroyImmediate(tilesRoot.GetChild(i).gameObject);
            }
        }
        tiles.Clear();

        // Align the grid around the parent’s local origin
        Vector2 originOffset = new Vector2(-totalWidth * 0.5f + tileSize.x * 0.5f,
                                           -totalHeight * 0.5f + tileSize.y * 0.5f);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                var go = new GameObject($"tile_{x}_{y}");
                go.transform.SetParent(tilesRoot, false);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sourceTile.sprite;
                sr.color = sourceTile.color;
                sr.flipX = sourceTile.flipX;
                sr.flipY = sourceTile.flipY;
                sr.drawMode = SpriteDrawMode.Simple; // ensures 1:1 sprite usage

                // Sorting
                if (!string.IsNullOrEmpty(overrideSortingLayer))
                    sr.sortingLayerName = overrideSortingLayer;
                else
                    sr.sortingLayerID = sourceTile.sortingLayerID;

                sr.sortingOrder = (overrideOrderInLayer != int.MinValue) ? overrideOrderInLayer : sourceTile.sortingOrder;

                // Match transform scale/rotation of the source
                go.transform.localScale = sourceTile.transform.localScale;
                go.transform.localRotation = sourceTile.transform.localRotation;

                // Position in grid
                var pos = new Vector3(originOffset.x + x * tileSize.x,
                                      originOffset.y + y * tileSize.y,
                                      0f);
                go.transform.localPosition = pos;

                tiles.Add(go.transform);
            }
        }

        lastParallax = parallax;

        // Hide the source tile (we’re duplicating it now)
#if UNITY_EDITOR
        if (!Application.isPlaying) sourceTile.enabled = false;
#endif
        if (Application.isPlaying) sourceTile.enabled = false;
    }

    Vector2 GetTileSize()
    {
        // world-space size of the sprite using the source renderer’s current transform
        var b = sourceTile.bounds;
        return b.size;
    }

    Vector2 GetCameraExtents()
    {
        if (!targetCamera.orthographic)
        {
            // For perspective, approximate size at the background’s Z plane:
            float dist = Mathf.Abs(transform.position.z - targetCamera.transform.position.z);
            float frustumHeight = 2.0f * dist * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * targetCamera.aspect;
            return new Vector2(frustumWidth * 0.5f, frustumHeight * 0.5f);
        }
        else
        {
            float halfH = targetCamera.orthographicSize;
            float halfW = halfH * targetCamera.aspect;
            return new Vector2(halfW, halfH);
        }
    }

    void WrapTilesToCoverCamera()
    {
        if (tiles.Count == 0) return;

        // Camera bounds (in world space)
        var camPos = targetCamera.transform.position;
        float minX = camPos.x - GetCameraExtents().x - margin.x;
        float maxX = camPos.x + GetCameraExtents().x + margin.x;
        float minY = camPos.y - GetCameraExtents().y - margin.y;
        float maxY = camPos.y + GetCameraExtents().y + margin.y;

        // Move tiles by full grid width/height when they drift out of view bounds
        float wrapW = totalWidth;
        float wrapH = totalHeight;

        // Because tilesRoot moves with parallax, check each tile’s world position
        foreach (var t in tiles)
        {
            var p = t.position;

            // Horizontal wrap
            while (p.x + tileSize.x * 0.5f < minX) p.x += wrapW;
            while (p.x - tileSize.x * 0.5f > maxX) p.x -= wrapW;

            // Vertical wrap
            if (followY)
            {
                while (p.y + tileSize.y * 0.5f < minY) p.y += wrapH;
                while (p.y - tileSize.y * 0.5f > maxY) p.y -= wrapH;
            }

            t.position = p;
        }
    }
}
