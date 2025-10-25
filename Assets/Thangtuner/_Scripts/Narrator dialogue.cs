using System;
using UnityEngine;
using DG.Tweening;
using Yarn.Unity;

public class UIPopupTween : MonoBehaviour
{
    [Header("Tween Settings")]
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private Ease easeType = Ease.OutBack;
    [SerializeField] private Vector3 shownScale = Vector3.one;
    [SerializeField] private Vector3 hiddenScale = Vector3.zero;

    [Header("Optional")]
    [SerializeField] private bool startHidden = true;

    private RectTransform rectTransform;
    private Tween currentTween;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (startHidden)
            rectTransform.localScale = hiddenScale;
    }

    private void OnEnable()
    {
        YarnGameManager.OnPopDialogueUI += PopIn;
        YarnGameManager.OnStopDialogueUI += PopOut;
    }
    private void OnDisable()
    {
        YarnGameManager.OnPopDialogueUI -= PopIn;
        YarnGameManager.OnStopDialogueUI -= PopOut;
    }

    /// <summary>
    /// Tween to shown scale (pop in).
    /// </summary>
    public void PopIn()
    {
        StartTween(shownScale);
    }

    /// <summary>
    /// Tween to hidden scale (pop out).
    /// </summary>
    public void PopOut()
    {
        StartTween(hiddenScale);
    }

    private void StartTween(Vector3 targetScale)
    {
        // Kill any running tween to avoid overlap
        currentTween?.Kill();

        // Start the tween
        currentTween = rectTransform.DOScale(targetScale, duration)
            .SetEase(easeType);
    }
}