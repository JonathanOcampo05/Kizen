using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.SceneManagement;

// Punto de entrada: pon este script en un GameObject vacío y dale Play.
// Construye fondo, jugador, spawner y UI por código.
public class KiZenGame : MonoBehaviour
{
    public static KiZenGame Instance { get; private set; }

    public int lives = 3;
    public float spawnInterval = 1.5f;

    public int Score { get; private set; }
    public bool GameOver { get; private set; }

    float nextSpawn;
    Sprite spiritSprite;
    GUIStyle style;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            cam = camGo.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.transform.position = new Vector3(0, 0, -10);
        cam.backgroundColor = new Color(0.93f, 0.9f, 0.82f); // arena zen
        cam.clearFlags = CameraClearFlags.SolidColor;

        // Línea de arena / jardín
        var ground = new GameObject("Garden");
        ground.transform.position = new Vector3(0, -4.6f, 0);
        ground.transform.localScale = new Vector3(20, 1.6f, 1);
        var gsr = ground.AddComponent<SpriteRenderer>();
        gsr.sprite = Sprites.Square(new Color(0.75f, 0.72f, 0.6f));
        gsr.sortingOrder = -1;

        // Jugador (monje)
        var player = new GameObject("Player");
        player.transform.position = new Vector3(0, -3.6f, 0);
        player.transform.localScale = new Vector3(0.9f, 1.3f, 1f);
        var psr = player.AddComponent<SpriteRenderer>();
        psr.sprite = Sprites.Square(new Color(0.9f, 0.45f, 0.1f));
        psr.sortingOrder = 1;
        player.AddComponent<PlayerController>();

        spiritSprite = Sprites.Circle(new Color(0.15f, 0.1f, 0.25f));
    }

    void Update()
    {
        if (GameOver)
        {
            bool restart;
#if ENABLE_INPUT_SYSTEM
            restart = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
            restart = Input.GetKeyDown(KeyCode.R);
#endif
            if (restart) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        if (Time.time >= nextSpawn)
        {
            // Se vuelve más rápido con el puntaje
            nextSpawn = Time.time + Mathf.Max(0.5f, spawnInterval - Score * 0.005f);
            SpawnSpirit();
        }
    }

    void SpawnSpirit()
    {
        var go = new GameObject("Spirit");
        go.transform.position = new Vector3(Random.Range(-8f, 8f), 6f, 0);
        go.transform.localScale = Vector3.one * 0.9f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = spiritSprite;
        sr.sortingOrder = 1;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        go.AddComponent<CircleCollider2D>();
        var s = go.AddComponent<Spirit>();
        s.speed = 2f + Score * 0.01f;
    }

    public void AddScore(int amount) => Score += amount;

    public void LoseLife()
    {
        lives--;
        if (lives <= 0) GameOver = true;
    }

    void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold };
            style.normal.textColor = new Color(0.2f, 0.15f, 0.1f);
        }
        GUI.Label(new Rect(20, 10, 400, 40), $"Puntaje: {Score}", style);
        GUI.Label(new Rect(20, 50, 400, 40), $"Vidas: {Mathf.Max(0, lives)}", style);
        if (GameOver)
        {
            var big = new GUIStyle(style) { fontSize = 48, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(0, Screen.height / 2f - 60, Screen.width, 60), "Game Over", big);
            GUI.Label(new Rect(0, Screen.height / 2f, Screen.width, 50), "Presiona R para reiniciar", style.fontSize > 0 ? new GUIStyle(style) { alignment = TextAnchor.MiddleCenter } : style);
        }
    }
}
