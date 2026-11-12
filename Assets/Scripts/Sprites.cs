using UnityEngine;

// Genera sprites simples por código (sin assets externos).
public static class Sprites
{
    public static Sprite Circle(Color color, int size = 64)
    {
        var tex = new Texture2D(size, size);
        float r = size / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(r, r));
                float a = Mathf.Clamp01(r - d);
                tex.SetPixel(x, y, new Color(color.r, color.g, color.b, a));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }

    public static Sprite Square(Color color)
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
    }
}
