using System.Collections;
using UnityEngine;

public class CrushChecker : MonoBehaviour
{
    [Header("Crush Detection")]
    public float raycastDistance = 0.75f;
    public LayerMask crushableLayers;
    
    [Header("Position Search")]
    public Vector2 boxCastSize = new Vector2(0.9f, 1.4f);
    public float searchStepDistance = 0.5f;
    public int maxSearchSteps = 5;
    
    [Header("Move Parameters")]
    public float minWallDistance = 0.3f;
    public float glitchDuration = 0.2f;

    private void Update()
    {
        CheckForCrush();
    }

    void CheckForCrush()
    {
        // Visualize raycasts with Debug.DrawRay (persists for duration)
        Debug.DrawRay(transform.position, Vector2.left * raycastDistance, Color.red, 0.5f);
        Debug.DrawRay(transform.position, Vector2.right * raycastDistance, Color.red, 0.5f);
        Debug.DrawRay(transform.position, Vector2.up * raycastDistance, Color.red, 0.5f);
        Debug.DrawRay(transform.position, Vector2.down * raycastDistance, Color.red, 0.5f);

        bool leftHit = Physics2D.Raycast(transform.position, Vector2.left, raycastDistance, crushableLayers);
        bool rightHit = Physics2D.Raycast(transform.position, Vector2.right, raycastDistance, crushableLayers);
        if (leftHit && rightHit)
        {
            TryGlitchToSafePosition(Vector2.left, Vector2.right);
            return;
        }

        bool upHit = Physics2D.Raycast(transform.position, Vector2.up, raycastDistance, crushableLayers);
        bool downHit = Physics2D.Raycast(transform.position, Vector2.down, raycastDistance, crushableLayers);
        if (upHit && downHit)
        {
            TryGlitchToSafePosition(Vector2.up, Vector2.down);
        }
    }

    void TryGlitchToSafePosition(Vector2 direction1, Vector2 direction2)
    {
        Vector2? safePosition = FindSafePosition(direction1);
        if (safePosition == null) safePosition = FindSafePosition(direction2);
        
        if (safePosition.HasValue) StartCoroutine(WaitAndGlitch(safePosition.Value, direction1, direction2));
    }
    
    IEnumerator WaitAndGlitch(Vector2 targetPosition, Vector2 direction1, Vector2 direction2)
    {
        while (true)
        {
            RaycastHit2D hit1 = Physics2D.Raycast(transform.position, direction1, raycastDistance, crushableLayers);
            RaycastHit2D hit2 = Physics2D.Raycast(transform.position, direction2, raycastDistance, crushableLayers);
            
            if (!hit1.collider || !hit2.collider) yield break;
            
            float wallDistance = hit1.distance + hit2.distance;
            if (wallDistance <= minWallDistance) break;

            yield return null;
        }
        
        Vector2 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < glitchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / glitchDuration;
            transform.position = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;
        Debug.Log("Glitched to safe position!");
    }
    
    private Vector2? FindSafePosition(Vector2 searchDirection)
    {
        for (int i = 1; i <= maxSearchSteps; i++)
        {
            Vector2 searchPosition = (Vector2)transform.position + (searchDirection * searchStepDistance * i);

            // Visualize boxcast search positions
            DrawBoxGizmo(searchPosition, boxCastSize, Color.yellow, 1f);

            RaycastHit2D hit = Physics2D.BoxCast(
                searchPosition,
                boxCastSize,
                0f,
                Vector2.zero,
                0f,
                crushableLayers
            );

            if (!hit.collider)
            {
                DrawBoxGizmo(searchPosition, boxCastSize, Color.green, 2f);
                return searchPosition;
            }
        }

        return null;
    }
    
    private void DrawBoxGizmo(Vector2 center, Vector2 size, Color color, float duration)
    {
        Vector2 halfSize = size * 0.5f;
        Vector2 topLeft = center + new Vector2(-halfSize.x, halfSize.y);
        Vector2 topRight = center + new Vector2(halfSize.x, halfSize.y);
        Vector2 bottomLeft = center + new Vector2(-halfSize.x, -halfSize.y);
        Vector2 bottomRight = center + new Vector2(halfSize.x, -halfSize.y);

        Debug.DrawLine(topLeft, topRight, color, duration);
        Debug.DrawLine(topRight, bottomRight, color, duration);
        Debug.DrawLine(bottomRight, bottomLeft, color, duration);
        Debug.DrawLine(bottomLeft, topLeft, color, duration);
    }
}
