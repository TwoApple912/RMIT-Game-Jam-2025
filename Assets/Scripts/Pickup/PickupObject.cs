using System;
using UnityEngine;

public abstract class PickupObject : MonoBehaviour
{
    [Header("Parameters")]
    public LayerMask pickUpLayer;

    private LayerMask initialLayer;
    private float initialMass;
    
    [Header("References")]
    public Collider2D collider;
    public Rigidbody2D rb;

    protected virtual void Awake()
    {
        collider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        
        pickUpLayer = LayerMask.NameToLayer("Picked Up Object");
    }

    protected virtual void Start()
    {
        initialLayer = gameObject.layer;
        initialMass = rb.mass;
    }

    public void PickedUp(GameObject player)
    {
        gameObject.layer = pickUpLayer;
        rb.velocity = Vector2.zero;
        // rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.mass = 0.1f;
    }
    
    public void Dropped(GameObject player)
    {
        gameObject.layer = initialLayer;
        // rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1;
        rb.mass = initialMass;
    }
}
