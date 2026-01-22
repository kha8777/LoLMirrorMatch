using UnityEngine;

// kích hoạt va chạm trigger
public class EzrealMarkExploder : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && gameObject.CompareTag("PlayerBullet"))
        {
            EzrealWMark mark = other.GetComponentInChildren<EzrealWMark>();
            if (mark != null)
            {
                var h = other.GetComponent<Health>();
                mark.TriggerMark(h);
            }
        }
    }
}