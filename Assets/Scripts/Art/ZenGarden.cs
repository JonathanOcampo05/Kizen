using UnityEngine;

// Construye el escenario por capas: cielo al atardecer, sol, montaÃ±as con niebla,
// torii, cerezo, arena rastrillada, pÃ©talos cayendo y motas de ki flotando.
public static class ZenGarden
{
    public const float GroundY = -3.3f;

    public static Transform Build()
    {
        var root = new GameObject("ZenGarden").transform;

        var sky = Make("Sky", Art.Sky, new Vector3(0, 0, 0), new Vector3(26f / 8f, 12f / 256f, 1), -100, root);
        sky.drawMode = SpriteDrawMode.Simple;

        var sun = Make("Sun", Art.Sun, new Vector3(3.4f, 2.0f, 0), Vector3.one * 1.1f, -96, root);
        sun.gameObject.AddComponent<Pulse>().Set(0.03f, 0.35f);

        Make("MountainsFar", Art.Mountains(0), new Vector3(0, -2.7f, 0), Vector3.one, -90, root);
        var mist1 = Make("Mist", Art.Mist, new Vector3(0, -1.5f, 0), new Vector3(1f, 1.4f, 1), -88, root, new Color(1, 1, 1, 0.55f));
        mist1.gameObject.AddComponent<Drift>().Set(new Vector3(0.6f, 0, 0), 0.05f);
        Make("MountainsMid", Art.Mountains(1), new Vector3(0, -3.05f, 0), Vector3.one, -80, root);
        var mist2 = Make("Mist", Art.Mist, new Vector3(0, -2.45f, 0), new Vector3(1.1f, 1.2f, 1), -78, root, new Color(1, 0.92f, 0.9f, 0.45f));
        mist2.gameObject.AddComponent<Drift>().Set(new Vector3(-0.8f, 0, 0), 0.04f);
        Make("MountainsNear", Art.Mountains(2), new Vector3(0, -3.35f, 0), Vector3.one, -70, root);

        Make("Torii", Art.Torii, new Vector3(6.4f, GroundY - 0.12f, 0), Vector3.one, -55, root);
        Make("Sand", Art.RakedSand, new Vector3(0, GroundY, 0), Vector3.one, -50, root);

        var tree = Make("CherryTree", Art.CherryTree, new Vector3(-7.6f, GroundY - 0.25f, 0), Vector3.one, -45, root);
        tree.gameObject.AddComponent<Sway>().Set(1.3f, 0.45f);

        Petals(root, -44, 28f, 0.10f, 0.16f, 0.75f);
        Petals(root, 60, 5f, 0.18f, 0.26f, 1f);
        KiMotes(root);

        var vig = Make("Vignette", Art.Vignette, Vector3.zero, new Vector3(26f, 12f, 1), 90, root);
        vig.color = Color.white;
        return root;
    }

    public static SpriteRenderer Make(string name, Sprite sprite, Vector3 pos, Vector3 scale, int order, Transform parent, Color? color = null, Material mat = null)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        if (color.HasValue) sr.color = color.Value;
        if (mat != null) sr.sharedMaterial = mat;
        return sr;
    }

    static void Petals(Transform root, int order, float rate, float minSize, float maxSize, float alpha)
    {
        var go = new GameObject(order < 0 ? "PetalsBack" : "PetalsFront");
        go.transform.SetParent(root, false);
        go.transform.localPosition = new Vector3(-1f, 6.2f, 0);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = true;
        main.prewarm = true;
        main.duration = 5f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(9f, 14f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.85f, 0.9f, alpha), new Color(1f, 0.68f, 0.8f, alpha));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 300;

        var em = ps.emission;
        em.rateOverTime = rate;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(26f, 0.2f, 0.1f);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(0.25f, 0.7f);
        vel.y = new ParticleSystem.MinMaxCurve(-0.75f, -1.25f);
        vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.5f;
        noise.frequency = 0.35f;
        noise.scrollSpeed = 0.2f;

        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-2f, 2f);

        var r = go.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = Art.PetalParticle;
        r.sortingOrder = order;
        ps.Play();
    }

    static void KiMotes(Transform root)
    {
        var go = new GameObject("KiMotes");
        go.transform.SetParent(root, false);
        go.transform.localPosition = new Vector3(0, -3.6f, 0);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.loop = true;
        main.prewarm = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 9f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.85f, 0.5f, 0.8f), new Color(1f, 0.95f, 0.75f, 0.5f));
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var em = ps.emission;
        em.rateOverTime = 4f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(24f, 1.2f, 0.1f);
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.x = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
        vel.y = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
        vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                  new[] { new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 0.3f), new GradientAlphaKey(0, 1) });
        col.color = g;
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 0.5f;
        var r = go.GetComponent<ParticleSystemRenderer>();
        r.sharedMaterial = Art.AdditiveParticle;
        r.sortingOrder = -30;
        ps.Play();
    }
}

// Balanceo suave (Ã¡rbol con viento).
public class Sway : MonoBehaviour
{
    float angle, speed, phase;
    public void Set(float angle, float speed) { this.angle = angle; this.speed = speed; phase = Random.value * 10f; }
    void Update() => transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * speed + phase) * angle);
}

// Deriva lenta de ida y vuelta (niebla).
public class Drift : MonoBehaviour
{
    Vector3 amp, basePos; float speed;
    public void Set(Vector3 amp, float speed) { this.amp = amp; this.speed = speed; basePos = transform.localPosition; }
    void Update() => transform.localPosition = basePos + amp * Mathf.Sin(Time.time * speed * Mathf.PI * 2f);
}

// Pulso de escala (sol, auras).
public class Pulse : MonoBehaviour
{
    float amount, speed; Vector3 baseScale;
    public void Set(float amount, float speed) { this.amount = amount; this.speed = speed; baseScale = transform.localScale; }
    void Update() => transform.localScale = baseScale * (1f + Mathf.Sin(Time.time * speed * Mathf.PI * 2f) * amount);
}

