using UnityEngine;
using System;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public float speed;
    [HideInInspector] public float damage;
    [HideInInspector] public DamageType damageType;

    public float lifetime = 3f;
    public bool isPiercing = false;

    public string ownerTag;
    public string targetTag;

    public Action<GameObject> OnHitTarget;

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
        if (other.CompareTag("PlayerBullet") || other.CompareTag("EnemyBullet")) return;

        if (gameObject.CompareTag("PlayerBullet") && other.CompareTag("Enemy") || // Player
            gameObject.CompareTag("EnemyBullet") && other.CompareTag("Player"))   // Enemy
        {
            var h = other.GetComponent<Health>();
            if (h != null)
            {
                OnHitTarget?.Invoke(other.gameObject);
                h.TakeDamage(damage, damageType);
                if (!isPiercing) Destroy(gameObject);
            }
        }

        // Obstacle
        if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
