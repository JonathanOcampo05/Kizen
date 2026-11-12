using UnityEngine;

// Sonido sintetizado por código (sin archivos de audio): disparo de ki, impacto,
// disolución de espíritu, gong, cuenco tibetano y un drone ambiental en loop.
public static class Sfx
{
    const int SR = 44100;
    static AudioSource[] pool;
    static AudioSource ambient;
    static int next;
    static AudioClip shoot, power, hit, kill, gong, bowl, bowlLow, ambientClip;

    public static void Init(GameObject host)
    {
        pool = new AudioSource[10];
        for (int i = 0; i < pool.Length; i++) { pool[i] = host.AddComponent<AudioSource>(); pool[i].playOnAwake = false; }
        ambient = host.AddComponent<AudioSource>();
        ambient.loop = true;
        ambient.volume = 0f;

        if (shoot == null) shoot = Make("Shoot", 0.2f, Shoot());
        if (power == null) power = Make("Power", 1.0f, Power());
        if (hit == null) hit = Make("Hit", 0.3f, Hit());
        if (kill == null) kill = Make("Kill", 0.6f, Kill());
        if (gong == null) gong = Make("Gong", 2.4f, Gong());
        if (bowl == null) bowl = Make("Bowl", 3.0f, Bowl(523.25f));
        if (bowlLow == null) bowlLow = Make("BowlLow", 3.5f, Bowl(349.23f));
        if (ambientClip == null) ambientClip = Make("Ambient", 12f, Ambient(), false);
        ambient.clip = ambientClip;
        ambient.Play();
    }

    public static float AmbientVolume { set { if (ambient) ambient.volume = value; } get => ambient ? ambient.volume : 0f; }

    static void Play(AudioClip c, float vol, float pitchVar = 0.06f)
    {
        if (pool == null || c == null) return;
        var s = pool[next];
        next = (next + 1) % pool.Length;
        if (s == null) return;
        s.pitch = 1f + Random.Range(-pitchVar, pitchVar);
        s.PlayOneShot(c, vol);
    }

    public static void PlayShoot() => Play(shoot, 0.35f, 0.1f);
    public static void PlayPower() => Play(power, 0.8f, 0.02f);
    public static void PlayHit() => Play(hit, 0.5f);
    public static void PlayKill() => Play(kill, 0.45f, 0.12f);
    public static void PlayGong() => Play(gong, 0.8f, 0.02f);
    public static void PlayBowl() => Play(bowl, 0.55f, 0.01f);
    public static void PlayBowlLow() => Play(bowlLow, 0.6f, 0.01f);

    // ------------------------------------------------------------ Síntesis

    static AudioClip Make(string name, float dur, System.Func<float, float> f, bool fade = true)
    {
        int n = (int)(dur * SR);
        var data = new float[n];
        for (int i = 0; i < n; i++) data[i] = Mathf.Clamp(f(i / (float)SR), -1f, 1f);
        if (fade)
        {
            int fl = Mathf.Min(n / 4, SR / 200);
            for (int i = 0; i < fl; i++) { data[i] *= i / (float)fl; data[n - 1 - i] *= i / (float)fl; }
        }
        var clip = AudioClip.Create(name, n, 1, SR, false);
        clip.SetData(data, 0);
        return clip;
    }

    const float TAU = Mathf.PI * 2f;

    static System.Func<float, float> Noise(int seed, float smooth)
    {
        var rnd = new System.Random(seed);
        float lp = 0f;
        return _ => { lp += ((float)rnd.NextDouble() * 2f - 1f - lp) * smooth; return lp; };
    }

    static System.Func<float, float> Shoot()
    {
        float phase = 0f;
        var noise = Noise(1, 0.3f);
        return t =>
        {
            float f = Mathf.Lerp(650f, 1350f, Mathf.Clamp01(t / 0.1f));
            phase += TAU * f / SR;
            return (Mathf.Sin(phase) * 0.5f + Mathf.Sin(phase * 2f) * 0.15f) * Mathf.Exp(-t * 20f) * 0.6f
                 + noise(t) * Mathf.Exp(-t * 35f) * 0.25f;
        };
    }

