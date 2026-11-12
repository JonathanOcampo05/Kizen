using System.Collections.Generic;
using UnityEngine;

// Generador de arte procedural: todos los sprites del juego se dibujan por cÃ³digo
// (sin imÃ¡genes externas). Cada sprite se genera una vez y se guarda en cachÃ©.
public static class Art
{
    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
    static readonly Dictionary<string, Material> mats = new Dictionary<string, Material>();

    // ---------------------------------------------------------------- Materiales

    static Material GetMat(string key, string shader, string fallback, Texture tex)
    {
        if (mats.TryGetValue(key, out var m) && m != null) return m;
        var sh = Shader.Find(shader);
        if (sh == null) sh = Shader.Find(fallback);
        m = new Material(sh) { name = key };
        if (tex != null) m.mainTexture = tex;
        mats[key] = m;
        return m;
    }

    /// Material aditivo (brillos, ki, partÃ­culas de energÃ­a).
    public static Material Additive => GetMat("additive", "Legacy Shaders/Particles/Additive", "Sprites/Default", null);
    public static Material AdditiveParticle => GetMat("additiveParticle", "Legacy Shaders/Particles/Additive", "Sprites/Default", SoftDot.texture);
    public static Material AlphaParticle => GetMat("alphaParticle", "Sprites/Default", "Sprites/Default", SoftDot.texture);
    public static Material PetalParticle => GetMat("petalParticle", "Sprites/Default", "Sprites/Default", Petal.texture);

    // ---------------------------------------------------------------- Lienzo

    class Paint
    {
        public readonly int w, h;
        public readonly Color[] px;

        public Paint(int w, int h) { this.w = w; this.h = h; px = new Color[w * h]; }

        public void Blend(int x, int y, Color c, float cov = 1f)
        {
            if (x < 0 || y < 0 || x >= w || y >= h) return;
            float a = c.a * cov;
            if (a <= 0.001f) return;
            int i = y * w + x;
            Color d = px[i];
            float outA = a + d.a * (1f - a);
            float k = d.a * (1f - a);
            px[i] = new Color(
                (c.r * a + d.r * k) / outA,
                (c.g * a + d.g * k) / outA,
                (c.b * a + d.b * k) / outA,
                outA);
        }

        public void Circle(float cx, float cy, float r, Color c, float soft = 1.2f)
        {
            int x0 = Mathf.FloorToInt(cx - r - 2), x1 = Mathf.CeilToInt(cx + r + 2);
            int y0 = Mathf.FloorToInt(cy - r - 2), y1 = Mathf.CeilToInt(cy + r + 2);
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    float d = Mathf.Sqrt((x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy));
                    Blend(x, y, c, Mathf.Clamp01((r - d) / soft + 0.5f));
                }
        }

        public void Ellipse(float cx, float cy, float rx, float ry, Color c, float angleDeg = 0f, float soft = 1.2f)
        {
            float rad = angleDeg * Mathf.Deg2Rad, cs = Mathf.Cos(rad), sn = Mathf.Sin(rad);
            float m = Mathf.Max(rx, ry) + 2;
            for (int y = Mathf.FloorToInt(cy - m); y <= Mathf.CeilToInt(cy + m); y++)
                for (int x = Mathf.FloorToInt(cx - m); x <= Mathf.CeilToInt(cx + m); x++)
                {
                    float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                    float lx = dx * cs + dy * sn, ly = -dx * sn + dy * cs;
                    float k = Mathf.Sqrt((lx / rx) * (lx / rx) + (ly / ry) * (ly / ry));
                    float sd = (k - 1f) * Mathf.Min(rx, ry);
                    Blend(x, y, c, Mathf.Clamp01(-sd / soft + 0.5f));
                }
        }

