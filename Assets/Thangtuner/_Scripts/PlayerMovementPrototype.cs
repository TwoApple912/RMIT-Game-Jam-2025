using UnityEngine;
    
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovementPrototype : MonoBehaviour
    {
        [Tooltip("Movement speed in units per second")]
        public float moveSpeed = 5f;
    
        Rigidbody2D rb;
        Vector2 input;
    
        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // ensure no gravity for flat 2D movement
            rb.freezeRotation = true;
        }
    
        void Update()
        {
            // Read raw input for snappy controls. Supports WASD and arrow keys.
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
    
            // Normalize so diagonal isn't faster
            if (input.sqrMagnitude > 1f) input.Normalize();
        }
    
        void FixedUpdate()
        {
            // Move using MovePosition for smooth physics-based motion
            Vector2 newPos = rb.position + input * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
    }