using UnityEngine;

public class LeverAnimatorController : MonoBehaviour
{
    [Header("References")]
    public Animator leverAnimator;
    
    void Awake()
    {
        if (leverAnimator == null) leverAnimator = GetComponent<Animator>();
    }
}
