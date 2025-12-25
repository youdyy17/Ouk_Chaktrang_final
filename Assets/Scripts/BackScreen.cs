using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuLogic : MonoBehaviour
{
    private UIDocument _document;
    private Button _uiButton;

    private void Awake()
    {
        _document = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        var root = _document.rootVisualElement;

        // ---------------------------------------------------------
        // THIS IS THE IMPORTANT CHANGE:
        // We look for the element specifically named "Button"
        // ---------------------------------------------------------
        _uiButton = root.Q<Button>("Button");

        if (_uiButton != null)
        {
            // Connect the function to the click event
            _uiButton.clicked += OnButtonClick;
            
            // Set transform origin to center so scaling happens from the middle
            _uiButton.style.transformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(50), 0);
            
            // Register hover events for pop-up animation
            _uiButton.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            _uiButton.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }
        else
        {
            Debug.LogError("Could not find an element named 'Button'. Please check the spelling in UI Builder!");
        }
    }
    
    private void OnMouseEnter(MouseEnterEvent evt)
    {
        // Pop up on hover (scale up slightly)
        _uiButton.style.scale = new Scale(new Vector2(1.1f, 1.1f));
    }
    
    private void OnMouseLeave(MouseLeaveEvent evt)
    {
        // Return to normal size
        _uiButton.style.scale = new Scale(new Vector2(1f, 1f));
    }

    private void OnButtonClick()
    {
        // This runs when you click
        Debug.Log("Button Clicked! Loading scene...");
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDisable()
    {
        // Always disconnect the event when the object is disabled
        if (_uiButton != null)
        {
            _uiButton.clicked -= OnButtonClick;
            _uiButton.UnregisterCallback<MouseEnterEvent>(OnMouseEnter);
            _uiButton.UnregisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }
    }
}