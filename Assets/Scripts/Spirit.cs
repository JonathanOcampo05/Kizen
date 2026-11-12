using UnityEngine;

// Espíritu oscuro que cae; si llega abajo, el jugador pierde una vida.
public class Spirit : MonoBehaviour
{
    public float speed = 2f;
    public float bottomY = -5.5f;

    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);
        if (transform.position.y < bottomY)
        {
            KiZenGame.Instance.LoseLife();
            Destroy(gameObject);
        }
    }
}
