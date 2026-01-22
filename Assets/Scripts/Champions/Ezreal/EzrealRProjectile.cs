using UnityEngine;
using System;

public class EzrealRProjectile : MonoBehaviour
{
    [HideInInspector] public float speed;
    [HideInInspector] public float damage;
    [HideInInspector] public DamageType damageType;

    public float lifetime = 3f;
    public bool isPiercing = true;

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

        bool isPlayerBullet = gameObject.CompareTag("PlayerBullet");
        bool isEnemyBullet = gameObject.CompareTag("EnemyBullet");

        if ((isPlayerBullet && other.CompareTag("Enemy")) || (isEnemyBullet && other.CompareTag("Player")))
        {
            var h = other.GetComponent<Health>();
            if (h != null)
            {
                float finalDamage = damage;

                if (other.CompareTag("Minion"))
                {
                    finalDamage *= 0.5f;
                }

                OnHitTarget?.Invoke(other.gameObject);
                h.TakeDamage(finalDamage, damageType);
            }
        }

        if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
