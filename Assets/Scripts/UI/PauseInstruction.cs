using System.Collections;
using UnityEngine;

public class PauseInstruction : MonoBehaviour
{
    [Header("Parameters")]
    public float appearDelay = 12f;
    public float fadeDuration = 1.2f;
    
    [Header("References")]
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        StartCoroutine(StartWaitTime());
    }

    IEnumerator StartWaitTime()
    {
        yield return new WaitForSeconds(appearDelay);
        
        if (spriteRenderer != null) StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        if (fadeDuration <= 0f)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            Color c = spriteRenderer.color;
            c.a = Mathf.Lerp(0f, 1f, t);
            spriteRenderer.color = c;
            yield return null;
        }

        Color final = spriteRenderer.color;
        final.a = 1f;
        spriteRenderer.color = final;
    }
}
