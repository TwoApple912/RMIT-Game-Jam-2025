using UnityEngine;
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
    // CanvasGroup representing the pause screen UI (e.g., a panel with your pause menu)
    public CanvasGroup pauseScreen;

    private Coroutine _uiFadeCoroutine;

    private void Awake()
    {
        if (pauseScreen == null) pauseScreen = GameObject.Find("Canvas/Pause Screen")?.GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        UpdateNextSceneNameFromCurrentScene();
        TimeManagerSetup();
        InitializePauseUIState();
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

    #region Pause, Restart & Exit

    public void Pause(bool pause = true)
    {
        // Update UI and time when pausing/unpausing
        if (pause)
        {
            ShowPauseUI(true);
            SetCustomTime(0f);
        }
        else
        {
            ShowPauseUI(false);
            SetCustomTime(1f);
        }
    }

    // Convenience for UI buttons
    public void Resume()
    {
        isPaused = false;
        Pause(false);
    }

    // Ensure initial visibility of the pause UI matches the isPaused flag
    private void InitializePauseUIState()
    {
        if (pauseScreen == null) return;

        // If starting paused, show immediately without fade; otherwise hide and deactivate
        if (isPaused)
        {
            SetCanvasGroupImmediate(pauseScreen, 1f, interactable: true, blocksRaycasts: true);
            if (!pauseScreen.gameObject.activeSelf) pauseScreen.gameObject.SetActive(true);
        }
        else
        {
            SetCanvasGroupImmediate(pauseScreen, 0f, interactable: false, blocksRaycasts: false);
            if (pauseScreen.gameObject.activeSelf) pauseScreen.gameObject.SetActive(false);
        }
    }

    // Public entry to show/hide the pause UI with fade
    public void ShowPauseUI(bool show)
    {
        if (pauseScreen == null)
            return;

        // Stop any existing fade
        if (_uiFadeCoroutine != null)
        {
            StopCoroutine(_uiFadeCoroutine);
            _uiFadeCoroutine = null;
        }

        // Ensure active before fade in
        if (show && !pauseScreen.gameObject.activeSelf)
        {
            pauseScreen.gameObject.SetActive(true);
        }

        // Set interactability up front when showing; defer disabling until fade-out completes when hiding
        if (show)
        {
            pauseScreen.interactable = true;
            pauseScreen.blocksRaycasts = true;
        }

        _uiFadeCoroutine = StartCoroutine(FadeCanvasGroupCoroutine(pauseScreen, show ? 1f : 0f, gradientFadeDuration, disableOnComplete: !show));
    }

    private void SetCanvasGroupImmediate(CanvasGroup cg, float alpha, bool interactable, bool blocksRaycasts)
    {
        cg.alpha = alpha;
        cg.interactable = interactable;
        cg.blocksRaycasts = blocksRaycasts;
    }

    private IEnumerator FadeCanvasGroupCoroutine(CanvasGroup cg, float targetAlpha, float duration, bool disableOnComplete)
    {
        if (cg == null)
            yield break;

        float startAlpha = cg.alpha;
        float elapsed = 0f;
        duration = Mathf.Max(0f, duration);

        if (duration <= 0f)
        {
            cg.alpha = targetAlpha;
        }
        else
        {
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime; // unscaled to work while paused
                float t = Mathf.Clamp01(elapsed / duration);
                cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }
            cg.alpha = targetAlpha;
        }

        if (Mathf.Approximately(targetAlpha, 0f))
        {
            // After hiding, disable interaction and optionally the GameObject
            cg.interactable = false;
            cg.blocksRaycasts = false;
            if (disableOnComplete)
            {
                cg.gameObject.SetActive(false);
            }
        }
        else
        {
            // After showing, ensure it's interactable
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }

        _uiFadeCoroutine = null;
    }

    public void RestartLevel()
    {
        if (SceneManager.GetActiveScene() != null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
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
