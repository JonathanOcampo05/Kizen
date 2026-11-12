using System.Collections.Generic;
using UnityEngine;

// Espíritu oscuro que desciende ondulando hacia el jardín.
// Wisp: 1 golpe. Oni: más grande, lento y resistente (3 golpes).
public class Spirit : MonoBehaviour
{
    public static readonly List<Spirit> All = new List<Spirit>();

    public float Radius { get; private set; }
    public bool IsOni { get; private set; }
    public int Points => IsOni ? 30 : 10;

    int hp;
    float speed, wobbleAmp, wobbleFreq, phase, baseX, age, flash;
    bool dead;
    SpriteRenderer body, aura;
    Transform visual;

    public static Spirit Spawn(Vector3 pos, bool oni, float speed)
    {
        var go = new GameObject(oni ? "Oni" : "Wisp");
        go.transform.position = pos;
        var s = go.AddComponent<Spirit>();
        s.Init(oni, speed);
        return s;
    }

    void Init(bool oni, float spd)
    {
        IsOni = oni;
        hp = oni ? 3 : 1;
        speed = spd;
        Radius = oni ? 0.55f : 0.38f;
        wobbleAmp = oni ? 0.35f : Random.Range(0.4f, 0.9f);
        wobbleFreq = oni ? 0.6f : Random.Range(0.8f, 1.5f);
        phase = Random.value * Mathf.PI * 2f;
        baseX = transform.position.x;

        visual = new GameObject("Visual").transform;
        visual.SetParent(transform, false);
        float scale = oni ? 1.25f : 0.9f;
        visual.localScale = Vector3.one * scale;

        aura = ZenGarden.Make("Aura", Art.Glow, Vector3.zero, Vector3.one * 2.4f, 4, visual,
            oni ? new Color(1f, 0.25f, 0.15f, 0.55f) : new Color(0.65f, 0.3f, 1f, 0.55f), Art.Additive);
        body = ZenGarden.Make("Body", Art.SpiritSprite(oni), Vector3.zero, Vector3.one, 5, visual);
        body.color = new Color(1, 1, 1, 0);
    }

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    void Update()
    {
        if (dead) return;
        age += Time.deltaTime;
        var p = transform.position;
        p.y -= speed * Time.deltaTime;
        p.x = baseX + Mathf.Sin(age * wobbleFreq * Mathf.PI * 2f * 0.5f + phase) * wobbleAmp;
        transform.position = p;

        // Animación: aparición, respiración del aura, inclinación según el movimiento y destello al recibir daño
        float fade = Mathf.Clamp01(age / 0.4f);
        flash = Mathf.Max(0f, flash - Time.deltaTime * 6f);
        body.color = Color.Lerp(new Color(1, 1, 1, fade), new Color(3f, 3f, 3f, fade), flash);
        float breathe = 1f + Mathf.Sin(age * 5f + phase) * 0.08f;
        aura.transform.localScale = Vector3.one * 2.4f * breathe;
        visual.localScale = new Vector3(1f / breathe, breathe, 1f) * (IsOni ? 1.25f : 0.9f);
        visual.localRotation = Quaternion.Euler(0, 0, Mathf.Cos(age * wobbleFreq * Mathf.PI + phase) * 10f);

        if (p.y < ZenGarden.GroundY + 0.15f) Corrupt();
    }

    /// Recibe daño de una esfera de ki.
    public void Hit(int damage)
    {
        if (dead) return;
        hp -= damage;
        flash = 1f;
        if (hp <= 0) { Die(); return; }
        transform.position += Vector3.up * 0.18f;
    }

    void Die()
    {
        dead = true;
        if (KiZenGame.Instance) KiZenGame.Instance.OnSpiritKilled(this);
        Destroy(gameObject);
    }

    /// El espíritu llegó al jardín y lo corrompe: el jugador pierde una vida.
    void Corrupt()
    {
        dead = true;
        if (KiZenGame.Instance) KiZenGame.Instance.OnSpiritReached(this);
        Destroy(gameObject);
    }

    /// Se disuelve sin dar puntos (al terminar la partida).
    public void Dissolve()
    {
        dead = true;
        Destroy(gameObject);
    }
}
