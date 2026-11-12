using UnityEngine;

// Monje: se mueve con A/D o flechas y dispara ki con Espacio.
public class PlayerController : MonoBehaviour
{
    public float speed = 8.5f;
    public float accel = 45f;
    public float cooldown = 0.2f;

    float vx, nextShot, recoil;
    bool controllable;
    Transform visual;
    SpriteRenderer body;

    public bool Controllable => controllable;
    public void SetControl(bool value) => controllable = value;

    void Start()
    {
        ZenGarden.Make("Shadow", Art.Glow, new Vector3(0, 0.02f, 0), new Vector3(1.8f, 0.38f, 1), 9, transform, new Color(0.1f, 0.05f, 0.1f, 0.45f));
        visual = new GameObject("Visual").transform;
        visual.SetParent(transform, false);
        body = ZenGarden.Make("Monk", Art.Monk, Vector3.zero, Vector3.one, 10, visual);
    }

    float LimitX
    {
        get
        {
            var cam = Camera.main;
            float half = cam ? cam.orthographicSize * cam.aspect : 8.9f;
            return Mathf.Min(half - 0.6f, 11f);
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        float input = 0f;
        bool fire = false;
        if (controllable)
        {
            if (KiZenGame.AutoPilot) AutoPilot(out input, out fire);
            else
            {
                input = Input.GetAxisRaw("Horizontal");
                fire = Input.GetKey(KeyCode.Space);
            }
        }

        vx = Mathf.MoveTowards(vx, input * speed, accel * dt);
        var p = transform.position;
        p.x = Mathf.Clamp(p.x + vx * dt, -LimitX, LimitX);
        if (Mathf.Abs(p.x) >= LimitX) vx = 0f;
        transform.position = p;

        // AnimaciÃ³n: respiraciÃ³n en reposo, paso al caminar, inclinaciÃ³n y retroceso al disparar
        bool moving = Mathf.Abs(vx) > 0.2f;
        float bob = moving ? Mathf.Abs(Mathf.Sin(Time.time * 11f)) * 0.06f : Mathf.Sin(Time.time * 2.2f) * 0.02f;
        recoil = Mathf.MoveTowards(recoil, 0f, dt * 5f);
        visual.localPosition = new Vector3(0, bob, 0);
        visual.localRotation = Quaternion.Euler(0, 0, -vx * 1.3f);
        visual.localScale = new Vector3(1f + recoil * 0.12f, 1f - recoil * 0.12f + (moving ? 0 : Mathf.Sin(Time.time * 2.2f) * 0.01f), 1f) * 1.3f;

        if (fire && Time.time >= nextShot) Shoot();
    }

    void Shoot()
    {
        nextShot = Time.time + cooldown;
        recoil = 1f;
        var firePos = transform.position + new Vector3(0, 0.8f, 0);
        KiBall.Spawn(firePos, false);
        Fx.Muzzle(firePos, false);
        Sfx.PlayShoot();
    }

    /// Defeat: el monje cae de rodillas (se aplana y oscurece).
    public void Defeat()
    {
        controllable = false;
        if (body) body.color = new Color(0.55f, 0.5f, 0.6f, 1f);
        if (visual) visual.localScale = new Vector3(1.4f, 1.05f, 1f);
    }

    // Piloto automÃ¡tico (solo para pruebas desde el editor/MCP): sigue al espÃ­ritu mÃ¡s bajo y dispara.
    void AutoPilot(out float input, out bool fire)
    {
        Spirit target = null;
        foreach (var s in Spirit.All)
            if (s && (target == null || s.transform.position.y < target.transform.position.y)) target = s;
        input = 0f; fire = false;
        if (target == null) return;
        float dx = target.transform.position.x - transform.position.x;
        input = Mathf.Abs(dx) > 0.25f ? Mathf.Sign(dx) : 0f;
        fire = Mathf.Abs(dx) < 0.6f;
    }
}

