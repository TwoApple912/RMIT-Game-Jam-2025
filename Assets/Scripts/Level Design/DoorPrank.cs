using UnityEngine;

public class DoorPrank : MonoBehaviour
{
    [Header("References")]
    public Door door;

    [Space]
    public SpriteRenderer spriteRenderer;

    // New: sprites and tuning
    [Header("Prank Sprites")]
    public Sprite commonSprite; // 90%
    public Sprite rareSprite;   // 10%
    [Range(0f, 1f)] public float rareChance = 0.1f;
    [Tooltip("How close the door must be to consider it at the closed position (world units).")]
    public float closedDistanceTolerance = 0.01f;

    // Internal state to ensure we only roll once per arrival
    private bool _hasRolledThisStay;

    private void Awake()
    {
        // Fallback assignment
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Check each frame; will only roll once when returning to the closed spot
        CheckClosedAndRoll();
    }

    // Returns true if the door's rigidbody is within tolerance of the closed position
    private bool IsAtClosedPosition()
    {
        if (door == null || door.rb == null)
            return false;

        Vector2 current = door.rb.position;
        Vector2 target = new Vector2(door.closedPosition.x, door.closedPosition.y);
        float sqTol = closedDistanceTolerance * closedDistanceTolerance;
        return (current - target).sqrMagnitude <= sqTol;
    }

    // Public so it can be called from other scripts or animation events if desired
    public void CheckClosedAndRoll()
    {
        bool atClosed = IsAtClosedPosition();

        if (atClosed)
        {
            if (!_hasRolledThisStay)
            {
                RollSprite();
                _hasRolledThisStay = true;
            }
        }
        else
        {
            // Reset latch when moving away so we can roll again on next return
            _hasRolledThisStay = false;
        }
    }

    private void RollSprite()
    {
        if (spriteRenderer == null)
            return;

        // If sprites are not assigned, do nothing gracefully
        if (commonSprite == null && rareSprite == null)
            return;

        float r = Random.value;
        bool useRare = r < rareChance && rareSprite != null;

        // Prefer a valid sprite; if rare/common missing, fallback to the other
        if (useRare)
        {
            spriteRenderer.sprite = rareSprite;
        }
        else
        {
            spriteRenderer.sprite = commonSprite != null ? commonSprite : rareSprite;
        }
    }
}
