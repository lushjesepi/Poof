using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple in-game audio manager.
/// - Plays background music throughout the game (persists across scenes)
/// - Plays one-shot SFX for resource pickup, player death, and win
/// </summary>
[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [Range(0f, 1f)]
    [SerializeField] private float backgroundMusicVolume = 0.6f;
    [SerializeField] private bool playMusicOnStart = true;

    [Header("SFX")]
    [SerializeField] private AudioClip resourcePickupSfx;
    [Range(0f, 1f)]
    [SerializeField] private float resourcePickupVolume = 0.9f;

    [SerializeField] private AudioClip playerDeathSfx;
    [Range(0f, 1f)]
    [SerializeField] private float playerDeathVolume = 1f;

    [SerializeField] private AudioClip winSfx;
    [Range(0f, 1f)]
    [SerializeField] private float winVolume = 1f;

    [Header("Debug")]
    [SerializeField] private bool logMissingClips = false;

    private AudioSource _musicSource;
    private AudioSource _sfxSource;
    private bool _hasPlayedWin;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        if (playMusicOnStart)
            PlayBackgroundMusic();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset per-run flags when loading scenes.
        _hasPlayedWin = false;

        // Keep music going even if scenes reload.
        if (playMusicOnStart && backgroundMusic != null && !_musicSource.isPlaying)
            PlayBackgroundMusic();
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic == null)
        {
            if (logMissingClips)
                Debug.LogWarning("AudioManager: Missing backgroundMusic clip.");
            return;
        }

        _musicSource.clip = backgroundMusic;
        _musicSource.volume = backgroundMusicVolume;
        if (!_musicSource.isPlaying)
            _musicSource.Play();
    }

    public void StopBackgroundMusic()
    {
        if (_musicSource.isPlaying)
            _musicSource.Stop();
    }

    public void PlayResourcePickup()
    {
        PlayOneShot(resourcePickupSfx, resourcePickupVolume, "resourcePickupSfx");
    }

    public void PlayPlayerDeath()
    {
        PlayOneShot(playerDeathSfx, playerDeathVolume, "playerDeathSfx");
    }

    public void PlayWin()
    {
        if (_hasPlayedWin)
            return;

        _hasPlayedWin = true;
        PlayOneShot(winSfx, winVolume, "winSfx");
    }

    private void PlayOneShot(AudioClip clip, float volume, string label)
    {
        if (clip == null)
        {
            if (logMissingClips)
                Debug.LogWarning($"AudioManager: Missing {label} clip.");
            return;
        }

        _sfxSource.PlayOneShot(clip, volume);
    }
}

