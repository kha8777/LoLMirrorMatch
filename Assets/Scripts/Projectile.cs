using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public float speed;
    [HideInInspector] public float damage;
    [HideInInspector] public DamageType damageType = DamageType.Physical;

    public float lifetime = 2f;
    public bool isPiercing = false;

    public string ownerTag;
    public string targetTag;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        // ignore collisions with other projectiles
        if (other.CompareTag("PlayerBullet") || other.CompareTag("EnemyBullet"))
            return;

        // Player bullet should hit Enemies
        if (gameObject.CompareTag("PlayerBullet") && other.CompareTag("Enemy"))
        {
            var h = other.GetComponent<Health>();
            if (h != null)
            {
                h.TakeDamage(damage, damageType);
                if (!isPiercing)
                {
                    Destroy(gameObject);
                }
                return;
            }
        }

        // Enemy bullet should hit Player
        if (gameObject.CompareTag("EnemyBullet") && other.CompareTag("Player"))
        {
            var h = other.GetComponent<Health>();
            if (h != null)
            {
                h.TakeDamage(damage, damageType);
                Destroy(gameObject);
                return;
            }
        }

        if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
