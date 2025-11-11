using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class PauseMenuController : MonoBehaviour
{
    // make PauseMenuController a singleton for easy access
    public static PauseMenuController Instance { get; private set; }

    // AI: Assign in Inspector (scene name must be in Build Settings)
    [SerializeField] private string _mainMenuSceneName = "MainMenu";

    // AI: Cached UI
    private UIDocument _uiDocument;
    private VisualElement _root;
    private Button _btnMainMenu;
    private Button _btnSettings;
    private Button _btnExit;

    // AI: Pause state and original times
    private bool _isPaused = false;
    private float _cachedTimeScale = 1f;
    private float _cachedFixedDelta = 0.02f;

    private PauseMenuController()
    {
        if (Instance != null)
        {
            throw new System.Exception("PauseMenuController: Multiple instances detected.");
        }

        Instance = this;
    }

    private void Awake()
    {
        // AI: Grab UIDocument and root
        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument != null)
        {
            _root = _uiDocument.rootVisualElement.Q<VisualElement>("Root");
        }

        // AI: Bind buttons
        var btnResume = _uiDocument.rootVisualElement.Q<Button>("btnResume");
        _btnMainMenu = _uiDocument.rootVisualElement.Q<Button>("btnMainMenu");
        _btnSettings = _uiDocument.rootVisualElement.Q<Button>("btnSettings");
        _btnExit = _uiDocument.rootVisualElement.Q<Button>("btnExit");

        if (btnResume != null)
        {
            btnResume.clicked += ResumeGame;
        }

        if (_btnMainMenu != null)
        {
            _btnMainMenu.clicked += () =>
            {
                SceneManager.LoadScene(_mainMenuSceneName);
            };
        }

        if (_btnSettings != null)
        {
            _btnSettings.clicked += OnSettingsClicked;
        }

        if (_btnExit != null)
        {
            _btnExit.clicked += OnExitClicked;
        }

        _root?.RegisterCallback<PointerDownEvent>(evt =>
        {
            Debug.Log($"AI: PointerDown on {_root?.name}");
        });

        _btnMainMenu?.RegisterCallback<ClickEvent>(evt =>
        {
            Debug.Log("AI: ClickEvent on Main Menu");
        });

        // AI: Ensure hidden on start
        SetMenuVisible(false);
    }

    public void PauseGame()
    {
        if (_isPaused)
        {
            return;
        }

        _isPaused = true;

        // AI: Cache times and freeze simulation
        _cachedTimeScale = Time.timeScale;
        _cachedFixedDelta = Time.fixedDeltaTime;

        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;

        // AI: Pause audio globally
        AudioListener.pause = true;

        // AI: Show UI & unlock cursor
        SetMenuVisible(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        if (!_isPaused)
        {
            return;
        }

        InputManager.Instance.SetInputState(InputState.Gameplay);

        _isPaused = false;

        // AI: Restore time and physics step
        Time.timeScale = _cachedTimeScale > 0f ? _cachedTimeScale : 1f;
        Time.fixedDeltaTime = _cachedFixedDelta > 0f ? _cachedFixedDelta : 0.02f;

        // AI: Resume audio
        AudioListener.pause = false;

        // AI: Hide UI & restore cursor (optional: lock if your game uses it)
        SetMenuVisible(false);

        // AI: If your game uses locked cursor during play, re-lock here:
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetMenuVisible(bool visible)
    {
        if (_root != null)
        {
            _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void OnMainMenuClicked()
    {
        // AI: Unpause before switching scenes to avoid persistent zero timescale
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        AudioListener.pause = false;

        if (!string.IsNullOrEmpty(_mainMenuSceneName))
        {
            SceneManager.LoadScene(_mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("AI: Main Menu scene name is not set on PauseMenuController.");
        }
    }

    private void OnSettingsClicked()
    {
        // AI: Placeholder — wire your settings panel or scene later
        Debug.Log("AI: Settings clicked (no-op for now).");
    }

    private void OnExitClicked()
    {
        // AI: Unpause first to clean up state
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        AudioListener.pause = false;

#if UNITY_EDITOR
        // AI: Stop Play Mode in Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // AI: Quit the application in builds
        Application.Quit();
#endif
    }
    
    private void LateUpdate()
    {
        if (_isPaused)
        {
            if (UnityEngine.Cursor.lockState != CursorLockMode.None)
            {
                UnityEngine.Cursor.lockState = CursorLockMode.None;
            }
            // if (UnityEngine.Cursor.visible == false)
            // {
            //     UnityEngine.Cursor.visible = true;
            // }
        }
    }
}
