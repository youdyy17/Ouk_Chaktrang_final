using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TimerUIStyler : MonoBehaviour
{
    [Header("Links")]
    public TimerController timer;
    public TextMeshProUGUI whiteText;
    public TextMeshProUGUI blackText;
    public Image whitePanel; // e.g., Canvas/TimerPanel/White Timer/Panel
    public Image blackPanel; // e.g., Canvas/TimerPanel/Black Timer/Panel

    [Header("Colors")]
    public Color activePanelColor = new Color(0.24f, 0.24f, 0.28f, 0.70f);
    public Color inactivePanelColor = new Color(0f, 0f, 0f, 0.35f);
    public Color whiteTextColor = Color.white;
    public Color blackTextColor = new Color32(220, 200, 160, 255);
    public Color lowTimeTextColor = new Color32(255, 110, 110, 255);
    [Tooltip("Below this time (seconds), text turns low-time color.")]
    public float lowTimeThreshold = 30f;

    [Header("Warning Style (Last 30 Seconds)")]
    [Tooltip("Enable flashing warning effect when time is low.")]
    public bool enableWarningFlash = true;
    [Tooltip("Panel color when time is critically low.")]
    public Color warningPanelColor = new Color(0.15f, 0.0f, 0.0f, 0.95f);
    [Tooltip("Secondary flash color for warning.")]
    public Color warningFlashColor = new Color(0.4f, 0.0f, 0.0f, 0.95f);
    [Tooltip("Speed of the warning flash effect.")]
    public float warningFlashSpeed = 2.5f;
    [Tooltip("Enable shake effect when time is very low.")]
    public bool enableWarningShake = false;
    [Tooltip("Intensity of the shake effect.")]
    public float shakeIntensity = 1.5f;
    [Tooltip("Speed of the shake effect.")]
    public float shakeSpeed = 15f;
    [Tooltip("Below this time (seconds), shake becomes more intense.")]
    public float criticalTimeThreshold = 10f;
    [Tooltip("Warning text color with high contrast.")]
    public Color warningTextColor = new Color(1f, 0.95f, 0.4f, 1f);

    [Header("Typography")]
    public TMP_FontAsset font;
    public int fontSize = 100;
    public bool bold = true;
    public bool enableOutline = true;
    [Range(0f, 1f)] public float outlineWidth = 0.22f;
    public Color outlineColor = Color.black;

    [Header("Active Pulse")]
    public bool pulseActiveSide = true;
    [Range(1f, 1.5f)] public float pulseScale = 1.2f;
    public float pulseSpeed = 3.5f;

    [Header("Padding")]
    [Tooltip("Uniform extra space inside each timer panel (does not move position).")]
    public float panelPadding = 24f;
    [Tooltip("Uniform inset for the text rect inside each panel so numbers don't touch edges.")]
    public float textPadding = 18f;
    [Tooltip("Apply padding once on Start; font style, color, and values are unchanged.")]
    public bool applyPaddingOnStart = true;

    [Header("Auto Size Panel to Text")]
    [Tooltip("If enabled, resizes each panel to wrap its text using uniform padding and keeps the text position unchanged.")]
    public bool autoSizePanelToText = true;
    [Tooltip("Uniform padding applied around the text when sizing its panel.")]
    public float uniformPadding = 18f;
    [Tooltip("Minimum panel size for readability.")]
    public Vector2 minPanelSize = new Vector2(220f, 90f);

    private float _prevWhite;
    private float _prevBlack;
    private bool _isBlackActive;

    private void Awake()
    {
        if (timer == null)
            timer = FindObjectOfType<TimerController>();
    }

    private void Start()
    {
        if (timer == null) return;
        ResetStylerState();
        ApplyBaseStyle(whiteText, whiteTextColor);
        ApplyBaseStyle(blackText, blackTextColor);
        if (autoSizePanelToText)
            FitPanelsAroundText();
        else if (applyPaddingOnStart)
            ApplyPaddingAndFit();
        ApplyActiveState(_isBlackActive);
    }

    public void ResetStylerState()
    {
        if (timer == null) return;
        _prevWhite = timer.whiteTime;
        _prevBlack = timer.blackTime;
        _isBlackActive = false; // White moves first
        ApplyActiveState(_isBlackActive);
    }

    private void Update()
    {
        if (timer == null) return;

        // Detect which side is actively counting down
        bool blackDec = timer.blackTime < _prevBlack - 0.0001f;
        bool whiteDec = timer.whiteTime < _prevWhite - 0.0001f;
        if (blackDec ^ whiteDec) // exactly one is decreasing
        {
            _isBlackActive = blackDec;
            ApplyActiveState(_isBlackActive);
        }

        _prevWhite = timer.whiteTime;
        _prevBlack = timer.blackTime;

        // Low-time color change - use high contrast warning text color
        if (whiteText != null)
        {
            bool whiteLow = timer.whiteTime <= lowTimeThreshold;
            whiteText.color = whiteLow ? warningTextColor : whiteTextColor;
        }
        if (blackText != null)
        {
            bool blackLow = timer.blackTime <= lowTimeThreshold;
            blackText.color = blackLow ? warningTextColor : blackTextColor;
        }

        // Warning effects for low time
        ApplyWarningEffects();

        // Pulse the active panel/text
        if (pulseActiveSide)
            PulseActive();
    }

    private void ApplyWarningEffects()
    {
        bool whiteLowTime = timer.whiteTime <= lowTimeThreshold && timer.whiteTime > 0;
        bool blackLowTime = timer.blackTime <= lowTimeThreshold && timer.blackTime > 0;

        // Apply warning flash to panels
        if (enableWarningFlash)
        {
            float flashT = (Mathf.Sin(Time.time * warningFlashSpeed) + 1f) * 0.5f;
            
            if (whiteLowTime && whitePanel != null && !_isBlackActive)
            {
                whitePanel.color = Color.Lerp(warningPanelColor, warningFlashColor, flashT);
            }
            if (blackLowTime && blackPanel != null && _isBlackActive)
            {
                blackPanel.color = Color.Lerp(warningPanelColor, warningFlashColor, flashT);
            }
        }

        // Apply shake effect
        if (enableWarningShake)
        {
            float whiteShakeMultiplier = timer.whiteTime <= criticalTimeThreshold ? 2f : 1f;
            float blackShakeMultiplier = timer.blackTime <= criticalTimeThreshold ? 2f : 1f;

            if (whiteLowTime && whiteText != null && !_isBlackActive)
            {
                float shakeX = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity * whiteShakeMultiplier;
                float shakeY = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * shakeIntensity * 0.5f * whiteShakeMultiplier;
                whiteText.rectTransform.anchoredPosition = GetOriginalPosition(whiteText) + new Vector2(shakeX, shakeY);
            }
            else if (whiteText != null)
            {
                whiteText.rectTransform.anchoredPosition = GetOriginalPosition(whiteText);
            }

            if (blackLowTime && blackText != null && _isBlackActive)
            {
                float shakeX = Mathf.Sin(Time.time * shakeSpeed) * shakeIntensity * blackShakeMultiplier;
                float shakeY = Mathf.Cos(Time.time * shakeSpeed * 1.3f) * shakeIntensity * 0.5f * blackShakeMultiplier;
                blackText.rectTransform.anchoredPosition = GetOriginalPosition(blackText) + new Vector2(shakeX, shakeY);
            }
            else if (blackText != null)
            {
                blackText.rectTransform.anchoredPosition = GetOriginalPosition(blackText);
            }
        }
    }

    private Dictionary<TextMeshProUGUI, Vector2> _originalPositions = new Dictionary<TextMeshProUGUI, Vector2>();

    private Vector2 GetOriginalPosition(TextMeshProUGUI text)
    {
        if (!_originalPositions.ContainsKey(text))
        {
            _originalPositions[text] = text.rectTransform.anchoredPosition;
        }
        return _originalPositions[text];
    }

    private void ApplyBaseStyle(TextMeshProUGUI tmp, Color baseColor)
    {
        if (tmp == null) return;
        if (font != null) tmp.font = font;
        tmp.fontSize = fontSize;
        tmp.color = baseColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;

        if (enableOutline)
        {
            var mat = Instantiate(tmp.fontMaterial);
            mat.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
            mat.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
            tmp.fontMaterial = mat;
        }
    }

    private void ApplyActiveState(bool blackActive)
    {
        if (whitePanel != null)
            whitePanel.color = blackActive ? inactivePanelColor : activePanelColor;
        if (blackPanel != null)
            blackPanel.color = blackActive ? activePanelColor : inactivePanelColor;
    }

    private void PulseActive()
    {
        float s = 1f + (pulseScale - 1f) * (0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed));
        if (_isBlackActive)
        {
            if (blackPanel != null) blackPanel.rectTransform.localScale = new Vector3(s, s, 1f);
            if (blackText != null) blackText.rectTransform.localScale = new Vector3(s, s, 1f);
            if (whitePanel != null) whitePanel.rectTransform.localScale = Vector3.one;
            if (whiteText != null) whiteText.rectTransform.localScale = Vector3.one;
        }
        else
        {
            if (whitePanel != null) whitePanel.rectTransform.localScale = new Vector3(s, s, 1f);
            if (whiteText != null) whiteText.rectTransform.localScale = new Vector3(s, s, 1f);
            if (blackPanel != null) blackPanel.rectTransform.localScale = Vector3.one;
            if (blackText != null) blackText.rectTransform.localScale = Vector3.one;
        }
    }

    // Improves spacing: enlarges panel sizeDelta and insets text sizeDelta without moving positions
    private void ApplyPaddingAndFit()
    {
        // Grow panels evenly on all sides (width/height). Anchors/position untouched => stays centered.
        if (whitePanel != null)
        {
            var prt = whitePanel.rectTransform;
            prt.sizeDelta = new Vector2(prt.sizeDelta.x + panelPadding * 2f, prt.sizeDelta.y + panelPadding * 2f);
        }
        if (blackPanel != null)
        {
            var prt = blackPanel.rectTransform;
            prt.sizeDelta = new Vector2(prt.sizeDelta.x + panelPadding * 2f, prt.sizeDelta.y + panelPadding * 2f);
        }

        // Fit text rects inside panels with uniform padding, keep anchors/positions unchanged
        if (whiteText != null && whitePanel != null)
        {
            var panelSize = whitePanel.rectTransform.sizeDelta;
            var rt = whiteText.rectTransform;
            float w = Mathf.Max(10f, panelSize.x - textPadding * 2f);
            float h = Mathf.Max(10f, panelSize.y - textPadding * 2f);
            rt.sizeDelta = new Vector2(w, h);
        }

        if (blackText != null && blackPanel != null)
        {
            var panelSize = blackPanel.rectTransform.sizeDelta;
            var rt = blackText.rectTransform;
            float w = Mathf.Max(10f, panelSize.x - textPadding * 2f);
            float h = Mathf.Max(10f, panelSize.y - textPadding * 2f);
            rt.sizeDelta = new Vector2(w, h);
        }
    }

    // Sizes each background panel to the text's preferred size + padding, without moving text
    private void FitPanelsAroundText()
    {
        if (whitePanel != null && whiteText != null)
            FitOne(whitePanel.rectTransform, whiteText);
        if (blackPanel != null && blackText != null)
            FitOne(blackPanel.rectTransform, blackText);
    }

    private void FitOne(RectTransform panel, TextMeshProUGUI text)
    {
        var tr = text.rectTransform;

        // Only copy anchors/pivot/position if they share the same parent. Otherwise, leave placement as-is.
        bool sameParent = panel.parent == tr.parent;
        if (sameParent)
        {
            panel.anchorMin = tr.anchorMin;
            panel.anchorMax = tr.anchorMax;
            panel.pivot = tr.pivot;
            panel.anchoredPosition = tr.anchoredPosition; // keep same on-screen position for the text
        }

        // Preferred size for current content
        Vector2 pref = text.GetPreferredValues(text.text, 4096f, 4096f);
        float w = Mathf.Max(minPanelSize.x, pref.x + uniformPadding * 2f);
        float h = Mathf.Max(minPanelSize.y, pref.y + uniformPadding * 2f);
        panel.sizeDelta = new Vector2(w, h);

        // Ensure panel renders behind the text if they share a parent
        if (sameParent)
        {
            int textIndex = tr.GetSiblingIndex();
            int panelIndex = panel.GetSiblingIndex();
            if (panelIndex > textIndex)
                panel.SetSiblingIndex(Mathf.Max(0, textIndex - 1));
        }
    }
}
