using UnityEngine;

public class IndicatorController : MonoBehaviour
{
    public float rotateSpeed;
    public float maxAngle;

    private void Update()
    {
        float pingPong = Mathf.PingPong(Time.time * rotateSpeed, 1f);
        float currentAngle = Mathf.Lerp(-maxAngle, maxAngle, pingPong);
        if (tag == "EnemyIndicator")
        {
            currentAngle += 180f;
        }
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);

    }
}
