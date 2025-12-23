using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusOverlay : MonoBehaviour
{
    [Header("Assign UI GameObjects (enable/disable)")]
    public GameObject checkImage;      // shown briefly
    public GameObject checkmateImage;  // persistent until hidden/reset
    public GameObject drawImage;       // persistent until hidden/reset

    [Header("Optional Winner Popup")]
    public GameObject winPanel;        // optional panel for win message
    public TextMeshProUGUI winText;    // optional text for win message

    [Header("Game End Message")]
    public TextMeshProUGUI gameEndText;           // dedicated text for game end message
    public Color whiteWinTextColor = new Color(1f, 1f, 1f, 1f);
    public Color blackWinTextColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color drawTextColor = new Color(0.9f, 0.8f, 0.2f, 1f);
    public float gameEndFontSize = 48f;

    [Header("Restart Button")]
    public Button restartButton;       // assign a UI Button in Inspector
    public PieceManager pieceManager;  // reference to reset the game

    [Header("Restart Button Style")]
    public Color buttonNormalColor = new Color(0.2f, 0.6f, 0.3f, 1f);
    public Color buttonHoverColor = new Color(0.3f, 0.75f, 0.4f, 1f);
    public Color buttonPressedColor = new Color(0.15f, 0.5f, 0.25f, 1f);
    public Color buttonTextColor = Color.white;
    public float buttonFontSize = 32f;
    public bool enableButtonPulse = true;
    public float buttonPulseSpeed = 2f;
    [Range(1f, 1.15f)] public float buttonPulseScale = 1.08f;

    private Coroutine _tempRoutine;
    private Vector3 _buttonOriginalScale = Vector3.one;

    private void Start()
    {
        // Hide restart button initially and setup click listener
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(false);
            restartButton.onClick.AddListener(OnRestartClicked);
            _buttonOriginalScale = restartButton.transform.localScale;
            ApplyButtonStyle();
        }

        // Auto-find PieceManager if not assigned
        if (pieceManager == null)
            pieceManager = FindObjectOfType<PieceManager>();
    }

    private void ApplyButtonStyle()
    {
        if (restartButton == null) return;

        // Apply color block for button states
        ColorBlock colors = restartButton.colors;
        colors.normalColor = buttonNormalColor;
        colors.highlightedColor = buttonHoverColor;
        colors.pressedColor = buttonPressedColor;
        colors.selectedColor = buttonHoverColor;
        colors.fadeDuration = 0.1f;
        restartButton.colors = colors;

        // Resize button to fit text properly
        RectTransform buttonRect = restartButton.GetComponent<RectTransform>();
        if (buttonRect != null)
        {
            buttonRect.sizeDelta = new Vector2(200f, 60f);
        }

        // Style the button text if present
        TextMeshProUGUI buttonText = restartButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.color = buttonTextColor;
            buttonText.fontSize = buttonFontSize;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.text = "Play Again";
            buttonText.enableWordWrapping = false;
            buttonText.overflowMode = TextOverflowModes.Overflow;
            buttonText.alignment = TextAlignmentOptions.Center;
            
            // Ensure text rect fills the button
            RectTransform textRect = buttonText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
            }
        }

        // Style the button background
        Image buttonImage = restartButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = Color.white; // Use white so ColorBlock tinting works properly
        }
    }

    private void Update()
    {
        // Pulse animation for restart button when visible
        if (enableButtonPulse && restartButton != null && restartButton.gameObject.activeInHierarchy)
        {
            float pulse = 1f + (buttonPulseScale - 1f) * (0.5f + 0.5f * Mathf.Sin(Time.time * buttonPulseSpeed));
            restartButton.transform.localScale = _buttonOriginalScale * pulse;
        }
    }

    private void OnRestartClicked()
    {
        if (pieceManager != null)
        {
            pieceManager.ResetPieces();
            pieceManager.mTimerController?.ResetTimers();
        }
        HideAll();
    }

    public void ShowCheck(float seconds = 1.5f)
    {
        HideAll();
        if (checkImage != null)
            checkImage.SetActive(true);

        if (_tempRoutine != null)
            StopCoroutine(_tempRoutine);
        _tempRoutine = StartCoroutine(HideAfter(seconds, checkImage));
    }

    public void ShowCheckmate()
    {
        HideAll();
        if (checkmateImage != null)
            checkmateImage.SetActive(true);
        ShowGameEndMessage("Checkmate!", whiteWinTextColor);
        ShowRestartButton();
    }

    public void ShowWin(Color winnerColor)
    {
        HideAll();

        // Prefer dedicated win panel if assigned; fallback to checkmate image.
        if (winPanel != null)
            winPanel.SetActive(true);
        else if (checkmateImage != null)
            checkmateImage.SetActive(true);

        bool isWhiteWin = (winnerColor == Color.white);
        string winLine = isWhiteWin ? "White Wins!" : "Black Wins!";
        Color textColor = isWhiteWin ? whiteWinTextColor : blackWinTextColor;

        if (winText != null)
        {
            winText.text = $"Checkmate\n{winLine}";
        }
        
        ShowGameEndMessage(winLine, textColor);
        ShowRestartButton();
    }

    public void ShowDraw()
    {
        HideAll();
        if (drawImage != null)
            drawImage.SetActive(true);
        ShowGameEndMessage("Draw!", drawTextColor);
        ShowRestartButton();
    }

    private void ShowGameEndMessage(string message, Color color)
    {
        if (gameEndText != null)
        {
            gameEndText.gameObject.SetActive(true);
            gameEndText.text = message;
            gameEndText.color = color;
            gameEndText.fontSize = gameEndFontSize;
            gameEndText.fontStyle = FontStyles.Bold;
            gameEndText.alignment = TextAlignmentOptions.Center;
            gameEndText.enableWordWrapping = false;
        }
    }

    public void HideAll()
    {
        if (_tempRoutine != null)
        {
            StopCoroutine(_tempRoutine);
            _tempRoutine = null;
        }

        if (checkImage != null) checkImage.SetActive(false);
        if (checkmateImage != null) checkmateImage.SetActive(false);
        if (drawImage != null) drawImage.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (restartButton != null) restartButton.gameObject.SetActive(false);
        if (gameEndText != null) gameEndText.gameObject.SetActive(false);
    }

    private void ShowRestartButton()
    {
        if (restartButton != null)
            restartButton.gameObject.SetActive(true);
    }

    private IEnumerator HideAfter(float seconds, GameObject go)
    {
        yield return new WaitForSeconds(seconds);
        if (go != null)
            go.SetActive(false);
        _tempRoutine = null;
    }
}
