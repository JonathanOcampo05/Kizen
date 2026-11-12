using UnityEngine;

// Esfera de ki disparada por el monje.
public class KiBall : MonoBehaviour
{
    public float speed = 10f;

    void Start() => Destroy(gameObject, 3f);

    void Update() => transform.Translate(Vector3.up * speed * Time.deltaTime);

    void OnTriggerEnter2D(Collider2D other)
    {
        var spirit = other.GetComponent<Spirit>();
        if (spirit == null) return;
        KiZenGame.Instance.AddScore(10);
        Destroy(spirit.gameObject);
        Destroy(gameObject);
    }
}
