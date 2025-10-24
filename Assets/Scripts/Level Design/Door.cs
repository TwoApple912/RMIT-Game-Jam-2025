using UnityEngine;

public class Door : Receiver
{
    [Header("Parameters")]
    public Vector3 closedPosition;
    public Vector3 closedRotation;
    public Vector3 openPosition;
    public Vector3 openRotation;
    public float moveDuration = 0.25f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 targetPosition;
    private Vector3 targetRotation;
    private float moveProgress = 0f;
    private bool isMoving = false;
    private Vector3 startPosition;
    private Vector3 startRotation;

    [Header("References")] public Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        targetPosition = closedPosition;
        targetRotation = closedRotation;
        rb.position = closedPosition;
        rb.rotation = closedRotation.z;
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            moveProgress += Time.fixedDeltaTime / moveDuration;

            if (moveProgress >= 1f)
            {
                moveProgress = 1f;
                isMoving = false;
            }

            float curveValue = easeCurve.Evaluate(moveProgress);
            Vector3 newPosition = Vector3.Lerp(startPosition, targetPosition, curveValue);
            Vector3 newRotation = Vector3.Lerp(startRotation, targetRotation, curveValue);

            rb.MovePosition(newPosition);
            rb.MoveRotation(newRotation.z);
        }
    }

    public override void Activated()
    {
        base.Activated();
        StartMovement(openPosition, openRotation);
    }

    public override void Deactivated()
    {
        base.Deactivated();
        StartMovement(closedPosition, closedRotation);
    }

    private void StartMovement(Vector3 targetPos, Vector3 targetRot)
    {
        startPosition = rb.position;
        startRotation = new Vector3(0, 0, rb.rotation);
        targetPosition = targetPos;
        targetRotation = targetRot;
        moveProgress = 0f;
        isMoving = true;
    }
}
