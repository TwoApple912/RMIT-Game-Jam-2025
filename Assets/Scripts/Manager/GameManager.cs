using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Pause")]
    public bool isPaused = false;
    [Space]
    public float timescaleTransitionDuration = 0.1f;
    public float gradientFadeDuration = 0.5f;

    [Header("References")]
    public Image pauseGradient;

    private Coroutine timescaleTransitionCoroutine;

    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            Pause(isPaused);
        }
    }

    #region Pause

    void Pause(bool pause = true)
    {
        // Stop any existing timescale transition
        if (timescaleTransitionCoroutine != null)
        {
            StopCoroutine(timescaleTransitionCoroutine);
        }

        if (pause)
        {
            timescaleTransitionCoroutine = StartCoroutine(TransitionTimescale(0f));
            LerpPauseGradientAlpha(1);
        }
        else
        {
            timescaleTransitionCoroutine = StartCoroutine(TransitionTimescale(1f));
            LerpPauseGradientAlpha(0);
        }
    }

    private IEnumerator TransitionTimescale(float targetTimeScale)
    {
        float startTimeScale = Time.timeScale;
        float elapsedTime = 0f;

        while (elapsedTime < timescaleTransitionDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / timescaleTransitionDuration;
            Time.timeScale = Mathf.Lerp(startTimeScale, targetTimeScale, t);
            
            yield return null;
        }

        // Ensure we reach the exact target timescale
        Time.timeScale = targetTimeScale;
        timescaleTransitionCoroutine = null;
    }

    public void LerpPauseGradientAlpha(float targetAlpha)
    {
        if (pauseGradient != null) StartCoroutine(LerpPauseGradientAlphaCoroutine(targetAlpha));
    }

    private IEnumerator LerpPauseGradientAlphaCoroutine(float targetAlpha)
    {
        if (pauseGradient == null) yield break;

        Color startColor = pauseGradient.color;
        float startAlpha = startColor.a;
        float elapsedTime = 0f;

        while (elapsedTime < gradientFadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaledDeltaTime to work even when paused
            float t = elapsedTime / gradientFadeDuration;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            
            pauseGradient.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
            
            yield return null;
        }

        // Ensure we reach the exact target alpha
        pauseGradient.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
    }

    #endregion
}
