using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;

    [Header("Base Stats")]
    public float HP = 0f;
    public float AD = 0f;
    public float AP = 0f;

    [Header("Unique Passives")]
    public ItemEffect effect; // Kéo MuramanaEffect hay EssenceReaverEffect vào đây
}