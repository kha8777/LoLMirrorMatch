using UnityEngine;

// vòng tròn trigger hiện trên champ
public class EzrealWMark : MonoBehaviour
{
    public float duration = 4f;
    [HideInInspector] public float bonusDamage;

    private void Start()
    {
        Destroy(gameObject, duration);
    }

    public void TriggerMark(Health targetHealth)
    {
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(bonusDamage, DamageType.Magical);
        }
        Destroy(gameObject);
    }
}