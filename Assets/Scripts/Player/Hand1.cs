using System;
using UnityEngine;

public class Hand1 : MonoBehaviour
{
    [Header("Parameters")] public float distanceFromItem = 0.5f;

    [Header("Rotation")] // new rotation settings
    public float rotationOffset = 0f; // degrees to offset the sprite's forward direction
    public float rotationSpeed = 720f; // degrees per second when smoothing
    public bool smoothRotation = true;

    [Header("References")]
    public PlayerScript player;
    SpriteRenderer spriteRenderer;
    [Space]
    public SpriteRenderer realHand1;

    private void Awake()
    {
        if (player == null) player = FindObjectOfType<PlayerScript>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        transform.parent = null;
        spriteRenderer.enabled = false;
    }

    void Update()
    {
        if (player == null || player.currentHeldItem == null)
        {
            spriteRenderer.enabled = false;
            realHand1.enabled = true;
            return;
        }
        
        spriteRenderer.enabled = true;
        realHand1.enabled = false;

        Transform itemT = null;
        if (player.currentHeldItem is GameObject)
        {
            itemT = player.currentHeldItem.transform;
        }
        else if (player.currentHeldItem is Component comp)
        {
            itemT = comp.transform;
        }

        if (itemT == null || player.transform == null) return;

        Vector3 dir = player.transform.position - itemT.position;
        if (dir.sqrMagnitude < 1e-6f)
        {
            // stay exactly on the item if positions coincide, but continue to rotate below
            transform.position = itemT.position;
        }
        else
        {
            Vector3 targetPos = itemT.position + dir.normalized * distanceFromItem;
            transform.position = targetPos;
        }

        // --- Rotation: rotate the hand to face the held item ---
        Vector3 lookDir = (transform.position - itemT.position);
        if (lookDir.sqrMagnitude > 1e-8f)
        {
            float targetAngle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg + rotationOffset;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle);

            if (smoothRotation)
            {
                // rotationSpeed is degrees per second; RotateTowards takes degrees
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.rotation = targetRot;
            }
        }
    }
}