        public void Poly(Color c, params Vector2[] p)
        {
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            foreach (var v in p) { minX = Mathf.Min(minX, v.x); minY = Mathf.Min(minY, v.y); maxX = Mathf.Max(maxX, v.x); maxY = Mathf.Max(maxY, v.y); }
            for (int y = Mathf.FloorToInt(minY) - 1; y <= Mathf.CeilToInt(maxY) + 1; y++)
                for (int x = Mathf.FloorToInt(minX) - 1; x <= Mathf.CeilToInt(maxX) + 1; x++)
                {
                    int inside = 0;
                    for (int s = 0; s < 4; s++)
                    {
                        float sx = x + 0.25f + 0.5f * (s & 1), sy = y + 0.25f + 0.5f * (s >> 1);
                        if (Inside(p, sx, sy)) inside++;
                    }
                    if (inside > 0) Blend(x, y, c, inside / 4f);
                }
        }

        static bool Inside(Vector2[] p, float x, float y)
        {
            bool r = false;
            for (int i = 0, j = p.Length - 1; i < p.Length; j = i++)
                if ((p[i].y > y) != (p[j].y > y) && x < (p[j].x - p[i].x) * (y - p[i].y) / (p[j].y - p[i].y) + p[i].x) r = !r;
            return r;
        }

