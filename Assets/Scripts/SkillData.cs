using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "Hero Data/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public Sprite skillIcon;
    public float damageLV1;
    public float damagePerLevel;
    public string damegeType; // Physical, Magical, True
    public float additionalPhysicDamagePercent; // phần trăm AD cộng thêm vào sát thương kỹ năng
    public float additionalMagicDamagePercent; // phần trăm AP cộng thêm vào sát thương kỹ năng
    public float cooldown;
    public float manaCost;
    public float projectileSpeed;
    public GameObject skillPrefab;
    public string animationClipName;
}