using UnityEngine;

public class LeverTriggerBox : MonoBehaviour
{
    [Header("Parameters")]
    public bool isLeftTriggerBox = true;
    
    [Header("References")]
    public LeverAnimatorController leverAnimatorController;
    
    void Awake()
    {
        if (leverAnimatorController == null) leverAnimatorController = GetComponentInParent<LeverAnimatorController>();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (isLeftTriggerBox)
            {
                leverAnimatorController.leverAnimator.SetTrigger("Left");
            }
            else
            {
                leverAnimatorController.leverAnimator.SetTrigger("Right");
            }
        }
    }
}
