using UnityEngine;

public class GameSoundManager : MonoBehaviour
{
    public static GameSoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource sfxSource;

    [Header("Sound Effects")]
    public AudioClip moveSound;
    public AudioClip captureSound;
    public AudioClip checkSound;
    public AudioClip checkmateSound;
    public AudioClip drawSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float moveVolume = 0.7f;
    [Range(0f, 1f)] public float captureVolume = 0.8f;
    [Range(0f, 1f)] public float checkVolume = 1f;
    [Range(0f, 1f)] public float checkmateVolume = 1f;
    [Range(0f, 1f)] public float drawVolume = 0.9f;

    // Track pending move sound (will be cancelled if check/checkmate occurs)
    private bool _hasPendingSound = false;
    private bool _pendingIsCapture = false;

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

        // Create audio source if not assigned
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
    }

    public void PlayMoveSound()
    {
        if (moveSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(moveSound, moveVolume);
        }
    }

    public void PlayCaptureSound()
    {
        if (captureSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(captureSound, captureVolume);
        }
        else
        {
            // Fallback to move sound if no capture sound
            PlayMoveSound();
        }
    }

    public void PlayCheckSound()
    {
        if (checkSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(checkSound, checkVolume);
        }
    }

    public void PlayCheckmateSound()
    {
        if (checkmateSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(checkmateSound, checkmateVolume);
        }
        else
        {
            // Fallback to check sound if no checkmate sound
            PlayCheckSound();
        }
    }

    public void PlayDrawSound()
    {
        if (drawSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(drawSound, drawVolume);
        }
        ClearPendingSound();
    }

    // Called by BasePiece.Move() to queue a move/capture sound
    public void SetPendingMoveSound(bool isCapture)
    {
        _hasPendingSound = true;
        _pendingIsCapture = isCapture;
    }

    // Called by PieceManager after evaluating board state
    // If no check/checkmate, play the pending move sound
    public void PlayPendingMoveSoundIfNoCheck()
    {
        if (_hasPendingSound)
        {
            if (_pendingIsCapture)
                PlayCaptureSound();
            else
                PlayMoveSound();
        }
        ClearPendingSound();
    }

    // Clear the pending sound (called when check/checkmate overrides it)
    public void ClearPendingSound()
    {
        _hasPendingSound = false;
        _pendingIsCapture = false;
    }
}
