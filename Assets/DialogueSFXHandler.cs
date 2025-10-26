using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Yarn.Unity;
using TMPro;

public class DialogueSFXHandler : MonoBehaviour
{
    [Header("FMOD Voice Variations")]
    [Tooltip("List of FMOD one-shots to randomly pick from while typewriter runs.")]
    public EventReference[] typewriterBlips;

    [Header("Behaviour")]
    [Tooltip("Number of characters between each blip. 1 = every char, 2 = every 2 chars, etc.")]
    public int blipInterval = 2;
    [Tooltip("Should stop all sounds when the line ends?")]
    public bool stopOnLineEnd = true;

    private TMP_Text _lineText;
    private CustomLinePresenter _presenter;
    private int _lastVisibleCount = 0;
    private bool _isTyping = false;

    private EventInstance _currentEvent;

    private void Awake()
    {
        _presenter = FindObjectOfType<CustomLinePresenter>();
        if (_presenter != null)
            _lineText = _presenter.lineText;
    }

    private void OnEnable()
    {
        CustomLinePresenter.OnLineDisplayed += OnLineStarted;
        CustomLinePresenter.OnLineEnded += OnLineEnded;
    }

    private void OnDisable()
    {
        CustomLinePresenter.OnLineDisplayed -= OnLineStarted;
        CustomLinePresenter.OnLineEnded -= OnLineEnded;
        StopCurrentEvent();
    }

    private void Update()
    {
        if (!_isTyping || _lineText == null) return;

        // Compare visible characters count
        int visibleNow = _lineText.maxVisibleCharacters;
        if (visibleNow > _lastVisibleCount)
        {
            int diff = visibleNow - _lastVisibleCount;
            _lastVisibleCount = visibleNow;

            // Play sound only every 'blipInterval' characters
            if (visibleNow % blipInterval == 0)
            {
                PlayRandomBlip();
            }
        }
    }

    private void OnLineStarted()
    {
        _isTyping = true;
        _lastVisibleCount = 0;
    }

    private void OnLineEnded()
    {
        _isTyping = false;
        if (stopOnLineEnd)
            StopCurrentEvent();
    }

    private void PlayRandomBlip()
    {
        if (typewriterBlips == null || typewriterBlips.Length == 0)
            return;

        // Randomly pick a blip sound
        int index = Random.Range(0, typewriterBlips.Length);
        var chosenEvent = typewriterBlips[index];

        if (!chosenEvent.IsNull)
        {
            RuntimeManager.PlayOneShot(chosenEvent, transform.position);
        }
    }

    private void StopCurrentEvent()
    {
        if (_currentEvent.isValid())
        {
            _currentEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _currentEvent.release();
        }
    }
}
