using UnityEngine;

// Screen shake basado en "trauma": cada impacto suma trauma y se disipa con el tiempo.
public class CameraShake : MonoBehaviour
{
    static CameraShake instance;
    Vector3 basePos;
    float trauma;

    void Awake() { instance = this; basePos = transform.position; }

    public static void Add(float amount)
    {
        if (instance) instance.trauma = Mathf.Min(1f, instance.trauma + amount);
    }

    void LateUpdate()
    {
        trauma = Mathf.Max(0f, trauma - Time.deltaTime * 1.6f);
        float s = trauma * trauma;
        float t = Time.time * 28f;
        transform.position = basePos + new Vector3(Mathf.PerlinNoise(t, 0.1f) - 0.5f, Mathf.PerlinNoise(0.7f, t) - 0.5f, 0f) * s * 0.7f;
        transform.rotation = Quaternion.Euler(0f, 0f, (Mathf.PerlinNoise(t, t) - 0.5f) * s * 4f);
    }
}
