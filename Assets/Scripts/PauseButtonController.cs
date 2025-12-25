using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseButtonController : MonoBehaviour
{
    public static PauseButtonController Instance { get; private set; }
    
    [Header("References")]
    public TimerController timerController;
    public Button pauseButton;
    
    [Header("Button Text")]
    public string pauseText = "ឈប់";
    public string resumeText = "បន្ត";
    
    [Header("Optional - Pause Panel")]
    public GameObject pausePanel;           // Optional panel to show when paused
    
    private bool _isPaused = false;
    private TextMeshProUGUI _buttonText;
    private Text _buttonTextLegacy;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Auto-find TimerController if not assigned
        if (timerController == null)
        {
            timerController = FindFirstObjectByType<TimerController>();
        }
        
        // Auto-find Button if not assigned
        if (pauseButton == null)
        {
            pauseButton = GetComponent<Button>();
        }
        
        // Get button text component (supports both TMP and legacy Text)
        if (pauseButton != null)
        {
            _buttonText = pauseButton.GetComponentInChildren<TextMeshProUGUI>();
            if (_buttonText == null)
            {
                _buttonTextLegacy = pauseButton.GetComponentInChildren<Text>();
            }
            
            // Add click listener
            pauseButton.onClick.AddListener(TogglePause);
        }
        
        // Hide pause panel initially
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        
        UpdateButtonText();
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;
        
        if (_isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
        
        UpdateButtonText();
    }

    public void PauseGame()
    {
        _isPaused = true;
        
        // Stop the timer
        if (timerController != null)
        {
            timerController.StopTimers();
        }
        
        // Show pause panel if assigned
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        
        // Pause background music if available
        if (BackgroundMusicManager.Instance != null)
        {
            BackgroundMusicManager.Instance.PauseMusic();
        }
        
        UpdateButtonText();
    }

    public void ResumeGame()
    {
        _isPaused = false;
        
        // Resume the timer
        if (timerController != null)
        {
            timerController.ResumeTimers();
        }
        
        // Hide pause panel if assigned
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        
        // Resume background music if available
        if (BackgroundMusicManager.Instance != null)
        {
            BackgroundMusicManager.Instance.ResumeMusic();
        }
        
        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        string text = _isPaused ? resumeText : pauseText;
        
        if (_buttonText != null)
        {
            _buttonText.text = text;
        }
        else if (_buttonTextLegacy != null)
        {
            _buttonTextLegacy.text = text;
        }
    }

    public bool IsPaused()
    {
        return _isPaused;
    }

    public void HidePauseButton()
    {
        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(false);
        }
    }

    public void ShowPauseButton()
    {
        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(TogglePause);
        }
    }
}
