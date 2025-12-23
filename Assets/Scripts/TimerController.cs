using System.Collections;
using UnityEngine;
using TMPro;

public class TimerController : MonoBehaviour
{
    public TextMeshProUGUI blackTimerText;
    public TextMeshProUGUI whiteTimerText;

    public float blackTime = 300f; // Start with 5 minutes for black
    public float whiteTime = 300f; // Start with 5 minutes for white
    public float timeIncrement = 5f; // Bonus time for each move, e.g., 5 seconds per move

    private bool isBlackTurn = false; // Track whose turn it is
    private bool isTimerRunning = true; // Whether the timer should be counting down

    private void Start()
    {
        // Ensure timers are initialized to starting values
        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            // Update the current player's timer
            if (isBlackTurn)
            {
                blackTime -= Time.deltaTime;
                if (blackTime < 0) blackTime = 0;
            }
            else
            {
                whiteTime -= Time.deltaTime;
                if (whiteTime < 0) whiteTime = 0;
            }

            // Update the display of the timers
            UpdateTimerDisplay();
        }
    }

    private void UpdateTimerDisplay()
    {
        // Format and display time for each player
        blackTimerText.text = FormatTime(blackTime);
        whiteTimerText.text = FormatTime(whiteTime);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format("{0}:{1:D2}", minutes, seconds);
    }

    public void SwitchTurn()
    {
        // Pause the timer and switch turns
        isBlackTurn = !isBlackTurn;

        // Add time increment to the current player’s timer
        if (!isBlackTurn)
        {
            blackTime += timeIncrement;
        }
        else
        {
            whiteTime += timeIncrement;
        }

        // Restart the timer for the next player
        isTimerRunning = true;
    }

    public void StopTimers()
    {
        // Stop the timer if game is paused or finished
        isTimerRunning = false;
    }

    public void ResumeTimers()
    {
        // Resume the timer if game is resumed
        isTimerRunning = true;
    }

    // You can use this method to reset both timers when starting a new game
    public void ResetTimers()
    {
        blackTime = 300f; // Reset to 5 minutes
        whiteTime = 300f; // Reset to 5 minutes
        isBlackTurn = false; // White moves first
        isTimerRunning = true;
        UpdateTimerDisplay();
        
        // Reset the timer UI styler if present
        TimerUIStyler styler = FindObjectOfType<TimerUIStyler>();
        if (styler != null)
            styler.ResetStylerState();
    }
}
