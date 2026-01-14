using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    // Giá trị level của người dùng (cập nhật từ hệ thống lưu / server)
    public int EzrealLevel = 1;
    public int LuxLevel = 1;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}