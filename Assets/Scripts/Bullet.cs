using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f; // Bullet speed
    public float lifetime = 10f; // Time before bullet destroys itself

    void Start()
    {
        // Destroy the bullet after 'lifetime' seconds to prevent infinite bullets in the scene
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move the bullet forward constantly
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Destroy the bullet on any collision
        Debug.Log(collision.collider.name);
        if (!collision.collider.CompareTag("Enemy") && !collision.collider.CompareTag("Bullet"))
        {


            if (collision.collider.CompareTag("Player"))
            {
                PlayerController.mainPlayer.TakeDamage();
            }
            Destroy(gameObject);
        }
    }
}
