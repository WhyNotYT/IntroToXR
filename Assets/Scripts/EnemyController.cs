using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public PlayerController player;
    public GameObject bullet;
    public Transform bulletSpawnPoint;

    public int health = 10;
    public float badAimFactor = 1;
    public bool canShoot = true;
    public GameObject explosion;
    void Start()
    {
        player = PlayerController.mainPlayer;
    }



    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            Destroy(this.gameObject);
        }
    }

    void TakeDamage()
    {
        health -= 4;


        if (health < 1)
        {

            GameManager.instance.EnemyKilled(this.gameObject);
            GameObject exp = Instantiate(explosion, this.transform.position, Quaternion.identity);

            Destroy(exp, 3);
            Destroy(this.gameObject);

        }
    }




    void Update()
    {
        this.transform.LookAt(player.transform);
    }

    public void Shoot()
    {
        if (canShoot){

        float distanceFromPlayer = Vector3.SqrMagnitude(player.transform.position - this.transform.position);
        if (player == null || bullet == null || bulletSpawnPoint == null || distanceFromPlayer < 4) return;

        // Spawn the bullet at the spawn point
        GameObject newBullet = Instantiate(bullet, bulletSpawnPoint.position, Quaternion.identity);
        Vector3 badAim = new Vector3(Random.Range(-badAimFactor, badAimFactor), Random.Range(-badAimFactor, badAimFactor), Random.Range(-badAimFactor, badAimFactor));
        // Make the bullet face the player
        Vector3 directionToPlayer = ((player.transform.position + badAim) - bulletSpawnPoint.position).normalized;
        newBullet.transform.forward = directionToPlayer;

    }}
}