    static System.Func<float, float> Power()
    {
        var noise = Noise(2, 0.08f);
        float phase = 0f;
        return t =>
        {
            phase += TAU * Mathf.Lerp(120f, 196f, Mathf.Clamp01(t * 8f)) / SR;
            float low = Mathf.Sin(phase) * Mathf.Exp(-t * 3.5f) * 0.55f + Mathf.Sin(phase * 2.005f) * Mathf.Exp(-t * 5f) * 0.2f;
            float chime = Mathf.Sin(TAU * 1568f * t) * Mathf.Exp(-t * 5f) * 0.16f + Mathf.Sin(TAU * 2349f * t) * Mathf.Exp(-t * 7f) * 0.08f;
            float whoosh = noise(t) * Mathf.Sin(Mathf.Clamp01(t / 0.35f) * Mathf.PI) * 0.9f;
            return (low + chime + whoosh) * 0.75f;
        };
    }

    static System.Func<float, float> Hit()
    {
        var noise = Noise(3, 0.25f);
        float phase = 0f, phase2 = 0f;
        return t =>
        {
            phase += TAU * Mathf.Lerp(190f, 55f, Mathf.Clamp01(t / 0.15f)) / SR;
            phase2 += TAU * Mathf.Lerp(1800f, 500f, Mathf.Clamp01(t / 0.08f)) / SR;
            return Mathf.Sin(phase) * Mathf.Exp(-t * 14f) * 0.7f + noise(t) * Mathf.Exp(-t * 22f) * 0.45f + Mathf.Sin(phase2) * Mathf.Exp(-t * 30f) * 0.2f;
        };
    }

    static System.Func<float, float> Kill()
    {
        var noise = Noise(4, 0.6f);
        return t =>
        {
            float trem = 0.6f + 0.4f * Mathf.Sin(TAU * 18f * t);
            float s = Mathf.Sin(TAU * 1175f * t) + Mathf.Sin(TAU * 1760f * t) * 0.7f + Mathf.Sin(TAU * 2637f * t) * 0.5f;
            return s * trem * Mathf.Exp(-t * 7f) * 0.17f + noise(t) * Mathf.Exp(-t * 12f) * 0.12f;
        };
    }

    static System.Func<float, float> Gong()
    {
        float[] ratio = { 1f, 1.52f, 2.0f, 2.76f, 3.9f };
        float[] amp = { 0.5f, 0.3f, 0.22f, 0.15f, 0.08f };
        float[] dec = { 1.1f, 1.7f, 2.2f, 3f, 4f };
        var noise = Noise(5, 0.4f);
        return t =>
        {
            float s = 0f;
            for (int i = 0; i < ratio.Length; i++) s += Mathf.Sin(TAU * 98f * ratio[i] * t) * amp[i] * Mathf.Exp(-t * dec[i]);
            s *= 1f + 0.25f * Mathf.Sin(TAU * 3f * t);
            return s * 0.75f + noise(t) * Mathf.Exp(-t * 30f) * 0.3f;
        };
    }

    static System.Func<float, float> Bowl(float f0)
    {
        float[] ratio = { 1f, 2.73f, 5.2f };
        float[] amp = { 0.5f, 0.24f, 0.1f };
        float[] dec = { 0.8f, 1.3f, 2.1f };
        return t =>
        {
            float s = 0f;
            for (int i = 0; i < ratio.Length; i++)
            {
                float f = f0 * ratio[i];
                s += (Mathf.Sin(TAU * f * t) + Mathf.Sin(TAU * f * 1.004f * t)) * 0.5f * amp[i] * Mathf.Exp(-t * dec[i]);
            }
            return s * Mathf.Clamp01(t / 0.012f) * 0.8f;
        };
    }

    // Drone ambiental de 12 s; todas las frecuencias completan ciclos enteros para un loop perfecto.
    static System.Func<float, float> Ambient()
    {
        return t =>
        {
            float a = Mathf.Sin(TAU * 110f * t) * 0.18f * (0.6f + 0.4f * Mathf.Sin(TAU * t / 12f));
            float b = Mathf.Sin(TAU * 165f * t) * 0.12f * (0.6f + 0.4f * Mathf.Sin(TAU * t / 6f + 1f));
            float c = Mathf.Sin(TAU * 220f * t) * 0.07f * (0.5f + 0.5f * Mathf.Sin(TAU * t / 4f));
            float d = Mathf.Sin(TAU * 330f * t) * 0.04f * (0.5f + 0.5f * Mathf.Sin(TAU * t / 3f + 2f));
            float e = Mathf.Sin(TAU * 880f * t) * 0.015f * (0.5f + 0.5f * Mathf.Sin(TAU * t / 2f));
            return (a + b + c + d + e) * 0.7f;
        };
    }
}
