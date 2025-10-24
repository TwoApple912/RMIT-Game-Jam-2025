using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [Header("Parameters")]
    public float runSpeed = 40f;
    [Space]
    public float pickedObjectPullStrength = 20f;
    public float throwForce = 12f;

    private float horizontalMove = 0f;
    private bool jumpPressed = false;
    private bool jumpHeld = false;
    private bool crouch = false;
    
    [Header("Tracker")]
    public bool allowMovement = true;
    [Space]
    public bool isHoldingItem = false;
    public PickupObject currentHeldItem;
    public List<GameObject> pickupItemsInRange = new List<GameObject>();
    
    [Header("References")]
    public CharacterController2D controller;
    [Space]
    public Transform holdAnchor;

    void Update()
    {
        TakeInput();
        UpdatePickedUpItemPosition();
    }

    void FixedUpdate()
    {
        ProcessMovement();
    }
    
    #region Input

    void TakeInput()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
        // crouch = Input.GetKey(KeyCode.LeftControl);
        
        if (allowMovement && Input.GetButtonDown("Jump")) jumpPressed = true;
        jumpHeld = Input.GetButton("Jump");
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isHoldingItem)
            { 
                DropHeldItem();
                Debug.Log("dropped");
            }
            else
            {
                PickUpNearestItem();
                Debug.Log("picked up");
            }
        }

        if (Input.GetMouseButtonDown(0) && isHoldingItem)
        {
            ThrowHeldItem();
        }
    }

    void ProcessMovement()
    {
        if (allowMovement) controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jumpPressed, jumpHeld);
        jumpPressed = false; // consume press
    }
    
    #endregion
    
    #region Pick up
    
    public void UpdatePickupItemsInRangeList(GameObject item, bool isEntering)
    {
        if (isEntering)
        {
            if (!pickupItemsInRange.Contains(item))
            {
                pickupItemsInRange.Add(item);
            }
        }
        else
        {
            if (pickupItemsInRange.Contains(item))
            {
                pickupItemsInRange.Remove(item);
            }
        }
    }
    
    void PickUpNearestItem()
    {
        GameObject nearestItem = ChooseNearestItem();
        Debug.Log(nearestItem);
        if (nearestItem != null)
        { 
            if (nearestItem.GetComponent<PickupObject>() != null)
            {
                isHoldingItem = true;
                currentHeldItem = nearestItem.GetComponent<PickupObject>();
                
                currentHeldItem.PickedUp(gameObject);
            }
        }
    }

    void DropHeldItem()
    {
        if (currentHeldItem != null)
        {
            currentHeldItem.Dropped(gameObject);
            currentHeldItem = null;
            isHoldingItem = false;
        }
    }
    
    GameObject ChooseNearestItem()
    {
        if (pickupItemsInRange.Count == 0) return null;

        GameObject nearestItem = null;
        float nearestDistanceSqr = float.MaxValue;
        Vector3 playerPosition = transform.position;

        foreach (GameObject item in pickupItemsInRange)
        {
            float distanceSqr = (item.transform.position - playerPosition).sqrMagnitude;
            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestItem = item;
            }
        }

        return nearestItem;
    }

    void UpdatePickedUpItemPosition()
    {
        if (isHoldingItem && currentHeldItem != null)
        {
            Vector3 targetPos = holdAnchor.position;
            Vector3 currentPos = currentHeldItem.transform.position;
            Vector3 direction = targetPos - currentPos;
            float distance = direction.magnitude;
            var rb2d = currentHeldItem.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                Vector3 desiredVelocity = direction.normalized * distance * pickedObjectPullStrength;
                rb2d.velocity = Vector2.Lerp(rb2d.velocity, desiredVelocity, Time.deltaTime * pickedObjectPullStrength);
                return;
            }

            var rb = currentHeldItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 desiredVelocity = direction.normalized * distance * pickedObjectPullStrength;
                rb.velocity = Vector3.Lerp(rb.velocity, desiredVelocity, Time.deltaTime * pickedObjectPullStrength);
                return;
            }

            currentHeldItem.transform.position =
                Vector3.Lerp(currentPos, targetPos, Time.deltaTime * pickedObjectPullStrength);
        }
    }

    #endregion

    void ThrowHeldItem()
    {
        if (currentHeldItem == null) return;

        PickupObject thrown = currentHeldItem;
        
        thrown.Dropped(gameObject);
        
        currentHeldItem = null;
        isHoldingItem = false;
        
        Camera cam = Camera.main;
        Vector3 throwDir = (holdAnchor.position - transform.position).normalized;
        
        var rb2d = thrown.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            Vector2 force = new Vector2(throwDir.x, throwDir.y) * throwForce;
            rb2d.AddForce(force, ForceMode2D.Impulse);
            return;
        }
        
        thrown.transform.position = thrown.transform.position + throwDir * 0.5f;
    }
}