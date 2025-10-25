using System;
using UnityEngine;

public abstract class PickupObject : MonoBehaviour, IAffectByCustomTime
{
    public float TimeMultiplier { get; set; }
    
    [Header("Parameters")]
    public LayerMask pickUpLayer;

    private LayerMask initialLayer;
    private float initialMass;
    
    [Header("Tracker")]
    public Vector2 lastVelocity;
    public float lastAngularVelocity;
    public bool hasRun1;
    public bool hasRun2;
    
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

    void FixedUpdate()
    {
        if (rb != null)
        {
            if (TimeMultiplier == 0f)
            {
                if (!hasRun1)
                {
                    lastVelocity = rb.velocity;
                    lastAngularVelocity = rb.angularVelocity;
                    
                    rb.velocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    
                    hasRun1 = true;
                    hasRun2 = false;
                }

                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            else if (TimeMultiplier == 1f)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;

                if (!hasRun2 && hasRun1)
                {
                    rb.velocity = lastVelocity;
                    rb.angularVelocity = lastAngularVelocity;
                    
                    hasRun2 = true;
                    hasRun1 = false;
                }
            }
            else
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.velocity *= TimeMultiplier;
                rb.angularVelocity *= TimeMultiplier;
            }
        }
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
