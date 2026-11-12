using UnityEngine;

// Efectos visuales: ráfagas de partículas y ondas expansivas.
public static class Fx
{
    /// Ráfaga de partículas que se destruye sola al terminar.
    public static void Burst(Vector3 pos, Color a, Color b, int count, float speed, float size, float life,
                             bool additive = true, int order = 20, float gravity = 0f)
    {
        var go = new GameObject("Burst");
        go.transform.position = pos;
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.1f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(life * 0.5f, life);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.25f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(size * 0.4f, size);
        main.startColor = new ParticleSystem.MinMaxGradient(a, b);
        main.gravityModifier = gravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;
        main.maxParticles = count + 4;

        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.08f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                  new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0, 1) });
        col.color = g;

        var sz = ps.sizeOverLifetime;
        sz.enabled = true;
        sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0.1f));

        var limit = ps.limitVelocityOverLifetime;
        limit.enabled = true;
        limit.dampen = 0.08f;
        limit.limit = 0.5f;

        var r = go.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = additive ? Art.AdditiveParticle : Art.AlphaParticle;
        r.sortingOrder = order;
        ps.Play();
    }

    /// Onda expansiva circular.
    public static void Ring(Vector3 pos, Color color, float endScale, float time, int order = 19)
    {
        var sr = ZenGarden.Make("Ring", Art.Ring, pos, Vector3.one * 0.2f, order, null, color, Art.Additive);
        sr.gameObject.AddComponent<RingAnim>().Set(endScale, time, color);
    }

    // ---- Combinaciones listas para usar

    public static void Muzzle(Vector3 pos, bool power)
    {
        if (power)
        {
            Burst(pos, new Color(0.7f, 1f, 1f), Color.white, 26, 5f, 0.35f, 0.5f);
            Ring(pos, new Color(0.6f, 0.95f, 1f, 0.9f), 3.5f, 0.45f);
        }
        else Burst(pos, new Color(1f, 0.9f, 0.5f), Color.white, 6, 2.2f, 0.14f, 0.25f);
    }

    public static void SpiritHit(Vector3 pos, bool oni)
    {
        Burst(pos, Color.white, oni ? new Color(1f, 0.5f, 0.3f) : new Color(0.8f, 0.6f, 1f), 8, 3f, 0.16f, 0.3f);
    }

    public static void SpiritDeath(Vector3 pos, bool oni)
    {
        Color main = oni ? new Color(1f, 0.35f, 0.2f) : new Color(0.7f, 0.4f, 1f);
        Burst(pos, main, Color.white, oni ? 45 : 28, oni ? 6f : 4.5f, oni ? 0.32f : 0.24f, 0.7f);
        Burst(pos, new Color(1f, 0.8f, 0.9f), new Color(1f, 0.65f, 0.8f), 10, 2f, 0.18f, 1.2f, false, 21, -0.05f);
        Ring(pos, new Color(main.r, main.g, main.b, 0.8f), oni ? 3.2f : 2.2f, 0.4f);
    }

    public static void Corruption(Vector3 pos)
    {
        Burst(pos, new Color(0.15f, 0.05f, 0.2f, 0.9f), new Color(0.4f, 0.1f, 0.3f, 0.8f), 30, 3.5f, 0.45f, 0.9f, false, 21, -0.1f);
        Ring(pos, new Color(1f, 0.2f, 0.25f, 0.9f), 4f, 0.5f);
    }
}

public class RingAnim : MonoBehaviour
{
    float end, time, t;
    Color color;
    SpriteRenderer sr;

    public void Set(float endScale, float duration, Color c) { end = endScale; time = duration; color = c; sr = GetComponent<SpriteRenderer>(); }

    void Update()
    {
        t += Time.deltaTime / time;
        float e = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
        transform.localScale = Vector3.one * Mathf.Lerp(0.2f, end, e);
        sr.color = new Color(color.r, color.g, color.b, color.a * (1f - t));
        if (t >= 1f) Destroy(gameObject);
    }
}
