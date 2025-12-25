using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager Instance { get; private set; }

    [Header("Audio Source")]
    public AudioSource musicSource;

    [Header("Background Music")]
    public AudioClip menuMusic;           // Music for MainMenu and Instruction scenes
    public AudioClip gameMusic;           // Optional: Different music for game scenes

    [Header("Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    public bool playOnAwake = true;
    public bool loopMusic = true;
    public float fadeSpeed = 1f;

    [Header("Scene Settings")]
    [Tooltip("Scenes where menu music should play")]
    public string[] menuScenes = { "MainMenu", "Instruction" };

    private bool _isFading = false;
    private float _targetVolume;

    private void Awake()
    {
        // Singleton pattern - persist across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Create audio source if not assigned
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Configure audio source
            musicSource.playOnAwake = false;
            musicSource.loop = loopMusic;
            musicSource.volume = musicVolume;
            
            // Subscribe to scene change events
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Start playing if enabled
            if (playOnAwake && menuMusic != null)
            {
                PlayMenuMusic();
            }
        }
        else if (Instance != this)
        {
            // Destroy duplicate immediately to prevent any interference
            DestroyImmediate(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if current scene is a menu scene
        bool isMenuScene = IsMenuScene(scene.name);
        
        if (isMenuScene)
        {
            // Play menu music in menu scenes
            if (menuMusic != null && (musicSource.clip != menuMusic || !musicSource.isPlaying))
            {
                PlayMenuMusic();
            }
        }
        else
        {
            // In game scenes, either play game music or stop
            if (gameMusic != null)
            {
                PlayGameMusic();
            }
            else
            {
                // Stop music in game scenes if no game music is assigned
                StopMusic();
            }
        }
    }

    private bool IsMenuScene(string sceneName)
    {
        foreach (string menuScene in menuScenes)
        {
            if (sceneName == menuScene)
            {
                return true;
            }
        }
        return false;
    }

    public void PlayMenuMusic()
    {
        if (menuMusic != null && musicSource != null)
        {
            if (musicSource.clip != menuMusic)
            {
                musicSource.clip = menuMusic;
            }
            musicSource.volume = musicVolume;
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
    }

    public void PlayGameMusic()
    {
        if (gameMusic != null && musicSource != null)
        {
            if (musicSource.clip != gameMusic)
            {
                musicSource.clip = gameMusic;
            }
            musicSource.volume = musicVolume;
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.UnPause();
        }
    }

    public void SetVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void FadeOut(float duration = 1f)
    {
        StartCoroutine(FadeVolume(0f, duration));
    }

    public void FadeIn(float duration = 1f)
    {
        StartCoroutine(FadeVolume(musicVolume, duration));
    }

    private System.Collections.IEnumerator FadeVolume(float targetVolume, float duration)
    {
        _isFading = true;
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;
        _isFading = false;

        // Stop music if faded out completely
        if (targetVolume <= 0f)
        {
            StopMusic();
        }
    }
}
