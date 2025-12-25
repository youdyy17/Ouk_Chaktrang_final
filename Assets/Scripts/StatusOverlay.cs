using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StatusOverlay : MonoBehaviour
{
    [Header("Assign UI GameObjects (enable/disable)")]
    public GameObject checkImage;      // shown briefly
    public GameObject checkmateImage;  // persistent until hidden/reset
    public GameObject drawImage;       // persistent until hidden/reset

    [Header("Optional Winner Popup")]
    public GameObject winPanel;        // optional panel for win message

    [Header("Game End Images")]
    public Image gameEndImage;           // Image component to show end game sprites
    public Sprite whiteWinSprite;        // Image for "White Wins!"
    public Sprite blackWinSprite;        // Image for "Black Wins!"
    public Sprite drawSprite;            // Image for "Draw!"
    public Sprite checkmateSprite;       // Image for "Checkmate!"
    public Color gameEndBackgroundColor = new Color(0f, 0f, 0f, 0f);  // Background color (transparent by default)
    public Vector2 gameEndImageSize = new Vector2(300f, 100f);  // Size of the end game image

    [Header("Restart Button")]
    public Button restartButton;       // assign a UI Button in Inspector
    public PieceManager pieceManager;  // reference to reset the game

    [Header("Restart Button Image")]
    public Sprite playAgainSprite;    // Assign the "Play Again" image in Inspector
    public Color buttonBackgroundColor = new Color(0.2f, 0.6f, 0.3f, 1f);  // Background color for transparent image
    public Vector2 buttonSize = new Vector2(200f, 100f);  // Width and Height of the button
    public bool enableButtonPulse = true;
    public float buttonPulseSpeed = 2f;
    [Range(1f, 1.15f)] public float buttonPulseScale = 1.08f;

    [Header("Exit Button")]
    public Button exitButton;          // Button to go back to menu
    public Sprite exitButtonSprite;    // Assign the "Exit" image in Inspector
    public Color exitButtonBackgroundColor = new Color(0.6f, 0.2f, 0.2f, 1f);  // Red background
    public Vector2 exitButtonSize = new Vector2(200f, 100f);  // Size of exit button
    [Range(0f, 50f)] public float exitButtonBorderRadius = 15f;  // Border radius for rounded corners
    public string menuSceneName = "MainMenu";  // Name of the menu scene

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
        
        // Setup exit button - always visible
        if (exitButton != null)
        {
            exitButton.gameObject.SetActive(true);
            exitButton.onClick.AddListener(OnExitClicked);
            ApplyExitButtonStyle();
        }
        
        // Hide game end image initially
        if (gameEndImage != null)
        {
            gameEndImage.gameObject.SetActive(false);
        }

        // Auto-find PieceManager if not assigned
        if (pieceManager == null)
            pieceManager = FindFirstObjectByType<PieceManager>();
    }

    private void ApplyButtonStyle()
    {
        if (restartButton == null) return;

        // Set button size
        RectTransform buttonRect = restartButton.GetComponent<RectTransform>();
        if (buttonRect != null)
        {
            buttonRect.sizeDelta = buttonSize;
        }

        // Hide any text children
        Text buttonText = restartButton.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.gameObject.SetActive(false);
        }
        
        // Set background color on button
        Image buttonImage = restartButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = buttonBackgroundColor;
        }
        
        // Create or find child image for the play again sprite
        Transform childImageTransform = restartButton.transform.Find("PlayAgainImage");
        Image childImage;
        
        if (childImageTransform == null)
        {
            // Create a new child GameObject for the image
            GameObject imageObj = new GameObject("PlayAgainImage");
            imageObj.transform.SetParent(restartButton.transform, false);
            
            // Add RectTransform and stretch to fill button
            RectTransform rectTransform = imageObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = new Vector2(5f, 5f);  // Small padding
            rectTransform.offsetMax = new Vector2(-5f, -5f);
            
            childImage = imageObj.AddComponent<Image>();
        }
        else
        {
            childImage = childImageTransform.GetComponent<Image>();
        }
        
        // Set the sprite on the child image
        if (childImage != null && playAgainSprite != null)
        {
            childImage.sprite = playAgainSprite;
            childImage.color = Color.white;
            childImage.preserveAspect = true;
            childImage.raycastTarget = false;  // Let clicks pass through to button
        }
    }

    private void ApplyExitButtonStyle()
    {
        if (exitButton == null) return;

        // Set button size
        RectTransform rectTransform = exitButton.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = exitButtonSize;
        }

        // Set button background with rounded corners
        Image buttonImage = exitButton.GetComponent<Image>();
        if (buttonImage != null)
        {
            // Generate rounded rectangle sprite
            Sprite roundedSprite = CreateRoundedRectSprite(
                (int)exitButtonSize.x, 
                (int)exitButtonSize.y, 
                (int)exitButtonBorderRadius, 
                exitButtonBackgroundColor
            );
            
            if (roundedSprite != null)
            {
                buttonImage.sprite = roundedSprite;
                buttonImage.color = Color.white;  // Color is baked into texture
                buttonImage.type = Image.Type.Simple;
            }
            else
            {
                buttonImage.color = exitButtonBackgroundColor;
            }
        }

        // Create or find child image for the exit sprite
        Transform childImageTransform = exitButton.transform.Find("ExitImage");
        Image childImage;

        if (childImageTransform == null)
        {
            GameObject imageObj = new GameObject("ExitImage");
            imageObj.transform.SetParent(exitButton.transform, false);

            RectTransform childRect = imageObj.AddComponent<RectTransform>();
            childRect.anchorMin = Vector2.zero;
            childRect.anchorMax = Vector2.one;
            childRect.offsetMin = new Vector2(5f, 5f);
            childRect.offsetMax = new Vector2(-5f, -5f);

            childImage = imageObj.AddComponent<Image>();
        }
        else
        {
            childImage = childImageTransform.GetComponent<Image>();
        }

        if (childImage != null && exitButtonSprite != null)
        {
            childImage.sprite = exitButtonSprite;
            childImage.color = Color.white;
            childImage.preserveAspect = true;
            childImage.raycastTarget = false;
        }
    }

    private void OnExitClicked()
    {
        // Reset time scale in case game was paused
        Time.timeScale = 1f;
        
        // Load the menu scene
        SceneManager.LoadScene(menuSceneName);
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
        Debug.Log("StatusOverlay: ShowCheckmate called");
        HideAll();
        if (checkmateImage != null)
            checkmateImage.SetActive(true);
        
        // Show checkmate image
        ShowGameEndImage(checkmateSprite);
        
        ShowRestartButton();
        HidePauseButton();
    }

    public void ShowWin(Color winnerColor)
    {
        Debug.Log($"StatusOverlay: ShowWin called - Winner: {(winnerColor == Color.white ? "White" : "Black")}");
        HideAll();

        // Prefer dedicated win panel if assigned; fallback to checkmate image.
        if (winPanel != null)
            winPanel.SetActive(true);
        else if (checkmateImage != null)
            checkmateImage.SetActive(true);

        bool isWhiteWin = (winnerColor == Color.white);
        
        // Show the appropriate win image
        Sprite winSprite = isWhiteWin ? whiteWinSprite : blackWinSprite;
        ShowGameEndImage(winSprite);
        
        ShowRestartButton();
        HidePauseButton();
    }

    public void ShowDraw()
    {
        HideAll();
        if (drawImage != null)
            drawImage.SetActive(true);
        
        // Show draw image
        ShowGameEndImage(drawSprite);
        
        ShowRestartButton();
        HidePauseButton();
    }

    private void ShowGameEndImage(Sprite sprite)
    {
        Debug.Log($"StatusOverlay: ShowGameEndImage - gameEndImage: {(gameEndImage != null ? "assigned" : "NULL")}, sprite: {(sprite != null ? sprite.name : "NULL")}");
        
        if (gameEndImage != null && sprite != null)
        {
            gameEndImage.gameObject.SetActive(true);
            gameEndImage.sprite = sprite;
            gameEndImage.color = Color.white;
            gameEndImage.preserveAspect = true;
            Debug.Log("StatusOverlay: GameEndImage activated!");
            
            // Set size
            RectTransform rect = gameEndImage.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = gameEndImageSize;
            }
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
        if (gameEndImage != null) gameEndImage.gameObject.SetActive(false);
        // Exit button stays visible - don't hide it
    }

    private void ShowRestartButton()
    {
        Debug.Log($"StatusOverlay: ShowRestartButton - restartButton: {(restartButton != null ? "assigned" : "NULL")}");
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
            Debug.Log("StatusOverlay: RestartButton activated!");
        }
    }

    private void HidePauseButton()
    {
        if (PauseButtonController.Instance != null)
        {
            PauseButtonController.Instance.HidePauseButton();
        }
    }

    private IEnumerator HideAfter(float seconds, GameObject go)
    {
        yield return new WaitForSeconds(seconds);
        if (go != null)
            go.SetActive(false);
        _tempRoutine = null;
    }

    private Sprite CreateRoundedRectSprite(int width, int height, int radius, Color color)
    {
        // Ensure minimum size
        width = Mathf.Max(width, radius * 2 + 1);
        height = Mathf.Max(height, radius * 2 + 1);
        radius = Mathf.Min(radius, Mathf.Min(width, height) / 2);

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];
        Color transparent = new Color(0, 0, 0, 0);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Check if pixel is inside rounded rectangle
                bool inside = true;

                // Check corners
                if (x < radius && y < radius)
                {
                    // Bottom-left corner
                    inside = (Mathf.Pow(x - radius, 2) + Mathf.Pow(y - radius, 2)) <= Mathf.Pow(radius, 2);
                }
                else if (x >= width - radius && y < radius)
                {
                    // Bottom-right corner
                    inside = (Mathf.Pow(x - (width - radius - 1), 2) + Mathf.Pow(y - radius, 2)) <= Mathf.Pow(radius, 2);
                }
                else if (x < radius && y >= height - radius)
                {
                    // Top-left corner
                    inside = (Mathf.Pow(x - radius, 2) + Mathf.Pow(y - (height - radius - 1), 2)) <= Mathf.Pow(radius, 2);
                }
                else if (x >= width - radius && y >= height - radius)
                {
                    // Top-right corner
                    inside = (Mathf.Pow(x - (width - radius - 1), 2) + Mathf.Pow(y - (height - radius - 1), 2)) <= Mathf.Pow(radius, 2);
                }

                pixels[y * width + x] = inside ? color : transparent;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
    }
}
