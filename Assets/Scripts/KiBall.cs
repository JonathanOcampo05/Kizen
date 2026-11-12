using System.Collections.Generic;
using UnityEngine;

// Esfera de ki disparada por el monje. Avanza hacia arriba con estela luminosa.
public class KiBall : MonoBehaviour
{
    float speed, radius, spin;
    int damage;
    bool pierce;
    Transform glow;
    readonly HashSet<Spirit> alreadyHit = new HashSet<Spirit>();

    public static KiBall Spawn(Vector3 pos, bool power)
    {
        var go = new GameObject(power ? "KiBlast" : "KiBall");
        go.transform.position = pos;
        var kb = go.AddComponent<KiBall>();
        kb.Init(power);
        return kb;
    }

    void Init(bool power)
    {
        speed = power ? 9f : 12.5f;
        damage = power ? 4 : 1;
        pierce = power;
        radius = power ? 0.6f : 0.22f;
        float size = power ? 1.15f : 0.45f;
        Color tint = power ? new Color(0.65f, 0.95f, 1f) : new Color(1f, 0.9f, 0.6f);

        glow = ZenGarden.Make("Glow", Art.Glow, Vector3.zero, Vector3.one * size * 2.6f, 7, transform,
            new Color(tint.r, tint.g, tint.b, 0.6f), Art.Additive).transform;
        ZenGarden.Make("Core", Art.KiOrb, Vector3.zero, Vector3.one * size, 8, transform, tint, Art.Additive);

        var trail = gameObject.AddComponent<TrailRenderer>();
        trail.sharedMaterial = Art.Additive;
        trail.time = power ? 0.35f : 0.16f;
        trail.minVertexDistance = 0.04f;
        trail.widthCurve = AnimationCurve.Linear(0, size * 0.55f, 1, 0);
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(tint, 0), new GradientColorKey(tint, 1) },
                  new[] { new GradientAlphaKey(0.8f, 0), new GradientAlphaKey(0, 1) });
        trail.colorGradient = g;
        trail.sortingOrder = 6;
    }

    void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;
        spin += Time.deltaTime;
        glow.localRotation = Quaternion.Euler(0, 0, spin * 360f);
        if (transform.position.y > 6.5f) { Destroy(gameObject); return; }

        var all = Spirit.All;
        for (int i = all.Count - 1; i >= 0; i--)
        {
            if (i >= all.Count) continue;
            var s = all[i];
            if (s == null || alreadyHit.Contains(s)) continue;
            if (Vector2.Distance(s.transform.position, transform.position) > radius + s.Radius) continue;
            s.Hit(damage);
            if (!pierce) { Destroy(gameObject); return; }
            alreadyHit.Add(s);
        }
    }
}
