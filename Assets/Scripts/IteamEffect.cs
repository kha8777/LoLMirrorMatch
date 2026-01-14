using UnityEngine;

// Class này không gắn vào GameObject, chỉ dùng để chứa logic
public abstract class ItemEffect : ScriptableObject
{
    // Hàm này gọi khi món đồ được mặc vào
    public abstract void OnEquip(ChampionState state);

    // Hàm này gọi khi món đồ bị tháo ra (để trừ chỉ số hoặc xóa nội tại)
    public abstract void OnUnequip(ChampionState state);
}