using UnityEngine;

public class AfterImageEffect : MonoBehaviour
{
    private SpriteRenderer sr;
    public float fadeSpeed = 2f; // Tốc độ mờ dần
    private Color color;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        color = sr.color;
    }

    void Update()
    {
        // Giảm dần alpha theo thời gian
        color.a -= fadeSpeed * Time.deltaTime;
        sr.color = color;

        // Khi mờ hẳn thì xóa object
        if (color.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}