        public void Capsule(Vector2 a, Vector2 b, float r0, float r1, Color c)
        {
            float m = Mathf.Max(r0, r1) + 2;
            int x0 = Mathf.FloorToInt(Mathf.Min(a.x, b.x) - m), x1 = Mathf.CeilToInt(Mathf.Max(a.x, b.x) + m);
            int y0 = Mathf.FloorToInt(Mathf.Min(a.y, b.y) - m), y1 = Mathf.CeilToInt(Mathf.Max(a.y, b.y) + m);
            Vector2 ab = b - a;
            float len2 = Mathf.Max(ab.sqrMagnitude, 0.0001f);
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / len2);
                    float d = Vector2.Distance(p, a + ab * t);
                    float r = Mathf.Lerp(r0, r1, t);
                    Blend(x, y, c, Mathf.Clamp01((r - d) / 1.2f + 0.5f));
                }
        }

        public void Glow(float cx, float cy, float r, Color c, float power = 2f)
        {
            for (int y = Mathf.FloorToInt(cy - r); y <= Mathf.CeilToInt(cy + r); y++)
                for (int x = Mathf.FloorToInt(cx - r); x <= Mathf.CeilToInt(cx + r); x++)
                {
                    float d = Mathf.Sqrt((x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy)) / r;
                    if (d < 1f) Blend(x, y, c, Mathf.Pow(1f - d, power));
                }
        }

        /// Contorno estilo ilustraciÃ³n alrededor de las zonas opacas.
        public void Outline(Color c, int thick)
        {
            var src = (Color[])px.Clone();
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    if (src[y * w + x].a > 0.95f) continue;
                    float best = 0f;
                    for (int oy = -thick; oy <= thick; oy++)
                        for (int ox = -thick; ox <= thick; ox++)
                        {
                            int nx = x + ox, ny = y + oy;
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h || ox * ox + oy * oy > thick * thick) continue;
                            best = Mathf.Max(best, src[ny * w + nx].a);
                        }
                    if (best <= 0f) continue;
                    Color top = src[y * w + x];
                    Color under = new Color(c.r, c.g, c.b, c.a * best);
                    float outA = top.a + under.a * (1 - top.a);
                    if (outA <= 0f) continue;
                    float k = under.a * (1 - top.a);
                    px[y * w + x] = new Color((top.r * top.a + under.r * k) / outA, (top.g * top.a + under.g * k) / outA, (top.b * top.a + under.b * k) / outA, outA);
                }
        }

        public Sprite ToSprite(string name, Vector2 pivot, float ppu, Vector4 border = default, TextureWrapMode wrap = TextureWrapMode.Clamp)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { name = name, wrapMode = wrap, filterMode = FilterMode.Bilinear };
            tex.SetPixels(px);
            tex.Apply(false, true);
            var s = Sprite.Create(tex, new Rect(0, 0, w, h), pivot, ppu, 0, SpriteMeshType.FullRect, border);
            s.name = name;
            return s;
        }
    }

    static Sprite Cached(string key, System.Func<Sprite> make)
    {
        if (cache.TryGetValue(key, out var s) && s != null) return s;
        s = make();
        cache[key] = s;
        return s;
    }

    static Color C(float r, float g, float b, float a = 1f) => new Color(r, g, b, a);

    // ---------------------------------------------------------------- Utilidades genÃ©ricas

    public static Sprite Glow => Cached("glow", () => { var c = new Paint(64, 64); c.Glow(32, 32, 32, Color.white, 2f); return c.ToSprite("Glow", new Vector2(0.5f, 0.5f), 64); });
    public static Sprite SoftDot => Cached("dot", () => { var c = new Paint(32, 32); c.Glow(16, 16, 16, Color.white, 1.4f); return c.ToSprite("SoftDot", new Vector2(0.5f, 0.5f), 32); });

    public static Sprite Ring => Cached("ring", () =>
    {
        var c = new Paint(128, 128);
        for (int y = 0; y < 128; y++)
            for (int x = 0; x < 128; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(64, 64));
                float a = Mathf.Exp(-Mathf.Pow((d - 54f) / 5f, 2f));
                c.Blend(x, y, Color.white, a);
            }
        return c.ToSprite("Ring", new Vector2(0.5f, 0.5f), 128);
    });

    public static Sprite Square => Cached("square", () => { var c = new Paint(4, 4); for (int i = 0; i < 16; i++) c.px[i] = Color.white; return c.ToSprite("Square", new Vector2(0.5f, 0.5f), 4); });

    public static Sprite RoundedRect => Cached("rrect", () =>
    {
        const int W = 64, H = 32; const float R = 15f;
        var c = new Paint(W, H);
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                float qx = Mathf.Max(Mathf.Abs(x + 0.5f - W / 2f) - (W / 2f - R), 0f);
                float qy = Mathf.Max(Mathf.Abs(y + 0.5f - H / 2f) - (H / 2f - R), 0f);
                float d = Mathf.Sqrt(qx * qx + qy * qy) - R;
                c.Blend(x, y, Color.white, Mathf.Clamp01(-d + 0.5f));
            }
        return c.ToSprite("RoundedRect", new Vector2(0.5f, 0.5f), 100, new Vector4(16, 15, 16, 15));
    });

    // ---------------------------------------------------------------- Personajes

    /// Monje con tÃºnica azafrÃ¡n, faja granate, mala de cuentas y manos en posiciÃ³n de ki.
    public static Sprite Monk => Cached("monk", () =>
    {
        const int W = 128, H = 176;
        var c = new Paint(W, H);
        Vector2 P(float u, float v) => new Vector2(u * W, v * H);
        Color robe = C(0.96f, 0.56f, 0.14f), robeDark = C(0.78f, 0.36f, 0.08f), sash = C(0.60f, 0.11f, 0.14f);
        Color skin = C(0.96f, 0.79f, 0.63f), skinShade = C(0.84f, 0.62f, 0.48f), ink = C(0.16f, 0.09f, 0.09f);

        c.Ellipse(0.39f * W, 0.04f * H, 0.10f * W, 0.03f * H, ink);
        c.Ellipse(0.61f * W, 0.04f * H, 0.10f * W, 0.03f * H, ink);
        c.Poly(robe, P(0.15f, 0.05f), P(0.85f, 0.05f), P(0.72f, 0.58f), P(0.28f, 0.58f));
        c.Poly(new Color(robeDark.r, robeDark.g, robeDark.b, 0.55f), P(0.56f, 0.05f), P(0.85f, 0.05f), P(0.72f, 0.58f), P(0.57f, 0.58f));
        c.Poly(robeDark, P(0.15f, 0.05f), P(0.85f, 0.05f), P(0.835f, 0.10f), P(0.165f, 0.10f));
        // Pliegues de la tÃºnica
        c.Capsule(P(0.40f, 0.12f), P(0.44f, 0.45f), 1.2f, 0.8f, new Color(robeDark.r, robeDark.g, robeDark.b, 0.6f));
        c.Capsule(P(0.30f, 0.12f), P(0.36f, 0.40f), 1.2f, 0.8f, new Color(robeDark.r, robeDark.g, robeDark.b, 0.5f));
        c.Ellipse(0.5f * W, 0.60f * H, 0.26f * W, 0.10f * H, robe);
        c.Poly(sash, P(0.28f, 0.66f), P(0.40f, 0.70f), P(0.75f, 0.22f), P(0.62f, 0.18f));
        c.Capsule(P(0.27f, 0.60f), P(0.45f, 0.48f), 0.075f * W, 0.065f * W, robe);
        c.Capsule(P(0.73f, 0.60f), P(0.55f, 0.48f), 0.075f * W, 0.065f * W, C(0.90f, 0.48f, 0.11f));
        c.Circle(0.5f * W, 0.485f * H, 0.06f * W, skin);
        c.Capsule(P(0.5f, 0.45f), P(0.5f, 0.53f), 0.8f, 0.8f, skinShade);
        // Cabeza
        c.Circle(0.5f * W, 0.665f * H, 0.065f * W, skinShade);
        c.Circle(0.365f * W, 0.77f * H, 0.03f * W, skinShade);
        c.Circle(0.635f * W, 0.77f * H, 0.03f * W, skinShade);
        c.Circle(0.5f * W, 0.775f * H, 0.135f * W, skin);
        c.Circle(0.455f * W, 0.815f * H, 0.055f * W, new Color(1f, 1f, 1f, 0.22f));
        c.Capsule(P(0.415f, 0.765f), P(0.465f, 0.756f), 1.6f, 1.4f, ink);
        c.Capsule(P(0.535f, 0.756f), P(0.585f, 0.765f), 1.4f, 1.6f, ink);
        c.Capsule(P(0.48f, 0.715f), P(0.52f, 0.715f), 1.1f, 1.1f, C(0.70f, 0.40f, 0.35f));
        c.Circle(0.5f * W, 0.835f * H, 2.4f, C(0.85f, 0.22f, 0.18f));
        // Mala (collar de cuentas)
        for (int k = 0; k <= 8; k++)
        {
            float a = Mathf.Lerp(200f, 340f, k / 8f) * Mathf.Deg2Rad;
            c.Circle(0.5f * W + Mathf.Cos(a) * 0.15f * W, 0.645f * H + Mathf.Sin(a) * 0.05f * H, 2.8f, C(0.36f, 0.18f, 0.10f));
        }
        c.Outline(C(0.12f, 0.06f, 0.07f), 2);
        return c.ToSprite("Monk", new Vector2(0.5f, 0.02f), 150);
    });

    /// EspÃ­ritu oscuro con cola espectral y ojos brillantes. oni = variante roja con cuernos.
    public static Sprite SpiritSprite(bool oni) => Cached(oni ? "oni" : "wisp", () =>
    {
        const int S = 128;
        var c = new Paint(S, S);
        Color core = oni ? C(0.28f, 0.03f, 0.06f) : C(0.17f, 0.06f, 0.30f);
        Color mid = oni ? C(0.72f, 0.12f, 0.12f) : C(0.43f, 0.17f, 0.64f);
        Color rim = oni ? C(1f, 0.55f, 0.30f) : C(0.78f, 0.50f, 0.98f);
        Color eye = oni ? C(1f, 0.86f, 0.25f) : C(1f, 0.95f, 1f);

        for (int i = 18; i >= 0; i--)
        {
            float t = i / 18f;
            float cy = (0.42f + t * 0.52f) * S;
            float cx = (0.5f + Mathf.Sin(t * 6f) * 0.07f * t) * S;
            float r = (0.22f * (1f - t) + 0.025f) * S;
            c.Circle(cx, cy, r, new Color(mid.r, mid.g, mid.b, (1f - t) * 0.85f), 3f);
        }
        if (oni)
        {
            Color horn = C(0.96f, 0.88f, 0.74f);
            c.Poly(horn, new Vector2(0.31f * S, 0.55f * S), new Vector2(0.41f * S, 0.61f * S), new Vector2(0.26f * S, 0.78f * S));
            c.Poly(horn, new Vector2(0.69f * S, 0.55f * S), new Vector2(0.59f * S, 0.61f * S), new Vector2(0.74f * S, 0.78f * S));
        }
        c.Circle(0.5f * S, 0.40f * S, 0.27f * S, mid, 1.5f);
        c.Circle(0.5f * S, 0.38f * S, 0.22f * S, core, 3f);
        c.Circle(0.42f * S, 0.48f * S, 0.08f * S, new Color(rim.r, rim.g, rim.b, 0.35f), 4f);
        c.Glow(0.41f * S, 0.40f * S, 0.11f * S, new Color(eye.r, eye.g, eye.b, 0.55f));
        c.Glow(0.59f * S, 0.40f * S, 0.11f * S, new Color(eye.r, eye.g, eye.b, 0.55f));
        c.Ellipse(0.41f * S, 0.40f * S, 0.055f * S, 0.032f * S, eye, -22f);
        c.Ellipse(0.59f * S, 0.40f * S, 0.055f * S, 0.032f * S, eye, 22f);
        if (oni)
        {
            for (int k = 0; k < 4; k++)
            {
                float x = (0.42f + k * 0.053f) * S;
                c.Poly(C(0.96f, 0.92f, 0.85f), new Vector2(x, 0.30f * S), new Vector2(x + 0.04f * S, 0.30f * S), new Vector2(x + 0.02f * S, 0.26f * S));
            }
        }
        return c.ToSprite(oni ? "Oni" : "Wisp", new Vector2(0.5f, 0.40f), 110);
    });

    /// Esfera de ki: nÃºcleo blanco con halo dorado.
    public static Sprite KiOrb => Cached("ki", () =>
    {
        var c = new Paint(64, 64);
        c.Glow(32, 32, 32, C(1f, 0.82f, 0.40f, 0.9f), 2.6f);
        c.Glow(32, 32, 17, C(1f, 0.97f, 0.85f), 1.6f);
        c.Circle(32, 32, 7.5f, Color.white, 2f);
        return c.ToSprite("KiOrb", new Vector2(0.5f, 0.5f), 64);
    });

    public static Sprite Petal => Cached("petal", () =>
    {
        var c = new Paint(32, 20);
        c.Ellipse(16, 10, 14, 8, C(1f, 0.80f, 0.87f));
        c.Ellipse(20, 10, 7, 5, C(1f, 0.92f, 0.95f, 0.8f));
        c.Ellipse(4, 10, 3, 3, C(0.92f, 0.55f, 0.68f, 0.9f));
        return c.ToSprite("Petal", new Vector2(0.5f, 0.5f), 100);
    });

    public static Sprite Lotus => Cached("lotus", () =>
    {
        var c = new Paint(64, 64);
        c.Ellipse(32, 15, 26, 7, C(0.30f, 0.52f, 0.32f));
        float[] ang = { -62f, 62f, -32f, 32f, 0f };
        foreach (float a in ang)
        {
            float r = a * Mathf.Deg2Rad;
            c.Ellipse(32 + Mathf.Sin(r) * 13, 24 + Mathf.Cos(r) * 13, 8f, 17f, C(0.98f, 0.62f, 0.75f), -a);
            c.Ellipse(32 + Mathf.Sin(r) * 13, 26 + Mathf.Cos(r) * 13, 4f, 10f, C(1f, 0.86f, 0.92f, 0.85f), -a);
        }
        c.Outline(C(0.25f, 0.08f, 0.15f, 0.9f), 2);
        return c.ToSprite("Lotus", new Vector2(0.5f, 0.5f), 64);
    });

    /// EnsÅ: cÃ­rculo zen pintado de un solo trazo de pincel.
    public static Sprite Enso => Cached("enso", () =>
    {
        const int S = 512;
        var c = new Paint(S, S);
        var rnd = new System.Random(3);
        float start = 115f, sweep = 320f;
        for (int i = 0; i <= 900; i++)
        {
            float t = i / 900f;
            float a = (start - t * sweep) * Mathf.Deg2Rad;
            float rad = 200f + Mathf.Sin(t * 9f) * 3f;
            float thick = Mathf.Lerp(30f, 6f, Mathf.Pow(t, 1.6f)) * (0.9f + 0.1f * Mathf.Sin(t * 40f));
            float x = 256 + Mathf.Cos(a) * rad, y = 256 + Mathf.Sin(a) * rad;
            c.Circle(x, y, thick, new Color(1f, 1f, 1f, 0.16f), 2.5f);
            if (rnd.NextDouble() < 0.12 && t > 0.3f)
                c.Circle(x + (float)(rnd.NextDouble() - 0.5) * thick * 1.6f, y + (float)(rnd.NextDouble() - 0.5) * thick * 1.6f, 1.5f, new Color(1f, 1f, 1f, 0.5f));
        }
        return c.ToSprite("Enso", new Vector2(0.5f, 0.5f), 100);
    });

    // ---------------------------------------------------------------- Escenario

    public static Sprite Sky => Cached("sky", () =>
    {
        var c = new Paint(8, 256);
        Color[] stops = { C(1f, 0.86f, 0.66f), C(0.98f, 0.66f, 0.52f), C(0.62f, 0.40f, 0.55f), C(0.24f, 0.18f, 0.38f), C(0.09f, 0.08f, 0.22f) };
        float[] at = { 0f, 0.22f, 0.45f, 0.72f, 1f };
        for (int y = 0; y < 256; y++)
        {
            float v = y / 255f;
            int k = 0;
            while (k < at.Length - 2 && v > at[k + 1]) k++;
            Color col = Color.Lerp(stops[k], stops[k + 1], Mathf.SmoothStep(0, 1, (v - at[k]) / (at[k + 1] - at[k])));
            for (int x = 0; x < 8; x++) c.px[y * 8 + x] = col;
        }
        return c.ToSprite("Sky", new Vector2(0.5f, 0.5f), 1);
    });

    public static Sprite Sun => Cached("sun", () =>
    {
        var c = new Paint(256, 256);
        c.Glow(128, 128, 128, C(1f, 0.62f, 0.45f, 0.55f), 2.2f);
        c.Circle(128, 128, 54, C(1f, 0.40f, 0.30f), 2f);
        c.Circle(120, 136, 40, C(1f, 0.52f, 0.38f, 0.5f), 12f);
        return c.ToSprite("Sun", new Vector2(0.5f, 0.5f), 80);
    });

    public static Sprite Mountains(int layer) => Cached("mtn" + layer, () =>
    {
        const int W = 1024, H = 256;
        var c = new Paint(W, H);
        float seed = 13.7f * (layer + 1);
        float baseH = new[] { 90f, 60f, 28f }[layer];
        float amp = new[] { 140f, 105f, 70f }[layer];
        Color top = new[] { C(0.58f, 0.44f, 0.62f), C(0.39f, 0.28f, 0.47f), C(0.20f, 0.15f, 0.27f) }[layer];
        Color bottom = new[] { C(0.95f, 0.72f, 0.66f), C(0.78f, 0.55f, 0.58f), C(0.42f, 0.30f, 0.40f) }[layer];
        Color rimLight = C(1f, 0.78f, 0.62f, 0.5f);
        for (int x = 0; x < W; x++)
        {
            float n = 0.55f * Mathf.PerlinNoise(x * 0.0042f + seed, seed) + 0.30f * Mathf.PerlinNoise(x * 0.013f + seed * 2f, 1f) + 0.15f * Mathf.PerlinNoise(x * 0.045f + seed * 3f, 2f);
            float ridge = baseH + amp * n;
            for (int y = 0; y < H; y++)
            {
                float cov = Mathf.Clamp01(ridge - y + 0.5f);
                if (cov <= 0f) break;
                Color col = Color.Lerp(bottom, top, Mathf.Pow(Mathf.Clamp01(y / ridge), 0.55f));
                if (ridge - y < 4f && layer < 2) col = Color.Lerp(col, rimLight, rimLight.a * (1f - (ridge - y) / 4f));
                col.a = 1f;
                c.Blend(x, y, col, cov);
            }
        }
        return c.ToSprite("Mountains" + layer, new Vector2(0.5f, 0f), W / 24f);
    });

    public static Sprite Mist => Cached("mist", () =>
    {
        const int W = 512, H = 64;
        var c = new Paint(W, H);
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                float v = Mathf.Abs(y / (float)(H - 1) - 0.5f) * 2f;
                float a = Mathf.Pow(1f - v, 2f) * (0.55f + 0.45f * Mathf.PerlinNoise(x * 0.012f, y * 0.05f));
                c.Blend(x, y, C(1f, 0.90f, 0.86f), a);
            }
        return c.ToSprite("Mist", new Vector2(0.5f, 0.5f), W / 24f);
    });

    public static Sprite Torii => Cached("torii", () =>
    {
        const int S = 160;
        var c = new Paint(S, S);
        Vector2 P(float u, float v) => new Vector2(u * S, v * S);
        Color red = C(0.80f, 0.17f, 0.12f), dark = C(0.16f, 0.07f, 0.07f), shade = C(0.62f, 0.12f, 0.09f);
        c.Poly(red, P(0.20f, 0.0f), P(0.29f, 0.0f), P(0.275f, 0.80f), P(0.225f, 0.80f));
        c.Poly(shade, P(0.71f, 0.0f), P(0.80f, 0.0f), P(0.775f, 0.80f), P(0.725f, 0.80f));
        c.Poly(dark, P(0.185f, 0.0f), P(0.305f, 0.0f), P(0.30f, 0.06f), P(0.19f, 0.06f));
        c.Poly(dark, P(0.695f, 0.0f), P(0.815f, 0.0f), P(0.81f, 0.06f), P(0.70f, 0.06f));
        c.Poly(red, P(0.10f, 0.60f), P(0.90f, 0.60f), P(0.90f, 0.655f), P(0.10f, 0.655f));
        c.Poly(red, P(0.47f, 0.655f), P(0.53f, 0.655f), P(0.53f, 0.74f), P(0.47f, 0.74f));
        c.Poly(red, P(0.10f, 0.74f), P(0.90f, 0.74f), P(0.90f, 0.79f), P(0.10f, 0.79f));
        c.Poly(red, P(0.08f, 0.79f), P(0.92f, 0.79f), P(0.99f, 0.865f), P(0.01f, 0.865f));
        c.Poly(dark, P(0.01f, 0.865f), P(0.99f, 0.865f), P(1.0f, 0.905f), P(0.0f, 0.905f));
        return c.ToSprite("Torii", new Vector2(0.5f, 0f), 80);
    });

    /// Cerezo (sakura) generado con ramificaciÃ³n recursiva.
    public static Sprite CherryTree => Cached("tree", () =>
    {
        const int W = 640, H = 512;
        var c = new Paint(W, H);
        var rnd = new System.Random(7);
        float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
        var blossoms = new List<Vector2>();
        Color bark = C(0.23f, 0.13f, 0.14f);

        void Branch(Vector2 p, float angle, float len, float thick, int depth)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector2 mid = p + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * len * 0.5f + new Vector2(R(-6, 6), R(-4, 4));
            Vector2 end = p + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * len;
            c.Capsule(p, mid, thick, thick * 0.85f, bark);
            c.Capsule(mid, end, thick * 0.85f, thick * 0.7f, bark);
            if (depth <= 2) blossoms.Add(mid);
            if (depth == 0 || len < 12f) { blossoms.Add(end); return; }
            int n = depth > 5 ? 2 : (rnd.NextDouble() < 0.45 ? 3 : 2);
            for (int i = 0; i < n; i++)
            {
                float spread = n == 2 ? (i == 0 ? R(-38, -14) : R(14, 38)) : (i - 1) * R(24, 40);
                Branch(end, angle + spread - (angle - 90f) * 0.12f, len * R(0.68f, 0.82f), thick * 0.66f, depth - 1);
            }
        }

        Branch(new Vector2(250, 0), 84f, 118f, 20f, 7);

        Color[] pinkDark = { C(0.90f, 0.50f, 0.65f, 0.9f), C(0.84f, 0.42f, 0.58f, 0.9f) };
        Color[] pinkLight = { C(1f, 0.76f, 0.84f, 0.92f), C(0.99f, 0.66f, 0.77f, 0.92f), C(1f, 0.88f, 0.92f, 0.92f) };
        foreach (var b in blossoms)
            for (int k = 0; k < 5; k++)
                c.Circle(b.x + R(-16, 16), b.y + R(-14, 12), R(7, 13), pinkDark[rnd.Next(pinkDark.Length)], 2f);
        foreach (var b in blossoms)
            for (int k = 0; k < 6; k++)
                c.Circle(b.x + R(-14, 14), b.y + R(-10, 14), R(5, 10), pinkLight[rnd.Next(pinkLight.Length)], 1.6f);
        return c.ToSprite("CherryTree", new Vector2(250f / W, 0f), 80);
    });

    /// Arena rastrillada (karesansui) con rocas, musgo y anillos concÃ©ntricos.
    public static Sprite RakedSand => Cached("sand", () =>
    {
        const int W = 1024, H = 200;
        var c = new Paint(W, H);
        Vector4[] rocks = { new Vector4(230, 150, 62, 30), new Vector4(318, 142, 30, 16), new Vector4(790, 148, 74, 34) };
        Color sandTop = C(0.90f, 0.84f, 0.72f), sandBottom = C(0.80f, 0.73f, 0.60f);
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                Color col = Color.Lerp(sandBottom, sandTop, y / (float)H);
                float best = 999f;
                foreach (var r in rocks)
                {
                    float nx = (x - r.x) / r.z, ny = (y - r.y) / r.w;
                    float d = (Mathf.Sqrt(nx * nx + ny * ny) - 1f) * (r.z + r.w) * 0.5f;
                    best = Mathf.Min(best, d);
                }
                float pattern = best < 70f ? Mathf.Sin(best * 0.42f) : Mathf.Sin(y * 0.42f + Mathf.PerlinNoise(x * 0.01f, 0.3f) * 4f);
                float groove = Mathf.SmoothStep(0.55f, 1f, pattern);
                float ridge = Mathf.SmoothStep(-0.6f, -1f, pattern);
                col = Color.Lerp(col, col * 0.86f, groove);
                col = Color.Lerp(col, Color.Lerp(col, Color.white, 0.25f), ridge);
                col.a = 1f;
                c.px[y * W + x] = col;
            }
        var rnd = new System.Random(5);
        foreach (var r in rocks)
        {
            c.Ellipse(r.x - r.z * 0.6f, r.y - r.w * 0.6f, r.z * 0.5f, r.w * 0.45f, C(0.36f, 0.48f, 0.27f, 0.9f), 0, 6f);
            c.Ellipse(r.x, r.y - 3, r.z, r.w, C(0.18f, 0.16f, 0.17f, 0.35f), 0, 8f);
            c.Ellipse(r.x, r.y, r.z, r.w, C(0.36f, 0.34f, 0.35f));
            c.Ellipse(r.x - r.z * 0.15f, r.y + r.w * 0.3f, r.z * 0.7f, r.w * 0.5f, C(0.58f, 0.56f, 0.55f, 0.7f), 0, 6f);
            for (int k = 0; k < 6; k++)
                c.Circle(r.x + (float)(rnd.NextDouble() - 0.5) * r.z, r.y - r.w * 0.8f + (float)rnd.NextDouble() * 6, 3 + (float)rnd.NextDouble() * 4, C(0.40f, 0.55f, 0.30f, 0.85f), 2f);
        }
        // Borde de piedras en la parte superior
        for (int x = -6; x < W + 6; x += 9)
            c.Circle(x + (float)rnd.NextDouble() * 4, H - 5 + (float)rnd.NextDouble() * 2, 5 + (float)rnd.NextDouble() * 3, Color.Lerp(C(0.42f, 0.39f, 0.36f), C(0.60f, 0.57f, 0.52f), (float)rnd.NextDouble()), 1.5f);
        return c.ToSprite("RakedSand", new Vector2(0.5f, 1f), W / 24f);
    });

    public static Sprite Vignette => Cached("vignette", () =>
    {
        var c = new Paint(256, 256);
        for (int y = 0; y < 256; y++)
            for (int x = 0; x < 256; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(128, 128)) / 128f;
                c.Blend(x, y, C(0.05f, 0.03f, 0.10f), Mathf.Pow(Mathf.Clamp01((d - 0.55f) / 0.75f), 1.5f) * 0.7f);
            }
        return c.ToSprite("Vignette", new Vector2(0.5f, 0.5f), 256);
    });
}

