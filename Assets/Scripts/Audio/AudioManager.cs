using UnityEngine;

/// <summary>
/// Управление аудио: громкость музыки и эффектов.
/// Сохраняет настройки в PlayerPrefs.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Tooltip("AudioSource на камере для фоновой музыки")]
    public AudioSource musicSource;

    // Ключи PlayerPrefs
    private const string Key_MusicVolume = "MusicVolume";
    private const string Key_MasterVolume = "MasterVolume";

    // Значения по умолчанию (0-1)
    public float musicVolume { get; private set; } = 0.5f;
    public float masterVolume { get; private set; } = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
        ApplyVolumes();
    }

    /// <summary>
    /// Загрузить настройки из PlayerPrefs
    /// </summary>
    public void LoadSettings()
    {
        musicVolume = PlayerPrefs.GetFloat(Key_MusicVolume, 0.5f);
        masterVolume = PlayerPrefs.GetFloat(Key_MasterVolume, 1f);
    }

    /// <summary>
    /// Сохранить настройки в PlayerPrefs
    /// </summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(Key_MusicVolume, musicVolume);
        PlayerPrefs.SetFloat(Key_MasterVolume, masterVolume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Применить громкость к AudioSource музыки
    /// </summary>
    public void ApplyVolumes()
    {
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
    }

    /// <summary>
    /// Установить громкость музыки (0-1)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        SaveSettings();
        ApplyVolumes();
    }

    /// <summary>
    /// Установить общую громкость (0-1)
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        SaveSettings();
        ApplyVolumes();
    }

    /// <summary>
    /// Найти AudioSource музыки на камере автоматически
    /// </summary>
    public void FindMusicSource()
    {
        if (musicSource == null)
        {
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                musicSource = mainCamera.GetComponent<AudioSource>();
            }
        }
    }

    void OnEnable()
    {
        FindMusicSource();
    }
}
