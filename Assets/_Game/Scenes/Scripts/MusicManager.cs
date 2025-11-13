using UnityEngine;
using UnityEngine.SceneManagement;

// This object is attached to a GameObject in the initial scene (e.g. SplashScreen)
// and then persists across the scenes to manage menu music playback.
[DefaultExecutionOrder(-100)]
public class MusicManager : MonoBehaviour
{
    // AI: Singleton instance so only one survives across scene loads.
    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioClip _menuMusic;
    [SerializeField] private float _menuVolume = 0.6f;
    [SerializeField] private float _fadeDuration = 0.75f;
    [SerializeField] private bool _autoResumeOnMenus = true;

    // AI: Backing fields (private variables use leading underscore per your rule).
    private AudioSource _source;
    private Coroutine _fadeRoutine;

    private void Awake()
    {
        // AI: Basic singleton guard.
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        // AI: Ensure there is an AudioSource.
        _source = GetComponent<AudioSource>();
        if (_source == null)
        {
            _source = this.gameObject.AddComponent<AudioSource>();
        }

        // AI: Configure default 2D music behavior.
        _source.playOnAwake = false;
        _source.loop = true;
        _source.spatialBlend = 0f;
        _source.volume = 0f; // AI: Start at 0 and fade in.
    }

    private void OnEnable()
    {
        // AI: Listen for scene changes to decide when to start/stop music.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // AI: If we started in a menu scene, kick off the music.
        string current = SceneManager.GetActiveScene().name;
        bool isGameplay = IsGameplayScene(current);

        if (!isGameplay)
        {
            StartMenuMusic();
        }
        else
        {
            StopImmediately();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool isGameplay = IsGameplayScene(scene.name);

        if (isGameplay)
        {
            FadeOutAndStop();
        }
        else
        {
            if (_autoResumeOnMenus)
            {
                if (_menuMusic != null)
                {
                    if (!_source.isPlaying || _source.clip != _menuMusic)
                    {
                        StartMenuMusic();
                    }
                }
            }
        }
    }

    private bool IsGameplayScene(string sceneName)
    {
        // AI: If you later add more gameplay levels, extend this check.
        if (sceneName == "Game")
        {
            return true;
        }

        return false;
    }

    public void StartMenuMusic()
    {
        if (_menuMusic == null)
        {
            return;
        }

        if (_source.clip != _menuMusic)
        {
            _source.clip = _menuMusic;
        }

        if (!_source.isPlaying)
        {
            _source.volume = 0f;
            _source.Play();
        }

        FadeTo(_menuVolume, _fadeDuration);
    }

    public void FadeOutAndStop()
    {
        FadeTo(0f, _fadeDuration, stopAtEnd: true);
    }

    public void StopImmediately()
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
            _fadeRoutine = null;
        }

        if (_source.isPlaying)
        {
            _source.Stop();
        }
    }

    private void FadeTo(float target, float duration, bool stopAtEnd = false)
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
        }

        _fadeRoutine = StartCoroutine(FadeRoutine(target, duration, stopAtEnd));
    }

    private System.Collections.IEnumerator FadeRoutine(float target, float duration, bool stopAtEnd)
    {
        // AI: Simple linear fade; good enough for menu music.
        float start = _source.volume;
        float time = 0f;

        if (duration <= 0f)
        {
            _source.volume = target;
            if (stopAtEnd)
            {
                _source.Stop();
            }
            yield break;
        }

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / duration);
            _source.volume = Mathf.Lerp(start, target, t);
            yield return null;
        }

        _source.volume = target;

        if (stopAtEnd)
        {
            _source.Stop();
        }

        _fadeRoutine = null;
    }
}
