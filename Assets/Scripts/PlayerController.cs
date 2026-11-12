using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Monje: se mueve con A/D o flechas y dispara ki con Espacio.
public class PlayerController : MonoBehaviour
{
    public float speed = 7f;
    public float cooldown = 0.3f;
    public float limitX = 8f;

    float nextShot;
    Sprite kiSprite;

    void Start() => kiSprite = Sprites.Circle(new Color(1f, 0.9f, 0.3f));

    void Update()
    {
        if (KiZenGame.Instance.GameOver) return;

        float x = 0f;
        bool shoot = false;
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
        shoot = kb.spaceKey.isPressed;
#else
        x = Input.GetAxisRaw("Horizontal");
        shoot = Input.GetKey(KeyCode.Space);
#endif
        var pos = transform.position;
        pos.x = Mathf.Clamp(pos.x + x * speed * Time.deltaTime, -limitX, limitX);
        transform.position = pos;

        if (shoot && Time.time >= nextShot)
        {
            nextShot = Time.time + cooldown;
            Shoot();
        }
    }

    void Shoot()
    {
        var go = new GameObject("KiBall");
        go.transform.position = transform.position + Vector3.up * 0.8f;
        go.transform.localScale = Vector3.one * 0.5f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = kiSprite;
        sr.sortingOrder = 2;
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        go.AddComponent<KiBall>();
    }
}
