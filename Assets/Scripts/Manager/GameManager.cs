using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public string nextSceneName;
    
    [Header("Pause")]
    public bool isPaused = false;
    [Space]
    public float gradientFadeDuration = 0.5f;
    
    [Header("Time Manager")]
    public List<IAffectByCustomTime> timeAffectedObjects = new List<IAffectByCustomTime>();

    [Header("References")]
    public Image pauseGradient;

    private Coroutine timescaleTransitionCoroutine;

    private void Start()
    {
        UpdateNextSceneNameFromCurrentScene();
        TimeManagerSetup();
    }

    private void Update()
    {
        // Pause
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
            LerpPauseGradientAlpha(1);
            SetCustomTime(0f);
        }
        else
        {
            LerpPauseGradientAlpha(0);
            SetCustomTime(1f);
        }
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

    #region Time Manager

    void TimeManagerSetup()
    {
        // Collect all IAffectByCustomTime instances (including inactive) and avoid duplicates
        timeAffectedObjects.Clear();
        var set = new HashSet<IAffectByCustomTime>();
        foreach (var mb in FindObjectsOfType<MonoBehaviour>(true))
        {
            if (mb is IAffectByCustomTime t && t != null)
                set.Add(t);
        }
        timeAffectedObjects.AddRange(set);
        
        //SetCustomTime(1f);
    }

    public void SetCustomTime(float multiplier)
    {
        foreach (var obj in timeAffectedObjects)
        {
            if (obj != null && obj is MonoBehaviour mb && mb.gameObject.activeInHierarchy)
            {
                obj.TimeMultiplier = multiplier;
            }
        }
    }

    #endregion

    #region Scene Manager
    
    private void UpdateNextSceneNameFromCurrentScene()
    {
        if (nextSceneName != string.Empty) return;
        Debug.Log("Updating next scene name from current scene.");
        
        const string prefix = "Level_";
        string currentName = SceneManager.GetActiveScene().name;

        if (currentName.StartsWith(prefix))
        {
            string indexPart = currentName.Substring(prefix.Length);
            if (int.TryParse(indexPart, out int index))
            {
                nextSceneName = $"{prefix}{index + 1}";
            }
        }
    }

    #endregion
}
