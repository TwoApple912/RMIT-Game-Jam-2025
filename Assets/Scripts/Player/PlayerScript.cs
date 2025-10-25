using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class PlayerScript : MonoBehaviour
{
    [Header("Parameters")]
    public bool allowInput = true;
    [Space]
    public float walkSpeed = 40f;
    [Space]
    public float pickedObjectPullStrength = 20f;
    public float throwForce = 12f;
    // public float colliderEnableDistance = 0.5f; // Distance threshold to re-enable collider

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
    private int originalItemLayer;
    private bool isItemAbovePlayer = false;
    
    [Header("References")]
    public CharacterController2D controller;
    public CapsuleCollider2D collider2D;
    public Animator animator;
    public Rigidbody2D rb;
    [Space]
    public Transform holdAnchor;
    [Space]
    public GameManager gameManager;

    void Awake()
    {
        if (controller == null) controller = GetComponent<CharacterController2D>();
        if (collider2D == null) collider2D = GetComponent<CapsuleCollider2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (gameManager == null) gameManager = FindObjectOfType<GameManager>();
    }
    
    void Update()
    {
        TakeInput();
        if (!gameManager.isPaused) UpdatePickedUpItemPosition();
    }

    void FixedUpdate()
    {
        ProcessMovement();
        
        animator.SetFloat("verticalVelocity", rb.velocity.y);
    }
    
    #region Input

    void TakeInput()
    {
        if (!allowInput) return;
        
        // Move
        horizontalMove = Input.GetAxisRaw("Horizontal") * walkSpeed;
        // crouch = Input.GetKey(KeyCode.LeftControl);
        
        // Jump
        if (allowMovement && Input.GetButtonDown("Jump")) jumpPressed = true;
        jumpHeld = Input.GetButton("Jump");
        
        // Pick up / drop item
        if (Input.GetMouseButtonDown(1) && !gameManager.isPaused)
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

        // Throw item
        if (Input.GetMouseButtonDown(0) && isHoldingItem && !gameManager.isPaused)
        {
            ThrowHeldItem();
        }
    }

    void ProcessMovement()
    {
        if (allowMovement)
        {
            controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jumpPressed, jumpHeld);
        }
        jumpPressed = false; // consume press
        
        if (horizontalMove != 0)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
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
                
                // Store original layer
                originalItemLayer = currentHeldItem.gameObject.layer;
                isItemAbovePlayer = false;
                
                currentHeldItem.PickedUp(gameObject);
                
                // // Disable collider when first picked up
                // if (currentHeldItem.collider != null)
                // {
                //     currentHeldItem.collider.isTrigger = true;
                // }
            }
        }
    }

    void DropHeldItem()
    {
        if (currentHeldItem != null)
        {
            // Restore original layer before dropping
            currentHeldItem.gameObject.layer = originalItemLayer;
            
            currentHeldItem.Dropped(gameObject);
            currentHeldItem = null;
            isHoldingItem = false;
            isItemAbovePlayer = false;
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
            
            // Check if item is above the character and change layer accordingly
            bool itemIsAbove = currentHeldItem.transform.position.y > transform.position.y;
            
            if (itemIsAbove && !isItemAbovePlayer)
            {
                // Item just moved above player - change layer to default
                currentHeldItem.gameObject.layer = LayerMask.NameToLayer("Picked Up Object But Overhead");
                isItemAbovePlayer = true;
            }
            else if (!itemIsAbove && isItemAbovePlayer)
            {
                // Item moved back below player - restore original layer
                currentHeldItem.gameObject.layer = originalItemLayer;
                isItemAbovePlayer = false;
            }
            
            // // Re-enable collider if item is close enough to the hold anchor
            // if (currentHeldItem.collider != null && currentHeldItem.collider.isTrigger && distance <= colliderEnableDistance)
            // {
            //     currentHeldItem.collider.isTrigger = false;
            // }
            
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
        
        // Restore original layer before throwing
        thrown.gameObject.layer = originalItemLayer;
        
        thrown.Dropped(gameObject);
        
        currentHeldItem = null;
        isHoldingItem = false;
        isItemAbovePlayer = false;
        
        Vector3 throwDir = (holdAnchor.position - collider2D.bounds.center).normalized;
        
        var rb2d = thrown.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.velocity *= 0.25f;
            Vector2 force = new Vector2(throwDir.x, throwDir.y) * throwForce;
            rb2d.AddForce(force, ForceMode2D.Impulse);
            return;
        }
        
        thrown.transform.position += throwDir * 0.5f;
    }

    public void Die()
    {
        allowInput = false;
        horizontalMove = 0f;
        rb.velocity = Vector2.zero;
        DropHeldItem();
        
        animator.SetTrigger("dieTrigger");
        animator.SetBool("died", true);

        StartCoroutine(DeadthCoroutine());
    }
    
    IEnumerator DeadthCoroutine()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}