using System;
using UnityEngine;

public class ItemHoldAnchor : MonoBehaviour
{
    [Header("Parameters")]
    public float holdOffset = 0.75f;
    public float holdAnchorMoveSpeed = 10f;

    private float currentAngle;
    
    [Header("Trackers")]
    public GameObject currentHeldItem;

    [Header("References")]
    public PlayerScript playerScript;
    public Transform holdAnchor;

    private void Awake()
    {
        if (playerScript == null) playerScript = GetComponent<PlayerScript>();
    }

    void Update()
    {
        UpdateHoldAnchorPosition();
    }

    void UpdateHoldAnchorPosition()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // Calculate target angle based on mouse direction
        Vector3 direction = mousePosition - transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Smoothly rotate current angle toward target angle
        currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, holdAnchorMoveSpeed * Time.deltaTime);

        // Position anchor at fixed distance using current angle
        float radians = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * holdOffset;
        holdAnchor.position = transform.position + offset;
    }
}
