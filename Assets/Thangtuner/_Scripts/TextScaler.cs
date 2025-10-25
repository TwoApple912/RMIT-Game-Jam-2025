using TMPro;
using UnityEngine;
using System.Collections;

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

    [Header("Fixed Preferred Size")]
    [SerializeField] private bool useFixedWidth = false;
    [SerializeField, Tooltip("Used only if 'Use Fixed Width' is enabled")]
    private float fixedPreferredWidth = 200f;

    [SerializeField] private bool useFixedHeight = false;
    [SerializeField, Tooltip("Used only if 'Use Fixed Height' is enabled")]
    private float fixedPreferredHeight = 50f;

    [Header("Height Step Delay")]
    [SerializeField, Tooltip("Delay inserted after each ~stepSize increase in height.")]
    private float stepDelay = 0.25f;

    [SerializeField, Tooltip("Size of each height chunk that requires a delay.")]
    private float stepSize = 40f;

    private Vector2 targetSize;          // full desired size (width + height with padding)
    private float allowedHeight;         // the current cap the height is allowed to lerp up to
    private Coroutine stepRoutine;

    private void Awake()
    {
        if (!text) text = GetComponent<TMP_Text>();
        if (!rectTransform && text) rectTransform = text.GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (rectTransform != null)
            allowedHeight = rectTransform.sizeDelta.y;
    }

    private void Update()
    {
        if (!text || !rectTransform) return;

        // --- compute preferred size ---
        float preferredWidth = useFixedWidth ? fixedPreferredWidth : text.preferredWidth;
        float preferredHeight = useFixedHeight ? fixedPreferredHeight : text.preferredHeight;

        targetSize = new Vector2(
            preferredWidth + horizontalPadding,
            preferredHeight + verticalPadding
        );

        // --- step logic (height only) ---
        float currentHeight = rectTransform.sizeDelta.y;

        if (targetSize.y < allowedHeight)
        {
            // shrinking: cancel step routine and allow immediate downsize
            if (stepRoutine != null)
            {
                StopCoroutine(stepRoutine);
                stepRoutine = null;
            }
            allowedHeight = targetSize.y;
        }
        else if (targetSize.y > allowedHeight)
        {
            // growing: start (or keep) a step routine to advance allowedHeight in chunks
            if (stepRoutine == null)
                stepRoutine = StartCoroutine(AdvanceHeightInSteps());
        }

        // --- apply lerp ---
        float targetHeightForThisFrame = Mathf.Min(targetSize.y, allowedHeight);
        Vector2 desired = new Vector2(targetSize.x, targetHeightForThisFrame);

        rectTransform.sizeDelta = Vector2.Lerp(
            rectTransform.sizeDelta,
            desired,
            Time.deltaTime * Mathf.Max(lerpSpeed, 0.0001f)
        );
    }

    private IEnumerator AdvanceHeightInSteps()
    {
        // keep stepping until we’ve granted the full desired height
        while (allowedHeight < targetSize.y)
        {
            // grant the next chunk (up to final target)
            float next = Mathf.Min(allowedHeight + Mathf.Max(stepSize, 0.0001f), targetSize.y);
            allowedHeight = next;

            // wait until we’ve visually reached (or nearly reached) this chunk, then delay
            // (prevents stacking delays if lerp is slow)
            while (rectTransform && rectTransform.sizeDelta.y < allowedHeight - 0.5f)
                yield return null;

            // if we already reached the final target, break (no extra delay)
            if (Mathf.Approximately(allowedHeight, targetSize.y))
                break;

            // insert the pause before granting the next 40-ish units
            float t = 0f;
            while (t < stepDelay)
            {
                // if the target shrank during the delay, stop immediately
                if (targetSize.y <= allowedHeight)
                {
                    stepRoutine = null;
                    yield break;
                }
                t += Time.deltaTime;
                yield return null;
            }
        }

        stepRoutine = null;
    }
}
