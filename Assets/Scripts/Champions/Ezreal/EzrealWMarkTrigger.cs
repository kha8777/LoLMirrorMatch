using UnityEngine;

// tạo mark nếu W trúng tướng
public class EzrealWMarkTrigger : MonoBehaviour
{
    public GameObject markPrefab;
    [HideInInspector] public float damageToApply;
    public Vector3 markOffset = new Vector3(0, -1f, 0);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EzrealWMark existingMark = other.GetComponentInChildren<EzrealWMark>();
            if (existingMark == null)
            {
                GameObject markObj = Instantiate(markPrefab, other.transform.position + markOffset, Quaternion.identity, other.transform);
                markObj.GetComponent<EzrealWMark>().bonusDamage = damageToApply;
            }
            Destroy(gameObject);
        }
    }
}