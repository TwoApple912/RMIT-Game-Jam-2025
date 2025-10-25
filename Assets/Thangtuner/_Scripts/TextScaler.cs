using TMPro;
using UnityEngine;

[ExecuteAlways]
public class TextScaler : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private RectTransform rectTransform;
    
    [Header("Padding")]
    [SerializeField] private float horizontalPadding = 20f;
    [SerializeField] private float verticalPadding = 10f;
    
    [Header("Smooth Resize")]
    [SerializeField] private float lerpSpeed = 10f;

    private Vector2 targetSize;

    private void Awake()
    {
        if (!text) text = GetComponent<TMP_Text>();
        if (!rectTransform && text) rectTransform = text.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!text || !rectTransform) return;

        // Calculate TMP preferred size
        float preferredWidth = text.preferredWidth;
        float preferredHeight = text.preferredHeight;

        // Apply padding
        targetSize = new Vector2(
            preferredWidth + horizontalPadding,
            preferredHeight + verticalPadding
        );

        // Smoothly resize RectTransform
        rectTransform.sizeDelta = Vector2.Lerp(
            rectTransform.sizeDelta,
            targetSize,
            Time.deltaTime * lerpSpeed
        );
    }
}