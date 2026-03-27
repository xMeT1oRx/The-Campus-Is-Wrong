using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Won, Lost }
    public GameState State { get; private set; } = GameState.Playing;

    [Header("Взаимодействие")]
    public float interactionDistance = 4f;

    private Camera playerCam;
    private AnomalyManager anomalyManager;
    private RoomGenerator roomGenerator;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        anomalyManager = GetComponent<AnomalyManager>();
        roomGenerator  = GetComponent<RoomGenerator>();

        // EventSystem нужен для кнопок победы/поражения и паузы
        if (FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // HUD создаётся автоматически
        if (HUD.Instance == null)
            new GameObject("HUD").AddComponent<HUD>();

        // PauseMenu создаётся автоматически
        if (PauseMenu.Instance == null)
            new GameObject("PauseMenu").AddComponent<PauseMenu>();
    }

    void Start()
    {
        roomGenerator.Generate();
        anomalyManager.SpawnAnomaly();
        playerCam = Camera.main;
    }

    void Update()
    {
        // Escape обрабатывает PauseMenu
        if (State != GameState.Playing) return;
        if (PauseMenu.Instance != null && PauseMenu.Instance.IsPaused) return;

        HandleInteraction();
    }

    void HandleInteraction()
    {
        var ray = new Ray(playerCam.transform.position, playerCam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            var ao = hit.collider.GetComponent<AnomalyObject>();
            if (ao != null)
            {
                HUD.Instance?.ShowPrompt(true);

                if (Input.GetKeyDown(KeyCode.E))
                    ReportAnomaly(ao);
                return;
            }
        }

        HUD.Instance?.ShowPrompt(false);
    }

    void ReportAnomaly(AnomalyObject obj)
    {
        if (anomalyManager.IsAnomaly(obj)) Win();
        else                               Lose();
    }

    void Win()
    {
        State = GameState.Won;
        UnlockCursor();
        HUD.Instance?.ShowWin();
    }

    void Lose()
    {
        State = GameState.Lost;
        UnlockCursor();
        HUD.Instance?.ShowLose();
    }

    static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    public void RestartGame()  => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    public void LoadMainMenu() => SceneManager.LoadScene("MainMenu");
}
