using UnityEngine;
using UnityEngine.SceneManagement;

// Controlador principal de Ki Zen. Pon este script en un GameObject vacío y dale Play:
// construye cámara, escenario, jugador, enemigos y UI por código.
public class KiZenGame : MonoBehaviour
{
    public static KiZenGame Instance { get; private set; }

    /// Piloto automático para pruebas (se activa desde el menú Ki Zen > AutoPilot).
    public static bool AutoPilot
    {
        get
        {
#if UNITY_EDITOR
            return UnityEditor.EditorPrefs.GetBool("KiZen.AutoPilot", false);
#else
            return false;
#endif
        }
    }

    enum State { Title, Playing, GameOver }

    public int startLives = 3;
    public float levelDuration = 20f;

    public int Score { get; private set; }
    public int Lives { get; private set; }
    public int Level { get; private set; }
    public bool GameOver => state == State.GameOver;

    State state;
    float stateTime, levelTimer, nextSpawn;
    int best;
    PlayerController player;
    GUIStyle style;

    void Awake()
    {
        Instance = this;
        Spirit.All.Clear();
        Time.timeScale = 1f;
    }

    void Start()
    {
        SetupCamera();
        ZenGarden.Build();

        var p = new GameObject("Player");
        p.transform.position = new Vector3(0, ZenGarden.GroundY - 0.75f, 0);
        player = p.AddComponent<PlayerController>();

        best = PlayerPrefs.GetInt("KiZen.Best", 0);
        Lives = startLives;
        state = State.Title;
    }

    void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.transform.SetPositionAndRotation(new Vector3(0, 0, -10), Quaternion.identity);
        cam.backgroundColor = new Color(0.09f, 0.08f, 0.2f);
        cam.clearFlags = CameraClearFlags.SolidColor;
    }

    void Update()
    {
        stateTime += Time.deltaTime;
        switch (state)
        {
            case State.Title:
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || (AutoPilot && stateTime > 1f))
                    StartGame();
                break;

            case State.Playing:
                levelTimer += Time.deltaTime;
                if (levelTimer >= levelDuration) { levelTimer = 0f; Level++; }
                if (Time.time >= nextSpawn) SpawnSpirit();
                break;

            case State.GameOver:
                if (stateTime > 1f && (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space) || (AutoPilot && stateTime > 4f)))
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                break;
        }
    }

    void SetState(State s) { state = s; stateTime = 0f; }

    void StartGame()
    {
        Score = 0;
        Lives = startLives;
        Level = 1;
        levelTimer = 0f;
        nextSpawn = Time.time + 1f;
        player.SetControl(true);
        SetState(State.Playing);
    }

    void SpawnSpirit()
    {
        nextSpawn = Time.time + Mathf.Max(0.45f, 1.5f - Level * 0.13f) * Random.Range(0.8f, 1.2f);
        var cam = Camera.main;
        float half = Mathf.Min(cam.orthographicSize * cam.aspect, 11f) - 1f;
        bool oni = Level >= 2 && Random.value < Mathf.Min(0.35f, 0.08f + Level * 0.05f);
        float spd = oni ? 0.9f + Level * 0.08f : 1.5f + Level * 0.17f;
        Spirit.Spawn(new Vector3(Random.Range(-half, half), 6.2f, 0), oni, spd);
    }

    public void OnSpiritKilled(Spirit s)
    {
        if (state != State.Playing) return;
        Score += s.Points;
    }

    public void OnSpiritReached(Spirit s)
    {
        if (state != State.Playing) return;
        Lives--;
        if (Lives <= 0) EndGame();
    }

    void EndGame()
    {
        SetState(State.GameOver);
        player.Defeat();
        for (int i = Spirit.All.Count - 1; i >= 0; i--) Spirit.All[i].Dissolve();
        if (Score > best) { best = Score; PlayerPrefs.SetInt("KiZen.Best", best); }
    }

    // UI provisional (se reemplaza por TextMeshPro más adelante)
    void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold };
            style.normal.textColor = Color.white;
        }
        GUI.Label(new Rect(20, 10, 400, 40), $"Puntaje: {Score}   Vidas: {Mathf.Max(0, Lives)}   Ola: {Level}", style);
        var center = new GUIStyle(style) { alignment = TextAnchor.MiddleCenter };
        if (state == State.Title) GUI.Label(new Rect(0, Screen.height / 2f - 30, Screen.width, 60), "KI ZEN - Presiona ESPACIO", center);
        if (state == State.GameOver) GUI.Label(new Rect(0, Screen.height / 2f - 30, Screen.width, 60), $"Game Over - {Score} pts - R para reiniciar", center);
    }
}
